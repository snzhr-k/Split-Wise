# FairSplit Custom Exception Architecture

## Overview

This document describes the custom exception system for FairSplit backend. It provides a clean, layered approach to error handling that ensures consistent, predictable error responses across all API endpoints.

## Design Goals

1. **Machine-readable error codes**: Every error response includes a stable `code` for client logic.
2. **Layered responsibility**: Each layer (Controller, Service, Repository) throws appropriate exceptions.
3. **Consistent response shape**: All errors follow the standard JSON schema from the error taxonomy.
4. **Type safety**: Specific exception classes prevent silent failures and unclear error semantics.
5. **DRY**: Centralized middleware handles all exception-to-response translation.
6. **Observable**: Every error includes `traceId` for correlation and debugging.

## Exception Hierarchy

```
Exception
└── AppException (base class - abstract)
    ├── ValidationException (400 Bad Request)
    │   └── InvalidSplitException (split-specific validation)
    ├── NotFoundException (404 Not Found)
    ├── BusinessRuleViolationException (422 Unprocessable Entity)
    ├── ConflictException (409 Conflict)
    ├── ForbiddenOperationException (403 Forbidden)
    ├── DataAccessException (503 Service Unavailable)
    └── InternalServerException (500 Internal Server Error)
```

All exceptions inherit from `AppException`, which provides:
- `ErrorCode` (string): Machine-readable code (e.g., "GROUP_NOT_FOUND")
- `StatusCode` (int): HTTP status code
- `Details` (IReadOnlyCollection<ValidationErrorDetail>?): Field-level validation errors
- `Message` (string): Human-readable summary

## Core Exception Classes

### AppException (Base Class)

The abstract base for all application exceptions. Defines the contract and integrates with middleware.

```csharp
public abstract class AppException : Exception
{
    public string ErrorCode { get; protected set; } = "INTERNAL_ERROR";
    public int StatusCode { get; protected set; } = 500;
    public IReadOnlyCollection<ValidationErrorDetail>? Details { get; protected set; }
    
    protected AppException(string message) : base(message) { }
    protected AppException(string message, Exception? innerException) : base(message, innerException) { }
    protected AppException(string message, IReadOnlyCollection<ValidationErrorDetail>? details) : base(message) { }
}
```

### ValidationException

Thrown for syntactically invalid or incomplete requests. Always 400 Bad Request.

```csharp
// Usage examples:
throw new ValidationException("Amount must be greater than zero.", "INVALID_AMOUNT");

throw new ValidationException(
    "Invalid split configuration.",
    "DUPLICATE_PARTICIPANTS",
    new[] { new ValidationErrorDetail("participants[0]", "duplicate member in list") }
);
```

### InvalidSplitException

Specializes `ValidationException` for expense split-specific validation errors.

```csharp
// Usage examples:
throw new InvalidSplitException("at least one participant is required");
throw new InvalidSplitException("participants cannot contain duplicates", "DUPLICATE_PARTICIPANTS");
throw new InvalidSplitException("custom split amounts must sum to total amount", "INVALID_SPLIT_SUM");
```

### NotFoundException

Thrown when a resource is not found in requested scope. Always 404 Not Found.

```csharp
// Usage examples:
throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
throw new NotFoundException("Expense not found in group.", "EXPENSE_NOT_FOUND");
throw new NotFoundException("Member not found in group.", "MEMBER_NOT_FOUND");
```

### BusinessRuleViolationException

Thrown when request violates domain business rules. Always 422 Unprocessable Entity.

```csharp
// Usage examples:
throw new BusinessRuleViolationException(
    "Custom split amounts must sum to total amount.",
    "INVALID_SPLIT_SUM"
);

throw new BusinessRuleViolationException(
    "Payer not in group members.",
    "PAYER_NOT_IN_GROUP"
);
```

### ConflictException

Thrown when request conflicts with resource state or violates uniqueness. Always 409 Conflict.

```csharp
// Usage examples:
throw new ConflictException(
    "A member with this display name already exists in the group.",
    "DUPLICATE_MEMBER"
);
```

### ForbiddenOperationException

Thrown when operation is disallowed in current context. Always 403 Forbidden.

