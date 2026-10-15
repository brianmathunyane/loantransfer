using LoanProcessor.Api.Contracts;
using LoanProcessor.Api.Mapping;
using LoanProcessor.Api.Messaging;
using LoanProcessor.Api.Validation;
using Microsoft.AspNetCore.Mvc;

namespace LoanProcessor.Api.Controllers;

[ApiController]
[Route("api/loan-transfers")]
public sealed class LoanTransfersController(
    IRawLoanBatchValidator validator,
    ILoanTransferPublisher publisher,
    ILogger<LoanTransfersController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(SubmitLoanTransfersResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostTransfers(
        [FromBody] RawLoanBatchRequest batch, CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(batch);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return new BadRequestObjectResult(new ValidationProblemDetails(ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more loan records failed validation.",
            });
        }

        var batchId = Guid.NewGuid();
        using var _ = logger.BeginScope(new Dictionary<string, object> { ["BatchId"] = batchId });

        var messages = batch.ToLoanTransferMessages(batchId);
        await publisher.PublishAsync(messages, cancellationToken);

        logger.LogInformation(
            "Accepted {RecordCount} loan record(s) across {LoanCount} loan(s).",
            batch.Records.Count, messages.Count);

        return Accepted(new SubmitLoanTransfersResponse(batchId, messages.Count, batch.Records.Count));
    }
}
