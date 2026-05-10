using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Infrastructure.Persistence;
using FairSplit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FairSplit.Api.Repositories.Implementations;

public sealed class SettlementRepository(FairSplitDbContext dbContext) : ISettlementRepository
{
    public async Task<IReadOnlyCollection<Settlement>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Settlements
            .AsNoTracking()
            .OrderByDescending(settlement => settlement.SettledAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Settlement>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken)
    {
        return await dbContext.Settlements
            .AsNoTracking()
            .Where(settlement => settlement.GroupId == groupId)
            .OrderByDescending(settlement => settlement.SettledAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Settlement?> GetByIdAsync(Guid groupId, Guid settlementId, CancellationToken cancellationToken)
    {
        return dbContext.Settlements
            .AsNoTracking()
            .FirstOrDefaultAsync(
                settlement => settlement.GroupId == groupId && settlement.Id == settlementId,
                cancellationToken);
    }

    public async Task AddAsync(Settlement settlement, CancellationToken cancellationToken)
    {
        await dbContext.Settlements.AddAsync(settlement, cancellationToken);
    }
}
