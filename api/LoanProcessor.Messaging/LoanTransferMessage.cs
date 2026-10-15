namespace LoanProcessor.Messaging;

/// <summary>
/// The Service Bus message contract shared by the API (producer) and the Function (consumer).
/// One message per loan: a self-contained snapshot of that loan's full ownership history,
/// since the transform needs every ownership row for a loan to compute period boundaries.
/// </summary>
public sealed record LoanTransferMessage(
    Guid BatchId,
    string LoanId,
    IReadOnlyList<OwnershipRecordPayload> OwnershipRecords);

public sealed record OwnershipRecordPayload(
    string Owner,
    DateOnly StartDate,
    IReadOnlyDictionary<string, decimal?> RpbByPeriod);
