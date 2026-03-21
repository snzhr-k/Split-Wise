using FairSplit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FairSplit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MembersController(IMemberService memberService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        await memberService.HandlePlaceholderAsync(cancellationToken);
        return StatusCode(StatusCodes.Status501NotImplemented, "Members endpoints are scaffolded but not implemented yet.");
    }
}
