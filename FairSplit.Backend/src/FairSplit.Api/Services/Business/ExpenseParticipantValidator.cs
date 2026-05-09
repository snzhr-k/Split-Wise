using FairSplit.Api.Services.Errors;
using FairSplit.Api.Services.Models;

namespace FairSplit.Api.Services.Business;

public sealed class ExpenseParticipantValidator : IExpenseParticipantValidator
{
    public void Validate(CreateExpenseCommand command)
    {
        var validationErrors = new List<ValidationErrorDetail>();

        if (command.Amount <= 0)
        {
            validationErrors.Add(new(
                Field: "amount",
                Issue: "must be greater than 0",
                Value: command.Amount
            ));
        }

        if (command.Participants.Count == 0)
        {
            validationErrors.Add(new(
                Field: "participants",
                Issue: "must have at least one participant",
                Value: null
            ));
        }

        var duplicateParticipants = command.Participants
            .GroupBy(participant => participant.MemberId)
            .Any(group => group.Count() > 1);

        if (duplicateParticipants)
        {
            validationErrors.Add(new(
                Field: "participants",
                Issue: "cannot contain duplicate members",
                Value: null
            ));
        }

        if (validationErrors.Any())
        {
            throw new InvalidSplitException(
                "Expense split configuration is invalid.",
                "INVALID_SPLIT_CONFIGURATION",
                validationErrors
            );
        }
    }
}
