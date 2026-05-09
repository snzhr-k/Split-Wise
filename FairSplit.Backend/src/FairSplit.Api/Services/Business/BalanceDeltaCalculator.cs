namespace FairSplit.Api.Services.Business;

public sealed class BalanceDeltaCalculator : IBalanceDeltaCalculator
{
    public IReadOnlyDictionary<Guid, decimal> CalculateDeltas(
        Guid payerMemberId,
        decimal amount,
        IReadOnlyDictionary<Guid, decimal> sharesByMemberId)
    {
        var deltas = sharesByMemberId.ToDictionary(item => item.Key, item => -item.Value);
        deltas[payerMemberId] = deltas.GetValueOrDefault(payerMemberId) + amount;
        return deltas;
    }
}
