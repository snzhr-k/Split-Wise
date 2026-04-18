using FairSplit.Api.Presentation.Models.Responses;
using FairSplit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FairSplit.Api.Controllers;

[ApiController]
[Route("api/groups/{groupId:guid}/balances")]
public sealed class BalancesController(IBalanceService balanceService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<BalanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<BalanceResponse>>> GetByGroupId(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var balances = await balanceService.GetByGroupIdAsync(groupId, cancellationToken);

        var response = balances
            .Select(balance => new BalanceResponse
            {
                GroupId = balance.GroupId,
                MemberId = balance.MemberId,
                NetAmount = balance.NetAmount
            })
            .ToList();

        return Ok(response);
    }
}
