namespace FairSplit.Api.Domain.Entities;

public sealed class Settlement
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid FromMemberId { get; set; }
    public Guid ToMemberId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset SettledAtUtc { get; set; }
}
