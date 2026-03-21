using FairSplit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FairSplit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ExpenseParticipantsController(IExpenseParticipantService expenseParticipantService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        await expenseParticipantService.HandlePlaceholderAsync(cancellationToken);
        return StatusCode(StatusCodes.Status501NotImplemented, "ExpenseParticipants endpoints are scaffolded but not implemented yet.");
    }
}
