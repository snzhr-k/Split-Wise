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

    public Task<Group?> GetByIdAsync(Guid groupId, CancellationToken cancellationToken)
    {
        return dbContext.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(group => group.Id == groupId, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLowerInvariant();

        return dbContext.Groups.AnyAsync(
            group => group.Name.ToLower() == normalizedName,
            cancellationToken);
    }

    public async Task AddAsync(Group group, CancellationToken cancellationToken)
    {
        await dbContext.Groups.AddAsync(group, cancellationToken);
    }
}
