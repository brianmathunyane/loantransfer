namespace LoanProcessor.Api.Contracts;

public sealed record RawLoanBatchRequest
{
    public required IReadOnlyList<RawLoanRecordRequest> Records { get; init; }
}
