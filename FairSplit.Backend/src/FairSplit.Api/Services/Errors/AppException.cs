namespace FairSplit.Api.Services.Errors;

/// <summary>
/// Base exception class for all application-level errors.
/// 
/// All domain/business logic errors should inherit from this class.
/// The global exception middleware catches AppException instances and translates them
/// to standardized HTTP error responses with machine-readable error codes.
/// </summary>
public abstract class AppException : Exception
{
    /// <summary>
    /// Machine-readable error code (e.g., "GROUP_NOT_FOUND", "INVALID_AMOUNT").
    /// Used by API clients to determine error handling logic.
    /// Should match codes defined in the error taxonomy.
    /// </summary>
    public string ErrorCode { get; protected set; } = "INTERNAL_ERROR";

    /// <summary>
    /// HTTP status code for this error class (e.g., 404, 400, 422).
    /// Used by the middleware to set the response status.
    /// </summary>
    public int StatusCode { get; protected set; } = 500;

    /// <summary>
    /// Optional list of validation/field errors.
    /// For validation errors, contains field-level details.
    /// For other errors, remains empty or null.
    /// </summary>
    public IReadOnlyCollection<ValidationErrorDetail>? Details { get; protected set; }

    /// <summary>
    /// Constructor for exceptions with message only.
    /// </summary>
    protected AppException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Constructor for exceptions with message and inner exception.
    /// </summary>
    protected AppException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Constructor for exceptions with message and validation details.
    /// </summary>
    protected AppException(string message, IReadOnlyCollection<ValidationErrorDetail>? details)
        : base(message)
    {
        Details = details;
    }
}
