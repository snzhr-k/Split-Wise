# FairSplit Backend (ASP.NET Core + PostgreSQL)

This folder contains the backend skeleton for FairSplit.

## Layer Roles

- `Controllers` (Presentation): HTTP-only layer. Validate incoming shape at API boundary, call services, map service results to responses.
- `Services` (Business Logic): All business rules, orchestration, and use-case flow.
- `Repositories` (Data Access): All persistence and query logic. No business decisions.

Supporting folders:

- `Domain`: Core domain entities.
- `Presentation/Models`: Request and response DTOs.
- `Infrastructure/Persistence`: PostgreSQL and EF Core wiring.
- `Shared`: Reusable helpers/utilities.

## Dependency Rule

- Controllers may depend on Services only.
- Services may depend on Repositories only.
- Repositories are the only layer that talks to the database.
- No layer may depend on a layer above it.
- No layer may skip a layer.
- Business logic should depend on abstractions (interfaces), not concrete data access classes.

## How To Add Features Safely

When adding a new feature:

1. Add/update DTOs in `Presentation/Models`.
2. Add service contract in `Services/Interfaces`.
3. Add service implementation in `Services/Implementations`.
4. Add repository contract in `Repositories/Interfaces`.
5. Add repository implementation in `Repositories/Implementations`.
6. Add/adjust controller endpoint to call the service.
7. Register new service/repository in DI (`Program.cs` or persistence DI extensions).

Rules for contributors:

- Keep controller methods thin.
- Never inject repositories directly into controllers.
- Never put SQL/EF query logic in services or controllers.
- Keep domain/business decisions in services.

## Run

From `src/FairSplit.Api`:

```bash
dotnet restore
dotnet run
```

## Architecture Enforcement

Layer rules are enforced by architecture tests in `tests/FairSplit.ArchitectureTests` using `NetArchTest.Rules`.

Run all tests (including architecture rules) from the backend root:

```bash
dotnet test FairSplit.slnx
```

If a developer introduces an invalid dependency (for example, a controller referencing a repository), the test suite fails and can block CI.
