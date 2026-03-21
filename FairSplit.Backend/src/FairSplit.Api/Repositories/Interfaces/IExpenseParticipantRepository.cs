using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Repositories.Interfaces;

public interface IExpenseParticipantRepository
{
    Task<IReadOnlyCollection<ExpenseParticipant>> GetAllAsync(CancellationToken cancellationToken);
    Task AddRangeAsync(
        IReadOnlyCollection<ExpenseParticipant> expenseParticipants,
        CancellationToken cancellationToken);
}
