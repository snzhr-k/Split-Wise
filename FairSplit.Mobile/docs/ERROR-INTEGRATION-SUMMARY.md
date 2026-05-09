# FairSplit Mobile Error Handling - Integration Summary

## What Changed

The mobile client now has a lightweight, centralized error handling system that:

1. **Parses structured backend errors** - All API responses are transformed into consistent `BackendError` objects
2. **Categorizes errors automatically** - Machine-readable codes map to user-facing actions
3. **Shows context-aware feedback** - Validation errors show inline, business errors show as toasts
4. **Provides field-level details** - Forms can display exactly which field failed and why
5. **Handles network gracefully** - Connection errors don't crash the app

---

## Before/After Examples

### Example 1: useExpenses Hook

**BEFORE:**
```typescript
export function useExpenses(groupId: string) {
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const loadExpenses = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const response = await getExpensesByGroupId(groupId);
      setExpenses(response);
    } catch {
      // Generic error message - no way to distinguish between error types
      setErrorMessage('Could not load expenses for this group. Check backend status and API URL.');
    } finally {
      setIsLoading(false);
    }
  }, [groupId]);

  return { expenses, isLoading, errorMessage, reload: loadExpenses };
}
```

**AFTER:**
```typescript
export function useExpenses(groupId: string) {
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<BackendError | null>(null);

  const loadExpenses = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await getExpensesByGroupId(groupId);
      setExpenses(response);
    } catch (err) {
      setError(err as BackendError);
      // User-appropriate toast shown automatically based on error category
      handleError(err, { skipToast: false });
    } finally {
      setIsLoading(false);
    }
  }, [groupId]);

  return {
    expenses,
    isLoading,
    error,
    errorMessage: error ? error.message : null, // backward compatible
    reload: loadExpenses,
    shouldNavigateBack: error ? shouldNavigateOnError(error) === 'back' : false,
  };
}
```

**Benefits:**
- ✅ Can check error code to distinguish GROUP_NOT_FOUND vs INTERNAL_ERROR
- ✅ Structured error automatically triggers appropriate handler
- ✅ Caller can add custom navigation logic based on error type
- ✅ Maintains backward compatibility with `errorMessage` property

---

### Example 2: useCreateExpense Hook

**BEFORE:**
```typescript
export function useCreateExpense(groupId: string) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const submit = async (request: CreateExpenseRequest): Promise<Expense | null> => {
    setIsSubmitting(true);
    setErrorMessage(null);

    try {
      return await createExpense(groupId, request);
    } catch (error) {
      const apiError = error as ApiError;
      // All errors shown the same way - no field-level details
      setErrorMessage(apiError.message || 'Could not create expense.');
      return null;
    } finally {
      setIsSubmitting(false);
    }
  };

  return { isSubmitting, errorMessage, submit };
}
```

**AFTER:**
```typescript
export function useCreateExpense(groupId: string) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<BackendError | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  const submit = async (request: CreateExpenseRequest): Promise<Expense | null> => {
    setIsSubmitting(true);
    setError(null);
    setFieldErrors({});

    try {
      return await createExpense(groupId, request);
    } catch (err) {
      if (isBackendError(err)) {
        setError(err);

        // Extract field-level errors for form display
        const category = categorizeErrorCode(err.code);
        if (category === ErrorCategory.VALIDATION) {
          // fieldErrors = { amount: "must be greater than 0", participants: "cannot contain duplicates" }
          setFieldErrors(extractValidationErrorsByField(err));
        }
      }

      // Appropriate handler based on error type
      handleError(err, { skipToast: false });
      return null;
    } finally {
      setIsSubmitting(false);
    }
  };

  return {
    isSubmitting,
    error,
    fieldErrors,
    errorMessage: error ? error.message : null, // backward compatible
    submit,
  };
}
```

**Benefits:**
- ✅ Validation errors extracted to `fieldErrors` map
- ✅ Forms show inline errors per field (not generic message)
- ✅ Different error categories handled appropriately
- ✅ Backward compatible `errorMessage` still available

