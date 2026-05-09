# Backend Validation Error Handling - Complete Summary

This document summarizes the complete validation error handling architecture for FairSplit React Native forms.

---

## The Big Picture

When a user submits a form and the backend returns validation errors:

```
Backend Error Response
  {
    "code": "VALIDATION_ERROR",
    "details": [
      {"field": "amount", "issue": "must be > 0"},
      {"field": "participants", "issue": "cannot be empty"}
    ]
  }
  ↓
API Client parses to BackendError
  ↓
Hook catches and extracts fieldErrors
  {
    amount: "must be > 0",
    participants: "cannot be empty"
  }
  ↓
Component renders with errors inline
  - Amount: red border + error text below
  - Participants: red border + error text below
  ↓
User edits field → error clears immediately
  ↓
User resubmits
```

---

## The Architecture

### Layer 1: API Transport
**File:** `src/api/httpClient.ts`

Already implemented. Returns structured `BackendError` with:
- `code`: Machine-readable (e.g., "VALIDATION_ERROR")
- `message`: Human-readable
- `details`: Array of `{field, issue, value}`
- `traceId`: Server-side request ID

### Layer 2: Error Classification
**File:** `src/services/errorTypes.ts`

Already implemented. `categorizeErrorCode()` maps codes to categories:
- VALIDATION → field-level errors
- NOT_FOUND → resource gone
- BUSINESS_RULE → business logic failure
- CONFLICT → state conflict
- INTERNAL → server error
- NETWORK → connection failed

### Layer 3: Utility Functions
**File:** `src/services/errorHandler.ts`

```typescript
/**
 * Extract field-level errors from backend validation error.
 * Converts error.details[] → Record<fieldName, errorMessage>
 */
export function extractFieldErrors(
  backendError: BackendError
): Record<string, string> {
  const fieldErrors: Record<string, string> = {};
  
  for (const detail of backendError.details) {
    if (!fieldErrors[detail.field]) {
      fieldErrors[detail.field] = detail.issue;
    }
  }
  
  return fieldErrors;
}
```

### Layer 4: API Hook
**File:** `src/features/expenses/hooks/useCreateExpense.ts`

```typescript
export function useCreateExpense(groupId: string) {
  const [error, setError] = useState<BackendError | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  const submit = async (request) => {
    try {
      return await createExpense(groupId, request);
    } catch (err) {
      if (isBackendError(err)) {
        setError(err);
        
        const category = categorizeErrorCode(err.code);
        if (category === ErrorCategory.VALIDATION) {
          // Extract details → field map
          setFieldErrors(extractFieldErrors(err));
        }
      }
      
      handleError(err);  // Show toast
      return null;
    }
  };

  return {
    error,
    fieldErrors,              // ← Ready for form display
    errorMessage: error?.message,
    submit,
  };
}
```

### Layer 5: Form Component
**File:** `src/features/expenses/screens/CreateExpenseScreen.tsx`

```typescript
export function CreateExpenseScreen() {
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [formError, setFormError] = useState<string | null>(null);
  const { fieldErrors: hooksFieldErrors, error: apiError } = useCreateExpense(groupId);

  // Extract backend errors
  useEffect(() => {
    if (!apiError) return;

    const category = categorizeErrorCode(apiError.code);

    if (category === ErrorCategory.VALIDATION) {
      // Map to state for display
      setFieldErrors(extractFieldErrors(apiError));
      setFormError(null);
    } else {
      // Non-validation errors: form-level
      if (apiError.details.length === 0) {
        setFormError(apiError.message);
      }
      setFieldErrors({});
    }
  }, [apiError]);

  // Clear error when user edits
  const handleAmountChange = (text: string) => {
    setAmountInput(text);
    
    if (fieldErrors.amount) {
      setFieldErrors(prev => {
        const updated = { ...prev };
        delete updated.amount;
        return updated;
      });
    }
  };

  return (
    <View>
      {/* Form-level error */}
      {formError && <Text style={styles.formError}>{formError}</Text>}

      {/* Field with error styling */}
      <TextInput
        style={[
          styles.input,
          fieldErrors.amount && styles.inputError,  // Red border
        ]}
        onChangeText={handleAmountChange}
      />
      {fieldErrors.amount && (
        <Text style={styles.fieldError}>{fieldErrors.amount}</Text>
      )}

      <Pressable onPress={handleSubmit}>
        <Text>Submit</Text>
      </Pressable>
    </View>
  );
}
```

---

## Key Concepts

