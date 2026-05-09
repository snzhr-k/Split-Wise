namespace FairSplit.Api.Services.Errors;

/// <summary>
/// Thrown for unexpected, unhandled server errors that do not fit other categories.
/// Used as a fallback when middleware encounters exceptions not derived from AppException.
/// 
/// HTTP Status: 500 Internal Server Error
/// Error Code: INTERNAL_ERROR
/// </summary>
public sealed class InternalServerException : AppException
{
    public InternalServerException(string message)
        : base(message)
    {
        ErrorCode = "INTERNAL_ERROR";
        StatusCode = 500;
    }

    public InternalServerException(string message, Exception? innerException)
        : base(message, innerException)
    {
        ErrorCode = "INTERNAL_ERROR";
        StatusCode = 500;
    }
}
