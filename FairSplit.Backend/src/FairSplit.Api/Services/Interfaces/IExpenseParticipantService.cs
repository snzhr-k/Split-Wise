namespace FairSplit.Api.Services.Interfaces;

public interface IExpenseParticipantService
{
    Task HandlePlaceholderAsync(CancellationToken cancellationToken);
}
