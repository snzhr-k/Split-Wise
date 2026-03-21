using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Interfaces;

namespace FairSplit.Api.Services.Implementations;

public sealed class BalanceService(IBalanceRepository balanceRepository) : IBalanceService
{
    public Task HandlePlaceholderAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
