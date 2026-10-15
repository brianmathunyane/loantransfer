using LoanProcessor.Messaging;
using LoanProcessor.Function.Transform;

namespace LoanProcessor.Function.Tests.Transform;

/// <summary>
/// Proves the transform against the exact worked example from the assessment PDF
/// ("Loan servicer transfers" problem statement): three loans, including one
/// (M345678) where an originator re-takes ownership after a servicer.
/// </summary>
public class LoanOwnershipTransformerTests
{
    private readonly LoanOwnershipTransformer _sut = new();

    private static OwnershipRecordPayload Record(string owner, int year, int month, params (string period, decimal? rpb)[] balances) =>
        new(owner, new DateOnly(year, month, 1), balances.ToDictionary(b => b.period, b => b.rpb));

    private static readonly Dictionary<string, decimal?> FullHistoryA123456 = new()
    {
        ["f202004"] = 1000m,
        ["f202003"] = 2000m,
        ["f202002"] = 3000m,
        ["f202001"] = 4000m,
        ["f201912"] = 5000m,
        ["f201911"] = 6000m,
        ["f201910"] = 7000m,
        ["f201909"] = 8000m,
        ["f201908"] = 4500m,
    };

    [Fact]
    public void Transform_A123456_AllocatesEachPeriodToItsOwningServicer()
    {
        var message = new LoanTransferMessage(Guid.NewGuid(), "A123456",
        [
            new OwnershipRecordPayload("Originator A", new DateOnly(2019, 8, 1), FullHistoryA123456),
            new OwnershipRecordPayload("Servicer A", new DateOnly(2019, 9, 1), FullHistoryA123456),
            new OwnershipRecordPayload("Servicer B", new DateOnly(2019, 12, 1), FullHistoryA123456),
            new OwnershipRecordPayload("Servicer C", new DateOnly(2020, 2, 1), FullHistoryA123456),
        ]);

        var result = _sut.Transform(message);

        Assert.Equal(4, result.Count);

        AssertPeriods(result, "Originator A", ("f201908", 4500m));
        AssertPeriods(result, "Servicer A", ("f201911", 6000m), ("f201910", 7000m), ("f201909", 8000m));
        AssertPeriods(result, "Servicer B", ("f202001", 4000m), ("f201912", 5000m));
        AssertPeriods(result, "Servicer C", ("f202004", 1000m), ("f202003", 2000m), ("f202002", 3000m));
    }

    [Fact]
    public void Transform_H789012_SingleOwner_GetsAllOfItsPeriodsAndNoneBeforeLoanExisted()
    {
        var message = new LoanTransferMessage(Guid.NewGuid(), "H789012",
        [
            Record("Originator B", 2020, 1,
                ("f202004", 5000m), ("f202003", 6000m), ("f202002", 7000m), ("f202001", 8000m),
                ("f201912", null), ("f201911", null), ("f201910", null), ("f201909", null), ("f201908", null)),
        ]);

        var result = _sut.Transform(message);

        var period = Assert.Single(result);
        Assert.Equal("Originator B", period.Owner);
        AssertPeriods(result, "Originator B", ("f202004", 5000m), ("f202003", 6000m), ("f202002", 7000m), ("f202001", 8000m));
    }

    [Fact]
    public void Transform_M345678_OriginatorReclaimingOwnershipAfterServicer_SplitsCorrectly()
    {
        var full = new Dictionary<string, decimal?>
        {
            ["f202004"] = 3000m,
            ["f202003"] = 4000m,
            ["f202002"] = 5000m,
            ["f202001"] = 6000m,
            ["f201912"] = 7000m,
            ["f201911"] = 8000m,
            ["f201910"] = 9000m,
            ["f201909"] = 10000m,
            ["f201908"] = null,
        };

        var message = new LoanTransferMessage(Guid.NewGuid(), "M345678",
        [
            new OwnershipRecordPayload("Originator C", new DateOnly(2019, 9, 1), full),
            new OwnershipRecordPayload("Servicer E", new DateOnly(2019, 11, 1), full),
            new OwnershipRecordPayload("Originator C", new DateOnly(2020, 2, 1), full),
        ]);

        var result = _sut.Transform(message);

        Assert.Equal(3, result.Count);
        Assert.Equal(["Originator C", "Servicer E", "Originator C"], result.Select(r => r.Owner));

        AssertPeriods(result.Where(r => r.StartDate == new DateOnly(2019, 9, 1)).ToList(), "Originator C",
            ("f201910", 9000m), ("f201909", 10000m));
        AssertPeriods(result.Where(r => r.StartDate == new DateOnly(2019, 11, 1)).ToList(), "Servicer E",
            ("f202001", 6000m), ("f201912", 7000m), ("f201911", 8000m));
        AssertPeriods(result.Where(r => r.StartDate == new DateOnly(2020, 2, 1)).ToList(), "Originator C",
            ("f202004", 3000m), ("f202003", 4000m), ("f202002", 5000m));
    }

    [Fact]
    public void Transform_NullRpbValues_AreNeverAllocatedToAnyOwner()
    {
        var message = new LoanTransferMessage(Guid.NewGuid(), "H789012",
        [
            Record("Originator B", 2020, 1, ("f202001", 8000m), ("f201912", null)),
        ]);

        var result = _sut.Transform(message);

        var period = Assert.Single(result);
        Assert.DoesNotContain(period.RpbByPeriod.Keys, k => k == new DateOnly(2019, 12, 1));
    }

    private static void AssertPeriods(
        IReadOnlyList<TransformedOwnershipPeriod> periods, string owner, params (string period, decimal rpb)[] expected)
    {
        var match = periods.Single(p => p.Owner == owner);
        var expectedDict = expected.ToDictionary(
            e => PeriodKeyToDate(e.period),
            e => e.rpb);

        Assert.Equal(expectedDict, match.RpbByPeriod);
    }

    private static DateOnly PeriodKeyToDate(string periodKey) =>
        new(int.Parse(periodKey[1..5]), int.Parse(periodKey[5..7]), 1);
}
