namespace FairSplit.Api.Services.Errors;

/// <summary>
/// Thrown when a request violates business domain rules.
/// Request is syntactically valid but violates business logic.
/// Examples: custom split amounts don't sum to total, payer not in group, invalid split calculation.
/// 
/// HTTP Status: 422 Unprocessable Entity
/// Error Code: BUSINESS_RULE_VIOLATION (or specific code like INVALID_SPLIT_SUM)
/// </summary>
public sealed class BusinessRuleViolationException : AppException
{
    public BusinessRuleViolationException(string message)
        : base(message)
    {
        ErrorCode = "BUSINESS_RULE_VIOLATION";
        StatusCode = 422;
    }

    public BusinessRuleViolationException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = 422;
    }
}

