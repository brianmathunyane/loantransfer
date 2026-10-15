using LoanProcessor.Messaging;

namespace LoanProcessor.Api.Messaging;

public interface ILoanTransferPublisher
{
    Task PublishAsync(IReadOnlyList<LoanTransferMessage> messages, CancellationToken cancellationToken);
}
