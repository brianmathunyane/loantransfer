using LoanProcessor.Data;
using LoanProcessor.Data.Entities;
using LoanProcessor.Function.Transform;
using Microsoft.EntityFrameworkCore;

namespace LoanProcessor.Function.Persistence;

/// <summary>
/// Replaces a loan's entire set of ownership periods atomically. Service Bus (and the
/// Functions host's retry behavior) only guarantee at-least-once delivery, and each message
/// already carries a loan's complete ownership history, so a redelivered or reprocessed
/// message must be safe to apply twice — delete-then-insert for the loan makes that true
/// without needing per-row upsert logic.
/// </summary>
public sealed class TransformedLoanWriter(LoanProcessorDbContext dbContext) : ITransformedLoanWriter
{
    public async Task ReplaceAsync(
        string loanId,
        Guid batchId,
        IReadOnlyList<TransformedOwnershipPeriod> periods,
        CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            await dbContext.OwnershipPeriods
                .Where(p => p.LoanId == loanId)
                .ExecuteDeleteAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var entities = periods.Select(p => new LoanOwnershipPeriodEntity
            {
                Id = Guid.NewGuid(),
                LoanId = p.LoanId,
                Owner = p.Owner,
                StartDate = p.StartDate,
                BatchId = batchId,
                CreatedAtUtc = now,
                PeriodBalances = p.RpbByPeriod
                    .Select(kvp => new LoanPeriodBalanceEntity { Id = Guid.NewGuid(), PeriodDate = kvp.Key, Rpb = kvp.Value })
                    .ToList(),
            });

            dbContext.OwnershipPeriods.AddRange(entities);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
