namespace FairSplit.Api.Presentation.Models.Responses;

public sealed class BalanceResponse
{
    public Guid GroupId { get; set; }
    public Guid MemberId { get; set; }
    public decimal NetAmount { get; set; }
}
