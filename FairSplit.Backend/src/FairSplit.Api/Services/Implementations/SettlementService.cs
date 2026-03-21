using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Interfaces;

namespace FairSplit.Api.Services.Implementations;

public sealed class SettlementService(ISettlementRepository settlementRepository) : ISettlementService
{
    public Task HandlePlaceholderAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