---

### Example 3: CreateExpenseScreen Integration

**BEFORE:**
```tsx
function CreateExpenseScreen({ group, onBack }: CreateExpenseScreenProps) {
  const [validationError, setValidationError] = useState<string | null>(null);
  const { isSubmitting, errorMessage, submit } = useCreateExpense(group.id);

  return (
    <View>
      <TextInput
        value={amountInput}
        onChangeText={setAmountInput}
        placeholder="e.g. 120.50"
        keyboardType="decimal-pad"
        style={styles.input}
      />
      
      {validationError ? <Text style={styles.errorText}>{validationError}</Text> : null}
      {errorMessage ? <Text style={styles.errorText}>{errorMessage}</Text> : null}
      
      <Pressable onPress={handleSubmit}>
        <Text>Create expense</Text>
      </Pressable>
    </View>
  );
}
```

**AFTER:**
```tsx
function CreateExpenseScreen({ group, onBack }: CreateExpenseScreenProps) {
  const [validationError, setValidationError] = useState<string | null>(null);
  const { isSubmitting, errorMessage, fieldErrors, submit } = useCreateExpense(group.id);

  return (
    <View>
      <TextInput
        value={amountInput}
        onChangeText={setAmountInput}
        placeholder="e.g. 120.50"
        keyboardType="decimal-pad"
        // Field-specific error styling
        style={[styles.input, fieldErrors.amount ? styles.inputError : null]}
      />
      
      {/* Field-level error displays near the field */}
      {fieldErrors.amount ? (
        <Text style={styles.fieldErrorText}>{fieldErrors.amount}</Text>
      ) : null}
      
      {/* Global errors (business rules, not found, etc.) */}
      {errorMessage ? <Text style={styles.errorText}>{errorMessage}</Text> : null}
      
      <Pressable onPress={handleSubmit} disabled={!canSubmit}>
        <Text>Create expense</Text>
      </Pressable>
      
      {/* Toast container shows at app root - user sees it automatically */}
    </View>
  );
}
```

**Benefits:**
- ✅ Amount input border turns red if validation fails
- ✅ Error message appears directly under amount field
- ✅ User immediately knows what's wrong
- ✅ Global errors (e.g., "Member not in group") show elsewhere
- ✅ Toasts handle system-level feedback (loading, success, network errors)

---

### Example 4: API Error Parsing

**BEFORE:**
```typescript
type ApiError = {
  status: number;
  message: string;
};

async function buildApiError(response: Response): Promise<ApiError> {
  let message = `Request failed: ${response.status}`;
  try {
    const errorBody = await response.json();
    if (errorBody.message) {
      message = errorBody.message;
    }
  } catch {
    // Keep default
  }
  return { status: response.status, message };
}

export async function apiPost<TResponse, TBody>(path: string, body: TBody): Promise<TResponse> {
  const response = await fetch(`${apiConfig.baseUrl}${path}`, {
    method: 'POST',
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw await buildApiError(response); // Only status + message
  }

  return response.json();
}
```

**HTTP 400 Response:**
```json
{
  "message": "Expense split configuration is invalid."
}
```

Problem: No error code, no field details, no traceId

---

**AFTER:**
```typescript
type BackendError = {
  code: string;
  message: string;
  details: ValidationDetail[];
  traceId: string;
};

async function parseBackendError(response: Response): Promise<BackendError | NetworkError> {
  let errorBody: BackendErrorResponse | null = null;

  try {
    errorBody = await response.json();
  } catch {
    return {
      code: 'NETWORK_ERROR',
      message: `Request failed with status ${response.status}`,
      originalError: new Error(`HTTP ${response.status}`),
    };
  }

  // Structure as BackendError with all properties
  return {
    code: errorBody?.code || 'UNKNOWN_ERROR',
    message: errorBody?.message || `Request failed: ${response.status}`,
    details: errorBody?.details || [],
    traceId: errorBody?.traceId || 'unknown',
  };
}

export async function apiPost<TResponse, TBody>(path: string, body: TBody): Promise<TResponse> {
  try {
    const response = await fetch(`${apiConfig.baseUrl}${path}`, {
      method: 'POST',
      body: JSON.stringify(body),
    });

    if (!response.ok) {
      throw await parseBackendError(response);
    }

    return response.json();
  } catch (error) {
    if (typeof error === 'object' && error?.code) {
      throw error; // Already structured
    }
    
    // Network error
    throw {
      code: 'NETWORK_ERROR',
      message: 'Failed to connect to server.',
      originalError: error,
    };
  }
}
```

