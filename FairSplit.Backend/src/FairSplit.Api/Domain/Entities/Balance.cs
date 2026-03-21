namespace FairSplit.Api.Domain.Entities;

public sealed class Balance
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid MemberId { get; set; }
    public decimal NetAmount { get; set; }
}
