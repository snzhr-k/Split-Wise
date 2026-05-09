# FairSplit API Error Taxonomy

## Purpose

This document defines a consistent error taxonomy for the FairSplit REST API.

It standardizes:
- error categories and machine-readable error codes
- HTTP status code mapping
- response payload shape
- backend layer ownership for creating/mapping errors
- React Native client behavior per error category

Scope: Groups, Members, Expenses, ExpenseParticipants, Balances, Settlements.

## Design Principles

- Use stable, machine-readable `code` values for client logic.
- Keep `message` human-readable and actionable.
- Keep transport semantics RESTful: status codes express class of failure.
- Return `traceId` for observability and support.
- Reserve `details` for field-level validation errors.
- Do not leak stack traces or raw database internals to clients.

## Canonical Error Categories

| Category | Error Code | HTTP Status | Description | FairSplit Examples |
|---|---|---:|---|---|
| Validation | `VALIDATION_ERROR` | `400 Bad Request` | Request format or field values are syntactically invalid. | `amount <= 0`; `participants` is empty; `splitType` is not one of `equal`/`custom`. |
| Resource Not Found | `RESOURCE_NOT_FOUND` | `404 Not Found` | Referenced resource does not exist in the requested scope. | Group ID does not exist; Expense ID not found in group; Member ID not found in group. |
| Business Rule Violation | `BUSINESS_RULE_VIOLATION` | `422 Unprocessable Entity` | Payload is syntactically valid, but violates domain rules. | Custom split total does not equal expense amount; payer is not a participant when rule requires inclusion; settlement amount exceeds outstanding balance. |
| Forbidden Operation | `FORBIDDEN_OPERATION` | `403 Forbidden` | Caller tries an operation that is disallowed for context/scope. | Member from another group is used in an expense; modifying expense belonging to different group context; settling debt across unrelated groups. |
| Conflict | `CONFLICT` | `409 Conflict` | Request conflicts with current resource state or uniqueness constraints. | Duplicate member display name in same group (if unique policy enabled); duplicate idempotency key for create expense; concurrent update conflict on same expense record. |
| Data Access / Persistence | `DATA_ACCESS_ERROR` | `503 Service Unavailable` (or `500`) | Database or persistence dependency is unavailable or failed unexpectedly. | PostgreSQL unreachable; transaction commit failure; repository timeout while loading balances. |
| Internal / Unexpected | `INTERNAL_ERROR` | `500 Internal Server Error` | Unhandled or unknown server failure. | Null-reference bug in service flow; mapping error in response projection; unexpected runtime exception in middleware chain. |

## Domain-Specific Error Codes

Use category-level codes above as defaults. Prefer specific codes where known.

### Validation (`400`)

- `INVALID_REQUEST_BODY`
- `MISSING_REQUIRED_FIELD`
- `INVALID_AMOUNT`
- `INVALID_SPLIT_TYPE`
- `EMPTY_PARTICIPANTS`
- `DUPLICATE_PARTICIPANTS`

### Resource Not Found (`404`)

- `GROUP_NOT_FOUND`
- `MEMBER_NOT_FOUND`
- `EXPENSE_NOT_FOUND`
- `SETTLEMENT_NOT_FOUND`
- `BALANCE_NOT_FOUND`

### Business Rule Violation (`422`)

- `INVALID_SPLIT_SUM`
- `INVALID_CUSTOM_SHARE`
- `PAYER_NOT_IN_GROUP`
- `PARTICIPANT_NOT_IN_GROUP`
- `SETTLEMENT_EXCEEDS_DEBT`

### Forbidden Operation (`403`)

- `MEMBER_OUTSIDE_GROUP_FORBIDDEN`
- `CROSS_GROUP_OPERATION_FORBIDDEN`

### Conflict (`409`)

- `DUPLICATE_MEMBER`
- `RESOURCE_VERSION_CONFLICT`
- `IDEMPOTENCY_CONFLICT`

### Data Access / Persistence (`503` or `500`)

- `DATABASE_UNAVAILABLE`
- `TRANSACTION_FAILED`
- `QUERY_TIMEOUT`

### Internal / Unexpected (`500`)

- `UNEXPECTED_ERROR`

## Standard Error Response Shape

All failed API responses should return JSON with this shape:

```json
{
  "code": "INVALID_AMOUNT",
  "message": "Amount must be greater than zero.",
  "details": [
    {
      "field": "amount",
      "issue": "must be greater than 0",
      "value": -10
    }
  ],
  "traceId": "00-6f2f0d9db1f83d9cc8f6a5f2f1c4bd9e-31e7f8f5a9a0a1cd-00"
}
```

Field contract:
- `code` (required, string): Stable machine-readable identifier.
- `message` (required, string): Human-readable summary suitable for UI.
- `details` (optional, array): Field-level diagnostics. Primarily for validation errors.
- `traceId` (required, string): Correlation identifier from ASP.NET Core request pipeline.

`details` object schema:
- `field` (required, string): Request field/path, for example `participants[1].shareAmount`.
- `issue` (required, string): Validation failure reason.
- `value` (optional, any JSON value): Rejected value when safe to expose.

Notes for your current backend:
- Your global exception middleware already returns `code`, `message`, and `traceId`.
- Add `details` only when available (typically model/state or service validation errors).
- Keep response `Content-Type` as `application/json`.

