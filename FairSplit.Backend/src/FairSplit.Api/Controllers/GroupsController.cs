using System.Security.Claims;
using FairSplit.Api.Presentation.Models.Requests;
using FairSplit.Api.Presentation.Models.Responses;
using FairSplit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FairSplit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class GroupsController(IGroupService groupService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<GroupResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<GroupResponse>>> Get(CancellationToken cancellationToken)
    {
        var groups = await groupService.GetAllAsync(cancellationToken);

        var response = groups
            .Select(group => new GroupResponse
            {
                Id = group.Id,
                Name = group.Name
            })
            .ToList();

        return Ok(response);
    }

    [HttpGet("{groupId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupResponse>> GetById(Guid groupId, CancellationToken cancellationToken)
    {
        var group = await groupService.GetByIdAsync(groupId, cancellationToken);

        return Ok(new GroupResponse
        {
            Id = group.Id,
            Name = group.Name
        });
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GroupResponse>> Create(
        [FromBody] GroupRequest request,
        CancellationToken cancellationToken)
    {
        var group = await groupService.CreateAsync(request.Name, cancellationToken);

        var response = new GroupResponse
        {
            Id = group.Id,
            Name = group.Name
        };

        return Created($"/api/groups/{response.Id}", response);
    }

    [HttpPost("{groupId:guid}/join")]
    [Authorize]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MemberResponse>> Join(
        Guid groupId,
        [FromBody] JoinGroupRequest request,
        CancellationToken cancellationToken)
    {
        var displayName = request.DisplayName?.Trim();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = User.FindFirstValue(ClaimTypes.Name)?.Trim();
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return BadRequest("displayName is required.");
        }

        var member = await groupService.JoinAsync(groupId, displayName, cancellationToken);

        return Ok(new MemberResponse
        {
            Id = member.Id,
            GroupId = member.GroupId,
            DisplayName = member.DisplayName
        });
    }
}
