# FairSplit Exception System - Migration Guide

## Overview

This guide helps migrate existing FairSplit service code to use the new custom exception system.

The goal is incremental adoption—update one service at a time without breaking changes.

## Step 1: Identify All Exceptions

First, audit your Service and Repository code for all exception throws:

```bash
cd FairSplit.Backend
grep -r "throw new" src/FairSplit.Api/Services/ --include="*.cs" | grep -v ".Tests"
grep -r "throw new" src/FairSplit.Api/Repositories/ --include="*.cs" | grep -v ".Tests"
```

Look for:
- Generic `throw new Exception(...)`
- Generic `throw new InvalidOperationException(...)`
- All business rule validations

## Step 2: Map to Exception Types

For each throw statement, determine the correct exception type:

| Old Pattern | New Exception | HTTP Status | Error Code |
|---|---|---|---|
| `throw new Exception("validation error")` | `ValidationException` | 400 | `VALIDATION_ERROR` |
| `throw new Exception("not found")` | `NotFoundException` | 404 | `RESOURCE_NOT_FOUND` |
| `throw new InvalidOperationException("business rule")` | `BusinessRuleViolationException` | 422 | `BUSINESS_RULE_VIOLATION` |
| `throw new Exception("duplicate key")` | `ConflictException` | 409 | `CONFLICT` |
| `throw new Exception("access denied")` | `ForbiddenOperationException` | 403 | `FORBIDDEN_OPERATION` |
| `throw new Exception("db error")` | `DataAccessException` | 503 | `DATA_ACCESS_ERROR` |
| Unexpected error | `InternalServerException` | 500 | `INTERNAL_ERROR` |

## Step 3: Update Service Methods

### Example: ExpenseService.CreateAsync

#### BEFORE

```csharp
public async Task<Expense> CreateAsync(CreateExpenseCommand command, CancellationToken cancellationToken)
{
    // Generic exceptions without codes
    if (command.Amount <= 0)
    {
        throw new Exception("amount must be greater than zero");
    }

    if (command.Participants.Count == 0)
    {
        throw new Exception("at least one participant is required");
    }

    var duplicateParticipants = command.Participants
        .GroupBy(participant => participant.MemberId)
        .Any(group => group.Count() > 1);

    if (duplicateParticipants)
    {
        throw new Exception("participants cannot contain duplicates");
    }

    var groupExists = await groupRepository.ExistsAsync(command.GroupId, cancellationToken);

    if (!groupExists)
    {
        throw new Exception($"Group '{command.GroupId}' was not found.");
    }

    var relatedMemberIds = command.Participants
        .Select(participant => participant.MemberId)
        .Append(command.PayerMemberId)
        .Distinct()
        .ToList();

    var members = await memberRepository.GetByIdsInGroupAsync(
        command.GroupId,
        relatedMemberIds,
        cancellationToken);

    var memberIdsInGroup = members.Select(member => member.Id).ToHashSet();

    foreach (var memberId in relatedMemberIds)
    {
        if (!memberIdsInGroup.Contains(memberId))
        {
            throw new Exception(
                $"Member '{memberId}' does not belong to group '{command.GroupId}'.");
        }
    }

    var sharesByMemberId = CalculateShares(command);

    // ... rest of method
}
```

#### AFTER

