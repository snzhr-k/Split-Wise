# FairSplit Exception System - Visual Quick Start

## 30-Second Overview

```
✅ Throw specific exceptions from Services
   └─> attach error code + message
       └─> Middleware catches automatically
           └─> Returns standardized JSON to client
```

## Exception Hierarchy (Visual)

```
                AppException
               /            \
         (400) /              \  (404)
   ValidationException    NotFoundException
      /        \
   (400)      (400)
Validation  InvalidSplit

        (422) BusinessRuleViolationException
        (409) ConflictException
        (403) ForbiddenOperationException
        (503) DataAccessException
        (500) InternalServerException
```

## When to Use Each Exception

```
INPUT VALIDATION? ──────────► ValidationException (400)
                              └─ e.g., amount <= 0
                              └─ e.g., empty participants

RESOURCE NOT EXIST? ────────► NotFoundException (404)
                              └─ e.g., group not found
                              └─ e.g., expense not found

DOMAIN RULE BROKEN? ────────► BusinessRuleViolationException (422)
                              └─ e.g., split sum mismatch
                              └─ e.g., payer not in group

STATE CONFLICTS? ───────────► ConflictException (409)
                              └─ e.g., duplicate member
                              └─ e.g., concurrent update

SCOPE/ACCESS DENIED? ───────► ForbiddenOperationException (403)
                              └─ e.g., cross-group member
                              └─ e.g., unauthorized action

DATABASE ERROR? ────────────► DataAccessException (503)
                              └─ e.g., connection failed
                              └─ e.g., transaction failed

UNEXPECTED BUG? ────────────► InternalServerException (500)
                              └─ or just let middleware catch it
```

## Minimal Complete Example

### Before (Your Current Code)

```csharp
public async Task<Expense> CreateAsync(CreateExpenseCommand command)
{
    if (command.Amount <= 0)
        throw new Exception("Invalid amount");  // ❌ Generic, no code, wrong status
    
    if (!await _groupRepository.ExistsAsync(command.GroupId))
        throw new Exception("Group not found");  // ❌ Returns 500, not 404
    
    // ...
}
```

### After (New Exception System)

```csharp
public async Task<Expense> CreateAsync(CreateExpenseCommand command, CancellationToken cancellationToken)
{
    if (command.Amount <= 0)
        throw new InvalidSplitException("Amount must be greater than zero.", "INVALID_AMOUNT");  // ✅ 400, code included
    
    if (!await _groupRepository.ExistsAsync(command.GroupId, cancellationToken))
        throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");  // ✅ 404, machine-readable code
    
    // ...
}
```

### Client Response

**Before:**
```json
{
  "error": "Group not found"
}
```
❌ No status code info, no way to distinguish from server crash

**After:**
```json
{
  "code": "GROUP_NOT_FOUND",
  "message": "Group was not found.",
  "details": [],
  "traceId": "00-3fa2...d0a1-00"
}
```
✅ Clear status (404), code-based handling, debuggable trace ID

## Copy-Paste Patterns

### Pattern 1: Validation

```csharp
if (condition)
    throw new ValidationException("User message.", "ERROR_CODE");

if (expressionParseError)
    throw new InvalidSplitException("User message.", "ERROR_CODE");
```

### Pattern 2: Not Found

```csharp
if (!resourceExists)
    throw new NotFoundException("User message.", "RESOURCE_TYPE_NOT_FOUND");
```

### Pattern 3: Business Rule

```csharp
if (violatesDomainRule)
    throw new BusinessRuleViolationException("User message.", "RULE_VIOLATED");
```

### Pattern 4: Scope/Auth

```csharp
if (!resourceInScope || !userCanAccess)
    throw new ForbiddenOperationException("User message.", "OPERATION_FORBIDDEN");
```

### Pattern 5: Duplicate/Conflict

```csharp
if (violatesUniquenessConstraint)
    throw new ConflictException("User message.", "RESOURCE_CONFLICT");
```

### Pattern 6: Database Error (Repository)

```csharp
try {
    await _context.SaveChangesAsync();
}
catch (DbUpdateException ex) {
    throw new DataAccessException("User message.", "DATABASE_ERROR", ex);
}
catch (OperationCanceledException ex) {
    throw new DataAccessException("User message.", "QUERY_TIMEOUT", ex);
}
```

## Error Code Naming Convention

| Category | Pattern | Example |
|----------|---------|---------|
| Validation | `INVALID_[FIELD]` | `INVALID_AMOUNT`, `INVALID_SPLIT_TYPE` |
| Not Found | `[RESOURCE]_NOT_FOUND` | `GROUP_NOT_FOUND`, `EXPENSE_NOT_FOUND` |
| Business Rule | `[RULE]_VIOLATION` or `INVALID_[OPERATION]` | `INVALID_SPLIT_SUM` |
| Forbidden | `[OPERATION]_FORBIDDEN` | `MEMBER_OUTSIDE_GROUP_FORBIDDEN` |
| Conflict | `DUPLICATE_[RESOURCE]` | `DUPLICATE_MEMBER` |
| Data Access | `[ISSUE]_ERROR` | `DATABASE_UNAVAILABLE`, `QUERY_TIMEOUT` |

