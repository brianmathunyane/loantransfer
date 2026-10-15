using System.Text.Json;
using LoanProcessor.Messaging;
using LoanProcessor.Function.Persistence;
using LoanProcessor.Function.Transform;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LoanProcessor.Function.Functions;

public sealed class LoanTransferProcessor(
    ILoanOwnershipTransformer transformer,
    ITransformedLoanWriter writer,
    ILogger<LoanTransferProcessor> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    [Function(nameof(LoanTransferProcessor))]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnection")] string messageBody,
        FunctionContext context)
    {
        var message = JsonSerializer.Deserialize<LoanTransferMessage>(messageBody, SerializerOptions)
            ?? throw new InvalidOperationException("Service Bus message body could not be deserialized into a LoanTransferMessage.");

        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            ["BatchId"] = message.BatchId,
            ["LoanId"] = message.LoanId,
        });

        logger.LogInformation(
            "Processing {OwnershipRecordCount} ownership record(s) for loan {LoanId}.",
            message.OwnershipRecords.Count, message.LoanId);

        var transformed = transformer.Transform(message);

        await writer.ReplaceAsync(message.LoanId, message.BatchId, transformed, context.CancellationToken);

        logger.LogInformation(
            "Persisted {PeriodCount} ownership period(s) for loan {LoanId}.",
            transformed.Count, message.LoanId);
    }
}
