using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Repositories.Interfaces;

public interface ISettlementRepository
{
    Task<IReadOnlyCollection<Settlement>> GetAllAsync(CancellationToken cancellationToken);
}