## HTTP Status Cheat Sheet

| Exception Type | Status | Code | Message | Example |
|---|---|---|---|---|
| ValidationException | **400** | `VALIDATION_ERROR` | "Amount must be positive." | `/api/expenses` with amount=-10 |
| InvalidSplitException | **400** | `INVALID_SPLIT` | "Split amounts don't sum to total." | Custom split: [50, 30] for 100 |
| NotFoundException | **404** | `GROUP_NOT_FOUND` | "Group was not found." | `/api/groups/bad-id` |
| BusinessRuleViolationException | **422** | `BUSINESS_RULE_VIOLATION` | "Payer not in group." | Payer outside group members |
| ConflictException | **409** | `CONFLICT` | "Duplicate member name." | Add member with existing name |
| ForbiddenOperationException | **403** | `FORBIDDEN_OPERATION` | "Member outside group." | Use member from other group |
| DataAccessException | **503** | `DATA_ACCESS_ERROR` | "Database unavailable." | PostgreSQL down |
| InternalServerException | **500** | `INTERNAL_ERROR` | "An unexpected error occurred." | Uncaught exception |

## Code Walkthrough

### How exceptions flow:

```
Service throws:
    ↓
throw new InvalidSplitException("Amount must be positive.", "INVALID_AMOUNT");
    │
    ├─> ErrorCode = "INVALID_AMOUNT"
    ├─> StatusCode = 400
    ├─> Message = "Amount must be positive."
    └─> Details = null
    ↓
Middleware catches:
    ├─> exception is AppException? YES ✅
    ├─> Extract: statusCode=400, code="INVALID_AMOUNT", message="..."
    └─> Build response
    ↓
HTTP Response (400):
    {
        "code": "INVALID_AMOUNT",
        "message": "Amount must be positive.",
        "details": [],
        "traceId": "00-..."
    }
    ↓
Client receives 400 Bad Request with error code
```

## Middleware Magic (Automatic)

```csharp
// You throw this:
throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");

// Middleware automatically generates this:
{
  "code": "GROUP_NOT_FOUND",           // From ErrorCode property
  "message": "Group was not found.",   // From Message property
  "details": [],                        // From Details property (or empty)
  "traceId": "00-3fa2103f..."          // From context.TraceIdentifier
}
```

**No additional code needed!** Middleware handles everything.

## Testing Your Exceptions

### Unit Test Pattern

```csharp
[Fact]
public async Task CreateExpense_WithInvalidAmount_ThrowsInvalidSplitException()
{
    var command = new CreateExpenseCommand { Amount = -10 };
    
    var ex = await Assert.ThrowsAsync<InvalidSplitException>(
        () => _service.CreateAsync(command, CancellationToken.None)
    );
    
    Assert.Equal("INVALID_AMOUNT", ex.ErrorCode);
    Assert.Equal(400, ex.StatusCode);
}
```

### Integration Test Pattern

```csharp
[Fact]
public async Task POST_CreateExpense_WithInvalidAmount_Returns400()
{
    var response = await _client.PostAsJsonAsync(
        "/api/groups/11111111-1111-1111-1111-111111111111/expenses",
        new { amount = -10 }
    );
    
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    var content = await response.Content.ReadAsAsync<dynamic>();
    Assert.Equal("INVALID_AMOUNT", (string)content.code);
}
```

## Debugging Tips

1. **Check the error code**: What code did the service throw?
2. **Check the message**: Is it user-friendly and actionable?
3. **Check the status code**: Does it match the category?
4. **Check details array**: Are validation field errors included?
5. **Check traceId**: Can you correlate in logs?

## Common Questions

**Q: Can I reuse error codes?**
A: Yes, but keep them unique per semantic error. Example: "GROUP_NOT_FOUND" is clearer than generic "NOT_FOUND".

**Q: Should I add details for non-validation errors?**
A: No, `Details` is reserved for validation field errors. Other errors leave it empty.

**Q: Do I need to handle exceptions in Controllers?**
A: No! Middleware handles everything. Controllers just call services.

**Q: Can I add custom exception types?**
A: Yes, inherit from `AppException` and set `ErrorCode` and `StatusCode`.

**Q: What if I need to wrap a provider exception?**
A: Use `DataAccessException(message, code, innerException)` in Repository layer.

## Files You Need to Know

1. **Services/Errors/AppException.cs** - The base class with properties
2. **Services/Errors/*.cs** - All specific exception types
3. **Infrastructure/Http/ExceptionHandlingMiddleware.cs** - The translator
4. **docs/exception-quick-reference.md** - Your daily reference
5. **docs/error-taxonomy.md** - Official error code catalog

## Ready to Use

✅ All exception classes built and compiled
✅ Middleware already active in your backend
✅ Error taxonomy aligns with error codes
✅ Mobile client can rely on the codes

**Start throwing exceptions with codes today!**
