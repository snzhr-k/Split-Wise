# FairSplit Backend (ASP.NET Core + PostgreSQL)

This folder contains the FairSplit backend implementation.

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

## Authentication

FairSplit now uses JWT bearer authentication for protected endpoints such as expense creation and balances.

- Dev token endpoint: `POST /api/auth/dev-token`
- Protected resources require `Authorization: Bearer <token>`
- JWT settings are in `src/FairSplit.Api/appsettings.json`

## API Coverage (May 2026)

Implemented endpoints include:

- Auth: `POST /api/auth/dev-token`
- Groups: `GET /api/groups`, `GET /api/groups/{groupId}`, `POST /api/groups`, `POST /api/groups/{groupId}/join`
- Members: `GET /api/groups/{groupId}/members`
- Expenses: `POST /api/groups/{groupId}/expenses`, `GET /api/groups/{groupId}/expenses`, `GET /api/groups/{groupId}/expenses/{expenseId}`
- Settlements: `POST /api/groups/{groupId}/settlements`, `GET /api/groups/{groupId}/settlements`, `GET /api/groups/{groupId}/settlements/{settlementId}`
- Balances: `GET /api/groups/{groupId}/balances`, `GET /api/groups/{groupId}/balances/{memberId}`

There are no active public API routes returning scaffold `501 Not Implemented` responses.

Public endpoint quick verification:

```bash
curl -sS -i http://localhost:5001/api/groups/11111111-1111-1111-1111-111111111111/members
```

Example dev-token request:

```bash
curl -sS -X POST http://localhost:5001/api/auth/dev-token \
	-H 'Content-Type: application/json' \
	--data-binary '{"memberId":"22222222-2222-2222-2222-222222222222","displayName":"Alice"}'
```

Use the returned `accessToken` in requests to protected endpoints.

Protected endpoint quick verification:

```bash
curl -sS -i http://localhost:5001/api/groups/11111111-1111-1111-1111-111111111111/balances \
	-H 'Authorization: Bearer <accessToken>'
```

## How To Test Current Working API

1. Start PostgreSQL and verify it is reachable:

```bash
brew services start postgresql@16
pg_isready -h localhost -p 5432
```

2. Run the API (use a free port; `5001` was used during current verification):

```bash
cd src/FairSplit.Api
ASPNETCORE_URLS=http://0.0.0.0:5001 dotnet run
```

If you see `address already in use`, pick another port and update mobile `.env` accordingly.

3. Seed minimal test data (1 group, 2 members):

```bash
psql "host=localhost port=5432 dbname=fairsplit user=postgres password=postgres" -c "
BEGIN;
DELETE FROM \"ExpenseParticipants\" WHERE \"ExpenseId\" IN (SELECT \"Id\" FROM \"Expenses\" WHERE \"GroupId\"='11111111-1111-1111-1111-111111111111');
DELETE FROM \"Balances\" WHERE \"GroupId\"='11111111-1111-1111-1111-111111111111';
DELETE FROM \"Expenses\" WHERE \"GroupId\"='11111111-1111-1111-1111-111111111111';
DELETE FROM \"Members\" WHERE \"GroupId\"='11111111-1111-1111-1111-111111111111';
DELETE FROM \"Groups\" WHERE \"Id\"='11111111-1111-1111-1111-111111111111';
INSERT INTO \"Groups\" (\"Id\",\"Name\") VALUES ('11111111-1111-1111-1111-111111111111','Smoke Test Group');
INSERT INTO \"Members\" (\"Id\",\"GroupId\",\"DisplayName\") VALUES
('22222222-2222-2222-2222-222222222222','11111111-1111-1111-1111-111111111111','Alice'),
('33333333-3333-3333-3333-333333333333','11111111-1111-1111-1111-111111111111','Bob');
COMMIT;
"
```

4. Expense flow smoke-test:

```bash
# 1) List groups
curl -sS -i http://localhost:5001/api/groups

# 2) List group members (used by mobile Create Expense)
curl -sS -i http://localhost:5001/api/groups/11111111-1111-1111-1111-111111111111/members

# 3) Create a dev token (copy accessToken)
curl -sS -X POST http://localhost:5001/api/auth/dev-token \
	-H 'Content-Type: application/json' \
	--data-binary '{"memberId":"22222222-2222-2222-2222-222222222222","displayName":"Alice"}'

# 4) List expenses in seeded group (expected: empty first time)
curl -sS -i http://localhost:5001/api/groups/11111111-1111-1111-1111-111111111111/expenses \
	-H 'Authorization: Bearer <accessToken>'

# 5) Create expense
curl -sS -i -X POST http://localhost:5001/api/groups/11111111-1111-1111-1111-111111111111/expenses \
	-H 'Content-Type: application/json' \
	-H 'Authorization: Bearer <accessToken>' \
	--data-binary @- <<'JSON'
{"payerMemberId":"22222222-2222-2222-2222-222222222222","amount":120.50,"splitType":"equal","participants":[{"memberId":"22222222-2222-2222-2222-222222222222"},{"memberId":"33333333-3333-3333-3333-333333333333"}]}
JSON

# 6) Get expense by id (replace {expenseId} with the id from POST response)
curl -sS -i http://localhost:5001/api/groups/11111111-1111-1111-1111-111111111111/expenses/{expenseId} \
	-H 'Authorization: Bearer <accessToken>'
```

7. Settlement flow smoke-test:

```bash
# 7) Create settlement
curl -sS -i -X POST http://localhost:5001/api/groups/11111111-1111-1111-1111-111111111111/settlements \
	-H 'Content-Type: application/json' \
	-H 'Authorization: Bearer <accessToken>' \
	--data-binary '{"fromMemberId":"33333333-3333-3333-3333-333333333333","toMemberId":"22222222-2222-2222-2222-222222222222","amount":20.00}'

# 8) List settlements
curl -sS -i http://localhost:5001/api/groups/11111111-1111-1111-1111-111111111111/settlements \
	-H 'Authorization: Bearer <accessToken>'
```

## Architecture Enforcement

Layer rules are enforced by architecture tests in `tests/FairSplit.ArchitectureTests` using `NetArchTest.Rules`.

Run all tests (including architecture rules) from the backend root:

```bash
dotnet test FairSplit.slnx
```

If a developer introduces an invalid dependency (for example, a controller referencing a repository), the test suite fails and can block CI.

## Test Projects

Solution test coverage includes:

- `tests/FairSplit.ArchitectureTests` for layer dependency rules
- `tests/FairSplit.LayerIsolationTests` for targeted isolation checks
- `tests/FairSplit.IntegrationTests` for submission-critical API flows (happy path and auth/ownership negatives)
