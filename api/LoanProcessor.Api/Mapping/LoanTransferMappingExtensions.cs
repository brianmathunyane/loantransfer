using LoanProcessor.Api.Contracts;
using LoanProcessor.Messaging;

namespace LoanProcessor.Api.Mapping;

public static class LoanTransferMappingExtensions
{
    public static IReadOnlyList<LoanTransferMessage> ToLoanTransferMessages(this RawLoanBatchRequest batch, Guid batchId) =>
        batch.Records
            .GroupBy(r => r.LoanId)
            .Select(group => new LoanTransferMessage(
                batchId,
                group.Key,
                group
                    .OrderBy(r => r.StartDate)
                    .Select(r => new OwnershipRecordPayload(r.Owner, r.StartDate, r.RpbByPeriod))
                    .ToList()))
            .ToList();
}
