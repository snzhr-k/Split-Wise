using Microsoft.AspNetCore.Mvc.Filters;
using FairSplit.Api.Services.Errors;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;

namespace FairSplit.Api.Presentation.Filters;

/// <summary>
/// Converts invalid ModelState into a ValidationException with field-level details.
/// Registered globally to ensure consistent validation handling.
/// </summary>
public sealed class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;

        var errors = new List<ValidationErrorDetail>();

        foreach (var kvp in context.ModelState)
        {
            var field = kvp.Key;
            var state = kvp.Value;
            foreach (var error in state.Errors)
            {
                var msg = string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Invalid value." : error.ErrorMessage;
                errors.Add(new ValidationErrorDetail(field, msg, state.AttemptedValue));
            }
        }

        throw new ValidationException("Validation failed.", errors);
    }
}