### 1. Field Errors Map
```typescript
type FieldErrors = Record<string, string>;

// Example from backend validation error
fieldErrors = {
  amount: "Amount must be greater than zero.",
  participants: "Participants array cannot be empty.",
  payerMemberId: "Payer must belong to the group."
}
```

**Usage:**
```typescript
fieldErrors.amount      // "Amount must be..."
fieldErrors.unknown     // undefined (no error)
```

### 2. Error Extraction
```typescript
// Input: Backend error response
{
  "details": [
    {"field": "amount", "issue": "must be > 0"}
  ]
}

// Process: extractFieldErrors()
for (const detail of error.details) {
  if (!map[detail.field]) {
    map[detail.field] = detail.issue;  // First error wins
  }
}

// Output: Extracted map
{
  amount: "must be > 0"
}
```

### 3. Error Clearing on Edit
```typescript
const handleChange = (text: string) => {
  // Update value
  setValue(text);
  
  // Clear error for this field
  setFieldErrors(prev => {
    const updated = { ...prev };
    delete updated.fieldName;  // Remove from map
    return updated;
  });
};
```

### 4. Fallback for Missing Details
```typescript
if (isBackendError(error)) {
  const category = categorizeErrorCode(error.code);
  
  if (category === ErrorCategory.VALIDATION) {
    if (error.details.length > 0) {
      // Show field-level errors
      setFieldErrors(extractFieldErrors(error));
    } else {
      // No details: show as form-level error
      setFormError(error.message);
    }
  }
}
```

---

## Example Scenarios

### Scenario 1: Multiple Field Errors

**User input:**
```
Amount: -50
Participants: [] (none selected)
```

**Backend response:**
```json
{
  "code": "VALIDATION_ERROR",
  "details": [
    {
      "field": "amount",
      "issue": "Amount must be greater than zero.",
      "value": -50
    },
    {
      "field": "participants",
      "issue": "Participants array cannot be empty.",
      "value": null
    }
  ]
}
```

**Frontend result:**
- Amount input: red border + error text " Amount must be greater than zero."
- Participants chips: red border + error text "Participants array cannot be empty."
- Warning toast: "Please check the highlighted fields"

### Scenario 2: Single Field Error

**User input:**
```
Amount: -50
Participants: [Member 1, Member 2]  (selected)
Payer: Member 1  (selected)
```

**Backend response:**
```json
{
  "code": "VALIDATION_ERROR",
  "details": [
    {
      "field": "amount",
      "issue": "Amount must be greater than zero."
    }
  ]
}
```

**Frontend result:**
- Amount input: red border + error text
- Participants: normal (no error)
- Warning toast shown

### Scenario 3: Validation Error with No Details

**Backend response:**
```json
{
  "code": "VALIDATION_ERROR",
  "details": [],
  "message": "Expense configuration is invalid"
}
```

**Frontend result:**
- No field errors displayed
- Form-level error box appears: "Expense configuration is invalid"
- Warning toast shown

### Scenario 4: Business Rule Error (Different Code)

**Backend response:**
```json
{
  "code": "MEMBER_NOT_IN_GROUP",
  "message": "Not all participants belong to this group.",
  "details": []
}
```

**Frontend result:**
- categorizeErrorCode() → BUSINESS_RULE (not VALIDATION)
- No field-level extraction
- Form-level error: "Not all participants belong to this group."
- Warning toast shown (from error handler)

---

## Visual Representation

