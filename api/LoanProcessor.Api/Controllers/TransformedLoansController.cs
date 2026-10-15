using LoanProcessor.Api.Contracts;
using LoanProcessor.Api.Mapping;
using LoanProcessor.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LoanProcessor.Api.Controllers;

[ApiController]
[Route("api/transformed-loans")]
public sealed class TransformedLoansController(ITransformedLoanRepository repository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransformedLoanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransformedLoans(CancellationToken cancellationToken)
    {
        var records = await repository.GetAllAsync(cancellationToken);
        return Ok(records.Select(r => r.ToDto()).ToList());
    }
}
