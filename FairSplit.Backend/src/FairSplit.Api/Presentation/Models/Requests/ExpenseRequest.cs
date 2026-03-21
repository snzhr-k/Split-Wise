namespace FairSplit.Api.Presentation.Models.Requests;

public sealed class ExpenseRequest
{
    public Guid PayerMemberId { get; set; }
    public decimal Amount { get; set; }
    public string SplitType { get; set; } = "equal";
    public IReadOnlyCollection<ExpenseParticipantShareRequest> Participants { get; set; } = [];
}

public sealed class ExpenseParticipantShareRequest
{
    public Guid MemberId { get; set; }
    public decimal? ShareAmount { get; set; }
}
