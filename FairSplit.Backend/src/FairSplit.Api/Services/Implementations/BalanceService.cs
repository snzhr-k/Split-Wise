using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Services.Errors;
using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Interfaces;

namespace FairSplit.Api.Services.Implementations;

public sealed class BalanceService(
    IBalanceRepository balanceRepository,
    IGroupRepository groupRepository) : IBalanceService
{
    public async Task<IReadOnlyCollection<Balance>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken)
    {
        var groupExists = await groupRepository.ExistsAsync(groupId, cancellationToken);

        if (!groupExists)
        {
            throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
        }

        return await balanceRepository.GetByGroupIdAsync(groupId, cancellationToken);
    }

    public async Task<Balance> GetByGroupAndMemberIdAsync(
        Guid groupId,
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var groupExists = await groupRepository.ExistsAsync(groupId, cancellationToken);

        if (!groupExists)
        {
            throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
        }

        var balance = await balanceRepository.GetByGroupAndMemberIdAsync(groupId, memberId, cancellationToken);

        if (balance is null)
        {
            throw new NotFoundException("Balance for this member was not found in the group.", "BALANCE_NOT_FOUND");
        }

        return balance;
    }
}
