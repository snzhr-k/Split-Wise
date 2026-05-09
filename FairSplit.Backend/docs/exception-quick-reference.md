# FairSplit Exception System - Quick Reference Guide

## Folder Structure

```
FairSplit.Backend/src/FairSplit.Api/
├── Services/
│   └── Errors/
│       ├── AppException.cs                        # Base class (abstract)
│       ├── ValidationErrorDetail.cs               # Record for field-level errors
│       ├── ValidationException.cs                 # 400 Bad Request
│       ├── InvalidSplitException.cs              # 400 (specializes ValidationException)
│       ├── NotFoundException.cs                   # 404 Not Found
│       ├── BusinessRuleViolationException.cs     # 422 Unprocessable Entity
│       ├── ConflictException.cs                  # 409 Conflict
│       ├── ForbiddenOperationException.cs        # 403 Forbidden
│       ├── DataAccessException.cs                # 503 Service Unavailable
│       └── InternalServerException.cs            # 500 Internal Server Error
├── Services/
│   └── Implementations/
│       ├── ExpenseService.cs                      # Throws domain exceptions
│       ├── GroupService.cs
│       ├── MemberService.cs
│       ├── BalanceService.cs
│       └── ...
├── Repositories/
│   └── Implementations/
│       └── *Repository.cs                         # Translate DB errors to standard exceptions
├── Controllers/
│   └── *Controller.cs                             # Call services, only handle DTO validation
└── Infrastructure/
    └── Http/
        └── ExceptionHandlingMiddleware.cs         # Catches all exceptions, returns JSON
```

## Exception Quick Reference Table

| Exception | HTTP | Code | When to Use | Layer |
|-----------|------|------|-------------|-------|
| `ValidationException` | 400 | `VALIDATION_ERROR` | Invalid request format/values | Controller, Service |
| `InvalidSplitException` | 400 | `INVALID_SPLIT` | Invalid expense split config | Service |
| `NotFoundException` | 404 | `RESOURCE_NOT_FOUND` | Resource doesn't exist | Service |
| `BusinessRuleViolationException` | 422 | `BUSINESS_RULE_VIOLATION` | Violates domain rule | Service |
| `ConflictException` | 409 | `CONFLICT` | Conflicts with state/constraints | Service, Repository |
| `ForbiddenOperationException` | 403 | `FORBIDDEN_OPERATION` | Operation disallowed in scope | Service |
| `DataAccessException` | 503 | `DATA_ACCESS_ERROR` | DB/persistence failure | Repository |
| `InternalServerException` | 500 | `INTERNAL_ERROR` | Unexpected unhandled error | Middleware (fallback) |

## Usage Patterns

### Pattern 1: Basic Throw (Most Common)

```csharp
// Service validation
if (command.Amount <= 0)
{
    throw new InvalidSplitException("Amount must be greater than zero.", "INVALID_AMOUNT");
}

// Resource not found
if (!groupExists)
{
    throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
}

// Domain rule violation
if (!isValidSplit)
{
    throw new BusinessRuleViolationException(
        "Custom split amounts must sum to total.",
        "INVALID_SPLIT_SUM"
    );
}
```

### Pattern 2: With Field-Level Details (Validation Forms)

```csharp
var errors = new List<ValidationErrorDetail>();

if (request.Amount <= 0)
    errors.Add(new("amount", "must be greater than 0", request.Amount));

if (request.Participants.Count == 0)
    errors.Add(new("participants", "must have at least one participant"));

if (errors.Any())
{
    throw new ValidationException(
        "Request validation failed.",
        "VALIDATION_ERROR",
        errors
    );
}
```

### Pattern 3: Repository Error Translation

```csharp
try
{
    return await _context.Expenses.FindAsync(expenseId);
}
catch (OperationCanceledException ex)
{
    throw new DataAccessException(
        "Database query timed out.",
        "QUERY_TIMEOUT",
        ex
    );
}
catch (DbUpdateException ex)
{
    throw new DataAccessException(
        "Database update failed.",
        "TRANSACTION_FAILED",
        ex
    );
}
```

