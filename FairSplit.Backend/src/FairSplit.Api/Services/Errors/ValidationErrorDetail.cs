namespace FairSplit.Api.Services.Errors;

/// <summary>
/// Represents a single field-level validation error or detail.
/// Used in the error response payload for client-side form validation and UX.
/// </summary>
public sealed record ValidationErrorDetail(
    /// <summary>Property name or JSON path of the field (e.g., "amount", "participants[0].shareAmount").</summary>
    string Field,
    
    /// <summary>Human-readable issue description (e.g., "must be greater than 0").</summary>
    string Issue,
    
    /// <summary>The rejected value (optional, only if safe to expose).</summary>
    object? Value = null
);
