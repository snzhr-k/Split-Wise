# FairSplit Mobile Error Handling Guide

## Overview

The mobile client implements a lightweight, centralized error handling system that parses structured backend error responses and provides consistent user feedback across the app.

**Key Principles:**
- No Redux, no complex state management
- Validation errors accessible to forms for inline display
- Different error categories handled differently
- Toast notifications for user feedback
- Network errors handled gracefully

---

## Error Categories

The system categorizes backend errors into 5 main types:

### 1. VALIDATION_ERROR
**Scenario:** User submitted invalid data (negative amount, empty fields, etc.)

**Frontend Behavior:**
- Don't show global toast
- Extract field-level details
- Forms display inline error messages near the field
- User can correct and retry

**Example Backend Response:**
```json
{
  "code": "INVALID_SPLIT_CONFIGURATION",
  "message": "Expense split configuration is invalid.",
  "details": [
    {
      "field": "amount",
      "issue": "must be greater than 0",
      "value": 0
    },
    {
      "field": "participants",
      "issue": "cannot contain duplicate members",
      "value": null
    }
  ],
  "traceId": "00-abc123def456-xyz"
}
```

**Frontend Usage:**
```tsx
const { fieldErrors } = useCreateExpense(groupId);

// fieldErrors.amount = "must be greater than 0"
// fieldErrors.participants = "cannot contain duplicate members"

<TextInput
  style={[styles.input, fieldErrors.amount ? styles.inputError : null]}
/>
{fieldErrors.amount ? <Text>{fieldErrors.amount}</Text> : null}
```

---

### 2. NOT_FOUND
**Scenario:** Resource doesn't exist (group deleted, expense removed by admin)

**Frontend Behavior:**
- Show user-friendly toast warning
- Suggest navigation action (usually back/pop)
- Example: "Expense not found. It may have been deleted."

**Example Backend Response:**
```json
{
  "code": "EXPENSE_NOT_FOUND",
  "message": "Expense was not found in this group.",
  "details": [],
  "traceId": "00-abc123def456-xyz"
}
```

---

### 3. BUSINESS_RULE_VIOLATION
**Scenario:** User action violates business logic

**Examples:**
- `MEMBER_NOT_IN_GROUP` - Selected member doesn't belong to the group
- `INVALID_SPLIT_SUM` - Custom splits don't sum to total amount
- `BALANCE_CONFLICT` - Balance calculation failed

