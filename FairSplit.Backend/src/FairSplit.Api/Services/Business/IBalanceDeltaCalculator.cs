namespace FairSplit.Api.Services.Business;

public interface IBalanceDeltaCalculator
{
    IReadOnlyDictionary<Guid, decimal> CalculateDeltas(
        Guid payerMemberId,
        decimal amount,
        IReadOnlyDictionary<Guid, decimal> sharesByMemberId);
}
