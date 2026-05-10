using FairSplit.Api.Presentation.Models.Requests;
using FairSplit.Api.Presentation.Models.Responses;
using FairSplit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FairSplit.Api.Controllers;

[ApiController]
[Route("api/groups/{groupId:guid}/settlements")]
[Authorize]
public sealed class SettlementsController(ISettlementService settlementService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<SettlementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<SettlementResponse>>> GetByGroupId(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var settlements = await settlementService.GetByGroupIdAsync(groupId, cancellationToken);

        var response = settlements.Select(MapResponse).ToList();

        return Ok(response);
    }

    [HttpGet("{settlementId:guid}")]
    [ProducesResponseType(typeof(SettlementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SettlementResponse>> GetById(
        Guid groupId,
        Guid settlementId,
        CancellationToken cancellationToken)
    {
        var settlement = await settlementService.GetByIdAsync(groupId, settlementId, cancellationToken);

        return Ok(MapResponse(settlement));
    }

    [HttpPost]
    [ProducesResponseType(typeof(SettlementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SettlementResponse>> Create(
        Guid groupId,
        [FromBody] SettlementRequest request,
        CancellationToken cancellationToken)
    {
        var settlement = await settlementService.CreateAsync(
            groupId,
            request.FromMemberId,
            request.ToMemberId,
            request.Amount,
            cancellationToken);

        var response = MapResponse(settlement);

        return Created($"/api/groups/{groupId}/settlements/{response.Id}", response);
    }

    private static SettlementResponse MapResponse(Domain.Entities.Settlement settlement)
    {
        return new SettlementResponse
        {
            Id = settlement.Id,
            GroupId = settlement.GroupId,
            FromMemberId = settlement.FromMemberId,
            ToMemberId = settlement.ToMemberId,
            Amount = settlement.Amount,
            SettledAtUtc = settlement.SettledAtUtc
        };
    }
}
