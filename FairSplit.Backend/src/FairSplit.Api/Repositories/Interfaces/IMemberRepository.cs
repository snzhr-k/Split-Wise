using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Repositories.Interfaces;

public interface IMemberRepository
{
    Task<IReadOnlyCollection<Member>> GetAllAsync(CancellationToken cancellationToken);
    Task<Member?> GetByIdAsync(Guid memberId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Member>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Member>> GetByIdsInGroupAsync(
        Guid groupId,
        IReadOnlyCollection<Guid> memberIds,
        CancellationToken cancellationToken);
    Task<bool> ExistsByDisplayNameInGroupAsync(Guid groupId, string displayName, CancellationToken cancellationToken);
    Task AddAsync(Member member, CancellationToken cancellationToken);
}
