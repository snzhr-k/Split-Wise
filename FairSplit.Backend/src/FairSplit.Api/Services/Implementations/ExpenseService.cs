using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Business;
using FairSplit.Api.Services.Errors;
using FairSplit.Api.Services.Interfaces;
using FairSplit.Api.Services.Models;
using FairSplit.Api.Shared.Utilities;

namespace FairSplit.Api.Services.Implementations;

public sealed class ExpenseService(
    IExpenseRepository expenseRepository,
    IGroupRepository groupRepository,
    IMemberRepository memberRepository,
    IExpenseParticipantRepository expenseParticipantRepository,
    IBalanceRepository balanceRepository,
    ITransactionManager transactionManager,
    IClock clock,
    IExpenseSplitCalculator splitCalculator,
    IExpenseParticipantValidator participantValidator,
    IBalanceDeltaCalculator balanceDeltaCalculator) : IExpenseService
{
    public async Task<IReadOnlyCollection<Expense>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken)
    {
        var groupExists = await groupRepository.ExistsAsync(groupId, cancellationToken);

        if (!groupExists)
        {
            throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
        }

        return await expenseRepository.GetByGroupIdAsync(groupId, cancellationToken);
    }

    public async Task<ExpenseDetailsModel> GetByIdAsync(Guid groupId, Guid expenseId, CancellationToken cancellationToken)
    {
        var groupExists = await groupRepository.ExistsAsync(groupId, cancellationToken);

        if (!groupExists)
        {
            throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
        }

        var expense = await expenseRepository.GetByIdAsync(groupId, expenseId, cancellationToken);

        if (expense is null)
        {
            throw new NotFoundException("Expense was not found in this group.", "EXPENSE_NOT_FOUND");
        }

        var participants = await expenseParticipantRepository.GetByExpenseIdAsync(expense.Id, cancellationToken);

        return new ExpenseDetailsModel
        {
            Expense = expense,
            Participants = participants
        };
    }

    public async Task<Expense> CreateAsync(CreateExpenseCommand command, CancellationToken cancellationToken)
    {
        participantValidator.Validate(command);

        Expense? createdExpense = null;

        await transactionManager.ExecuteInTransactionAsync(async innerCancellationToken =>
        {
            var groupExists = await groupRepository.ExistsAsync(command.GroupId, innerCancellationToken);

            if (!groupExists)
            {
                throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
            }

            var relatedMemberIds = command.Participants
                .Select(participant => participant.MemberId)
                .Append(command.PayerMemberId)
                .Distinct()
                .ToList();

            var members = await memberRepository.GetByIdsInGroupAsync(
                command.GroupId,
                relatedMemberIds,
                innerCancellationToken);

            var memberIdsInGroup = members.Select(member => member.Id).ToHashSet();

            foreach (var memberId in relatedMemberIds)
            {
                if (!memberIdsInGroup.Contains(memberId))
                {
                    throw new ForbiddenOperationException(
                        "Not all participants belong to this group.",
                        "MEMBER_NOT_IN_GROUP"
                    );
                }
            }

            var sharesByMemberId = splitCalculator.CalculateShares(command);

            createdExpense = new Expense
            {
                Id = Guid.NewGuid(),
                GroupId = command.GroupId,
                PayerMemberId = command.PayerMemberId,
                Amount = command.Amount,
                CreatedAtUtc = clock.UtcNow
            };

            await expenseRepository.AddAsync(createdExpense, innerCancellationToken);

            var expenseParticipants = sharesByMemberId
                .Select(item => new ExpenseParticipant
                {
                    Id = Guid.NewGuid(),
                    ExpenseId = createdExpense.Id,
                    MemberId = item.Key,
                    ShareAmount = item.Value
                })
                .ToList();

            await expenseParticipantRepository.AddRangeAsync(expenseParticipants, innerCancellationToken);

            var balanceDeltas = balanceDeltaCalculator.CalculateDeltas(
                command.PayerMemberId,
                command.Amount,
                sharesByMemberId);

            await balanceRepository.ApplyDeltasAsync(command.GroupId, balanceDeltas, innerCancellationToken);
        }, cancellationToken);

        return createdExpense ?? throw new InternalServerException("Expense creation did not complete. This is an internal error.");
    }
}
