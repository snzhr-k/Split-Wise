using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Services.Interfaces;

public interface IBalanceService
{
    Task<IReadOnlyCollection<Balance>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken);
    Task<Balance> GetByGroupAndMemberIdAsync(Guid groupId, Guid memberId, CancellationToken cancellationToken);
}