```csharp
// Usage examples:
throw new ForbiddenOperationException(
    "Member does not belong to this group.",
    "MEMBER_OUTSIDE_GROUP_FORBIDDEN"
);
```

### DataAccessException

Thrown when database or persistence layer encounters errors. Status 503 or 500.

```csharp
// Usage examples in Repository layer:
try {
    return await context.Groups.FindAsync(groupId);
}
catch (OperationCanceledException ex) {
    throw new DataAccessException(
        "Database query timed out.",
        "QUERY_TIMEOUT",
        ex
    );
}
catch (DbUpdateException ex) {
    throw new DataAccessException(
        "Failed to update database.",
        "TRANSACTION_FAILED",
        ex
    );
}
```

### InternalServerException

Fallback for unhandled/unexpected errors. Always 500 Internal Server Error.

```csharp
// Middleware automatically wraps unexpected exceptions
// But you can explicitly throw if you detect an impossible state:
throw new InternalServerException(
    "Expense creation completed but result was null. This should never happen."
);
```

## ValidationErrorDetail

Record for field-level validation errors in the `details` array.

```csharp
public sealed record ValidationErrorDetail(
    string Field,           // "amount" or "participants[0].shareAmount"
    string Issue,           // "must be greater than 0"
    object? Value = null    // The rejected value (e.g., -10)
);
```

## Standard Error Response Shape

All failed responses use this JSON schema:

```json
{
  "code": "INVALID_SPLIT",
  "message": "Custom split amounts must sum to total amount.",
  "details": [
    {
      "field": "participants[0].shareAmount",
      "issue": "sum does not equal total",
      "value": 50.00
    }
  ],
  "traceId": "00-3fa2103f3cc9d01a5e77a39346f6e12a-a10e1f6e9e0ce7fe-00"
}
```

## Layer Responsibility Matrix

### Controller Layer

**Responsibility**: Input shape/DTO validation at API boundary. Do NOT encode business logic.

```csharp
[HttpPost("{groupId:guid}/expenses")]
public async Task<IActionResult> CreateExpense(
    Guid groupId,
    CreateExpenseRequest request,
    CancellationToken cancellationToken)
{
    // DTO ModelState validation is automatic via ASP.NET Core model binding
    // If invalid, framework returns 400 Bad Request automatically
    
    // Controllers should NOT throw exceptions for validation—let the framework handle it
    // Throw only if you need custom validation logic
    
    var expense = await _expenseService.CreateAsync(
        new CreateExpenseCommand
        {
            GroupId = groupId,
            PayerMemberId = request.PayerMemberId,
            Amount = request.Amount,
            SplitType = Enum.Parse<ExpenseSplitType>(request.SplitType),
            Participants = request.Participants.Select(p => new ParticipantInput
            {
                MemberId = p.MemberId,
                ShareAmount = p.ShareAmount
            }).ToList()
        },
        cancellationToken
    );

    return CreatedAtAction(nameof(GetExpense), new { groupId, expenseId = expense.Id }, expense);
    // Service may throw ValidationException, NotFoundException, BusinessRuleViolationException, etc.
    // Middleware catches and converts to standardized response
}
```

### Service Layer

**Responsibility**: Business logic, domain invariants, and semantic validation. Throw domain-specific exceptions.