```csharp
public async Task<Expense> CreateAsync(CreateExpenseCommand command, CancellationToken cancellationToken)
{
    // Validation errors with specific error codes
    if (command.Amount <= 0)
    {
        throw new InvalidSplitException("total amount must be greater than zero", "INVALID_AMOUNT");
    }

    if (command.Participants.Count == 0)
    {
        throw new InvalidSplitException("at least one participant is required", "EMPTY_PARTICIPANTS");
    }

    var duplicateParticipants = command.Participants
        .GroupBy(participant => participant.MemberId)
        .Any(group => group.Count() > 1);

    if (duplicateParticipants)
    {
        throw new InvalidSplitException("participants cannot contain duplicates", "DUPLICATE_PARTICIPANTS");
    }

    // Resource not found with specific error code
    var groupExists = await groupRepository.ExistsAsync(command.GroupId, cancellationToken);

    if (!groupExists)
    {
        throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
    }

    var relatedMemberIds = command.Participants
        .Select(participant => participant.MemberId)
        .Append(command.PayerMemberId)
        .Distinct()
        .ToList();

    var members = await memberRepository.GetByIdsInGroupAsync(
        command.GroupId,
        relatedMemberIds,
        cancellationToken);

    var memberIdsInGroup = members.Select(member => member.Id).ToHashSet();

    // Domain scope violation with specific error code
    foreach (var memberId in relatedMemberIds)
    {
        if (!memberIdsInGroup.Contains(memberId))
        {
            throw new ForbiddenOperationException(
                "Member does not belong to this group.",
                "MEMBER_OUTSIDE_GROUP_FORBIDDEN");
        }
    }

    var sharesByMemberId = CalculateShares(command);

    // ... rest of method
}
```

### Key Changes:
1. ✅ Validation errors → `InvalidSplitException` with specific codes
2. ✅ Not found → `NotFoundException` with specific codes
3. ✅ Domain scope violations → `ForbiddenOperationException`
4. ✅ All throws include both human-readable message AND machine-readable error code

## Step 4: Update Helper Methods

### CalculateShares Example

#### BEFORE

```csharp
private static IReadOnlyDictionary<Guid, decimal> CalculateShares(CreateExpenseCommand command)
{
    return command.SplitType switch
    {
        ExpenseSplitType.Equal => CalculateEqualShares(command),
        ExpenseSplitType.Custom => CalculateCustomShares(command),
        _ => throw new Exception("split type is not supported")  // Generic!
    };
}

private static IReadOnlyDictionary<Guid, decimal> CalculateCustomShares(CreateExpenseCommand command)
{
    var shares = new Dictionary<Guid, decimal>();

    foreach (var participant in command.Participants)
    {
        if (participant.ShareAmount is null || participant.ShareAmount < 0)
        {
            throw new Exception("custom split requires non-negative amounts for all participants");
        }

        shares[participant.MemberId] = participant.ShareAmount.Value;
    }

    var totalCustomAmount = shares.Values.Sum();

    if (totalCustomAmount != command.Amount)
    {
        throw new Exception("custom split amounts must sum to total amount");
    }

    return shares;
}
```

#### AFTER

```csharp
private static IReadOnlyDictionary<Guid, decimal> CalculateShares(CreateExpenseCommand command)
{
    return command.SplitType switch
    {
        ExpenseSplitType.Equal => CalculateEqualShares(command),
        ExpenseSplitType.Custom => CalculateCustomShares(command),
        _ => throw new InvalidSplitException("split type is not supported", "INVALID_SPLIT_TYPE")
    };
}

private static IReadOnlyDictionary<Guid, decimal> CalculateCustomShares(CreateExpenseCommand command)
{
    var shares = new Dictionary<Guid, decimal>();

    foreach (var participant in command.Participants)
    {
        if (participant.ShareAmount is null || participant.ShareAmount < 0)
        {
            throw new InvalidSplitException(
                "custom split requires non-negative amounts for all participants",
                "INVALID_CUSTOM_SHARE");
        }

        shares[participant.MemberId] = participant.ShareAmount.Value;
    }

    var totalCustomAmount = shares.Values.Sum();

    if (totalCustomAmount != command.Amount)
    {
        throw new InvalidSplitException(
            "custom split amounts must sum to total amount",
            "INVALID_SPLIT_SUM");
    }

    return shares;
}
```

## Step 5: Update Repositories

Repositories should translate provider-specific exceptions to standard ones.

### Example: Repository Error Translation

#### BEFORE

