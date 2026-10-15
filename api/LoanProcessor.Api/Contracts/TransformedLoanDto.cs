namespace LoanProcessor.Api.Contracts;

public sealed record TransformedLoanDto(
    string LoanId,
    string Owner,
    DateOnly StartDate,
    IReadOnlyDictionary<string, decimal> RpbByPeriod);
