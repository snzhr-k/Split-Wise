using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Errors;
using FairSplit.Api.Services.Interfaces;

namespace FairSplit.Api.Services.Implementations;

public sealed class GroupService(
    IGroupRepository groupRepository,
    IMemberRepository memberRepository,
    ITransactionManager transactionManager) : IGroupService
{
    public Task<IReadOnlyCollection<Domain.Entities.Group>> GetAllAsync(CancellationToken cancellationToken)
    {
        return groupRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Group> GetByIdAsync(Guid groupId, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetByIdAsync(groupId, cancellationToken);

        if (group is null)
        {
            throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
        }

        return group;
    }

    public async Task<Domain.Entities.Group> CreateAsync(string name, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ValidationException("Group name is required.", "GROUP_NAME_REQUIRED");
        }

        Domain.Entities.Group? createdGroup = null;

        await transactionManager.ExecuteInTransactionAsync(async innerCancellationToken =>
        {
            var groupNameAlreadyExists = await groupRepository.ExistsByNameAsync(normalizedName, innerCancellationToken);

            if (groupNameAlreadyExists)
            {
                throw new ConflictException("A group with this name already exists.", "GROUP_NAME_EXISTS");
            }

            createdGroup = new Domain.Entities.Group
            {
                Id = Guid.NewGuid(),
                Name = normalizedName
            };

            await groupRepository.AddAsync(createdGroup, innerCancellationToken);
        }, cancellationToken);

        return createdGroup ?? throw new InternalServerException("Group creation did not complete. This is an internal error.");
    }

    public async Task<Domain.Entities.Member> JoinAsync(Guid groupId, string displayName, CancellationToken cancellationToken)
    {
        var normalizedDisplayName = displayName.Trim();

        if (string.IsNullOrWhiteSpace(normalizedDisplayName))
        {
            throw new ValidationException("Display name is required.", "DISPLAY_NAME_REQUIRED");
        }

        Domain.Entities.Member? createdMember = null;

        await transactionManager.ExecuteInTransactionAsync(async innerCancellationToken =>
        {
            var groupExists = await groupRepository.ExistsAsync(groupId, innerCancellationToken);

            if (!groupExists)
            {
                throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
            }

            var displayNameExists = await memberRepository.ExistsByDisplayNameInGroupAsync(
                groupId,
                normalizedDisplayName,
                innerCancellationToken);

            if (displayNameExists)
            {
                throw new ConflictException("Display name is already taken in this group.", "DISPLAY_NAME_EXISTS");
            }

            createdMember = new Domain.Entities.Member
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                DisplayName = normalizedDisplayName
            };

            await memberRepository.AddAsync(createdMember, innerCancellationToken);
        }, cancellationToken);

        return createdMember ?? throw new InternalServerException("Group join did not complete. This is an internal error.");
    }
}
