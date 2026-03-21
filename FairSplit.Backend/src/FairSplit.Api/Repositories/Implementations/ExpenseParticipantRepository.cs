using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Infrastructure.Persistence;
using FairSplit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FairSplit.Api.Repositories.Implementations;

public sealed class ExpenseParticipantRepository(FairSplitDbContext dbContext) : IExpenseParticipantRepository
{
    public async Task<IReadOnlyCollection<ExpenseParticipant>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.ExpenseParticipants
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task AddRangeAsync(
        IReadOnlyCollection<ExpenseParticipant> expenseParticipants,
        CancellationToken cancellationToken)
    {
        dbContext.ExpenseParticipants.AddRange(expenseParticipants);
        return Task.CompletedTask;
    }
}
