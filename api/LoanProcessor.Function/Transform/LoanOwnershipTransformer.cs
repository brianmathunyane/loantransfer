using LoanProcessor.Messaging;
using LoanProcessor.Data;

namespace LoanProcessor.Function.Transform;

/// <summary>
/// Slices each period's RPB to the single owner whose ownership window contains it.
/// A period belongs to an owner if the period's month falls on or after the owner's start
/// month and strictly before the next owner's start month (the current/last owner's window
/// is open-ended). Pure and side-effect free so it can be unit tested without any
/// infrastructure.
/// </summary>
public sealed class LoanOwnershipTransformer : ILoanOwnershipTransformer
{
    public IReadOnlyList<TransformedOwnershipPeriod> Transform(LoanTransferMessage message)
    {
        var orderedRecords = message.OwnershipRecords
            .OrderBy(r => r.StartDate)
            .ToList();

        var canonicalBalances = BuildCanonicalBalances(orderedRecords);
        var results = new List<TransformedOwnershipPeriod>(orderedRecords.Count);

        for (var i = 0; i < orderedRecords.Count; i++)
        {
            var current = orderedRecords[i];
            var windowStart = MonthStart(current.StartDate);
            var windowEnd = i + 1 < orderedRecords.Count ? MonthStart(orderedRecords[i + 1].StartDate) : (DateOnly?)null;

            var allocated = canonicalBalances
                .Where(kvp => kvp.Key >= windowStart && (windowEnd is null || kvp.Key < windowEnd))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            results.Add(new TransformedOwnershipPeriod(message.LoanId, current.Owner, current.StartDate, allocated));
        }

        return results;
    }

    // The raw data repeats the same RPB values on every ownership row for a loan;
    // collapse them into one canonical period -> RPB map before slicing by owner.
    private static Dictionary<DateOnly, decimal> BuildCanonicalBalances(IEnumerable<OwnershipRecordPayload> records)
    {
        var balances = new Dictionary<DateOnly, decimal>();

        foreach (var record in records)
        {
            foreach (var (periodKey, value) in record.RpbByPeriod)
            {
                if (value is decimal rpb && PeriodKeyConverter.TryParse(periodKey, out var periodDate))
                {
                    balances[periodDate] = rpb;
                }
            }
        }

        return balances;
    }

    private static DateOnly MonthStart(DateOnly date) => new(date.Year, date.Month, 1);
}
