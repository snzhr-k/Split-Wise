using System;
using System.ComponentModel.DataAnnotations;

namespace FairSplit.Api.Presentation.Validation;

/// <summary>
/// Validates that a GUID property is not Guid.Empty.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public NotEmptyGuidAttribute()
    {
        ErrorMessage = "The {0} field must be a non-empty GUID.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return false;

        if (value is Guid g) return g != Guid.Empty;

        if (Guid.TryParse(value.ToString(), out var parsed)) return parsed != Guid.Empty;

        return false;
    }
}