```csharp
public async Task<Expense?> GetByIdAsync(Guid groupId, Guid expenseId, CancellationToken cancellationToken)
{
    // No error handling - exceptions bubble up raw from EF Core
    return await _context.Expenses
        .Where(e => e.GroupId == groupId && e.Id == expenseId)
        .FirstOrDefaultAsync(cancellationToken);
}

public async Task AddAsync(Expense expense, CancellationToken cancellationToken)
{
    // No error handling - raw DbUpdateException surfaces to client
    _context.Expenses.Add(expense);
    await _context.SaveChangesAsync(cancellationToken);
}
```

#### AFTER

```csharp
public async Task<Expense?> GetByIdAsync(Guid groupId, Guid expenseId, CancellationToken cancellationToken)
{
    try
    {
        return await _context.Expenses
            .Where(e => e.GroupId == groupId && e.Id == expenseId)
            .FirstOrDefaultAsync(cancellationToken);
    }
    catch (OperationCanceledException ex)
    {
        throw new DataAccessException(
            "Database query was cancelled or timed out.",
            "QUERY_TIMEOUT",
            ex);
    }
    catch (InvalidOperationException ex)
    {
        throw new DataAccessException(
            "Unexpected database error.",
            "DATABASE_ERROR",
            ex);
    }
}

public async Task AddAsync(Expense expense, CancellationToken cancellationToken)
{
    try
    {
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") ?? false)
    {
        throw new ConflictException(
            "Expense with this configuration already exists.",
            "DUPLICATE_EXPENSE",
            ex);
    }
    catch (DbUpdateException ex)
    {
        throw new DataAccessException(
            "Failed to persist expense to database.",
            "TRANSACTION_FAILED",
            ex);
    }
    catch (OperationCanceledException ex)
    {
        throw new DataAccessException(
            "Database operation was cancelled.",
            "QUERY_TIMEOUT",
            ex);
    }
}
```

## Step 6: Update Controllers (Minimal Changes)

Controllers rarely need updates since the middleware handles exception translation:

```csharp
[HttpPost("{groupId:guid}/expenses")]
public async Task<IActionResult> CreateExpense(
    Guid groupId,
    CreateExpenseRequest request,
    CancellationToken cancellationToken)
{
    // No change needed here!
    // ModelState validation is automatic
    // Service throws new exceptions automatically
    // Middleware catches them automatically

    var expense = await _expenseService.CreateAsync(
        new CreateExpenseCommand
        {
            GroupId = groupId,
            PayerMemberId = request.PayerMemberId,
            Amount = request.Amount,
            SplitType = Enum.Parse<ExpenseSplitType>(request.SplitType),
            Participants = request.Participants.Select(p => new ParticipantInput
            {
                MemberId = p.MemberId,
                ShareAmount = p.ShareAmount
            }).ToList()
        },
        cancellationToken);

    return CreatedAtAction(nameof(GetExpense), new { groupId, expenseId = expense.Id }, expense);
}
```

## Step 7: Add Tests

### Unit Test Example

```csharp
[Fact]
public async Task CreateExpense_WithInvalidAmount_ThrowsInvalidSplitException()
{
    var command = new CreateExpenseCommand
    {
        GroupId = Guid.NewGuid(),
        PayerMemberId = Guid.NewGuid(),
        Amount = -10,
        SplitType = ExpenseSplitType.Equal,
        Participants = new[] { new ParticipantInput { MemberId = Guid.NewGuid() } }
    };

    var exception = await Assert.ThrowsAsync<InvalidSplitException>(
        () => _expenseService.CreateAsync(command, CancellationToken.None)
    );

    Assert.Equal("INVALID_AMOUNT", exception.ErrorCode);
    Assert.Equal(400, exception.StatusCode);
    Assert.Contains("greater than zero", exception.Message);
}

[Fact]
public async Task CreateExpense_WithGroupNotFound_ThrowsNotFoundException()
{
    var nonExistentGroupId = Guid.Parse("00000000-0000-0000-0000-000000000000");
    var command = new CreateExpenseCommand
    {
        GroupId = nonExistentGroupId,
        PayerMemberId = Guid.NewGuid(),
        Amount = 100,
        SplitType = ExpenseSplitType.Equal,
        Participants = new[] { new ParticipantInput { MemberId = Guid.NewGuid() } }
    };

    var exception = await Assert.ThrowsAsync<NotFoundException>(
        () => _expenseService.CreateAsync(command, CancellationToken.None)
    );

    Assert.Equal("GROUP_NOT_FOUND", exception.ErrorCode);
    Assert.Equal(404, exception.StatusCode);
}
```

