using FairSplit.Api.Services.Errors;
using FairSplit.Api.Services.Models;

namespace FairSplit.Api.Services.Business;

public sealed class ExpenseSplitCalculator : IExpenseSplitCalculator
{
    public IReadOnlyDictionary<Guid, decimal> CalculateShares(CreateExpenseCommand command)
    {
        return command.SplitType switch
        {
            ExpenseSplitType.Equal => CalculateEqualShares(command),
            ExpenseSplitType.Custom => CalculateCustomShares(command),
            _ => throw new InvalidSplitException(
                "Split type is not supported.",
                "INVALID_SPLIT_TYPE",
                new[]
                {
                    new ValidationErrorDetail(
                        Field: "splitType",
                        Issue: "must be 'equal' or 'custom'",
                        Value: command.SplitType
                    )
                }
            )
        };
    }

    private static IReadOnlyDictionary<Guid, decimal> CalculateEqualShares(CreateExpenseCommand command)
    {
        var participantCount = command.Participants.Count;
        var baseShare = decimal.Round(command.Amount / participantCount, 2, MidpointRounding.AwayFromZero);
        var shares = command.Participants
            .Select(participant => new KeyValuePair<Guid, decimal>(participant.MemberId, baseShare))
            .ToList();

        var distributed = shares.Sum(item => item.Value);
        var adjustment = command.Amount - distributed;

        if (adjustment != 0)
        {
            var lastIndex = shares.Count - 1;
            shares[lastIndex] = new KeyValuePair<Guid, decimal>(
                shares[lastIndex].Key,
                shares[lastIndex].Value + adjustment);
        }

        return shares.ToDictionary(item => item.Key, item => item.Value);
    }

    private static IReadOnlyDictionary<Guid, decimal> CalculateCustomShares(CreateExpenseCommand command)
    {
        var shares = new Dictionary<Guid, decimal>();
        var validationErrors = new List<ValidationErrorDetail>();
        var participantsList = command.Participants.ToList();

        for (int i = 0; i < participantsList.Count; i++)
        {
            var participant = participantsList[i];

            if (participant.ShareAmount is null || participant.ShareAmount < 0)
            {
                validationErrors.Add(new(
                    Field: $"participants[{i}].shareAmount",
                    Issue: "must be a non-negative number",
                    Value: participant.ShareAmount
                ));
                continue;
            }

            shares[participant.MemberId] = participant.ShareAmount.Value;
        }

        if (validationErrors.Any())
        {
            throw new InvalidSplitException(
                "Custom split amounts are invalid.",
                "INVALID_CUSTOM_SHARE",
                validationErrors
            );
        }

        var totalCustomAmount = shares.Values.Sum();

        if (totalCustomAmount != command.Amount)
        {
            throw new InvalidSplitException(
                "Custom split amounts must sum to total expense amount.",
                "INVALID_SPLIT_SUM",
                new[]
                {
                    new ValidationErrorDetail(
                        Field: "participants",
                        Issue: $"total of share amounts ({totalCustomAmount}) must equal total amount ({command.Amount})",
                        Value: new { sum = totalCustomAmount, expected = command.Amount }
                    )
                }
            );
        }

        return shares;
    }
}
