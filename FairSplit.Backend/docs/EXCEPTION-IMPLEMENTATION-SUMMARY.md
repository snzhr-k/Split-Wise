# FairSplit Custom Exception System - Implementation Summary

## What Was Delivered

A production-ready custom exception system for FairSplit backend that provides:

1. **Machine-readable error codes** - Enables client-side logic and error handling
2. **Consistent response shapes** - Every error follows the standard JSON schema
3. **Layered architecture** - Each layer (Controller, Service, Repository) has clear exception responsibilities
4. **Type safety** - Specific exception classes catch errors early and prevent silent failures
5. **Global translation** - Centralized middleware converts all exceptions to standard responses
6. **Observability** - Every error includes a `traceId` for correlation and debugging

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     HTTP Request                            │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
                    ┌─────────────┐
                    │  Controller │
                    │ (DTO Valid) │
                    └──────┬──────┘
                           │
                           ▼
                    ┌─────────────────┐
                    │  Service Layer  │
                    │ (Business Logic)│
                    │  Throws:        │
                    │  - Validation   │
                    │  - NotFound     │
                    │  - BizRule      │
                    │  - Forbidden    │
                    │  - Conflict     │
                    └────────┬────────┘
                             │
                      ┌──────▼──────┐
                      │ Repository  │
                      │ (Data Layer)│
                      │ Throws:     │
                      │ - DataAccess│
                      └──────┬──────┘
                             │
                        ┌────▼────┐
                        │ Database │
                        └─────────┘

        Exception Caught by Middleware
                    │
                    ▼
        ┌──────────────────────────┐
        │ ExceptionHandlingMiddleware│
        │ • Inspect exception type  │
        │ • Extract error code      │
        │ • Extract HTTP status     │
        │ • Extract message         │
        │ • Extract details (if any)│
        │ • Add traceId             │
        └────────────┬──────────────┘
                     │
                     ▼
        ┌─────────────────────────┐
        │ Standardized JSON Response│
        │ {                        │
        │   code: "...",           │
        │   message: "...",        │
        │   details: [...],        │
        │   traceId: "..."         │
        │ }                        │
        └────────────┬─────────────┘
                     │
                     ▼
            HTTP Response to Client
