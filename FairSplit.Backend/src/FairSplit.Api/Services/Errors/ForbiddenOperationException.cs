namespace FairSplit.Api.Services.Errors;

public sealed class ForbiddenOperationException(string message) : Exception(message)
{
}
