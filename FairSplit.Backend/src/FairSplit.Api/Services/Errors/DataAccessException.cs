namespace FairSplit.Api.Services.Errors;

/// <summary>
/// Thrown when a data access, persistence, or database operation fails.
/// Used by the Repository layer when queries timeout, connections fail, or transactions fail.
/// 
/// HTTP Status: 503 Service Unavailable (or 500 on rare cases)
/// Error Code: DATA_ACCESS_ERROR or specific codes like DATABASE_UNAVAILABLE, TRANSACTION_FAILED
/// </summary>
public sealed class DataAccessException : AppException
{
    public DataAccessException(string message)
        : base(message)
    {
        ErrorCode = "DATA_ACCESS_ERROR";
        StatusCode = 503;
    }

    public DataAccessException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = 503;
    }

    public DataAccessException(string message, Exception? innerException)
        : base(message, innerException)
    {
        ErrorCode = "DATA_ACCESS_ERROR";
        StatusCode = 503;
    }

    public DataAccessException(string message, string errorCode, Exception? innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = 503;
    }
}
