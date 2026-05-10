ADR-0001: Service-level Transactions and Unit-of-Work via ITransactionManager

Status: Accepted

Date: 2026-05-10

Context

- The backend uses EF Core with a shared `FairSplitDbContext` (`Infrastructure/Persistence/FairSplitDbContext.cs`).
- Business operations (for example, creating an `Expense`, related `ExpenseParticipants`, and applying `Balance` deltas) span multiple repositories and require atomic commits. See `Services/Implementations/ExpenseService.cs` for example usage.
- Repositories add entities to the `DbContext` but do not call `SaveChanges`; transactional commit is centralized.

Decision

All multi-repository mutating business operations will be executed within a service-level transaction boundary provided by an `ITransactionManager` abstraction. Implementations of `ITransactionManager` will:

- Begin a DB transaction on the `DbContext` (e.g., `dbContext.Database.BeginTransactionAsync`).
- Execute the provided operation delegate.
- Call `dbContext.SaveChangesAsync()` once at the end of the operation.
- Commit if successful or roll back on failure.

Repositories must not call `SaveChanges` directly; services coordinating domain operations are responsible for invoking the transaction manager.

Consequences

- Positive: Atomic, consistent business operations across multiple repositories; reduced risk of partial updates; simpler rollback semantics.
- Positive: Centralized persistence lifecycle makes testing and mocking transaction boundaries easier (`ITransactionManager` can be mocked in layer-isolation tests).
- Negative: Developer discipline required — forgetting to wrap operations in the transaction manager causes correctness bugs.
- Negative: Nested or distributed transactions are not supported by this simple model; supporting them later requires extending the abstraction (savepoints, ambient transactions, or adopting sagas).
- Operational: Long-running transactions can hold locks; guidelines are needed to limit transaction duration and operation size.

## Migration / Rollout

- For existing code that performs mutating operations outside the transaction manager, incrementally refactor service methods to call `ExecuteInTransactionAsync`. Add tests that assert `TransactionManager` is invoked (layer-isolation tests already mock `ITransactionManager`).
- If nested transactions are needed later, extend `ITransactionManager` to support savepoints or provide a composable API (e.g., `BeginSavepointAsync` / `ReleaseSavepointAsync`). For cross-service flows consider a saga pattern.

## Known Risks & Mitigations

- Developer omission: forgetting to use the transaction manager causes inconsistency. Mitigation: add a static analysis rule or code review checklist and include a unit/integration test pattern that verifies repository changes are committed only via the transaction manager.
- Long-running transactions: large operations may hold DB locks. Mitigation: enforce per-operation size/time guidelines and split large batch jobs into background processes.
- Testing complexity for nested transactions: start with simple cases and add savepoint support only when necessary.

## Example (correct usage)

```csharp
await transactionManager.ExecuteInTransactionAsync(async ct =>
{
	await expenseRepository.AddAsync(expense, ct);
	await expenseParticipantRepository.AddRangeAsync(participants, ct);
	await balanceRepository.ApplyDeltasAsync(groupId, deltas, ct);
}, cancellationToken);
```

## Governance / Owner

- Owner: backend lead (update this ADR when the transaction model changes).
- Tests: `tests/FairSplit.LayerIsolationTests` should include checks that critical mutating flows use `ITransactionManager`.

