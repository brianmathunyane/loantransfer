using LoanProcessor.Function.Transform;

namespace LoanProcessor.Function.Persistence;

public interface ITransformedLoanWriter
{
    Task ReplaceAsync(
        string loanId,
        Guid batchId,
        IReadOnlyList<TransformedOwnershipPeriod> periods,
        CancellationToken cancellationToken);
}
