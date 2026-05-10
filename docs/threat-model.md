# Threat Model — FairSplit

## 1. System Overview

FairSplit is an expense-sharing MVP comprised of a React Native (Expo) mobile client and an ASP.NET Core Web API backend persisting to PostgreSQL via EF Core. The backend follows a layered architecture (Controllers, Services, Repositories). OpenAPI documents the API surface and structured exception middleware standardizes error responses. Core domain concepts include groups, members, expenses, expense participants, balances and settlements.

The project is scoped for local/demo (unhosted) use in a university setting. Business rules and most domain invariants are enforced in the service layer (e.g., `ExpenseService`) and transactional updates are performed with a TransactionManager/EF Core DbContext. JWT authentication is configured in `Program.cs` and enforced on protected resources such as balances and expenses. For local/demo convenience, token issuance currently uses `POST /api/auth/dev-token` and the mobile app reads the token from `EXPO_PUBLIC_DEV_AUTH_TOKEN`.

## 2. Trust Boundary Diagram

Client (React Native)
  [CreateExpenseScreen, useCreateExpense]
         |
         |  JSON API (HTTP in local demo, HTTPS required for production)
         v
HTTP/API Boundary
  ASP.NET Core Pipeline
    - Authentication / Authorization middleware (JWT bearer on protected endpoints)
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
| T1 | Inconsistent authentication coverage across endpoints can expose data paths | Spoofing / Information Disclosure | `Program.cs` auth pipeline, Controllers | High | Keep JWT bearer enforcement on protected endpoints; explicitly review which read endpoints are public vs protected; map `User` claim → caller identity where authorization is required | Partially mitigated |
| T2 | Trusting client-supplied member/group IDs (horizontal impersonation) | Elevation of Privilege / Spoofing | `ExpensesController`, `ExpenseService` | High | Controllers must derive `callerMemberId` from authenticated principal and override client `payerMemberId`; persist `createdBy` | Planned |
| T3 | Crafted expense payloads altering splits (balance manipulation) | Tampering | Mobile → POST /api/groups/{id}/expenses; `ExpenseService` | High | Server-side recalculation/normalization of shares; validate sums/rounding; reject malformed shares | Partially mitigated (Service checks) |
| T4 | Replay / double-submit creating duplicate expenses | Repudiation / Tampering | POST /api/groups/{id}/expenses, TransactionManager | Medium | Support `Idempotency-Key` header or detect duplicates via business keys (short TTL cache) | Planned |
| T5 | Dev DB credentials in `appsettings.json` expose data | Information Disclosure / Elevation | `appsettings.json`, local Postgres | High | Remove creds from repo; use env vars; provide `.env.example` and demo credentials | Planned |
| T6 | Missing rate-limiting / large participant lists cause DoS (expensive balance recalcs) | Denial of Service | Controllers, ExpenseService recalculation, DB | High | Add request-size limits, participant-count limits, simple rate-limiter middleware (in-memory for demo) | Planned |
| T7 | Swagger/OpenAPI exposure in dev reveals API surface and sample payloads | Information Disclosure | Swagger UI (development) | Medium | Disable Swagger UI except on localhost or protect with simple auth during demos | Planned |
| T8 | Race conditions / lost updates on `Balances` under concurrency | Tampering / Denial of Service | `Balances` table, Repositories, EF Core transactions | Medium | Add optimistic concurrency (`RowVersion`), short transactions, catch `DbUpdateConcurrencyException` and retry | Planned |

## 4. Current Endpoint Auth State (May 2026)

- Public:
  - `POST /api/auth/dev-token`
  - `GET /api/groups`
  - `GET /api/groups/{groupId}/members`
- JWT-protected (`[Authorize]`):
  - `GET/POST /api/groups/{groupId}/expenses`
  - `GET /api/groups/{groupId}/expenses/{expenseId}`
  - `GET /api/groups/{groupId}/balances`

Security implication:
- The app can bootstrap group/member selection without login UX, but expense and balance operations still require a bearer token.
- This is acceptable for local demo scope but should be replaced with explicit login for production deployment.

## 5. Accepted Risks (MVP justification)

- Simplified auth bootstrap flow — For local demo use, a dev token endpoint and `.env`-provided bearer token are acceptable; this is not sufficient for production-grade user auth lifecycle.
- No centralized secrets management — The project runs locally; using environment variables and local dev-only credentials is acceptable for development, provided secrets are not committed and demo credentials are rotated/removed before sharing.
- Limited rate-limiting / no global DDoS protection — Demo traffic is low and local; external DDoS protections and WAFs are out of scope for the MVP.

## 6. Recommendations (Top 3 before production deployment)

1. Replace dev-token bootstrap with full login + secure token lifecycle
- Add a real login/identity flow, short-lived access tokens, optional refresh flow, and secure token storage/rotation. Keep controller-level authorization derived from authenticated claims.

2. Add idempotency for expense creation and optimistic concurrency for `Balances`
- Support `Idempotency-Key` header (short in-memory cache) or implement duplicate detection based on business keys; add a `RowVersion`/version column to `Balances` and handle `DbUpdateConcurrencyException` with retries. This preserves financial invariants under retries and concurrency.

3. Harden local dev visibility and secrets
- Remove DB credentials from repository files; use environment variables and `.env.example`; disable or restrict Swagger UI to localhost or protect with simple auth during demos. This reduces accidental information disclosure.

---

This document is suitable for inclusion in project documentation, ADRs, and presentation materials. For help implementing the recommended fixes I can generate a small PR scaffold (JWT auth snippet + controller change) or create a checklist for the repo.
