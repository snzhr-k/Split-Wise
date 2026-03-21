namespace FairSplit.Api.Services.Interfaces;

public interface IBalanceService
{
    Task HandlePlaceholderAsync(CancellationToken cancellationToken);
}
