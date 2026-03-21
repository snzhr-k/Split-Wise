namespace FairSplit.Api.Services.Models;

public sealed class CreateExpenseCommand
{
    public Guid GroupId { get; init; }
    public Guid PayerMemberId { get; init; }
    public decimal Amount { get; init; }
    public ExpenseSplitType SplitType { get; init; }
    public IReadOnlyCollection<CreateExpenseParticipantCommand> Participants { get; init; } = [];
}

public sealed class CreateExpenseParticipantCommand
{
    public Guid MemberId { get; init; }
    public decimal? ShareAmount { get; init; }
}
