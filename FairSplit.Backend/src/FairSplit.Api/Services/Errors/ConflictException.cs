namespace FairSplit.Api.Services.Errors;

/// <summary>
/// Thrown when a request conflicts with current resource state or violates uniqueness constraints.
/// Examples: duplicate member display name, concurrent update conflict, duplicate key violation.
/// 
/// HTTP Status: 409 Conflict
/// Error Code: CONFLICT (or specific code like DUPLICATE_MEMBER)
/// </summary>
public sealed class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message)
    {
        ErrorCode = "CONFLICT";
        StatusCode = 409;
    }

    public ConflictException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = 409;
    }
}

