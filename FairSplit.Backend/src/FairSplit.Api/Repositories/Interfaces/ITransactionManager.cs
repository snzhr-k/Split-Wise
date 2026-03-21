namespace FairSplit.Api.Repositories.Interfaces;

public interface ITransactionManager
{
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken);
}
