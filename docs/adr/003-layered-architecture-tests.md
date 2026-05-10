ADR-0003: Layered Architecture with Automated Layer-Isolation Tests

Status: Accepted

Date: 2026-05-10

Context

- The repository is organized into presentation (Controllers), services (business logic), and repositories (data access). Controllers invoke services; services invoke repositories (see `Controllers/*` and `Services/Implementations/*`).
- Automated architecture and layer-isolation tests are present under `tests/FairSplit.ArchitectureTests` and `tests/FairSplit.LayerIsolationTests`.

Decision

Adopt a strict layered architecture (Controllers → Services → Repositories) and enforce the intent with automated architecture tests that run as part of the test suite. Intentional deviations require an approval path and a documented exception.

Consequences

- Positive: Clear separation of concerns improves maintainability and makes it obvious where to implement domain logic.
- Positive: Services can be unit-tested in isolation with repository mocks; architectural regressions are detected early by automated tests.
- Negative: Increased boilerplate and possible friction during refactors; the team must manage test exceptions and review them as part of PRs.
- Operational: Tests must be maintained alongside refactors to avoid brittleness and false positives.

## Test Enforcement / Exception Process

- Architecture tests run as part of the test suite. Intentional, documented deviations must follow a small process: open an RFC PR describing the need, add an architectural exception comment in the test (with a ticket link), and include a sunset plan for removing the exception.

## Migration / Evolution

- If the team decides to migrate to Hexagonal/Ports-and-Adapters, perform an incremental migration: identify a single adapter (e.g., repository) to convert, add adapter interfaces, wire integration tests, and update architecture tests to accept the new layered boundaries.

## Known Risks & Mitigations

- False positives during refactor: keep tests focused on meaningful violations and allow temporary exceptions via the exception process above.
- Maintenance overhead: schedule periodic architecture test reviews during major refactors and assign ownership.

## Governance / Owner

- Owner: architecture lead (maintains `tests/FairSplit.ArchitectureTests` rules and approves exceptions). Document rule changes in this ADR and in `docs/architecture-guidelines.md`.