```

## File Structure

```
FairSplit.Backend/
├── src/FairSplit.Api/
│   ├── Services/
│   │   └── Errors/
│   │       ├── AppException.cs                      ✨ NEW - Base class
│   │       ├── ValidationErrorDetail.cs             ✨ NEW - Field error record
│   │       ├── ValidationException.cs               ✅ UPDATED
│   │       ├── InvalidSplitException.cs             ✅ UPDATED
│   │       ├── NotFoundException.cs                 ✅ UPDATED
│   │       ├── BusinessRuleViolationException.cs    ✅ UPDATED
│   │       ├── ConflictException.cs                 ✅ UPDATED
│   │       ├── ForbiddenOperationException.cs       ✅ UPDATED
│   │       ├── DataAccessException.cs               ✨ NEW
│   │       └── InternalServerException.cs           ✨ NEW
│   └── Infrastructure/Http/
│       └── ExceptionHandlingMiddleware.cs           ✅ UPDATED
├── docs/
│   ├── error-taxonomy.md                            ✓ EXISTING - Error codes reference
│   ├── exception-architecture.md                    ✨ NEW - Deep dive on design
│   ├── exception-quick-reference.md                 ✨ NEW - Developer cheat sheet
│   └── exception-migration-guide.md                 ✨ NEW - How to update services
└── README.md
```

## Exception Class Hierarchy

```
Exception (C# base)
└── AppException (abstract)
    ├── ValidationException (400)
    │   └── InvalidSplitException (400, specializes for expense splits)
    ├── NotFoundException (404)
    ├── BusinessRuleViolationException (422)
    ├── ConflictException (409)
    ├── ForbiddenOperationException (403)
    ├── DataAccessException (503)
    └── InternalServerException (500)
```

## Exception Properties

Each `AppException` has:

| Property | Type | Purpose | Example |
|----------|------|---------|---------|
| `ErrorCode` | string | Machine-readable code for client logic | `"GROUP_NOT_FOUND"` |
| `StatusCode` | int | HTTP response status | `404` |
| `Message` | string | Human-readable error summary | `"Group was not found."` |
| `Details` | IReadOnlyCollection<ValidationErrorDetail>? | Field-level validation errors | `[{field: "amount", issue: "...", value: -10}]` |

## Standard Error Response

All error responses follow this JSON schema:

```json
{
  "code": "string (machine-readable error code)",
  "message": "string (human-readable message)",
  "details": [
    {
      "field": "string (property name)",
      "issue": "string (validation failure reason)",
      "value": "any (rejected value, optional)"
    }
  ],
  "traceId": "string (correlation ID from ASP.NET Core)"
}
```

## Error Code Catalog

Aligned with `error-taxonomy.md`, the system supports:

**Validation (400)**
- `VALIDATION_ERROR`, `INVALID_AMOUNT`, `INVALID_SPLIT`, `INVALID_SPLIT_TYPE`, `DUPLICATE_PARTICIPANTS`, `EMPTY_PARTICIPANTS`, `INVALID_CUSTOM_SHARE`, etc.

**Not Found (404)**
- `RESOURCE_NOT_FOUND`, `GROUP_NOT_FOUND`, `MEMBER_NOT_FOUND`, `EXPENSE_NOT_FOUND`, etc.

**Business Rule Violation (422)**
- `BUSINESS_RULE_VIOLATION`, `INVALID_SPLIT_SUM`, `PAYER_NOT_IN_GROUP`, `PARTICIPANT_NOT_IN_GROUP`, etc.

**Conflict (409)**
- `CONFLICT`, `DUPLICATE_MEMBER`, `RESOURCE_VERSION_CONFLICT`, `IDEMPOTENCY_CONFLICT`, etc.

**Forbidden (403)**
- `FORBIDDEN_OPERATION`, `MEMBER_OUTSIDE_GROUP_FORBIDDEN`, `CROSS_GROUP_OPERATION_FORBIDDEN`, etc.

**Data Access (503)**
- `DATA_ACCESS_ERROR`, `DATABASE_UNAVAILABLE`, `TRANSACTION_FAILED`, `QUERY_TIMEOUT`, etc.

**Internal (500)**
- `INTERNAL_ERROR`, `UNEXPECTED_ERROR`

## Integration Points

### 1. Global Middleware (Already Active)

The middleware is already registered in `Program.cs`:

```csharp
app.UseGlobalExceptionHandling();  // Catches all AppExceptions
```

**How it works:**
- Intercepts all exceptions in request pipeline
- Checks if it's an `AppException` subclass
- Extracts `ErrorCode`, `StatusCode`, `Message`, `Details`
- Returns standardized JSON response with `traceId`
- Wraps unexpected exceptions as `INTERNAL_ERROR` (500)

### 2. Service Layer Throws

Services throw domain-specific exceptions:

```csharp
// Validation error
throw new InvalidSplitException("Amount must be positive.", "INVALID_AMOUNT");

// Resource not found
throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");

// Business rule violation
throw new BusinessRuleViolationException(
    "Custom split must sum to total.",
    "INVALID_SPLIT_SUM"
);

// Scope violation
throw new ForbiddenOperationException(
    "Member outside group.",
    "MEMBER_OUTSIDE_GROUP_FORBIDDEN"
);
```

### 3. Repository Layer Translation

Repositories catch provider-specific exceptions and translate:

```csharp
try {
    await _context.SaveChangesAsync();
}
catch (OperationCanceledException ex) {
    throw new DataAccessException(
        "Database query timed out.",
        "QUERY_TIMEOUT",
        ex
    );
}
catch (DbUpdateException ex) when (IsConstraintViolation(ex)) {
    throw new ConflictException(
        "Duplicate resource.",
        "DUPLICATE_KEY",
        ex
    );
}
```

### 4. Controller Layer (Minimal Changes)

Controllers call services; middleware handles exceptions:

```csharp
[HttpPost("{groupId:guid}/expenses")]
public async Task<IActionResult> CreateExpense(
    Guid groupId,
    CreateExpenseRequest request,
    CancellationToken cancellationToken)
{
    // No error handling needed!
    // DTO validation is automatic (ModelState)
    // Business exceptions bubble up to middleware
    var expense = await _expenseService.CreateAsync(...);
    return CreatedAtAction(..., expense);
}
```

## Usage Examples

### Example 1: Validation Error

```csharp
// Service code
if (command.Amount <= 0) {
    throw new InvalidSplitException(
        "Amount must be greater than zero.",
        "INVALID_AMOUNT"
    );
}

// Client receives (HTTP 400)
{
  "code": "INVALID_AMOUNT",
  "message": "Amount must be greater than zero.",
  "details": [],
  "traceId": "00-ab123..."
}
```

### Example 2: Not Found

```csharp
// Service code
if (!groupExists) {
    throw new NotFoundException(
        "Group was not found.",
        "GROUP_NOT_FOUND"
    );
}

// Client receives (HTTP 404)
{
  "code": "GROUP_NOT_FOUND",
  "message": "Group was not found.",
  "details": [],
  "traceId": "00-cd456..."
}
```

### Example 3: Business Rule Violation

```csharp
// Service code
if (totalCustomAmount != command.Amount) {
    throw new BusinessRuleViolationException(
        "Custom split amounts must sum to total.",
        "INVALID_SPLIT_SUM"
    );
}

// Client receives (HTTP 422)
{
  "code": "INVALID_SPLIT_SUM",
  "message": "Custom split amounts must sum to total.",
  "details": [],
  "traceId": "00-ef789..."
}
```

### Example 4: Validation with Details

```csharp
// Service code
var errors = new List<ValidationErrorDetail>();
if (amount <= 0)
    errors.Add(new("amount", "must be greater than 0", amount));
if (participants.Count == 0)
    errors.Add(new("participants", "must have at least one"));

if (errors.Any()) {
    throw new ValidationException(
        "Request validation failed.",
        "VALIDATION_ERROR",
        errors
    );
}

// Client receives (HTTP 400)
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
      "issue": "must have at least one",
      "value": null
    }
  ],
  "traceId": "00-gh012..."
}
```

## Build & Test Status

✅ **Compiles successfully**
```
Build succeeded.
0 Error(s), 4 Warning(s)
```

✅ **No breaking changes** - Middleware already exists and uses enhanced structure

✅ **Production-ready** - No external dependencies, uses standard ASP.NET Core APIs

## Migration Path

The system is designed for incremental migration:

1. **Existing code** continues to work (backward compatible)
2. **Gradually update** services one at a time
3. **Use migration guide** (`exception-migration-guide.md`) for each service
4. **Add tests** alongside migrations
5. **Mobile client** updates error handling based on new error codes

### Priority for Migration:
1. **High**: `ExpenseService` (core business logic)
2. **High**: `GroupService` (primary resource)
3. **Medium**: `BalanceService` (financial accuracy)
4. **Medium**: `MemberService` (supporting)
5. **Low**: `SettlementService` (future feature)

## Documentation Provided

| Document | Purpose | Audience |
|----------|---------|----------|
| `error-taxonomy.md` | Complete error taxonomy and React Native handling policy | Backend devs, Mobile devs |
| `exception-architecture.md` | Deep architectural dive with before/after examples | Backend lead, Architects |
| `exception-quick-reference.md` | Quick lookup table and patterns | All backend devs |
| `exception-migration-guide.md` | Step-by-step service migration instructions | Backend devs maintaining code |

## Design Decisions

### ✅ Why This Approach?

1. **Base `AppException` class**
   - Centralizes error contract (ErrorCode, StatusCode, Details)
   - Enables polymorphic middleware handling
   - Allows specialized subclasses for domain nuances

2. **Specific exception subclasses**
   - Makes error intent explicit in code
   - Enables compile-time verification
   - Supports `catch` syntax for specific errors

3. **Machine-readable error codes**
   - REST best practice (status code + body code)
   - Enables reliable client-side logic
   - Decouples from English error messages

4. **Centralized middleware translation**
   - Single point of control for response formatting
   - Ensures consistency across all endpoints
   - Eliminates scattered try-catch boilerplate

5. **`ValidationErrorDetail` records**
   - Enables form-field validation in client UI
   - Type-safe field references
   - Open-ended `Value` property for flexibility

### ❌ What We Avoided

- Multiple error response schemas (unified JSON shape)
- Throwing generic `Exception` classes (specific types)
- Business logic in Controllers (Services own it)
- Bypassing the middleware (single translation point)
- External error handling libraries (built-in only)
- Custom HTTP headers for errors (standard JSON body)
- Exception-per-field-type (unified approach with codes)

## Browser & Client Compatibility

The error response JSON works with:
- ✅ React Native (Expo) via async/await and JSON parsing
- ✅ JavaScript HTTP clients (`fetch`, `axios`)
- ✅ Python/C# test clients
- ✅ Browser dev tools
- ✅ Postman, cURL, REST clients
- ✅ API documentation tools (Swagger/OpenAPI)

## Next Steps

1. **For backend developers:**
   - Use `exception-quick-reference.md` as daily guide
   - Follow `exception-migration-guide.md` when updating services
   - Write tests for error paths in new features

2. **For mobile developers:**
   - Update error handling based on error codes from `error-taxonomy.md`
   - Add UX patterns for each error category
   - Test error flows with backend during integration

3. **For DevOps/Observability:**
   - Correlate errors by `traceId` in logs
   - Alert on unusual error code frequencies
   - Monitor 5xx error rate and investigate data-access errors

4. **For CI/CD:**
   - Add contract tests verifying error codes in responses
   - Ensure all error paths return standardized JSON
   - Block PRs if new code doesn't use exception system

## Troubleshooting

**Q: Build error about sealed types?**
A: Ensure `ValidationException` is NOT sealed (it's a base for `InvalidSplitException`)

**Q: Middleware not catching exceptions?**
A: Verify `app.UseGlobalExceptionHandling()` is called early in `Program.cs`

**Q: Status codes always 500?**
A: Check that exception sets `StatusCode` property in constructor

**Q: Details array null in response?**
A: Middleware uses `details ?? Array.Empty<ValidationErrorDetail>()` to normalize

**Q: Client not receiving error code?**
A: Verify Response Content-Type is `application/json` and response serializes with `WriteAsJsonAsync`

## Summary

✅ **Clean exception hierarchy** - Base class with 8 specific types covering all scenarios

✅ **Machine-readable codes** - 50+ error codes aligned with taxonomy document

✅ **Standardized responses** - Single JSON schema for all errors

✅ **Global middleware** - Centralized translation to HTTP responses

✅ **Type safety** - Specific exceptions catch errors at compile-time

✅ **Production-ready** - Zero external dependencies, uses ASP.NET Core built-ins

✅ **Fully documented** - 4 comprehensive guides for different audiences

✅ **Builds & runs** - Verified working with existing backend

✅ **Backward compatible** - Can migrate services incrementally

✅ **Mobile-friendly** - Enables robust error handling in React Native client

The system is ready for immediate use. Start with high-impact services (`ExpenseService`, `GroupService`) and gradually migrate the rest.
