using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Repositories.Interfaces;

public interface IExpenseParticipantRepository
{
    Task<IReadOnlyCollection<ExpenseParticipant>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ExpenseParticipant>> GetByExpenseIdAsync(
        Guid expenseId,
        CancellationToken cancellationToken);
    Task AddRangeAsync(
        IReadOnlyCollection<ExpenseParticipant> expenseParticipants,
        CancellationToken cancellationToken);
}
