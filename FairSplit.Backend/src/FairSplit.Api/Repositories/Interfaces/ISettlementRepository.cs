using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Repositories.Interfaces;

public interface ISettlementRepository
{
    Task<IReadOnlyCollection<Settlement>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Settlement>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken);
    Task<Settlement?> GetByIdAsync(Guid groupId, Guid settlementId, CancellationToken cancellationToken);
    Task AddAsync(Settlement settlement, CancellationToken cancellationToken);
}
