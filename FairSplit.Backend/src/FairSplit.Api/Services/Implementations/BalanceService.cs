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
            throw new NotFoundException($"Group '{groupId}' was not found.");
        }

        return await balanceRepository.GetByGroupIdAsync(groupId, cancellationToken);
    }
}
