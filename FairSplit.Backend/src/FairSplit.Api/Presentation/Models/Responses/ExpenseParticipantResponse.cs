namespace FairSplit.Api.Presentation.Models.Responses;

public sealed class ExpenseParticipantResponse
{
    public Guid Id { get; set; }
    public Guid ExpenseId { get; set; }
    public Guid MemberId { get; set; }
    public decimal ShareAmount { get; set; }
}
