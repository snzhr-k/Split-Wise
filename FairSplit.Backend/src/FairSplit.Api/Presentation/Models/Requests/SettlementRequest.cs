namespace FairSplit.Api.Presentation.Models.Requests;

public sealed class SettlementRequest
{
    public Guid GroupId { get; set; }
    public Guid FromMemberId { get; set; }
    public Guid ToMemberId { get; set; }
    public decimal Amount { get; set; }
}
