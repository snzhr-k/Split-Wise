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
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ExpenseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<ExpenseResponse>>> GetByGroupId(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var expenses = await expenseService.GetByGroupIdAsync(groupId, cancellationToken);

        var response = expenses
            .Select(MapExpenseResponse)
            .ToList();

        return Ok(response);
    }

    [HttpGet("{expenseId:guid}")]
    [ProducesResponseType(typeof(ExpenseDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseDetailsResponse>> GetById(
        Guid groupId,
        Guid expenseId,
        CancellationToken cancellationToken)
    {
        var expenseDetails = await expenseService.GetByIdAsync(groupId, expenseId, cancellationToken);

        var response = new ExpenseDetailsResponse
        {
            Id = expenseDetails.Expense.Id,
            GroupId = expenseDetails.Expense.GroupId,
            PayerMemberId = expenseDetails.Expense.PayerMemberId,
            Amount = expenseDetails.Expense.Amount,
            CreatedAtUtc = expenseDetails.Expense.CreatedAtUtc,
            Participants = expenseDetails.Participants
                .Select(participant => new ExpenseParticipantResponse
                {
                    Id = participant.Id,
                    ExpenseId = participant.ExpenseId,
                    MemberId = participant.MemberId,
                    ShareAmount = participant.ShareAmount
                })
                .ToList()
        };

        return Ok(response);
    }

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

        var response = MapExpenseResponse(createdExpense);

        return Created($"/api/groups/{groupId}/expenses/{response.Id}", response);
    }

    private static ExpenseResponse MapExpenseResponse(Domain.Entities.Expense expense)
    {
        return new ExpenseResponse
        {
            Id = expense.Id,
            GroupId = expense.GroupId,
            PayerMemberId = expense.PayerMemberId,
            Amount = expense.Amount,
            CreatedAtUtc = expense.CreatedAtUtc
        };
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