**Frontend Behavior:**
- Show backend message directly (it's user-facing)
- Example: "Not all participants belong to this group."

---

### 4. CONFLICT
**Scenario:** State/uniqueness constraint violation

**Examples:**
- `DUPLICATE_EXPENSE_PARTICIPANT` - Same member listed twice
- `DUPLICATE_MEMBER` - Already a member of the group

**Frontend Behavior:**
- Show backend message
- Example: "This member is already in the group."

---

### 5. INTERNAL_ERROR
**Scenario:** Backend server error (database failure, unexpected exception)

**Frontend Behavior:**
- Show generic safe message
- Include trace ID for support debugging
- Example: "Something went wrong. Please try again or contact support. [abc123de]"

---

## Architecture

### Layer 1: API Transport (`src/api/httpClient.ts`)

```typescript
// Converts raw HTTP responses to structured errors
async function parseBackendError(response: Response): Promise<BackendError>

// Wraps fetch with error parsing
export async function apiPost<T, B>(path: string, body: B): Promise<T>
```

**Key Point:** All network requests now return structured `BackendError` objects with:
- `code` - Machine-readable error code (e.g., "INVALID_SPLIT_CONFIGURATION")
- `message` - Human-readable message
- `details` - Field-level validation errors
- `traceId` - Server-side request ID for debugging

### Layer 2: Error Classification (`src/services/errorTypes.ts`)

```typescript
export enum ErrorCategory {
  VALIDATION = 'VALIDATION',
  NOT_FOUND = 'NOT_FOUND',
  BUSINESS_RULE = 'BUSINESS_RULE',
  CONFLICT = 'CONFLICT',
  INTERNAL = 'INTERNAL',
  NETWORK = 'NETWORK',
  UNKNOWN = 'UNKNOWN',
}

export function categorizeErrorCode(code: string): ErrorCategory
```

**Key Point:** Error codes map to categories based on patterns:
- `INVALID_*`, `VALIDATION_*` → VALIDATION
- `*_NOT_FOUND` → NOT_FOUND
- `MEMBER_NOT_IN_GROUP`, `BALANCE_CONFLICT` → BUSINESS_RULE
- `DUPLICATE_*` → CONFLICT
- `INTERNAL_ERROR`, `DATA_ACCESS_ERROR` → INTERNAL

### Layer 3: Toast Service (`src/services/toastService.ts`)

```typescript
toastService.success(message, duration)
toastService.error(message, duration)
toastService.warning(message, duration)
toastService.info(message, duration)
```

**Key Point:** Simple event-driven system, no context providers:
1. Any component/hook can call `toastService.showError(...)`
2. ToastContainer subscribes to changes and renders
3. Auto-dismisses after duration

### Layer 4: Central Error Handler (`src/services/errorHandler.ts`)

```typescript
export function handleError(error: unknown, options?: {
  skipToast?: boolean;
  onNavigationRequired?: (reason) => void;
  onValidationError?: (error) => void;
}): void
```

**Decision Logic:**
- VALIDATION → Extract field errors, optionally show warning toast
- NOT_FOUND → Show warning toast, trigger navigation callback
- BUSINESS_RULE → Show backend message as warning
- CONFLICT → Show backend message as warning
- INTERNAL → Show generic error with traceId
- NETWORK → Show connection error

### Layer 5: Hooks & Components

```typescript
// Hook that provides error handling
const { error, fieldErrors, errorMessage, submit } = useCreateExpense(groupId);

// Component that renders toasts
<ToastContainer />

// Field-level error display (inline)
{fieldErrors.amount ? <Text>{fieldErrors.amount}</Text> : null}
```

---

## Usage Examples

### Example 1: Form with Inline Validation

```tsx
function CreateExpenseScreen() {
  const { fieldErrors, errorMessage, submit } = useCreateExpense(groupId);

  return (
    <>
      <TextInput
        value={amount}
        style={[styles.input, fieldErrors.amount ? styles.inputError : null]}
      />
      {fieldErrors.amount && (
        <Text style={styles.fieldError}>{fieldErrors.amount}</Text>
      )}

      {errorMessage && (
        <Text style={styles.globalError}>{errorMessage}</Text>
      )}
    </>
  );
}
```

**Flow:**
1. User enters "-50" amount
2. User submits form
3. Backend returns: `{ code: "INVALID_SPLIT_CONFIGURATION", details: [{field: "amount", issue: "must be greater than 0", value: -50}] }`
4. Hook extracts `fieldErrors.amount = "must be greater than 0"`
5. Input border highlights red, error text shows below field
6. Warning toast shows: "Please check the highlighted fields and try again."
7. User can edit amount and resubmit without leaving screen

### Example 2: Resource Not Found

```tsx
function ExpensesListScreen() {
  const { expenses, error, shouldNavigateBack } = useExpenses(groupId);

  React.useEffect(() => {
    if (shouldNavigateBack) {
      navigation.goBack();
    }
  }, [shouldNavigateBack]);

  return <FlatList data={expenses} />;
}
```

**Flow:**
1. Screen loads expenses for groupId "abc123"
2. Group was deleted by another user
3. Backend returns: `{ code: "GROUP_NOT_FOUND", message: "Group was not found." }`
4. Hook sets `shouldNavigateBack = true`
5. Toast shows: "Group not found. It may have been deleted."
6. Screen automatically pops after 4 seconds (or on toast tap)

### Example 3: Business Rule Violation

```tsx
// User tries to add member to group they're not part of
const { error } = useCreateExpense(groupId);

// Backend returns:
// { code: "MEMBER_NOT_IN_GROUP", message: "Not all participants belong to this group." }

// Handler:
// 1. Toast shows: "Not all participants belong to this group."
// 2. User understands they must select members from the group
// 3. User can go back and select correct members
```

---

## Integration Checklist

### Phase 1: Setup (Already Done)
- ✅ Created `src/services/errorTypes.ts` - Error type definitions
- ✅ Created `src/services/toastService.ts` - Toast system
- ✅ Created `src/services/errorHandler.ts` - Central error handler
- ✅ Updated `src/api/httpClient.ts` - Parse structured errors
- ✅ Created `src/components/common/ToastContainer.tsx` - Toast UI
- ✅ Created `src/app/hooks/useToasts.ts` - Toast hook

### Phase 2: Hook Integration (Already Done)
- ✅ Updated `useExpenses.ts` - Use error handler
- ✅ Updated `useCreateExpense.ts` - Extract field errors
- ✅ Updated `CreateExpenseScreen.tsx` - Display field errors

### Phase 3: App-Wide Setup (Next Step)
- Add `<ToastContainer />` to `RootScreen.tsx`

### Phase 4: Additional Features (Future)
- Implement `useGroupMembers.ts` with error handling
- Implement balance detail screens with error handling
- Add retry logic for network errors
- Add offline mode if desired

---

## API Error Code Reference

**Validation Errors:**
- `INVALID_SPLIT_CONFIGURATION` - Invalid expense setup
- `INVALID_SPLIT_TYPE` - Unknown split type (e.g., "custom" not supported)
- `INVALID_CUSTOM_SHARE` - Invalid participant share amounts
- `INVALID_SPLIT_SUM` - Custom shares don't sum to total
- `VALIDATION_ERROR` - Generic validation failure

**Not Found:**
- `GROUP_NOT_FOUND` - Group doesn't exist or was deleted
- `EXPENSE_NOT_FOUND` - Expense doesn't exist or was deleted
- `MEMBER_NOT_FOUND` - Member doesn't exist

**Business Rules:**
- `MEMBER_NOT_IN_GROUP` - Participant not in group
- `BALANCE_CONFLICT` - Balance calculation failed
- `BUSINESS_RULE_VIOLATION` - Generic business rule failure

**Conflicts:**
- `DUPLICATE_EXPENSE_PARTICIPANT` - Same member twice in expense
- `DUPLICATE_MEMBER` - Member already in group
- `CONFLICT` - Generic state conflict

**Internal:**
- `INTERNAL_ERROR` - Unexpected server error
- `DATA_ACCESS_ERROR` - Database or persistence failure

---

## Testing Error Scenarios

### Test 1: Validation Error (Create Expense with -50 amount)
```bash
curl -X POST http://localhost:5000/api/groups/{groupId}/expenses \
  -H 'Content-Type: application/json' \
  -d '{
    "amount": -50,
    "splitType": "equal",
    "payerMemberId": "{memberId}",
    "participants": [{"memberId": "{memberId}"}]
  }'

# Response:
# {
#   "code": "INVALID_SPLIT_CONFIGURATION",
#   "message": "Expense split configuration is invalid.",
#   "details": [{
#     "field": "amount",
#     "issue": "must be greater than 0",
#     "value": -50
#   }],
#   "traceId": "..."
# }
```

### Test 2: Not Found (Delete group, then fetch expenses)
```bash
# Group exists, fetch works
curl http://localhost:5000/api/groups/{groupId}/expenses

# Delete group via backend
# Fetch again - returns 404 with GROUP_NOT_FOUND code
```

### Test 3: Business Rule (Add member outside group)
```bash
curl -X POST http://localhost:5000/api/groups/{groupId}/expenses \
  -H 'Content-Type: application/json' \
  -d '{
    "amount": 100,
    "splitType": "equal",
    "payerMemberId": "{memberFromOtherGroup}",
    "participants": [{"memberId": "{memberFromOtherGroup}"}]
  }'

# Response:
# {
#   "code": "MEMBER_NOT_IN_GROUP",
#   "message": "Not all participants belong to this group.",
#   "details": [],
#   "traceId": "..."
# }
```

---

## Troubleshooting

**Q: Toast doesn't show after error**
- Check: Is `<ToastContainer />` rendered in RootScreen?
- Check: Is error handler called with `skipToast: false`?

**Q: Field errors don't show inline**
- Check: Is hook returning `fieldErrors` object?
- Check: Is screen displaying `fieldErrors.fieldName` in JSX?
- Check: Is error category VALIDATION? (Only validation shows inline)

**Q: How to add custom toast?**
```tsx
import { toastService } from '../services/toastService';

toastService.success('Expense created!', 2000);
toastService.error('Something failed', 5000);
```

**Q: How to skip automatic toast but handle manually?**
```tsx
handleError(err, { 
  skipToast: true,
  onValidationError: (error) => {
    // Custom handling
  }
});
```

---

## Future Enhancements

1. **Retry Logic** - Auto-retry network errors with exponential backoff
2. **Offline Mode** - Queue requests, sync when connection restored
3. **Error Analytics** - Track most common errors for improvement
4. **Haptic Feedback** - Vibrate phone on error notification
5. **Debug Mode** - Toggle to show full error details including traceId and stack
