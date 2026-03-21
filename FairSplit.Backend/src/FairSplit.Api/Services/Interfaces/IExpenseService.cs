using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Services.Models;

namespace FairSplit.Api.Services.Interfaces;

public interface IExpenseService
{
    Task<Expense> CreateAsync(CreateExpenseCommand command, CancellationToken cancellationToken);
}
