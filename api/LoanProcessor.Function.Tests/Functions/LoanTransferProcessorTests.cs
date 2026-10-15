using System.Text.Json;
using LoanProcessor.Messaging;
using LoanProcessor.Function.Functions;
using LoanProcessor.Function.Persistence;
using LoanProcessor.Function.Transform;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LoanProcessor.Function.Tests.Functions;

public class LoanTransferProcessorTests
{
    private readonly Mock<ILoanOwnershipTransformer> _transformer = new();
    private readonly Mock<ITransformedLoanWriter> _writer = new();
    private readonly LoanTransferProcessor _sut;

    public LoanTransferProcessorTests()
    {
        _sut = new LoanTransferProcessor(_transformer.Object, _writer.Object, NullLogger<LoanTransferProcessor>.Instance);
    }

    [Fact]
    public async Task Run_DeserializesMessageAndPersistsTheTransformResult()
    {
        var message = new LoanTransferMessage(Guid.NewGuid(), "A123456",
        [
            new OwnershipRecordPayload("Originator A", new DateOnly(2019, 8, 1),
                new Dictionary<string, decimal?> { ["f201908"] = 4500m }),
        ]);
        var body = JsonSerializer.Serialize(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var transformed = new List<TransformedOwnershipPeriod>
        {
            new("A123456", "Originator A", new DateOnly(2019, 8, 1),
                new Dictionary<DateOnly, decimal> { [new DateOnly(2019, 8, 1)] = 4500m }),
        };
        _transformer.Setup(t => t.Transform(It.IsAny<LoanTransferMessage>())).Returns(transformed);

        await _sut.Run(body, Mock.Of<FunctionContext>());

        _transformer.Verify(t => t.Transform(It.Is<LoanTransferMessage>(m => m.LoanId == "A123456")), Times.Once);
        _writer.Verify(w => w.ReplaceAsync("A123456", message.BatchId, transformed, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_WithUnparsableBody_ThrowsRatherThanSwallowingTheError()
    {
        await Assert.ThrowsAsync<JsonException>(() => _sut.Run("not valid json", Mock.Of<FunctionContext>()));

        _writer.Verify(
            w => w.ReplaceAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IReadOnlyList<TransformedOwnershipPeriod>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