```
┌─────────────────────────────────────────────────────────┐
│ CreateExpenseScreen State                               │
├─────────────────────────────────────────────────────────┤
│ amountInput: "-50"                                      │
│ payerMemberId: "member-1"                               │
│ selectedParticipantIds: []                              │
│                                                         │
│ fieldErrors: {                                          │
│   amount: "must be > 0",                                │
│   participants: "cannot be empty"                       │
│ }                                                       │
│ formError: null                                         │
└─────────────────────────────────────────────────────────┘
        ↓
┌─────────────────────────────────────────────────────────┐
│ Render                                                  │
├─────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────┐ │
│ │ Amount                                              │ │
│ │ ┌─────────────────────────────────────────────────┐ │ │
│ │ │-50                         ← red border         │ │ │
│ │ └─────────────────────────────────────────────────┘ │ │
│ │ ❌ must be > 0                                      │ │
│ │                                                     │ │
│ │ Participants                                        │ │
│ │ ┌─────────────────────────────────────────────────┐ │ │
│ │ │ (empty)                    ← red border         │ │ │
│ │ └─────────────────────────────────────────────────┘ │ │
│ │ ❌ cannot be empty                                  │ │
│ │                                                     │ │
│ │ [Submit]                                            │ │
│ └─────────────────────────────────────────────────────┘ │
│                                                         │
│ 🟡 Warning Toast:                                       │
│    "Please check the highlighted fields"               │
└─────────────────────────────────────────────────────────┘
        ↓
User edits amount: "50"
        ↓
handleAmountChange()
  - setAmountInput("50")
  - setFieldErrors removes "amount" key
        ↓
┌─────────────────────────────────────────────────────────┐
│ New State                                               │
├─────────────────────────────────────────────────────────┤
│ fieldErrors: {                                          │
│   participants: "cannot be empty"  ← "amount" gone     │
│ }                                                       │
└─────────────────────────────────────────────────────────┘
        ↓
┌─────────────────────────────────────────────────────────┐
│ Re-render                                               │
├─────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────┐ │
│ │ Amount                                              │ │
│ │ ┌─────────────────────────────────────────────────┐ │ │
│ │ │50                          ← normal border      │ │ │
│ │ └─────────────────────────────────────────────────┘ │ │
│ │ (no error text)                                     │ │
│ │                                                     │ │
│ │ Participants                                        │ │
│ │ ┌─────────────────────────────────────────────────┐ │ │
│ │ │ (empty)                    ← red border         │ │ │
│ │ └─────────────────────────────────────────────────┘ │ │
│ │ ❌ cannot be empty                                  │ │
│ │                                                     │ │
│ │ [Submit]                                            │ │
│ └─────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

---

## Implementation Checklist

### Phase 1: Core Utilities
- [x] `src/services/errorHandler.ts` - `extractFieldErrors()` created
- [x] `src/services/errorTypes.ts` - ErrorCategory enum exists
- [x] `src/api/httpClient.ts` - Structured error parsing

### Phase 2: Hooks
- [x] `src/features/expenses/hooks/useCreateExpense.ts` - Returns `fieldErrors`

### Phase 3: Components (To Do)
- [ ] `src/features/expenses/screens/CreateExpenseScreen.tsx` - Add validation state
- [ ] useEffect to extract backend errors
- [ ] Field change handlers to clear errors
- [ ] JSX updates to show inline errors
- [ ] Style definitions for error states

### Phase 4: Testing
- [ ] Test scenario 1: Multiple errors
- [ ] Test scenario 2: Single error
- [ ] Test scenario 3: No details (form-level)
- [ ] Test scenario 4: Business rule error
- [ ] Test error clearing on edit

### Phase 5: Reusability (Future)
- [ ] Apply pattern to CreateGroupScreen
- [ ] Apply pattern to CreateSettlementScreen
- [ ] Apply pattern to EditExpenseScreen
- [ ] Generic `useFormValidation()` hook

---

## Key Files

| File | Purpose |
|------|---------|
| `src/services/errorTypes.ts` | Error categorization |
| `src/services/errorHandler.ts` | Field extraction utility |
| `src/api/httpClient.ts` | Structured error parsing |
| `src/features/expenses/hooks/useCreateExpense.ts` | API hook with error extraction |
| `src/features/expenses/screens/CreateExpenseScreen.tsx` | Form component with error display |
| `docs/FORM-VALIDATION-PATTERN.md` | Architecture guide |
| `docs/CREATE-EXPENSE-FORM-EXAMPLE.md` | Complete code example |
| `docs/FORM-VALIDATION-IMPLEMENTATION.md` | Implementation steps |

---

## Quick Reference: Backend Error to Frontend Display

```typescript
// Backend returns
{
  "code": "VALIDATION_ERROR",
  "details": [
    {"field": "amount", "issue": "must be > 0"}
  ]
}

// Hook receives and extracts
fieldErrors = {amount: "must be > 0"}

// Component displays
<TextInput
  style={fieldErrors.amount && styles.inputError}  // Red border
/>
{fieldErrors.amount && <Text>{fieldErrors.amount}</Text>}  // Error text

// User edits
<TextInput onChangeText={(text) => {
  setValue(text);
  setFieldErrors(prev => {
    const updated = {...prev};
    delete updated.amount;  // Clear error
    return updated;
  });
}} />

// Result: Error cleared immediately
```

---

## Next Steps

1. Read `docs/FORM-VALIDATION-PATTERN.md` for architecture overview
2. Read `docs/CREATE-EXPENSE-FORM-EXAMPLE.md` for complete example
3. Follow `docs/FORM-VALIDATION-IMPLEMENTATION.md` for step-by-step implementation
4. Implement in CreateExpenseScreen
5. Test all 4 scenarios
6. Reapply pattern to other forms

Done! Now your forms handle backend validation errors like a professional app.