## Layer Responsibility Matrix

| Error Category | Primary Producer | Secondary Producer | Mapping to HTTP Response |
|---|---|---|---|
| Validation (`400`) | Controller (DTO/model binding) and Service (use-case validation) | Repository (rare, only shape-related input constraints) | Global exception middleware |
| Resource Not Found (`404`) | Service (resource existence checks) | Repository (null result), Controller (route-level prechecks) | Global exception middleware |
| Business Rule Violation (`422`) | Service (domain invariants) | None | Global exception middleware |
| Forbidden Operation (`403`) | Service (authorization/scope in domain context) | Controller (authz policy gate) | Global exception middleware |
| Conflict (`409`) | Service (state transition conflicts) and Repository (unique/constraint conflicts) | Controller (idempotency prechecks) | Global exception middleware |
| Data Access / Persistence (`503`/`500`) | Repository / Data layer | Service (transaction orchestration) | Global exception middleware |
| Internal (`500`) | Any layer (unexpected exception) | None | Global exception middleware fallback |

Implementation policy (non-code):
- Controllers should not encode business rules.
- Services should throw domain-meaningful exceptions with specific codes.
- Repositories should translate provider-specific failures into data-access exceptions.
- Middleware is the single place that translates exceptions to HTTP status + payload schema.

## React Native Client Handling Policy (Expo)

| Category | UX Behavior | Retry Policy | Navigation Behavior |
|---|---|---|---|
| Validation (`400`) | Show inline field errors; keep form values; focus first invalid field. | No automatic retry. | Stay on current screen. |
| Resource Not Found (`404`) | Show toast/snackbar: "Item no longer exists." | Optional one-time refetch if list/detail cache may be stale. | Navigate back from stale detail screens if entity removed. |
| Business Rule Violation (`422`) | Show contextual message near action area and in toast for visibility. | No automatic retry; require user correction. | Stay on current flow so user can adjust input. |
| Forbidden (`403`) | Show blocking message explaining scope restriction. | No retry until context changes. | Navigate to allowed scope (for example, group list) if action cannot continue. |
| Conflict (`409`) | Show conflict toast and prompt user to refresh/reload current data. | Offer manual retry after refresh. | Usually remain on screen; optionally open latest state view. |
| Data Access (`503`/`500`) | Show non-blocking banner/toast: "Server temporarily unavailable." | Exponential backoff for safe GETs; manual retry for mutations. | Stay on screen; preserve pending input where possible. |
| Internal (`500`) | Show generic error toast and fallback UI state. | Limited retry with backoff; stop after small threshold. | Do not force navigation unless screen cannot recover. |

Client-side handling rules:
- Branch behavior primarily on `code`; use HTTP status as fallback.
- Always log `traceId` with client telemetry and crash/error reports.
- Never show raw backend exception text when message is technical.
- For mutation failures, keep user input in memory to avoid re-entry friction.

## Recommended Mapping for Current ASP.NET Core Middleware

Your current middleware map can be normalized to these canonical codes:

- `not_found` -> `RESOURCE_NOT_FOUND` (or specific `GROUP_NOT_FOUND`, `EXPENSE_NOT_FOUND`)
- `validation_error` -> `VALIDATION_ERROR` (or specific validation code)
- `conflict` -> `CONFLICT` (or specific conflict code)
- `forbidden` -> `FORBIDDEN_OPERATION`
- `business_rule_violation` -> `BUSINESS_RULE_VIOLATION`
- `unexpected_error` -> `INTERNAL_ERROR` / `UNEXPECTED_ERROR`

Recommendation:
- Keep backward compatibility during migration by documenting aliases.
- Prefer introducing specific codes incrementally per endpoint/use-case.

## Example Error Payloads

### 1) Validation error (`400`)

```json
{
  "code": "INVALID_SPLIT_TYPE",
  "message": "splitType must be 'equal' or 'custom'.",
  "details": [
    {
      "field": "splitType",
      "issue": "unsupported enum value",
      "value": "percent"
    }
  ],
  "traceId": "00-..."
}
```

### 2) Not found (`404`)

```json
{
  "code": "GROUP_NOT_FOUND",
  "message": "Group '11111111-1111-1111-1111-111111111111' was not found.",
  "traceId": "00-..."
}
```

### 3) Business rule violation (`422`)

```json
{
  "code": "INVALID_SPLIT_SUM",
  "message": "Custom split amounts must sum to total amount.",
  "traceId": "00-..."
}
```

### 4) Conflict (`409`)

```json
{
  "code": "DUPLICATE_MEMBER",
  "message": "A member with this display name already exists in the group.",
  "traceId": "00-..."
}
```

### 5) Data access (`503`)

```json
{
  "code": "DATABASE_UNAVAILABLE",
  "message": "Database is temporarily unavailable. Please try again.",
  "traceId": "00-..."
}
```

## Operational Notes

- Include `traceId` in backend logs for every exception write-path.
- Keep code strings immutable once published to mobile clients.
- Add integration tests that assert both HTTP status and `code` values.
- Add contract tests in mobile for error parsing and UX branching.

## Versioning

Taxonomy version: `v1`.

Change policy:
- Additive changes (new specific codes) are non-breaking.
- Replacing/removing existing codes is breaking and must be versioned and coordinated between backend and mobile.