### Pattern 4: Scope/Authorization Check

```csharp
// Member must belong to group
if (!memberIdsInGroup.Contains(memberId))
{
    throw new ForbiddenOperationException(
        "Member does not belong to this group.",
        "MEMBER_OUTSIDE_GROUP_FORBIDDEN"
    );
}

// Cannot modify resource across groups
if (expense.GroupId != command.GroupId)
{
    throw new ForbiddenOperationException(
        "Expense does not belong to this group.",
        "CROSS_GROUP_OPERATION_FORBIDDEN"
    );
}
```

### Pattern 5: Uniqueness Constraint Violation

```csharp
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") ?? false)
{
    throw new ConflictException(
        "A member with this display name already exists in the group.",
        "DUPLICATE_MEMBER"
    );
}
```

## Error Response Examples

### Validation Error (400)
```json
{
  "code": "INVALID_AMOUNT",
  "message": "Amount must be greater than zero.",
  "details": [],
  "traceId": "00-..."
}
```

### Not Found (404)
```json
{
  "code": "GROUP_NOT_FOUND",
  "message": "Group was not found.",
  "details": [],
  "traceId": "00-..."
}
```

### Business Rule Violation (422)
```json
{
  "code": "INVALID_SPLIT_SUM",
  "message": "Custom split amounts must sum to total amount.",
  "details": [],
  "traceId": "00-..."
}
```

### Validation with Details (400)
```json
{
  "code": "VALIDATION_ERROR",
  "message": "Request validation failed.",
  "details": [
    {
      "field": "amount",
      "issue": "must be greater than 0",
      "value": -10
    },
    {
      "field": "participants",
      "issue": "must have at least one participant",
      "value": null
    }
  ],
  "traceId": "00-..."
}
```

## Error Code Catalog

### Validation (400)
- `VALIDATION_ERROR` - Generic validation error
- `INVALID_AMOUNT` - Amount is invalid (≤ 0)
- `INVALID_SPLIT` - Expense split configuration error
- `INVALID_SPLIT_TYPE` - splitType not in allowed enum
- `DUPLICATE_PARTICIPANTS` - Participants list contains duplicates
- `EMPTY_PARTICIPANTS` - No participants provided
- `INVALID_CUSTOM_SHARE` - Individual share amount invalid
- `INVALID_REQUEST_BODY` - Request body parsing failed
- `MISSING_REQUIRED_FIELD` - Required field is missing

### Not Found (404)
- `RESOURCE_NOT_FOUND` - Generic not found
- `GROUP_NOT_FOUND` - Group ID doesn't exist
- `MEMBER_NOT_FOUND` - Member ID not found
- `EXPENSE_NOT_FOUND` - Expense ID not found in group
- `SETTLEMENT_NOT_FOUND` - Settlement not found
- `BALANCE_NOT_FOUND` - Balance record not found

### Business Rule (422)
- `BUSINESS_RULE_VIOLATION` - Generic business rule violation
- `INVALID_SPLIT_SUM` - Custom split amounts don't sum to total
- `PAYER_NOT_IN_GROUP` - Payer is not a member of group
- `PARTICIPANT_NOT_IN_GROUP` - Participant not in group
- `SETTLEMENT_EXCEEDS_DEBT` - Settlement amount > outstanding balance

### Forbidden (403)
- `FORBIDDEN_OPERATION` - Generic forbidden operation
- `MEMBER_OUTSIDE_GROUP_FORBIDDEN` - Member referenced from outside group
- `CROSS_GROUP_OPERATION_FORBIDDEN` - Cross-group operation attempted

### Conflict (409)
- `CONFLICT` - Generic conflict
- `DUPLICATE_MEMBER` - Member display name already exists
- `RESOURCE_VERSION_CONFLICT` - Concurrent update conflict
- `IDEMPOTENCY_CONFLICT` - Idempotency key violation

### Data Access (503/500)
- `DATA_ACCESS_ERROR` - Generic data access error
- `DATABASE_UNAVAILABLE` - Database unreachable
- `TRANSACTION_FAILED` - Transaction commit failed
- `QUERY_TIMEOUT` - Query exceeded timeout

