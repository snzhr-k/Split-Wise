using FairSplit.Api.Services.Errors;
using System.Text.Json;

namespace FairSplit.Api.Infrastructure.Http;

/// <summary>
/// Global exception handling middleware.
/// 
/// Catches all exceptions in the pipeline:
/// - AppException-derived: maps to standardized error response with error code and status
/// - Unexpected exceptions: wrapped as INTERNAL_ERROR, status 500
/// 
/// All errors include a traceId for observability and support correlation.
/// </summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, code, message, details) = exception switch
        {
            // Handle AppException-derived exceptions
            AppException appEx => (
                appEx.StatusCode,
                appEx.ErrorCode,
                appEx.Message,
                appEx.Details
            ),

            // Fallback for unhandled exceptions
            _ => (
                500,
                "INTERNAL_ERROR",
                "An unexpected error occurred.",
                null as IReadOnlyCollection<ValidationErrorDetail>
            )
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var errorResponse = new ErrorResponse(
            Code: code,
            Message: message,
            Details: details ?? Array.Empty<ValidationErrorDetail>(),
            TraceId: context.TraceIdentifier
        );

        await context.Response.WriteAsJsonAsync(errorResponse);
    }

    private sealed record ErrorResponse(
        string Code,
        string Message,
        IReadOnlyCollection<ValidationErrorDetail> Details,
        string TraceId
    );
}

