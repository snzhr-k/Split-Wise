using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Services.Interfaces;

public interface IGroupService
{
    Task<IReadOnlyCollection<Group>> GetAllAsync(CancellationToken cancellationToken);
    Task<Group> GetByIdAsync(Guid groupId, CancellationToken cancellationToken);
    Task<Group> CreateAsync(string name, CancellationToken cancellationToken);
    Task<Member> JoinAsync(Guid groupId, string displayName, CancellationToken cancellationToken);
}
