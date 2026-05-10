using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Interfaces;
using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Services.Implementations;

public sealed class MemberService(IMemberRepository memberRepository) : IMemberService
{
    public async Task<IReadOnlyCollection<Member>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken)
    {
        return await memberRepository.GetByGroupIdAsync(groupId, cancellationToken);
    }

    public Task HandlePlaceholderAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
