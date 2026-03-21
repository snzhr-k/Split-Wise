using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Infrastructure.Persistence;
using FairSplit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FairSplit.Api.Repositories.Implementations;

public sealed class GroupRepository(FairSplitDbContext dbContext) : IGroupRepository
{
    public async Task<IReadOnlyCollection<Group>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Groups
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid groupId, CancellationToken cancellationToken)
    {
        return dbContext.Groups.AnyAsync(group => group.Id == groupId, cancellationToken);
    }
}