### Internal (500)
- `INTERNAL_ERROR` - Unexpected unhandled error

## How Middleware Works

```csharp
// 1. Exception is thrown in Service/Repository
throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");

// 2. Middleware catches it
catch (Exception ex) {
    await HandleExceptionAsync(context, ex);
}

// 3. Middleware inspects the exception
var appEx = exception as AppException;
if (appEx != null) {
    statusCode = appEx.StatusCode;      // 404
    code = appEx.ErrorCode;              // "GROUP_NOT_FOUND"
    message = appEx.Message;             // "Group was not found."
    details = appEx.Details;             // null or empty
}

// 4. Middleware returns standardized JSON
{
  "code": "GROUP_NOT_FOUND",
  "message": "Group was not found.",
  "details": [],
  "traceId": "00-..."
}
```

## Best Practices

### ✅ DO

- **Throw specific exceptions** in Services based on domain rules
- **Translate DB errors** to standard exceptions in Repositories
- **Include error code and message** with every throw
- **Use `Details` for validation** errors with multiple fields
- **Log the traceId** in error responses for debugging
- **Keep messages user-friendly** and actionable
- **Inherit from AppException** for all custom domain exceptions

### ❌ DON'T

- Throw generic `Exception` class
- Put business logic validation in Controllers
- Catch and swallow exceptions silently
- Return HTTP 500 for validation/business errors
- Expose stack traces in error messages to client
- Use different error codes for the same semantic error
- Create new exception types for minor variations (use error codes instead)

## Integration Checklist for Existing Code

- [ ] Audit all existing `throw new Exception(...)` statements
- [ ] Replace with specific AppException subclasses
- [ ] Add error codes to all throws
- [ ] For validation errors with multiple fields, use `Details`
- [ ] Test that each error returns correct HTTP status code
- [ ] Verify error codes match the taxonomy document
- [ ] Add integration tests for each error path
- [ ] Update client mobile app error handling based on codes

## Testing Error Responses

### Unit Test Example
```csharp
[Fact]
public async Task CreateExpense_WithInvalidAmount_ThrowsValidationException()
{
    var command = new CreateExpenseCommand { Amount = -10 };
    
    var exception = await Assert.ThrowsAsync<InvalidSplitException>(
        () => _service.CreateAsync(command, CancellationToken.None)
    );
    
    Assert.Equal("INVALID_AMOUNT", exception.ErrorCode);
    Assert.Equal(400, exception.StatusCode);
}
```

### Integration Test Example
```csharp
[Fact]
public async Task POST_CreateExpense_WithInvalidAmount_Returns400WithErrorCode()
{
    var response = await _client.PostAsJsonAsync(
        "/api/groups/11111111-1111-1111-1111-111111111111/expenses",
        new { amount = -10, ... }
    );
    
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    
    var content = await response.Content.ReadAsAsync<dynamic>();
    Assert.Equal("INVALID_AMOUNT", (string)content.code);
    Assert.Equal("VALIDATION_ERROR", (string)content.code);
}
```

## Troubleshooting

### "Build error: cannot derive from sealed type"
**Solution**: Remove `sealed` keyword from base exception class to allow specialization.

### "Error response doesn't include traceId"
**Solution**: Middleware automatically adds `context.TraceIdentifier`. Ensure middleware is registered in `Program.cs`:
```csharp
app.UseGlobalExceptionHandling();  // Must be early in pipeline
```

### "Status codes are always 500"
**Solution**: Ensure exception sets `StatusCode` property:
```csharp
ErrorCode = "INVALID_AMOUNT";
StatusCode = 400;  // Must be set in constructor
```

### "Details array is null in response"
**Solution**: Middleware always returns non-null details array:
```csharp
Details: details ?? Array.Empty<ValidationErrorDetail>()
```

## Related Documentation

- See `error-taxonomy.md` for complete error code catalog and design
- See `exception-architecture.md` for detailed architectural decisions and examples
- See backendREADME.md for layer dependency rules
