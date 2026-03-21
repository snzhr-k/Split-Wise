namespace FairSplit.Api.Presentation.Models.Responses;

public sealed class ExpenseResponse
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid PayerMemberId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
