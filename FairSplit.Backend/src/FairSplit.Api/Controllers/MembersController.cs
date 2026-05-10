using FairSplit.Api.Services.Interfaces;
using FairSplit.Api.Presentation.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FairSplit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MembersController(IMemberService memberService) : ControllerBase
{
    [HttpGet("/api/groups/{groupId:guid}/members")]
    [ProducesResponseType(typeof(IReadOnlyCollection<MemberResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<MemberResponse>>> GetByGroupId(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var members = await memberService.GetByGroupIdAsync(groupId, cancellationToken);

        var response = members
            .Select(member => new MemberResponse
            {
                Id = member.Id,
                GroupId = member.GroupId,
                DisplayName = member.DisplayName
            })
            .ToList();

        return Ok(response);
    }
}
