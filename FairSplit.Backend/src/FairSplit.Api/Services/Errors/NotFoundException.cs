namespace FairSplit.Api.Services.Errors;

/// <summary>
/// Thrown when a referenced resource does not exist in the requested scope.
/// Examples: group by ID not found, expense not found in group, member not found.
/// 
/// HTTP Status: 404 Not Found
/// Error Code: RESOURCE_NOT_FOUND (or specific code like GROUP_NOT_FOUND)
/// </summary>
public sealed class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(message)
    {
        ErrorCode = "RESOURCE_NOT_FOUND";
        StatusCode = 404;
    }

    public NotFoundException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = 404;
    }
}

