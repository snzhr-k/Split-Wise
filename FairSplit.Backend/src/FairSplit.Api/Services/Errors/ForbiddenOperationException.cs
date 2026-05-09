namespace FairSplit.Api.Services.Errors;

/// <summary>
/// Thrown when an operation is disallowed in the current context or scope.
/// Examples: member from another group used in expense, cross-group settlement attempt.
/// 
/// HTTP Status: 403 Forbidden
/// Error Code: FORBIDDEN_OPERATION (or specific code like MEMBER_OUTSIDE_GROUP_FORBIDDEN)
/// </summary>
public sealed class ForbiddenOperationException : AppException
{
    public ForbiddenOperationException(string message)
        : base(message)
    {
        ErrorCode = "FORBIDDEN_OPERATION";
        StatusCode = 403;
    }

    public ForbiddenOperationException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = 403;
    }
}

