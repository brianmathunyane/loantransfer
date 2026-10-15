namespace LoanProcessor.Data.Repositories;

public sealed record TransformedLoanRecord(
    string LoanId,
    string Owner,
    DateOnly StartDate,
    IReadOnlyDictionary<string, decimal> RpbByPeriod);
