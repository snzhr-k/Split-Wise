using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Services.Models;

public sealed class ExpenseDetailsModel
{
    public required Expense Expense { get; init; }
    public required IReadOnlyCollection<ExpenseParticipant> Participants { get; init; }
}
