using FairSplit.Api.Infrastructure.Persistence;
using FairSplit.Api.Repositories.Interfaces;

namespace FairSplit.IntegrationTests.Infrastructure;

public sealed class TestTransactionManager(FairSplitDbContext dbContext) : ITransactionManager
{
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        await operation(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
