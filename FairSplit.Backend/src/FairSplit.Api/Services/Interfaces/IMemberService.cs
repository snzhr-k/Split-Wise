namespace FairSplit.Api.Services.Interfaces;

public interface IMemberService
{
    Task<IReadOnlyCollection<FairSplit.Api.Domain.Entities.Member>> GetByGroupIdAsync(
        Guid groupId,
        CancellationToken cancellationToken);
}
