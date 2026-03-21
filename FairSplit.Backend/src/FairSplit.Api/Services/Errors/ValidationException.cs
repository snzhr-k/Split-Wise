namespace FairSplit.Api.Services.Errors;

public class ValidationException(string message) : BusinessRuleViolationException(message)
{
}
