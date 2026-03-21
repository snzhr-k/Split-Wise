using FairSplit.Api.Presentation.Models.Responses;
using FairSplit.Api.Services.Interfaces;
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
}