**HTTP 400 Response:**
```json
{
  "code": "INVALID_SPLIT_CONFIGURATION",
  "message": "Expense split configuration is invalid.",
  "details": [
    {
      "field": "amount",
      "issue": "must be greater than 0",
      "value": -50
    },
    {
      "field": "participants",
      "issue": "cannot contain duplicate members",
      "value": null
    }
  ],
  "traceId": "00-123abc456def-xyz"
}
```

Benefits:
- ✅ Error code tells caller what kind of error
- ✅ Details array enables form field mapping
- ✅ TraceId helps with debugging (show to user, reference in support ticket)
- ✅ Network errors handled separately from backend errors

---

## Architecture Changes

```
┌─────────────────────────────────────────────────────────────────┐
│ Component/Screen                                                │
│ (CreateExpenseScreen)                                           │
├─────────────────────────────────────────────────────────────────┤
│ Shows fieldErrors.amount inline                                 │
│ Shows error toast from toastService                             │
└────────────────┬────────────────────────────────────────────────┘
                 │ calls submit()
┌────────────────▼────────────────────────────────────────────────┐
│ Hook (useCreateExpense)                                         │
├─────────────────────────────────────────────────────────────────┤
│ 1. Catches error from API call                                  │
│ 2. Extracts fieldErrors to show inline                          │
│ 3. Calls handleError() for toast/navigation                     │
└────────────────┬────────────────────────────────────────────────┘
                 │ calls handleError()
┌────────────────▼────────────────────────────────────────────────┐
│ Error Handler (errorHandler.ts)                                 │
├─────────────────────────────────────────────────────────────────┤
│ 1. Categorizes error by code                                    │
│ 2. VALIDATION → extract fields, show warning                    │
│ 3. NOT_FOUND → show warning, trigger navigation                 │
│ 4. BUSINESS_RULE → show message as warning                      │
│ 5. CONFLICT → show message as warning                           │
│ 6. INTERNAL → show generic + traceId                            │
└────────────────┬────────────────────────────────────────────────┘
                 │ calls toastService
┌────────────────▼────────────────────────────────────────────────┐
│ Toast Service (toastService.ts)                                 │
├─────────────────────────────────────────────────────────────────┤
│ 1. Add toast to queue                                           │
│ 2. Notify all listeners                                         │
│ 3. Auto-dismiss after duration                                  │
└────────────────┬────────────────────────────────────────────────┘
                 │ emits update
┌────────────────▼────────────────────────────────────────────────┐
│ ToastContainer Component                                        │
├─────────────────────────────────────────────────────────────────┤
│ useToasts() hook subscribes                                     │
│ Renders all active toasts with animations                       │
└─────────────────────────────────────────────────────────────────┘
         │
         └─────── At app root, shows to user
```

---

## Error Code Mapping

Backend error codes automatically map to frontend behavior:

