using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Infrastructure.Persistence;
using FairSplit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FairSplit.Api.Repositories.Implementations;

public sealed class BalanceRepository(FairSplitDbContext dbContext) : IBalanceRepository
{
    public async Task<IReadOnlyCollection<Balance>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Balances
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task ApplyDeltasAsync(
        Guid groupId,
        IReadOnlyDictionary<Guid, decimal> deltasByMemberId,
        CancellationToken cancellationToken)
    {
        if (deltasByMemberId.Count == 0)
        {
            return;
        }

        var memberIds = deltasByMemberId.Keys.ToList();

        var existingBalances = await dbContext.Balances
            .Where(balance => balance.GroupId == groupId && memberIds.Contains(balance.MemberId))
            .ToListAsync(cancellationToken);

        foreach (var memberId in memberIds)
        {
            var delta = deltasByMemberId[memberId];
            var balance = existingBalances.FirstOrDefault(item => item.MemberId == memberId);

            if (balance is null)
            {
                dbContext.Balances.Add(new Balance
                {
                    Id = Guid.NewGuid(),
                    GroupId = groupId,
                    MemberId = memberId,
                    NetAmount = delta
                });

                continue;
            }

            balance.NetAmount += delta;
        }
    }
}
