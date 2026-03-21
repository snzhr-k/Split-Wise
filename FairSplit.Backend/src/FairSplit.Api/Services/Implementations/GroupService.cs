using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Interfaces;

namespace FairSplit.Api.Services.Implementations;

public sealed class GroupService(IGroupRepository groupRepository) : IGroupService
{
    public Task<IReadOnlyCollection<Domain.Entities.Group>> GetAllAsync(CancellationToken cancellationToken)
    {
        return groupRepository.GetAllAsync(cancellationToken);
    }
}
