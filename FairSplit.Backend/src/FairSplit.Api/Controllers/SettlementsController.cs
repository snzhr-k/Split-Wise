using FairSplit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FairSplit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SettlementsController(ISettlementService settlementService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        await settlementService.HandlePlaceholderAsync(cancellationToken);
        return StatusCode(StatusCodes.Status501NotImplemented, "Settlements endpoints are scaffolded but not implemented yet.");
    }
}
