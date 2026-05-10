using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Errors;
using FairSplit.Api.Services.Interfaces;
using FairSplit.Api.Shared.Utilities;

namespace FairSplit.Api.Services.Implementations;

public sealed class SettlementService(
    ISettlementRepository settlementRepository,
    IGroupRepository groupRepository,
    IMemberRepository memberRepository,
    IBalanceRepository balanceRepository,
    ITransactionManager transactionManager,
    IClock clock) : ISettlementService
{
    public async Task<IReadOnlyCollection<Domain.Entities.Settlement>> GetByGroupIdAsync(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var groupExists = await groupRepository.ExistsAsync(groupId, cancellationToken);

        if (!groupExists)
        {
            throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
        }

        return await settlementRepository.GetByGroupIdAsync(groupId, cancellationToken);
    }

    public async Task<Domain.Entities.Settlement> GetByIdAsync(
        Guid groupId,
        Guid settlementId,
        CancellationToken cancellationToken)
    {
        var groupExists = await groupRepository.ExistsAsync(groupId, cancellationToken);

        if (!groupExists)
        {
            throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
        }

        var settlement = await settlementRepository.GetByIdAsync(groupId, settlementId, cancellationToken);

        if (settlement is null)
        {
            throw new NotFoundException("Settlement was not found in this group.", "SETTLEMENT_NOT_FOUND");
        }

        return settlement;
    }

    public async Task<Domain.Entities.Settlement> CreateAsync(
        Guid groupId,
        Guid fromMemberId,
        Guid toMemberId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            throw new ValidationException("Settlement amount must be greater than zero.", "SETTLEMENT_AMOUNT_INVALID");
        }

        if (fromMemberId == toMemberId)
        {
            throw new ValidationException("fromMemberId and toMemberId must be different.", "SETTLEMENT_MEMBERS_MUST_DIFFER");
        }

        Domain.Entities.Settlement? createdSettlement = null;

        await transactionManager.ExecuteInTransactionAsync(async innerCancellationToken =>
        {
            var groupExists = await groupRepository.ExistsAsync(groupId, innerCancellationToken);

            if (!groupExists)
            {
                throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
            }

            var requiredMemberIds = new[] { fromMemberId, toMemberId };

            var members = await memberRepository.GetByIdsInGroupAsync(
                groupId,
                requiredMemberIds,
                innerCancellationToken);

            if (members.Count != requiredMemberIds.Length)
            {
                throw new ForbiddenOperationException(
                    "Settlement members must belong to the requested group.",
                    "MEMBER_NOT_IN_GROUP");
            }

            createdSettlement = new Domain.Entities.Settlement
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                FromMemberId = fromMemberId,
                ToMemberId = toMemberId,
                Amount = amount,
                SettledAtUtc = clock.UtcNow
            };

            await settlementRepository.AddAsync(createdSettlement, innerCancellationToken);

            var settlementBalanceDeltas = new Dictionary<Guid, decimal>
            {
                [fromMemberId] = amount,
                [toMemberId] = -amount
            };

            await balanceRepository.ApplyDeltasAsync(groupId, settlementBalanceDeltas, innerCancellationToken);
        }, cancellationToken);

        return createdSettlement ?? throw new InternalServerException("Settlement creation did not complete. This is an internal error.");
    }
}
