using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Interfaces;
using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Services.Errors;

namespace FairSplit.Api.Services.Implementations;

public sealed class MemberService(
    IMemberRepository memberRepository,
    IGroupRepository groupRepository) : IMemberService
{
    public async Task<IReadOnlyCollection<Member>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken)
    {
        var groupExists = await groupRepository.ExistsAsync(groupId, cancellationToken);

        if (!groupExists)
        {
            throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
        }

        return await memberRepository.GetByGroupIdAsync(groupId, cancellationToken);
    }
}
