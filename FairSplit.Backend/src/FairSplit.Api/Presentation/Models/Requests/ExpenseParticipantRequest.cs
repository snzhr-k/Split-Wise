namespace FairSplit.Api.Presentation.Models.Requests;

public sealed class ExpenseParticipantRequest
{
    public Guid ExpenseId { get; set; }
    public Guid MemberId { get; set; }
    public decimal ShareAmount { get; set; }
}
