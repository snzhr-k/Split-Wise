using FairSplit.Api.Services.Models;

namespace FairSplit.Api.Services.Business;

public interface IExpenseSplitCalculator
{
    IReadOnlyDictionary<Guid, decimal> CalculateShares(CreateExpenseCommand command);
}
