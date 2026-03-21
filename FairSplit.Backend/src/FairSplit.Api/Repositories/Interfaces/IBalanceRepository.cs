using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Repositories.Interfaces;

public interface IBalanceRepository
{
    Task<IReadOnlyCollection<Balance>> GetAllAsync(CancellationToken cancellationToken);
    Task ApplyDeltasAsync(
        Guid groupId,
        IReadOnlyDictionary<Guid, decimal> deltasByMemberId,
        CancellationToken cancellationToken);
}
