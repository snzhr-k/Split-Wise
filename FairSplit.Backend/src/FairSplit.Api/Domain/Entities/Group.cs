namespace FairSplit.Api.Domain.Entities;

public sealed class Group
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
