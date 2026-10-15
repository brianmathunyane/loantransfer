namespace LoanProcessor.Api.Contracts;

public sealed record RawLoanRecordRequest
{
    public required string LoanId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required string Owner { get; init; }
    public required IReadOnlyDictionary<string, decimal?> RpbByPeriod { get; init; }
}
