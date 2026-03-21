namespace FairSplit.Api.Services.Interfaces;

public interface ISettlementService
{
    Task HandlePlaceholderAsync(CancellationToken cancellationToken);
}
