namespace FairSplit.Api.Services.Errors;

public sealed class InvalidSplitException(string message)
    : ValidationException(message)
{
}
