using FairSplit.Api.Services.Errors;
using System.Text.Json;

namespace FairSplit.Api.Infrastructure.Http;

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
            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        var (statusCode, code, message) = exception switch
        {
            NotFoundException ex => (StatusCodes.Status404NotFound, "not_found", ex.Message),
            ValidationException ex => (StatusCodes.Status400BadRequest, "validation_error", ex.Message),
            ConflictException ex => (StatusCodes.Status409Conflict, "conflict", ex.Message),
            ForbiddenOperationException ex => (StatusCodes.Status403Forbidden, "forbidden", ex.Message),
            BusinessRuleViolationException ex => (StatusCodes.Status422UnprocessableEntity, "business_rule_violation", ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "unexpected_error", "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse(
            Code: code,
            Message: message,
            TraceId: context.TraceIdentifier);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private sealed record ErrorResponse(string Code, string Message, string TraceId);
}
