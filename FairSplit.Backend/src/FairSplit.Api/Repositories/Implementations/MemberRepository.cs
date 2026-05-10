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

    public Task<Member?> GetByIdAsync(Guid memberId, CancellationToken cancellationToken)
    {
        return dbContext.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(member => member.Id == memberId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Member>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken)
    {
        return await dbContext.Members
            .AsNoTracking()
            .Where(member => member.GroupId == groupId)
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

    public Task<bool> ExistsByDisplayNameInGroupAsync(
        Guid groupId,
        string displayName,
        CancellationToken cancellationToken)
    {
        var normalizedDisplayName = displayName.Trim().ToLowerInvariant();

        return dbContext.Members.AnyAsync(
            member => member.GroupId == groupId && member.DisplayName.ToLower() == normalizedDisplayName,
            cancellationToken);
    }

    public async Task AddAsync(Member member, CancellationToken cancellationToken)
    {
        await dbContext.Members.AddAsync(member, cancellationToken);
    }
}
