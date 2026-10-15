using Microsoft.EntityFrameworkCore;

namespace LoanProcessor.Data.Repositories;

public sealed class TransformedLoanRepository(LoanProcessorDbContext dbContext) : ITransformedLoanRepository
{
    public async Task<IReadOnlyList<TransformedLoanRecord>> GetAllAsync(CancellationToken cancellationToken)
    {
        var ownershipPeriods = await dbContext.OwnershipPeriods
            .AsNoTracking()
            .Include(p => p.PeriodBalances)
            .OrderBy(p => p.LoanId)
            .ThenBy(p => p.StartDate)
            .ToListAsync(cancellationToken);

        return ownershipPeriods
            .Select(p => new TransformedLoanRecord(
                p.LoanId,
                p.Owner,
                p.StartDate,
                p.PeriodBalances
                    .OrderByDescending(b => b.PeriodDate)
                    .ToDictionary(b => PeriodKeyConverter.ToPeriodKey(b.PeriodDate), b => b.Rpb)))
            .ToList();
    }
}
