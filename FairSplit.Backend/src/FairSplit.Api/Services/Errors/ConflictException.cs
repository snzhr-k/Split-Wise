namespace FairSplit.Api.Services.Errors;

public sealed class ConflictException(string message) : Exception(message)
{
}
