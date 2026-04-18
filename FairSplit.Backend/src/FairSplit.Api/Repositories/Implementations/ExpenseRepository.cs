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
            .OrderByDescending(expense => expense.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Expense>> GetByGroupIdAsync(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.GroupId == groupId)
            .OrderByDescending(expense => expense.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Expense?> GetByIdAsync(Guid groupId, Guid expenseId, CancellationToken cancellationToken)
    {
        return dbContext.Expenses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                expense => expense.GroupId == groupId && expense.Id == expenseId,
                cancellationToken);
    }

    public Task AddAsync(Expense expense, CancellationToken cancellationToken)
    {
        dbContext.Expenses.Add(expense);
        return Task.CompletedTask;
    }
}
