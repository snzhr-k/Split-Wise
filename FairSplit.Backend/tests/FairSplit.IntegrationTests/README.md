# FairSplit Integration Tests

This project validates submission-critical API flows against an in-memory host:

- Auth negative paths (`401` for missing/invalid bearer token)
- Group create/read/join and members list flow
- Expense create/list/get and resulting balances
- Settlement create/list/get and balance adjustments
- Negative ownership checks (`403` when members are outside the target group)

Run with:

```bash
dotnet test FairSplit.slnx
```
