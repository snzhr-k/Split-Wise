using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Repositories.Interfaces;

public interface IExpenseRepository
{
    Task<IReadOnlyCollection<Expense>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Expense>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken);
    Task<Expense?> GetByIdAsync(Guid groupId, Guid expenseId, CancellationToken cancellationToken);
    Task AddAsync(Expense expense, CancellationToken cancellationToken);
}
