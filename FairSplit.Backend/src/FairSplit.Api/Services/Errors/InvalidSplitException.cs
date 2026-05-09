namespace FairSplit.Api.Services.Errors;

/// <summary>
/// Thrown when an expense split calculation or configuration is invalid.
/// Specializes ValidationException for split-specific errors.
/// Examples: invalid split type, duplicate participants, split sum mismatch, invalid amounts.
/// 
/// HTTP Status: 400 Bad Request
/// Error Code: Specific split codes like INVALID_SPLIT_TYPE, DUPLICATE_PARTICIPANTS
/// </summary>
public sealed class InvalidSplitException : ValidationException
{
    public InvalidSplitException(string message)
        : base(message, "INVALID_SPLIT")
    {
    }

    public InvalidSplitException(string message, string errorCode)
        : base(message, errorCode)
    {
    }

    public InvalidSplitException(string message, string errorCode, IReadOnlyCollection<ValidationErrorDetail>? details = null)
        : base(message, errorCode, details)
    {
    }
}
