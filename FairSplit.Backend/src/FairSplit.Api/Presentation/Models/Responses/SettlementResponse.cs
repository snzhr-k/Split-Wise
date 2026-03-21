namespace FairSplit.Api.Presentation.Models.Responses;

public sealed class SettlementResponse
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public decimal Amount { get; set; }
}
