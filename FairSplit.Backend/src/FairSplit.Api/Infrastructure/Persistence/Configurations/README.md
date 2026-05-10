# EF Core Entity Configurations

This folder contains the active EF Core `IEntityTypeConfiguration<T>` mappings used by `FairSplitDbContext`.

Current configuration files:

- `GroupConfiguration.cs`
- `MemberConfiguration.cs`
- `ExpenseConfiguration.cs`
- `ExpenseParticipantConfiguration.cs`
- `SettlementConfiguration.cs`
- `BalanceConfiguration.cs`

Conventions used here:

- Monetary fields use fixed precision (`numeric(18,2)`).
- Required indexes are defined explicitly for common query paths.
- Referential actions are explicit (`Cascade` or `Restrict`) per domain rule.
- Unique constraints are used where domain invariants require it (for example group-member balance uniqueness).