| Code | Category | Frontend Action |
|------|----------|-----------------|
| `VALIDATION_ERROR` | VALIDATION | Extract fields, show inline + warning toast |
| `INVALID_SPLIT_CONFIGURATION` | VALIDATION | Show "Please check the highlighted fields" |
| `INVALID_SPLIT_TYPE` | VALIDATION | Unlikely to reach frontend (preset in UI) |
| `INVALID_CUSTOM_SHARE` | VALIDATION | Show array indices in field errors |
| `INVALID_SPLIT_SUM` | VALIDATION | Show sum mismatch details |
| `GROUP_NOT_FOUND` | NOT_FOUND | "Group not found. It may have been deleted." + back |
| `EXPENSE_NOT_FOUND` | NOT_FOUND | "Expense not found. It may have been deleted." + back |
| `MEMBER_NOT_FOUND` | NOT_FOUND | "Member not found." + back |
| `MEMBER_NOT_IN_GROUP` | BUSINESS_RULE | "Not all participants belong to this group." |
| `BALANCE_CONFLICT` | BUSINESS_RULE | Show conflict message |
| `DUPLICATE_EXPENSE_PARTICIPANT` | CONFLICT | "Member already added to this expense" |
| `DUPLICATE_MEMBER` | CONFLICT | "This member is already in the group" |
| `INTERNAL_ERROR` | INTERNAL | "Something went wrong. [abc123]" |
| `DATA_ACCESS_ERROR` | INTERNAL | "Backend connection failed. [abc123]" |
| `NETWORK_ERROR` | NETWORK | "No connection. Check internet and retry." |

---

## File Structure

```
FairSplit.Mobile/
├── src/
│   ├── services/
│   │   ├── errorTypes.ts          [NEW] Error type definitions
│   │   ├── errorHandler.ts        [NEW] Central error handler
│   │   └── toastService.ts        [NEW] Event-driven toast system
│   ├── api/
│   │   ├── httpClient.ts          [UPDATED] Structured error parsing
│   │   └── types.ts               [UPDATED] Export error types
│   ├── app/
│   │   └── hooks/
│   │       └── useToasts.ts       [NEW] Toast subscription hook
│   ├── components/
│   │   └── common/
│   │       └── ToastContainer.tsx [NEW] Toast renderer component
│   └── features/
│       └── expenses/
│           ├── hooks/
│           │   ├── useExpenses.ts           [UPDATED] Error handling
│           │   └── useCreateExpense.ts     [UPDATED] Field errors
│           └── screens/
│               └── CreateExpenseScreen.tsx [UPDATED] Display inline errors
└── docs/
    └── ERROR-HANDLING.md          [NEW] Complete guide
```

---

## Next Steps

1. **Add ToastContainer to RootScreen**
   ```tsx
   import { ToastContainer } from '../components/common/ToastContainer';
   
   export function RootScreen() {
     return (
       <>
         <Navigation />
         <ToastContainer />
       </>
     );
   }
   ```

2. **Test Error Scenarios**
   - Create expense with negative amount → see inline error
   - Delete group, fetch expenses → see not-found toast + auto-back
   - Add member outside group → see business rule toast

3. **Extend to Other Features**
   - useGroupMembers hook
   - useBalance hook
   - useSettlements hook
   - Settings/preferences screens

---

## Summary

| Feature | Before | After |
|---------|--------|-------|
| Error Information | Status + message | Code + message + details + traceId |
| Validation Handling | Generic string | Field-level details per error |
| Form Feedback | Global error message | Inline errors per field |
| User Notifications | Basic error text | Type-aware toasts (error/warning/success) |
| Error Categories | None | Automatic categorization (5 types) |
| Resource Not Found | Generic message | Specific resource + navigation action |
| Network Errors | Crash or generic | Handled gracefully with reconnect message |
| Code Reusability | Local error handling | Centralized error handler |
| Maintainability | Scattered catch blocks | Single source of truth |

---

## Testing Checklist

- [ ] Validation error shows field-specific error inline
- [ ] Amount field border goes red on validation error
- [ ] Warning toast shows "Please check highlighted fields"
- [ ] Business rule error shows appropriate message
- [ ] Not found error shows resource name + navigation back
- [ ] Internal error shows generic message + traceId
- [ ] Network error suggests checking connection
- [ ] ToastContainer renders at app root
- [ ] Toast auto-dismisses after duration
- [ ] User can manually dismiss toast by tapping X
- [ ] Multiple toasts stack vertically
