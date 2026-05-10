# ADR 002 — Standardized Error Taxonomy and Global Exception Handling Middleware

**Date:** 2026-05-10

**Status:** Accepted

## Context

## Decision

Adopt and maintain a standardized error taxonomy as the canonical API error contract. Implement a global exception handling middleware that:


Service code should throw `AppException` subclasses (e.g., `ValidationException`, `NotFoundException`, `ForbiddenOperationException`) rather than returning ad-hoc HTTP results.

## Consequences


## Rationale / Drivers


## Alternatives Considered


### Alternatives analysis (brief)


- Error codes are considered part of the public API contract. Non-breaking additions (new error codes) are allowed without versioning; renaming or removing codes requires a minor API version bump and coordination with clients.
- Backward compatibility: when changing an error code, add the new code while keeping the old one for at least one release cycle and document mapping in `docs/error-taxonomy.md`.


ADR-0002: Standardized Error Taxonomy and Global Exception Handling Middleware

Status: Accepted

Date: 2026-05-10

Context

- The repository contains a structured `ErrorResponse` defined in `openapi.yaml` and client parsing code in `FairSplit.Mobile/src/api/httpClient.ts` and `FairSplit.Mobile/src/services/errorTypes.ts`.
- The server defines `AppException` and specific exception subclasses in `Services/Errors/*` that carry `ErrorCode`, `StatusCode`, and optional `Details`.
- A global `ExceptionHandlingMiddleware` translates `AppException` instances into a consistent JSON `ErrorResponse` including a `traceId` (`Infrastructure/Http/ExceptionHandlingMiddleware.cs`).

Decision

Adopt a standardized error taxonomy as the canonical API error contract and enforce it via global exception handling middleware. Service code will throw `AppException` subclasses; the middleware will:

- Map `AppException` instances to a structured `ErrorResponse` (`code`, `message`, `details`, `traceId`) and set the appropriate HTTP status code.
- Map unexpected exceptions to a generic `INTERNAL_ERROR` with HTTP 500 and a sanitized client message.
- Ensure `openapi.yaml` references the `ErrorResponse` schema for relevant responses.

Consequences

- Positive: Clients have a stable, machine-readable error shape enabling programmatic handling, improved UX, and form binding.
- Positive: Centralized mapping reduces duplication and inconsistent status mapping across controllers.
- Negative: Maintaining a taxonomy introduces governance overhead; changes to error codes are backward-compatibility concerns for clients.
- Security: Messages must be sanitized to avoid leaking sensitive information; logs must retain full diagnostics correlated by `traceId`.
## Security & Privacy Rules

- Error `message` should not include sensitive data (PII, secrets, stack traces). `details` may include field-level validation issues (field name and issue) but must not echo sensitive values.
- Log full exception details server-side; only sanitized messages and a `traceId` are returned to clients.

## Known Risks & Mitigations

- Taxonomy maintenance burden: maintain an owner and a documented process for adding/removing codes (see Governance below).
- Client coupling: mobile code depends on stable codes; mitigate with versioning policy and deprecation windows.

## Example (server middleware behavior)

```csharp
// ExceptionHandlingMiddleware maps AppException -> ErrorResponse
var errorResponse = new ErrorResponse(
	Code: appEx.ErrorCode,
	Message: appEx.Message,
	Details: appEx.Details ?? Array.Empty<ValidationErrorDetail>(),
	TraceId: context.TraceIdentifier
);
await context.Response.WriteAsJsonAsync(errorResponse);
```

## Governance / Owner

- Owner: backend lead (maintains `docs/error-taxonomy.md` and coordinates client updates).
- Change process: propose code change, update `openapi.yaml`, add client mapping note, and deprecate older codes with a one-release window.


## Related Code

- `Infrastructure/Http/ExceptionHandlingMiddleware.cs`
- `Services/Errors/AppException.cs` and derived types in `Services/Errors/`
- `openapi.yaml` (components → ErrorResponse and response refs)
- Mobile client parsing: `FairSplit.Mobile/src/api/httpClient.ts` and `FairSplit.Mobile/src/services/errorTypes.ts`

## Notes

This ADR captures both an API contract (error shape & codes) and the operational middleware that enforces consistent behavior across the server. The taxonomy should be versioned and documented for client implementers.