```csharp
public async Task<Expense> CreateAsync(CreateExpenseCommand command, CancellationToken cancellationToken)
{
    // Business rule: amount must be positive
    if (command.Amount <= 0)
    {
        throw new InvalidSplitException("total amount must be greater than zero", "INVALID_AMOUNT");
    }

    // Business rule: must have participants
    if (command.Participants.Count == 0)
    {
        throw new InvalidSplitException("at least one participant is required", "EMPTY_PARTICIPANTS");
    }

    // Business rule: no duplicate participants
    var duplicateParticipants = command.Participants
        .GroupBy(p => p.MemberId)
        .Any(g => g.Count() > 1);
    if (duplicateParticipants)
    {
        throw new InvalidSplitException("participants cannot contain duplicates", "DUPLICATE_PARTICIPANTS");
    }

    // Domain check: group exists
    var groupExists = await _groupRepository.ExistsAsync(command.GroupId, cancellationToken);
    if (!groupExists)
    {
        throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
    }

    // Domain check: all members belong to group
    var relatedMemberIds = command.Participants
        .Select(p => p.MemberId)
        .Append(command.PayerMemberId)
        .Distinct()
        .ToList();

    var members = await _memberRepository.GetByIdsInGroupAsync(
        command.GroupId,
        relatedMemberIds,
        cancellationToken
    );

    var memberIdsInGroup = members.Select(m => m.Id).ToHashSet();
    foreach (var memberId in relatedMemberIds)
    {
        if (!memberIdsInGroup.Contains(memberId))
        {
            throw new ForbiddenOperationException(
                $"Member does not belong to group.",
                "MEMBER_OUTSIDE_GROUP_FORBIDDEN"
            );
        }
    }

    // Business rule: split amounts must be valid
    var sharesByMemberId = CalculateShares(command);
    // (CalculateShares throws InvalidSplitException if totals don't match)

    // Persist within transaction
    Expense? createdExpense = null;
    await _transactionManager.ExecuteInTransactionAsync(async innerCt =>
    {
        // Create and persist expense...
        createdExpense = new Expense { ... };
        await _expenseRepository.AddAsync(createdExpense, innerCt);
        // ...
    }, cancellationToken);

    return createdExpense ?? throw new InternalServerException("Expense creation did not complete.");
}

private static IReadOnlyDictionary<Guid, decimal> CalculateShares(CreateExpenseCommand command)
{
    return command.SplitType switch
    {
        ExpenseSplitType.Equal => CalculateEqualShares(command),
        ExpenseSplitType.Custom => CalculateCustomShares(command),
        _ => throw new InvalidSplitException("split type is not supported", "INVALID_SPLIT_TYPE")
    };
}

private static IReadOnlyDictionary<Guid, decimal> CalculateCustomShares(CreateExpenseCommand command)
{
    var shares = new Dictionary<Guid, decimal>();

    foreach (var participant in command.Participants)
    {
        if (participant.ShareAmount is null || participant.ShareAmount < 0)
        {
            throw new InvalidSplitException(
                "custom split requires non-negative amounts for all participants",
                "INVALID_CUSTOM_SHARE"
            );
        }

        shares[participant.MemberId] = participant.ShareAmount.Value;
    }

    var totalCustomAmount = shares.Values.Sum();
    if (totalCustomAmount != command.Amount)
    {
        throw new InvalidSplitException(
            "custom split amounts must sum to total amount",
            "INVALID_SPLIT_SUM"
        );
    }

    return shares;
}
```

### Repository Layer

**Responsibility**: Query execution, data persistence, provider-specific error translation.

```csharp
public async Task<Expense?> GetByIdAsync(Guid groupId, Guid expenseId, CancellationToken cancellationToken)
{
    try
    {
        return await _context.Expenses
            .Where(e => e.GroupId == groupId && e.Id == expenseId)
            .FirstOrDefaultAsync(cancellationToken);
    }
    catch (OperationCanceledException ex)
    {
        throw new DataAccessException(
            "Database query was cancelled or timed out.",
            "QUERY_TIMEOUT",
            ex
        );
    }
    catch (DbUpdateException ex)
    {
        throw new DataAccessException(
            "Database operation failed.",
            "DATABASE_ERROR",
            ex
        );
    }
    catch (InvalidOperationException ex)
    {
        throw new DataAccessException(
            "Unexpected database error.",
            "DATABASE_ERROR",
            ex
        );
    }
}

public async Task AddAsync(Expense expense, CancellationToken cancellationToken)
{
    try
    {
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
    {
        // Handle constraint violation
        throw new ConflictException(
            "Expense with this configuration already exists.",
            "DUPLICATE_EXPENSE",
            ex
        );
    }
    catch (DbUpdateException ex)
    {
        throw new DataAccessException(
            "Failed to persist expense to database.",
            "TRANSACTION_FAILED",
            ex
        );
    }
}
```

### Middleware Layer

**Responsibility**: Catch all exceptions and translate to standardized JSON responses.

The enhanced middleware automatically handles:

