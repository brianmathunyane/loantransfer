using System.ComponentModel.DataAnnotations;

namespace LoanProcessor.Api.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    [Required]
    public required string ConnectionString { get; init; }

    [Required]
    public required string QueueName { get; init; }
}
