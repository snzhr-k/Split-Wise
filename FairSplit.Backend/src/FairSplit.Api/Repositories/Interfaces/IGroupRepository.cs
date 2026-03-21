using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Repositories.Interfaces;

public interface IGroupRepository
{
    Task<IReadOnlyCollection<Group>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid groupId, CancellationToken cancellationToken);
}