```csharp
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
            // Any AppException-derived exception
            AppException appEx => (
                appEx.StatusCode,
                appEx.ErrorCode,
                appEx.Message,
                appEx.Details
            ),

            // Unexpected exceptions wrapped automatically
            _ => (
                500,
                "INTERNAL_ERROR",
                "An unexpected error occurred.",
                null as IReadOnlyCollection<ValidationErrorDetail>?
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
}
```

## Before and After Examples

### Example 1: Invalid Expense Amount

#### BEFORE

```csharp
// Service (unclear error semantics)
public async Task<Expense> CreateAsync(CreateExpenseCommand command)
{
    if (command.Amount <= 0)
    {
        throw new Exception("Amount must be positive");  // Generic exception!
    }
    // ...
}

// Middleware (catches generic exception)
catch (Exception ex) {
    // Cannot distinguish root cause
    // Returns 500 Internal Server Error (wrong status)
    context.Response.StatusCode = 500;
}

// Client receives:
{
  "error": "An error occurred.",
  "traceId": "..."
}
// Client cannot differentiate validation error from server crash!
```

#### AFTER

```csharp
// Service (specific exception, clear intent)
public async Task<Expense> CreateAsync(CreateExpenseCommand command, CancellationToken cancellationToken)
{
    if (command.Amount <= 0)
    {
        throw new InvalidSplitException("total amount must be greater than zero", "INVALID_AMOUNT");
    }
    // ...
}

// Middleware (catches and translates)
catch (AppException appEx) => (appEx.StatusCode, appEx.ErrorCode, appEx.Message, appEx.Details)

// Client receives:
{
  "code": "INVALID_AMOUNT",
  "message": "total amount must be greater than zero",
  "details": [],
  "traceId": "00-..."
}
// Client can now handle validation errors differently from server errors!
```

### Example 2: Group Not Found

#### BEFORE

```csharp
// Service
var groupExists = await _groupRepository.ExistsAsync(groupId, cancellationToken);
if (!groupExists)
{
    throw new Exception($"Group '{groupId}' was not found.");  // Generic!
}

// Middleware (no status code info)
catch (Exception ex) {
    context.Response.StatusCode = 500;  // Wrong! Should be 404
}

// Client receives status 500 and cannot retry with backoff
```

#### AFTER

```csharp
// Service
var groupExists = await _groupRepository.ExistsAsync(groupId, cancellationToken);
if (!groupExists)
{
    throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
}

// Middleware (knows status code)
catch (AppException appEx) {
    context.Response.StatusCode = appEx.StatusCode;  // 404 from exception
}

// Client receives:
{
  "code": "GROUP_NOT_FOUND",
  "message": "Group was not found.",
  "details": [],
  "traceId": "00-..."
}
// Client knows to show "Item not found" toast and no retry
```

### Example 3: Member Outside Group

#### BEFORE

```csharp
// Service (domain rule, but implementation unclear)
if (!memberIdsInGroup.Contains(memberId))
{
    throw new Exception("Member does not belong to this group.");
}

// Middleware
catch (Exception ex) {
    context.Response.StatusCode = 500;  // Generic fallback
}

// Client response:
{
  "error": "Member does not belong to this group.",
  "traceId": "..."
}
// Client cannot parse code or determine proper handling
```

#### AFTER

```csharp
// Service (specific exception with semantic code)
if (!memberIdsInGroup.Contains(memberId))
{
    throw new ForbiddenOperationException(
        "Member does not belong to this group.",
        "MEMBER_OUTSIDE_GROUP_FORBIDDEN"
    );
}

// Middleware
catch (AppException appEx) {
    context.Response.StatusCode = appEx.StatusCode;  // 403 Forbidden
}

// Client response:
{
  "code": "MEMBER_OUTSIDE_GROUP_FORBIDDEN",
  "message": "Member does not belong to this group.",
  "details": [],
  "traceId": "00-..."
}
// Client can show blocking message and disable action
```

### Example 4: Custom Split Sum Mismatch

#### BEFORE

```csharp
// Service
var totalCustomAmount = shares.Values.Sum();
if (totalCustomAmount != command.Amount)
{
    throw new Exception("Split amounts do not sum to total.");  // No field info!
}

// Middleware (no details support)
// Client cannot show which field is wrong

// Client response:
{
  "error": "Split amounts do not sum to total.",
  "traceId": "..."
}
// User doesn't know which participant amount to fix
```

