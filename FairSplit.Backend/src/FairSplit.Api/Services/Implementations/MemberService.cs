using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Interfaces;

namespace FairSplit.Api.Services.Implementations;

public sealed class MemberService(IMemberRepository memberRepository) : IMemberService
{
    public Task HandlePlaceholderAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
