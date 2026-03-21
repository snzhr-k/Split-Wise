namespace FairSplit.Api.Domain.Entities;

public sealed class ExpenseParticipant
{
    public Guid Id { get; set; }
    public Guid ExpenseId { get; set; }
    public Guid MemberId { get; set; }
    public decimal ShareAmount { get; set; }
}
