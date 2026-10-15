namespace LoanProcessor.Api.Contracts;

public sealed record SubmitLoanTransfersResponse(Guid BatchId, int LoanCount, int RecordCount);
