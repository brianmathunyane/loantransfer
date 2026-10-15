namespace LoanProcessor.Function.Transform;

public sealed record TransformedOwnershipPeriod(
    string LoanId,
    string Owner,
    DateOnly StartDate,
    IReadOnlyDictionary<DateOnly, decimal> RpbByPeriod);
