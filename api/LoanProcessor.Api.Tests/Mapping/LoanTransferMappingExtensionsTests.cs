using LoanProcessor.Api.Contracts;
using LoanProcessor.Api.Mapping;

namespace LoanProcessor.Api.Tests.Mapping;

public class LoanTransferMappingExtensionsTests
{
    private static RawLoanRecordRequest Record(string loanId, DateOnly startDate, string owner) => new()
    {
        LoanId = loanId,
        StartDate = startDate,
        Owner = owner,
        RpbByPeriod = new Dictionary<string, decimal?> { ["f201908"] = 1000m },
    };

    [Fact]
    public void ToLoanTransferMessages_GroupsRecordsByLoanId()
    {
        var batch = new RawLoanBatchRequest
        {
            Records =
            [
                Record("A123456", new DateOnly(2019, 8, 1), "Originator A"),
                Record("A123456", new DateOnly(2019, 9, 1), "Servicer A"),
                Record("A123456", new DateOnly(2019, 12, 1), "Servicer B"),
                Record("A123456", new DateOnly(2020, 2, 1), "Servicer C"),
                Record("H789012", new DateOnly(2020, 1, 1), "Originator B"),
                Record("M345678", new DateOnly(2019, 9, 1), "Originator C"),
                Record("M345678", new DateOnly(2019, 11, 1), "Servicer E"),
                Record("M345678", new DateOnly(2020, 2, 1), "Originator C"),
            ],
        };

        var messages = batch.ToLoanTransferMessages(Guid.NewGuid());

        Assert.Equal(3, messages.Count);
        Assert.Equal(4, messages.Single(m => m.LoanId == "A123456").OwnershipRecords.Count);
        Assert.Single(messages.Single(m => m.LoanId == "H789012").OwnershipRecords);
        Assert.Equal(3, messages.Single(m => m.LoanId == "M345678").OwnershipRecords.Count);
    }

    [Fact]
    public void ToLoanTransferMessages_OrdersOwnershipRecordsByStartDateAscending()
    {
        var batch = new RawLoanBatchRequest
        {
            Records =
            [
                Record("A123456", new DateOnly(2020, 2, 1), "Servicer C"),
                Record("A123456", new DateOnly(2019, 8, 1), "Originator A"),
                Record("A123456", new DateOnly(2019, 12, 1), "Servicer B"),
                Record("A123456", new DateOnly(2019, 9, 1), "Servicer A"),
            ],
        };

        var message = batch.ToLoanTransferMessages(Guid.NewGuid()).Single();

        Assert.Equal(
            ["Originator A", "Servicer A", "Servicer B", "Servicer C"],
            message.OwnershipRecords.Select(r => r.Owner));
    }

    [Fact]
    public void ToLoanTransferMessages_StampsEveryMessageWithTheSameBatchId()
    {
        var batchId = Guid.NewGuid();
        var batch = new RawLoanBatchRequest
        {
            Records =
            [
                Record("A123456", new DateOnly(2019, 8, 1), "Originator A"),
                Record("H789012", new DateOnly(2020, 1, 1), "Originator B"),
            ],
        };

        var messages = batch.ToLoanTransferMessages(batchId);

        Assert.All(messages, m => Assert.Equal(batchId, m.BatchId));
    }
}
