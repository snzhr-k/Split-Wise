namespace FairSplit.Api.Services.Errors;

public sealed class NotFoundException(string message) : Exception(message)
{
}
