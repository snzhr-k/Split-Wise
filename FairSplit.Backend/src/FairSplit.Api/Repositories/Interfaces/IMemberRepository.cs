using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Repositories.Interfaces;

public interface IMemberRepository
{
    Task<IReadOnlyCollection<Member>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Member>> GetByIdsInGroupAsync(
        Guid groupId,
        IReadOnlyCollection<Guid> memberIds,
        CancellationToken cancellationToken);
}
