using LoanProcessor.Data;
using LoanProcessor.Function.Persistence;
using LoanProcessor.Function.Transform;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LoanProcessor.Function.Tests.Persistence;

/// <summary>
/// Uses a real (SQLite, in-memory) relational provider rather than EF's InMemory provider,
/// because InMemory doesn't support ExecuteDeleteAsync, which the idempotent replace relies on.
/// </summary>
public sealed class TransformedLoanWriterTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LoanProcessorDbContext _dbContext;
    private readonly TransformedLoanWriter _sut;

    public TransformedLoanWriterTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LoanProcessorDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new LoanProcessorDbContext(options);
        _dbContext.Database.EnsureCreated();
        _sut = new TransformedLoanWriter(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    private static TransformedOwnershipPeriod Period(string loanId, string owner, int year, int month, params (int year, int month, decimal rpb)[] balances) =>
        new(loanId, owner, new DateOnly(year, month, 1),
            balances.ToDictionary(b => new DateOnly(b.year, b.month, 1), b => b.rpb));

    [Fact]
    public async Task ReplaceAsync_PersistsOwnershipPeriodsAndTheirBalances()
    {
        var periods = new[] { Period("A123456", "Originator A", 2019, 8, (2019, 8, 4500m)) };

        await _sut.ReplaceAsync("A123456", Guid.NewGuid(), periods, CancellationToken.None);

        var saved = await _dbContext.OwnershipPeriods.Include(p => p.PeriodBalances).ToListAsync();
        var period = Assert.Single(saved);
        Assert.Equal("Originator A", period.Owner);
        var balance = Assert.Single(period.PeriodBalances);
        Assert.Equal(4500m, balance.Rpb);
    }

    [Fact]
    public async Task ReplaceAsync_CalledTwiceForTheSameLoan_ReplacesRatherThanDuplicates()
    {
        var firstVersion = new[] { Period("A123456", "Originator A", 2019, 8, (2019, 8, 4500m)) };
        var secondVersion = new[]
        {
            Period("A123456", "Originator A", 2019, 8, (2019, 8, 4500m)),
            Period("A123456", "Servicer A", 2019, 9, (2019, 9, 8000m)),
        };

        await _sut.ReplaceAsync("A123456", Guid.NewGuid(), firstVersion, CancellationToken.None);
        await _sut.ReplaceAsync("A123456", Guid.NewGuid(), secondVersion, CancellationToken.None);

        var saved = await _dbContext.OwnershipPeriods.Include(p => p.PeriodBalances).ToListAsync();
        Assert.Equal(2, saved.Count);
        Assert.Equal(2, await _dbContext.PeriodBalances.CountAsync()); // only secondVersion's balances remain; firstVersion's were deleted, not left orphaned
    }

    [Fact]
    public async Task ReplaceAsync_DoesNotAffectOtherLoans()
    {
        await _sut.ReplaceAsync("A123456", Guid.NewGuid(), [Period("A123456", "Originator A", 2019, 8, (2019, 8, 4500m))], CancellationToken.None);
        await _sut.ReplaceAsync("H789012", Guid.NewGuid(), [Period("H789012", "Originator B", 2020, 1, (2020, 1, 8000m))], CancellationToken.None);

        var loanIds = await _dbContext.OwnershipPeriods.Select(p => p.LoanId).ToListAsync();
        Assert.Contains("A123456", loanIds);
        Assert.Contains("H789012", loanIds);
    }
}
