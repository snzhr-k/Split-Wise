namespace FairSplit.Api.Services.Interfaces;

public interface IMemberService
{
    Task HandlePlaceholderAsync(CancellationToken cancellationToken);
}
