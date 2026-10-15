using System.Text.Json;
using Azure.Messaging.ServiceBus;
using LoanProcessor.Messaging;
using Microsoft.Extensions.Options;

namespace LoanProcessor.Api.Messaging;

public sealed class ServiceBusLoanTransferPublisher : ILoanTransferPublisher, IAsyncDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusLoanTransferPublisher> _logger;

    public ServiceBusLoanTransferPublisher(
        ServiceBusClient client,
        IOptions<ServiceBusOptions> options,
        ILogger<ServiceBusLoanTransferPublisher> logger)
    {
        _sender = client.CreateSender(options.Value.QueueName);
        _logger = logger;
    }

    public async Task PublishAsync(IReadOnlyList<LoanTransferMessage> messages, CancellationToken cancellationToken)
    {
        var pending = new Queue<ServiceBusMessage>(messages.Select(ToServiceBusMessage));

        while (pending.Count > 0)
        {
            using var batch = await _sender.CreateMessageBatchAsync(cancellationToken);

            while (pending.Count > 0 && batch.TryAddMessage(pending.Peek()))
            {
                pending.Dequeue();
            }

            if (batch.Count == 0)
            {
                throw new InvalidOperationException("A loan transfer message is too large to fit in a Service Bus batch.");
            }

            await _sender.SendMessagesAsync(batch, cancellationToken);
        }

        _logger.LogInformation("Published {MessageCount} loan transfer message(s) to Service Bus.", messages.Count);
    }

    private static ServiceBusMessage ToServiceBusMessage(LoanTransferMessage message) =>
        new(JsonSerializer.Serialize(message, SerializerOptions))
        {
            ContentType = "application/json",
            Subject = "LoanTransferBatchReceived",
            CorrelationId = message.BatchId.ToString(),
            MessageId = $"{message.BatchId}:{message.LoanId}",
        };

    public async ValueTask DisposeAsync() => await _sender.DisposeAsync();
}
