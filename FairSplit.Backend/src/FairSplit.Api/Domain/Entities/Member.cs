namespace FairSplit.Api.Domain.Entities;

public sealed class Member
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
