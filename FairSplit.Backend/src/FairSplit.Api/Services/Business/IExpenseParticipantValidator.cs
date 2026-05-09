using FairSplit.Api.Services.Models;

namespace FairSplit.Api.Services.Business;

public interface IExpenseParticipantValidator
{
    void Validate(CreateExpenseCommand command);
}
