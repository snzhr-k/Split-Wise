using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Services.Interfaces;

public interface IGroupService
{
    Task<IReadOnlyCollection<Group>> GetAllAsync(CancellationToken cancellationToken);
}
