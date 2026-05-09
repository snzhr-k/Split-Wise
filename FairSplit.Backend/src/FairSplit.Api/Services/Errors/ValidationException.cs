namespace FairSplit.Api.Services.Errors;

/// <summary>
/// Thrown when a request contains syntactically invalid or incomplete data.
/// Examples: missing required fields, invalid data types, empty collections.
/// 
/// HTTP Status: 400 Bad Request
/// Error Code: VALIDATION_ERROR (or specific code like INVALID_AMOUNT)
/// </summary>
public class ValidationException : AppException
{
    public ValidationException(string message)
        : base(message)
    {
        ErrorCode = "VALIDATION_ERROR";
        StatusCode = 400;
    }

    public ValidationException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = 400;
    }

    public ValidationException(string message, IReadOnlyCollection<ValidationErrorDetail>? details = null)
        : base(message, details)
    {
        ErrorCode = "VALIDATION_ERROR";
        StatusCode = 400;
    }

    public ValidationException(string message, string errorCode, IReadOnlyCollection<ValidationErrorDetail>? details = null)
        : base(message, details)
    {
        ErrorCode = errorCode;
        StatusCode = 400;
    }
}
