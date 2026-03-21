namespace FairSplit.Api.Presentation.Models.Requests;

public sealed class BalanceRequest
{
    public Guid GroupId { get; set; }
    public Guid MemberId { get; set; }
}
