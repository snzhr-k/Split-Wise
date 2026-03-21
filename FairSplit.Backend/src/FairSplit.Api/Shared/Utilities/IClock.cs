namespace FairSplit.Api.Shared.Utilities;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