### Integration Test Example

```csharp
[Fact]
public async Task POST_CreateExpense_WithInvalidAmount_Returns400BadRequest()
{
    // Arrange
    var group = await _testDataBuilder.CreateGroupAsync();
    var request = new CreateExpenseRequest
    {
        Amount = -10,  // Invalid!
        PayerMemberId = Guid.NewGuid(),
        SplitType = "equal",
        Participants = new[] { new ParticipantInputDto { MemberId = Guid.NewGuid() } }
    };

    // Act
    var response = await _client.PostAsJsonAsync(
        $"/api/groups/{group.Id}/expenses",
        request
    );

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var errorContent = await response.Content.ReadAsAsync<ErrorResponse>();
    Assert.Equal("INVALID_AMOUNT", errorContent.Code);
    Assert.NotNull(errorContent.TraceId);
}
```

## Migration Priority

Migrate services in this order:

1. **High Priority** (core business logic)
   - `ExpenseService` - highest impact on API reliability
   - `GroupService` - affects group lifecycle errors
   - `BalanceService` - financial accuracy depends on this

2. **Medium Priority** (secondary features)
   - `MemberService`
   - `SettlementService`

3. **Low Priority** (future/stubs)
   - `ExpenseParticipantService` (if used independently)

## Verification Steps

After migrating each service:

1. ✅ Rebuild: `dotnet build FairSplit.slnx`
2. ✅ Run tests: `dotnet test FairSplit.slnx`
3. ✅ Start API: `dotnet run` from `src/FairSplit.Api`
4. ✅ Test endpoints with cURL or Postman
5. ✅ Verify error responses match expected format
6. ✅ Verify status codes are correct
7. ✅ Verify error codes match taxonomy

## Example Migration: GroupService

### Before

```csharp
public class GroupService
{
    public async Task<Group> GetByIdAsync(Guid groupId, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);
        if (group is null)
            throw new Exception($"Group '{groupId}' was not found.");
        return group;
    }
}
```

### After

```csharp
public class GroupService
{
    public async Task<Group> GetByIdAsync(Guid groupId, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);
        if (group is null)
            throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
        return group;
    }
}
```

That's it! The middleware automatically handles the rest.

## Common Mistakes to Avoid

❌ **DON'T**: Use generic `Exception` class
```csharp
throw new Exception("something went wrong");  // Bad!
```

✅ **DO**: Use specific exception classes
```csharp
throw new InvalidSplitException("Amount must be positive.", "INVALID_AMOUNT");
```

---

❌ **DON'T**: Forget to set error code
```csharp
throw new NotFoundException("Not found");  // Missing code!
```

✅ **DO**: Always include error code
```csharp
throw new NotFoundException("Group was not found.", "GROUP_NOT_FOUND");
```

---

❌ **DON'T**: Put HTTP status codes in error messages
```csharp
throw new ValidationException("400: Invalid amount");  // Wrong!
```

✅ **DO**: Let exception class handle status codes
```csharp
throw new ValidationException("Amount must be positive.", "INVALID_AMOUNT");
// StatusCode is 400 automatically
```

---

❌ **DON'T**: Catch exceptions you can't handle
```csharp
try {
    await _service.CreateAsync(command);
}
catch (Exception ex) {
    // Don't do this - let middleware handle it!
}
```

✅ **DO**: Only catch exceptions you specifically handle
```csharp
try {
    await _expenseRepository.AddAsync(expense);
}
catch (DbUpdateException ex) {
    // Repository can translate to standard exception
    throw new DataAccessException("Failed to save.", "TRANSACTION_FAILED", ex);
}
```