#### AFTER

```csharp
// Service (with field-level details)
var totalCustomAmount = shares.Values.Sum();
if (totalCustomAmount != command.Amount)
{
    throw new InvalidSplitException(
        "custom split amounts must sum to total amount",
        "INVALID_SPLIT_SUM",
        new[]
        {
            new ValidationErrorDetail(
                Field: "participants",
                Issue: "split amounts must sum to total",
                Value: new { total = totalCustomAmount, expected = command.Amount }
            )
        }
    );
}

// Middleware (supports details)
// Client response:
{
  "code": "INVALID_SPLIT_SUM",
  "message": "custom split amounts must sum to total amount",
  "details": [
    {
      "field": "participants",
      "issue": "split amounts must sum to total",
      "value": {
        "total": 100.0,
        "expected": 120.50
      }
    }
  ],
  "traceId": "00-..."
}
// Client can show exactly what needs to be adjusted
```

## Recommended Patterns

### Pattern 1: Domain Validation in Services

Always validate domain rules in the Service layer, not the Controller.

```csharp
public async Task<Expense> CreateAsync(CreateExpenseCommand command, CancellationToken cancellationToken)
{
    // Service validates business rules
    if (command.Amount <= 0)
        throw new InvalidSplitException(...);

    // Service checks domain state
    if (!groupExists)
        throw new NotFoundException(...);

    // Service enforces invariants
    if (!memberIdsContainPayer)
        throw new ForbiddenOperationException(...);

    // Service persists
    // Controller just returns 201 Created
}
```

### Pattern 2: Repository Error Translation

Catch provider-specific exceptions in Repository and translate to standard exceptions.

```csharp
public async Task AddAsync(Expense expense, CancellationToken cancellationToken)
{
    try
    {
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException ex) when (IsConstraintViolation(ex))
    {
        throw new ConflictException("Constraint violation message.", "DUPLICATE_KEY", ex);
    }
    catch (DbUpdateException ex)
    {
        throw new DataAccessException("Database update failed.", "TRANSACTION_FAILED", ex);
    }
    catch (OperationCanceledException ex)
    {
        throw new DataAccessException("Operation timed out.", "QUERY_TIMEOUT", ex);
    }
}
```

### Pattern 3: Validation Details for Form Errors

Include field-level details for multi-field validation.

```csharp
throw new ValidationException(
    "Request validation failed.",
    "VALIDATION_ERROR",
    new[]
    {
        new ValidationErrorDetail("amount", "must be greater than 0", request.Amount),
        new ValidationErrorDetail("splitType", "must be 'equal' or 'custom'", request.SplitType)
    }
);
```

## Integration Checklist

- [x] Base `AppException` class in `Services/Errors/AppException.cs`
- [x] `ValidationErrorDetail` record in `Services/Errors/ValidationErrorDetail.cs`
- [x] Specific exception classes: `ValidationException`, `NotFoundException`, `BusinessRuleViolationException`, `ConflictException`, `ForbiddenOperationException`, `DataAccessException`, `InternalServerException`
- [x] Enhanced global middleware in `Infrastructure/Http/ExceptionHandlingMiddleware.cs`
- [x] All exception classes inherit from `AppException`
- [x] All exceptions set `ErrorCode` and `StatusCode`
- [x] Middleware catches `AppException` and extracts error properties
- [x] Middleware wraps unexpected exceptions as `INTERNAL_ERROR`, 500
- [x] Ensure Controllers never throw business rule exceptions (Services do)
- [x] Ensure Repositories translate provider errors to standard exceptions
- [x] Test all error paths produce correct status codes and error codes
- [x] Document error codes in the taxonomy
- [ ] Update error handling in existing Services (incremental)
- [ ] Add integration tests for each exception type
- [ ] Add unit tests for middleware error mapping

## Next Steps

1. Gradually migrate existing Service methods to use specific exceptions
2. Add integration tests that verify error codes and status codes
3. Update mobile client error handling based on error codes
4. Log traceId with every error for observability
