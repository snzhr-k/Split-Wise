namespace FairSplit.Api.Services.Errors;

public class BusinessRuleViolationException(string message) : Exception(message)
{
}
