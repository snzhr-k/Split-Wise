using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Infrastructure.Persistence;
using FairSplit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FairSplit.Api.Repositories.Implementations;

public sealed class ExpenseRepository(FairSplitDbContext dbContext) : IExpenseRepository
{
    public async Task<IReadOnlyCollection<Expense>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Expenses
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Expense expense, CancellationToken cancellationToken)
    {
        dbContext.Expenses.Add(expense);
        return Task.CompletedTask;
    }
}
