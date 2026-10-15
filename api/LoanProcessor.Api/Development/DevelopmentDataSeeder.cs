using LoanProcessor.Data;
using LoanProcessor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanProcessor.Api.Development;

/// <summary>
/// Inserts demo transformed loans so GET /api/transformed-loans has data to return before the
/// Azure Function worker (which owns this table in production) exists. No-ops once any row exists,
/// so it steps out of the way the moment the worker starts writing real data.
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(LoanProcessorDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.OwnershipPeriods.AnyAsync(cancellationToken))
        {
            return;
        }

        var batchId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        LoanOwnershipPeriodEntity Period(string loanId, string owner, DateOnly startDate, params (DateOnly period, decimal rpb)[] balances) =>
            new()
            {
                Id = Guid.NewGuid(),
                LoanId = loanId,
                Owner = owner,
                StartDate = startDate,
                BatchId = batchId,
                CreatedAtUtc = now,
                PeriodBalances = balances
                    .Select(b => new LoanPeriodBalanceEntity { Id = Guid.NewGuid(), PeriodDate = b.period, Rpb = b.rpb })
                    .ToList(),
            };

        DateOnly D(int year, int month) => new(year, month, 1);

        var seedData = new[]
        {
            Period("A123456", "Originator A", D(2019, 8), (D(2019, 8), 4500m)),
            Period("A123456", "Servicer A", D(2019, 9), (D(2019, 11), 6000m), (D(2019, 10), 7000m), (D(2019, 9), 8000m)),
            Period("A123456", "Servicer B", D(2019, 12), (D(2020, 1), 4000m), (D(2019, 12), 5000m)),
            Period("A123456", "Servicer C", D(2020, 2), (D(2020, 4), 1000m), (D(2020, 3), 2000m), (D(2020, 2), 3000m)),
            Period("H789012", "Originator B", D(2020, 1), (D(2020, 4), 5000m), (D(2020, 3), 6000m), (D(2020, 2), 7000m), (D(2020, 1), 8000m)),
            Period("M345678", "Originator C", D(2019, 9), (D(2019, 10), 9000m), (D(2019, 9), 10000m)),
            Period("M345678", "Servicer E", D(2019, 11), (D(2020, 1), 6000m), (D(2019, 12), 7000m), (D(2019, 11), 8000m)),
            Period("M345678", "Originator C", D(2020, 2), (D(2020, 4), 3000m), (D(2020, 3), 4000m), (D(2020, 2), 5000m)),
        };

        dbContext.OwnershipPeriods.AddRange(seedData);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
