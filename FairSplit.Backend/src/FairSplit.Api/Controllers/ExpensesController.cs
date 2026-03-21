using FairSplit.Api.Presentation.Models.Requests;
using FairSplit.Api.Presentation.Models.Responses;
using FairSplit.Api.Services.Errors;
using FairSplit.Api.Services.Interfaces;
using FairSplit.Api.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace FairSplit.Api.Controllers;

[ApiController]
[Route("api/groups/{groupId:guid}/expenses")]
public sealed class ExpensesController(IExpenseService expenseService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseResponse>> Create(
        Guid groupId,
        [FromBody] ExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateExpenseCommand
        {
            GroupId = groupId,
            PayerMemberId = request.PayerMemberId,
            Amount = request.Amount,
            SplitType = ParseSplitType(request.SplitType),
            Participants = request.Participants
                .Select(participant => new CreateExpenseParticipantCommand
                {
                    MemberId = participant.MemberId,
                    ShareAmount = participant.ShareAmount
                })
                .ToList()
        };

        var createdExpense = await expenseService.CreateAsync(command, cancellationToken);

        var response = new ExpenseResponse
        {
            Id = createdExpense.Id,
            GroupId = createdExpense.GroupId,
            PayerMemberId = createdExpense.PayerMemberId,
            Amount = createdExpense.Amount,
            CreatedAtUtc = createdExpense.CreatedAtUtc
        };

        return Created($"/api/groups/{groupId}/expenses/{response.Id}", response);
    }

    private static ExpenseSplitType ParseSplitType(string splitType)
    {
        if (string.Equals(splitType, "equal", StringComparison.OrdinalIgnoreCase))
        {
            return ExpenseSplitType.Equal;
        }

        if (string.Equals(splitType, "custom", StringComparison.OrdinalIgnoreCase))
        {
            return ExpenseSplitType.Custom;
        }

        throw new InvalidSplitException("splitType must be 'equal' or 'custom'");
    }
}
