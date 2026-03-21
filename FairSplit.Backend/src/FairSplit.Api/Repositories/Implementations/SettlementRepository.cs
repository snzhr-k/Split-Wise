using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Infrastructure.Persistence;
using FairSplit.Api.Repositories.Interfaces;

namespace FairSplit.Api.Repositories.Implementations;

public sealed class SettlementRepository(FairSplitDbContext dbContext) : ISettlementRepository
{
    public Task<IReadOnlyCollection<Settlement>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Data access for settlements is not implemented yet.");
    }
}
