using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Repositories.Interfaces;
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
    IClock clock) : IExpenseService
{
    public async Task<Expense> CreateAsync(CreateExpenseCommand command, CancellationToken cancellationToken)
    {
        if (command.Amount <= 0)
        {
            throw new InvalidSplitException("total amount must be greater than zero");
        }

        if (command.Participants.Count == 0)
        {
            throw new InvalidSplitException("at least one participant is required");
        }

        var duplicateParticipants = command.Participants
            .GroupBy(participant => participant.MemberId)
            .Any(group => group.Count() > 1);

        if (duplicateParticipants)
        {
            throw new InvalidSplitException("participants cannot contain duplicates");
        }

        Expense? createdExpense = null;

        await transactionManager.ExecuteInTransactionAsync(async innerCancellationToken =>
        {
            var groupExists = await groupRepository.ExistsAsync(command.GroupId, innerCancellationToken);

            if (!groupExists)
            {
                throw new NotFoundException($"Group '{command.GroupId}' was not found.");
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
                        $"Member '{memberId}' does not belong to group '{command.GroupId}'.");
                }
            }

            var sharesByMemberId = CalculateShares(command);

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

            var balanceDeltas = sharesByMemberId.ToDictionary(item => item.Key, item => -item.Value);
            balanceDeltas[command.PayerMemberId] = balanceDeltas.GetValueOrDefault(command.PayerMemberId) + command.Amount;

            await balanceRepository.ApplyDeltasAsync(command.GroupId, balanceDeltas, innerCancellationToken);
        }, cancellationToken);

        return createdExpense ?? throw new InvalidOperationException("Expense creation did not complete.");
    }

    private static IReadOnlyDictionary<Guid, decimal> CalculateShares(CreateExpenseCommand command)
    {
        return command.SplitType switch
        {
            ExpenseSplitType.Equal => CalculateEqualShares(command),
            ExpenseSplitType.Custom => CalculateCustomShares(command),
            _ => throw new InvalidSplitException("split type is not supported")
        };
    }

    private static IReadOnlyDictionary<Guid, decimal> CalculateEqualShares(CreateExpenseCommand command)
    {
        var participantCount = command.Participants.Count;
        var baseShare = decimal.Round(command.Amount / participantCount, 2, MidpointRounding.AwayFromZero);
        var shares = command.Participants
            .Select(participant => new KeyValuePair<Guid, decimal>(participant.MemberId, baseShare))
            .ToList();

        var distributed = shares.Sum(item => item.Value);
        var adjustment = command.Amount - distributed;

        if (adjustment != 0)
        {
            var lastIndex = shares.Count - 1;
            shares[lastIndex] = new KeyValuePair<Guid, decimal>(
                shares[lastIndex].Key,
                shares[lastIndex].Value + adjustment);
        }

        return shares.ToDictionary(item => item.Key, item => item.Value);
    }

    private static IReadOnlyDictionary<Guid, decimal> CalculateCustomShares(CreateExpenseCommand command)
    {
        var shares = new Dictionary<Guid, decimal>();

        foreach (var participant in command.Participants)
        {
            if (participant.ShareAmount is null || participant.ShareAmount < 0)
            {
                throw new InvalidSplitException("custom split requires non-negative amounts for all participants");
            }

            shares[participant.MemberId] = participant.ShareAmount.Value;
        }

        var totalCustomAmount = shares.Values.Sum();

        if (totalCustomAmount != command.Amount)
        {
            throw new InvalidSplitException("custom split amounts must sum to total amount");
        }

        return shares;
    }
}
