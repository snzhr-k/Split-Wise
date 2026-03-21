namespace FairSplit.Api.Domain.Entities;

public sealed class Expense
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid PayerMemberId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
