using FairSplit.Api.Domain.Entities;

namespace FairSplit.Api.Repositories.Interfaces;

public interface IExpenseRepository
{
    Task<IReadOnlyCollection<Expense>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Expense expense, CancellationToken cancellationToken);
}
