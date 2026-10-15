using LoanProcessor.Messaging;

namespace LoanProcessor.Function.Transform;

public interface ILoanOwnershipTransformer
{
    IReadOnlyList<TransformedOwnershipPeriod> Transform(LoanTransferMessage message);
}
