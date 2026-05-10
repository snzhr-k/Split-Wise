# Submission Readiness Plan (FairSplit)

Date: 2026-05-10  
Scope: Convert current MVP into submission-ready implementation aligned with documented API contract and quality expectations.

## 1. Goal

Raise readiness from current MVP level to a spec-aligned submission by:
- Completing missing API features.
- Removing scaffold/placeholder behavior (`501` endpoints).
- Aligning OpenAPI with actual implementation.
- Adding integration coverage for core and auth-protected flows.
- Hardening auth behavior for non-trivial grading scrutiny.

## 2. Current State (Audit Snapshot)

- Core flow works: groups list, members list, expense create/list, balances.
- Build and tests pass locally.
- JWT auth is configured and enforced on key protected endpoints.
- Gaps remain in endpoint completeness and test depth.

## 3. Target Outcome

- All required endpoints implemented (or explicitly out-of-scope and removed from contract).
- No placeholder controller actions returning `501`.
- OpenAPI and runtime behavior fully synchronized.
- Integration tests validate happy path + auth failure path + key domain invariants.
- Submission package includes clear docs and repeatable verification commands.

## 4. Work Plan (Phased)

## Phase A: Contract and Endpoint Completion (Highest Priority)

### A1. Implement missing Group endpoints
- Implement `GET /api/groups/{groupId}`.
- Implement `POST /api/groups` (if kept in contract).
- Implement `POST /api/groups/{groupId}/join`.

Exit criteria:
- Endpoints return contract-matching status codes and response shapes.
- Endpoint behavior is documented in OpenAPI and tested.

### A2. Implement Settlements end-to-end
- Replace scaffold in `FairSplit.Backend/src/FairSplit.Api/Controllers/SettlementsController.cs`.
- Implement settlement create/list/get paths per OpenAPI.
- Ensure settlement writes update balances transactionally.

Exit criteria:
- Settlement lifecycle works through API.
- Balances reflect settlement effects correctly.

### A3. Remove all placeholder `501` behavior
- Replace scaffolded members fallback in `FairSplit.Backend/src/FairSplit.Api/Controllers/MembersController.cs`.
- Implement or remove `ExpenseParticipantsController` routes.
- Ensure no public route returns `501` unless intentionally out-of-scope and documented.

Exit criteria:
- No unresolved placeholder endpoints in active API.

## Phase B: OpenAPI and Implementation Alignment

### B1. Contract synchronization pass
- Compare each path in `openapi.yaml` with actual controller routes and behavior.
- Update either code or contract so they exactly match.
- Keep copied docs spec in sync:
  - `FairSplit.Backend/docs/openapi/openapi.yaml`

Exit criteria:
- API contract parity report shows no missing/mismatched paths.

## Phase C: Testing and Quality Coverage

### C1. Integration tests for core flows
Add real tests in `FairSplit.Backend/tests/FairSplit.IntegrationTests` for:
- Group list/read/join or create (depending on final scope).
- Members list for group.
- Expense create/list/get (authorized).
- Balance retrieval (authorized).
- Settlement create/list/get (authorized, after implementation).

Exit criteria:
- Integration test project is no longer placeholder.
- Tests run in CI/local and pass consistently.

### C2. Authorization and negative-path tests
Add tests for:
- Missing bearer token on protected endpoints -> `401`.
- Invalid token -> `401`.
- Invalid group/member ownership relationships -> `403` or validation errors as designed.

Exit criteria:
- Auth enforcement is covered by tests, not only manual checks.

## Phase D: Auth and Security Hardening (Submission Plus)

### D1. Tighten mutation identity handling
- For protected mutation endpoints, derive caller identity from JWT claims where applicable.
- Avoid relying solely on client-supplied actor IDs for authorization decisions.

Exit criteria:
- Service/controller authorization paths use authenticated identity consistently.

### D2. Document dev-token scope clearly
- Keep `POST /api/auth/dev-token` marked demo/dev only.
- Update docs to clearly separate demo auth from production-grade auth expectations.

Exit criteria:
- Reviewer can easily identify security tradeoffs and intentional MVP scope.

## 5. Recommended Execution Order

1. Phase A (endpoint completeness)
2. Phase B (contract sync)
3. Phase C (integration + auth tests)
4. Phase D (hardening/docs polish)

Reason: endpoint completeness first avoids writing tests against unstable or changing routes.

## 6. Definition of Done (Submission Gate)

All items below must be true before final submission:
- `dotnet build FairSplit.slnx` passes.
- `dotnet test FairSplit.slnx` passes with no placeholder test projects.
- No active API endpoint returns scaffold `501`.
- OpenAPI paths and implemented routes fully match.
- Core and auth-negative integration tests pass.
- README and threat model reflect final auth behavior and feature scope.

## 7. Verification Commands

```bash
cd FairSplit.Backend

dotnet build FairSplit.slnx

dotnet test FairSplit.slnx

# Optional smoke checks after backend startup:
# GET groups
curl -sS -i http://localhost:5001/api/groups

# Protected expenses list (requires token)
curl -sS -i http://localhost:5001/api/groups/<groupId>/expenses \
  -H 'Authorization: Bearer <accessToken>'
```

## 8. Optional Stretch Goals

- Add idempotency for expense creation (`Idempotency-Key`).
- Add optimistic concurrency (`RowVersion`) for balance updates.
- Add minimal rate limiting for abuse resistance in demo environments.

These are not required for MVP acceptance but improve technical quality and risk posture.
