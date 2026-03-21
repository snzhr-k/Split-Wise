using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Infrastructure.Persistence;
using FairSplit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FairSplit.Api.Repositories.Implementations;

public sealed class MemberRepository(FairSplitDbContext dbContext) : IMemberRepository
{
    public async Task<IReadOnlyCollection<Member>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Members
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Member>> GetByIdsInGroupAsync(
        Guid groupId,
        IReadOnlyCollection<Guid> memberIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.Members
            .AsNoTracking()
            .Where(member => member.GroupId == groupId && memberIds.Contains(member.Id))
            .ToListAsync(cancellationToken);
    }
}
