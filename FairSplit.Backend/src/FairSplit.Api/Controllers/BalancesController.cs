using FairSplit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FairSplit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BalancesController(IBalanceService balanceService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        await balanceService.HandlePlaceholderAsync(cancellationToken);
        return StatusCode(StatusCodes.Status501NotImplemented, "Balances endpoints are scaffolded but not implemented yet.");
    }
}
