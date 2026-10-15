using LoanProcessor.Api.Contracts;
using LoanProcessor.Api.Controllers;
using LoanProcessor.Api.Messaging;
using LoanProcessor.Api.Validation;
using LoanProcessor.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LoanProcessor.Api.Tests.Controllers;

public class LoanTransfersControllerTests
{
    private readonly Mock<IRawLoanBatchValidator> _validator = new();
    private readonly Mock<ILoanTransferPublisher> _publisher = new();
    private readonly LoanTransfersController _sut;

    public LoanTransfersControllerTests()
    {
        _sut = new LoanTransfersController(
            _validator.Object,
            _publisher.Object,
            NullLogger<LoanTransfersController>.Instance);
    }

    private static RawLoanBatchRequest SampleBatch() => new()
    {
        Records =
        [
            new RawLoanRecordRequest
            {
                LoanId = "A123456",
                StartDate = new DateOnly(2019, 8, 1),
                Owner = "Originator A",
                RpbByPeriod = new Dictionary<string, decimal?> { ["f201908"] = 4500m },
            },
        ],
    };

    [Fact]
    public async Task PostTransfers_WithValidBatch_PublishesAndReturnsAccepted()
    {
        _validator.Setup(v => v.Validate(It.IsAny<RawLoanBatchRequest>())).Returns(new BatchValidationResult());

        var result = await _sut.PostTransfers(SampleBatch(), CancellationToken.None);

        var accepted = Assert.IsType<AcceptedResult>(result);
        var response = Assert.IsType<SubmitLoanTransfersResponse>(accepted.Value);
        Assert.Equal(1, response.LoanCount);
        Assert.Equal(1, response.RecordCount);

        _publisher.Verify(
            p => p.PublishAsync(
                It.Is<IReadOnlyList<LoanTransferMessage>>(m => m.Count == 1 && m[0].LoanId == "A123456"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PostTransfers_WithInvalidBatch_ReturnsValidationProblemAndDoesNotPublish()
    {
        var invalidResult = new BatchValidationResult();
        invalidResult.AddError("records[0]: loanId is required.");
        _validator.Setup(v => v.Validate(It.IsAny<RawLoanBatchRequest>())).Returns(invalidResult);

        var result = await _sut.PostTransfers(SampleBatch(), CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);

        _publisher.Verify(
            p => p.PublishAsync(It.IsAny<IReadOnlyList<LoanTransferMessage>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
