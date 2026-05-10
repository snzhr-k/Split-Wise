using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Services.Interfaces;

public interface ISettlementService
{
    Task<IReadOnlyCollection<Settlement>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken);
    Task<Settlement> GetByIdAsync(Guid groupId, Guid settlementId, CancellationToken cancellationToken);
    Task<Settlement> CreateAsync(
        Guid groupId,
        Guid fromMemberId,
        Guid toMemberId,
        decimal amount,
        CancellationToken cancellationToken);
}
