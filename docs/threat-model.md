# Threat Model — FairSplit

## 1. System Overview

FairSplit is an expense-sharing MVP comprised of a React Native (Expo) mobile client and an ASP.NET Core Web API backend persisting to PostgreSQL via EF Core. The backend follows a layered architecture (Controllers, Services, Repositories). OpenAPI documents the API surface and structured exception middleware standardizes error responses. Core domain concepts include groups, members, expenses, expense participants, balances and settlements.

The project is scoped for local/demo (unhosted) use in a university setting. Business rules and most domain invariants are enforced in the service layer (e.g., `ExpenseService`) and transactional updates are performed with a TransactionManager/EF Core DbContext. Authentication is only partially implemented: OpenAPI declares `bearerAuth`, but `AddAuthentication()`/JWT validation is not yet fully configured and some endpoints currently rely primarily on service-layer ownership checks.

## 2. Trust Boundary Diagram

Client (React Native)
  [CreateExpenseScreen, useCreateExpense]
         |
         |  HTTPS (TLS) / JSON API
         v
HTTP/API Boundary
  ASP.NET Core Pipeline
    - Authentication / Authorization middleware (incomplete)
    - Exception middleware
    - Controllers (e.g., `ExpensesController`)
         |
         v
Service Layer (ExpenseService, GroupService)
  - Business rules, ownership checks, transaction orchestration
         |
         v
Repository Layer (EF Core Repositories)
  - DB mapping, queries, persistence
         |
         v
PostgreSQL (Balances, Expenses, Members, ...)

External input boundaries:
- Mobile request payloads (untrusted)
- Swagger/OpenAPI dev UI (exposes contract)

Trust boundaries:
- Mobile client ↔ HTTP/API
- Controllers ↔ Services
- Services ↔ Repositories
- Backend ↔ PostgreSQL

## 3. Threat Table

| ID | Threat | STRIDE Category | Affected Component | Severity | Mitigation | Status |
|----|--------|----------------|--------------------|---------:|-----------|--------|
| T1 | Missing authentication enforcement (unauthenticated access / identity spoofing) | Spoofing | `Program.cs` auth pipeline, Controllers, POST /api/* endpoints | Critical | Configure JWT bearer auth; require `UseAuthentication()` and `[Authorize]`; map `User` claim → `memberId` at controller boundary | Planned |
| T2 | Trusting client-supplied member/group IDs (horizontal impersonation) | Elevation of Privilege / Spoofing | `ExpensesController`, `ExpenseService` | High | Controllers must derive `callerMemberId` from authenticated principal and override client `payerMemberId`; persist `createdBy` | Planned |
| T3 | Crafted expense payloads altering splits (balance manipulation) | Tampering | Mobile → POST /api/groups/{id}/expenses; `ExpenseService` | High | Server-side recalculation/normalization of shares; validate sums/rounding; reject malformed shares | Partially mitigated (Service checks) |
| T4 | Replay / double-submit creating duplicate expenses | Repudiation / Tampering | POST /api/groups/{id}/expenses, TransactionManager | Medium | Support `Idempotency-Key` header or detect duplicates via business keys (short TTL cache) | Planned |
| T5 | Dev DB credentials in `appsettings.json` expose data | Information Disclosure / Elevation | `appsettings.json`, local Postgres | High | Remove creds from repo; use env vars; provide `.env.example` and demo credentials | Planned |
| T6 | Missing rate-limiting / large participant lists cause DoS (expensive balance recalcs) | Denial of Service | Controllers, ExpenseService recalculation, DB | High | Add request-size limits, participant-count limits, simple rate-limiter middleware (in-memory for demo) | Planned |
| T7 | Swagger/OpenAPI exposure in dev reveals API surface and sample payloads | Information Disclosure | Swagger UI (development) | Medium | Disable Swagger UI except on localhost or protect with simple auth during demos | Planned |
| T8 | Race conditions / lost updates on `Balances` under concurrency | Tampering / Denial of Service | `Balances` table, Repositories, EF Core transactions | Medium | Add optimistic concurrency (`RowVersion`), short transactions, catch `DbUpdateConcurrencyException` and retry | Planned |

## 4. Accepted Risks (MVP justification)

- Simplified auth model / no refresh tokens — For a local university demo, a minimal JWT-based auth without refresh-token complexity is acceptable; refresh tokens introduce infra complexity beyond the project scope.
- No centralized secrets management — The project runs locally; using environment variables and local dev-only credentials is acceptable for development, provided secrets are not committed and demo credentials are rotated/removed before sharing.
- Limited rate-limiting / no global DDoS protection — Demo traffic is low and local; external DDoS protections and WAFs are out of scope for the MVP.

## 5. Recommendations (Top 3 before production deployment)

1. Implement authentication and controller-level caller identity enforcement
- Configure `AddAuthentication().AddJwtBearer(...)`, enable `UseAuthentication()`, require `[Authorize]` on protected controllers, and derive `callerMemberId` from `User` claims in controllers (do not trust client `payerMemberId`). This removes the highest-severity spoofing/impersonation risks.

2. Add idempotency for expense creation and optimistic concurrency for `Balances`
- Support `Idempotency-Key` header (short in-memory cache) or implement duplicate detection based on business keys; add a `RowVersion`/version column to `Balances` and handle `DbUpdateConcurrencyException` with retries. This preserves financial invariants under retries and concurrency.

3. Harden local dev visibility and secrets
- Remove DB credentials from repository files; use environment variables and `.env.example`; disable or restrict Swagger UI to localhost or protect with simple auth during demos. This reduces accidental information disclosure.

---

This document is suitable for inclusion in project documentation, ADRs, and presentation materials. For help implementing the recommended fixes I can generate a small PR scaffold (JWT auth snippet + controller change) or create a checklist for the repo.
