# Form Validation Implementation Guide

Complete step-by-step guide to add validation error handling to CreateExpenseScreen.

---

## Step 1: Create Utility Function

**File:** `src/services/errorHandler.ts` (add this export)

```typescript
/**
 * Extract field-level errors from backend validation error.
 * 
 * Maps error.details array to Record<fieldName, errorMessage>.
 * If multiple errors on same field, first one wins.
 * 
 * Usage:
 *   const errors = extractFieldErrors(backendError);
 *   setFieldErrors(errors);
 */
export function extractFieldErrors(
  backendError: BackendError
): Record<string, string> {
  const fieldErrors: Record<string, string> = {};

  for (const detail of backendError.details) {
    // First error for each field wins
    if (!fieldErrors[detail.field]) {
      fieldErrors[detail.field] = detail.issue;
    }
  }

  return fieldErrors;
}
```

---

## Step 2: Update useCreateExpense Hook

**File:** `src/features/expenses/hooks/useCreateExpense.ts`

Current hook already returns `fieldErrors`, but let's verify it has the field extraction:

```typescript
import { useState } from 'react';

import { createExpense } from '../api/expensesApi';
import type { CreateExpenseRequest, Expense } from '../types';
import { handleError, extractFieldErrors } from '../../../services/errorHandler';
import { isBackendError, ErrorCategory, categorizeErrorCode } from '../../../services/errorTypes';
import type { BackendError } from '../../../services/errorTypes';

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

        // Extract field errors for form display
        const category = categorizeErrorCode(err.code);
        if (category === ErrorCategory.VALIDATION) {
          // This extracts the details array to Record<field, message>
          setFieldErrors(extractFieldErrors(err));
        }
      }

      // Central handler shows appropriate toast
      handleError(err, {
        skipToast: false,
      });

      return null;
    } finally {
      setIsSubmitting(false);
    }
  };

  return {
    isSubmitting,
    error,
    fieldErrors,           // ← Return extracted field errors
    errorMessage: error ? error.message : null,
    submit,
  };
}
```

---

## Step 3: Update CreateExpenseScreen

**File:** `src/features/expenses/screens/CreateExpenseScreen.tsx`

Replace the current implementation with the complete one from `CREATE-EXPENSE-FORM-EXAMPLE.md`.

**Key changes:**

### 3a. Add state for validation errors
```typescript
// Validation state
const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
const [formError, setFormError] = useState<string | null>(null);
```

### 3b. Add effect to extract backend errors
```typescript
import { isBackendError, categorizeErrorCode, ErrorCategory } from '../../../services/errorTypes';
import { extractFieldErrors } from '../../../services/errorHandler';

useEffect(() => {
  if (!apiError) return;

  if (!isBackendError(apiError)) return;

  const category = categorizeErrorCode(apiError.code);

  if (category === ErrorCategory.VALIDATION) {
    const errors = extractFieldErrors(apiError);
    setFieldErrors(errors);
    setFormError(null);
  } else {
    if (apiError.details.length === 0) {
      setFormError(apiError.message);
    }
    setFieldErrors({});
  }
}, [apiError]);
```

### 3c. Update field change handlers to clear errors
```typescript
const handleAmountChange = (text: string) => {
  setAmountInput(text);
  
  // Clear error for this field
  if (fieldErrors.amount) {
    setFieldErrors(prev => {
      const updated = { ...prev };
      delete updated.amount;
      return updated;
    });
  }
};

const handleToggleParticipant = (memberId: string) => {
  setSelectedParticipantIds(previous => {
    if (previous.includes(memberId)) {
      return previous.filter(id => id !== memberId);
    }
    return [...previous, memberId];
  });

  // Clear error when user edits
  if (fieldErrors.participants) {
    setFieldErrors(prev => {
      const updated = { ...prev };
      delete updated.participants;
      return updated;
    });
  }
};

const handleSelectPayer = (memberId: string) => {
  setPayerMemberId(memberId);

  if (!selectedParticipantIds.includes(memberId)) {
    setSelectedParticipantIds(previous => [...previous, memberId]);
  }

  // Clear error when user edits
  if (fieldErrors.payerMemberId) {
    setFieldErrors(prev => {
      const updated = { ...prev };
      delete updated.payerMemberId;
      return updated;
    });
  }
};
```

### 3d. Update submit handler
```typescript
const handleSubmit = async () => {
  // Clear previous errors
  setFieldErrors({});
  setFormError(null);

  // Submit to backend
  const createdExpense = await submit({
    payerMemberId: normalizedPayerId,
    amount: Number(amountInput),
    splitType: 'equal',
    participants: selectedParticipantIds.map(memberId => ({
      memberId,
    })),
  });

  // On success, navigate
  if (createdExpense) {
    onBack();
  }

  // On failure with validation errors,
  // they're extracted by the useEffect
  // and displayed inline
};
```

### 3e. Update JSX to display errors

Wrap amount input:
```typescript
<Text style={styles.label}>Amount</Text>
<TextInput
  value={amountInput}
  onChangeText={handleAmountChange}
  placeholder="e.g. 120.50"
  keyboardType="decimal-pad"
  style={[
    styles.input,
    fieldErrors.amount ? styles.inputError : null,  // Red if error
  ]}
/>
{fieldErrors.amount && (
  <Text style={styles.fieldErrorText}>{fieldErrors.amount}</Text>
)}
```

Wrap payer chips:
```typescript
<View
  style={[
    styles.chipsRow,
    fieldErrors.payerMemberId ? styles.chipsRowError : null,  // Red border
  ]}
>
  {members.map(member => (
    <Pressable
      key={member.id}
      onPress={() => handleSelectPayer(member.id)}
      style={[
        styles.chip,
        normalizedPayerId === member.id ? styles.chipSelected : null,
      ]}
    >
      <Text style={styles.chipText}>{member.displayName}</Text>
    </Pressable>
  ))}
</View>
{fieldErrors.payerMemberId && (
  <Text style={styles.fieldErrorText}>{fieldErrors.payerMemberId}</Text>
)}
```

Wrap participants chips:
```typescript
<View
  style={[
    styles.chipsRow,
    fieldErrors.participants ? styles.chipsRowError : null,
  ]}
>
  {members.map(member => (
    <Pressable
      key={member.id}
      onPress={() => handleToggleParticipant(member.id)}
      style={[
        styles.chip,
        selectedParticipantIds.includes(member.id) ? styles.chipSelected : null,
      ]}
    >
      <Text style={styles.chipText}>{member.displayName}</Text>
    </Pressable>
  ))}
</View>
{fieldErrors.participants && (
  <Text style={styles.fieldErrorText}>{fieldErrors.participants}</Text>
)}
```

Add form-level error above submit:
```typescript
{formError && (
  <View style={styles.formErrorContainer}>
    <Text style={styles.formErrorText}>{formError}</Text>
  </View>
)}
```

### 3f. Add styles

```typescript
const styles = StyleSheet.create({
  // ... existing styles ...
  
  // Form-level error
  formErrorContainer: {
    backgroundColor: `${theme.colors.danger}15`,
    borderRadius: theme.radius.md,
    borderLeftWidth: 4,
    borderLeftColor: theme.colors.danger,
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.sm,
  },
  formErrorText: {
    fontSize: theme.typography.fontSize.md,
    color: theme.colors.danger,
  },
  
  // Input with error
  inputError: {
    borderColor: theme.colors.danger,
    borderWidth: 2,
    backgroundColor: `${theme.colors.danger}08`,
  },
  
  // Field error text
  fieldErrorText: {
    fontSize: theme.typography.fontSize.sm,
    color: theme.colors.danger,
    marginTop: -theme.spacing.xs,
    marginBottom: theme.spacing.sm,
  },
  
  // Chips with error
  chipsRowError: {
    borderWidth: 2,
    borderColor: theme.colors.danger,
    backgroundColor: `${theme.colors.danger}08`,
  },
});
```

---

## Step 4: Test the Implementation

### Test 1: Backend returns validation errors
```typescript
// Backend returns:
{
  "code": "VALIDATION_ERROR",
  "message": "Expense data invalid",
  "details": [
    {"field": "amount", "issue": "must be > 0"},
    {"field": "participants", "issue": "cannot be empty"}
  ]
}

// Expected:
// - Amount input has red border
// - "must be > 0" shown below amount input
// - Participants chips have red border
// - "cannot be empty" shown below participants
// - Warning toast shows
```

### Test 2: Error clears on edit
```typescript
// User taps amount field and starts editing
// Expected:
// - Red border disappears immediately
// - Error text disappears
// - Can resubmit
```

### Test 3: No field details (generic error)
```typescript
// Backend returns:
{
  "code": "VALIDATION_ERROR",
  "details": [],
  "message": "Something went wrong"
}

// Expected:
// - No field-specific errors
// - Form-level error box appears above submit
// - Shows "Something went wrong"
```

### Test 4: Business rule error (not validation)
```typescript
// Backend returns:
{
  "code": "MEMBER_NOT_IN_GROUP",
  "details": [],
  "message": "Not all participants belong to this group"
}

// Expected:
// - No field-level errors
// - Form-level error shows
// - WARNING toast shows (not red highlight)
```

---

## Complete File Checklist

- [ ] `src/services/errorHandler.ts` - Add `extractFieldErrors()` function
- [ ] `src/features/expenses/hooks/useCreateExpense.ts` - Hook already has fieldErrors
- [ ] `src/features/expenses/screens/CreateExpenseScreen.tsx` - Update with full implementation
- [ ] All imports added (isBackendError, categorizeErrorCode, ErrorCategory, extractFieldErrors)

---

## How It Works: Complete Flow

```
1. User fills form and taps "Create"
   ↓
2. handleSubmit() called
   - Clears fieldErrors
   - Calls submit() hook
   ↓
3. Hook calls backend API
   ↓
4. Backend returns validation error with details[]
   ↓
5. Hook catches error
   - Sets error state
   - Categorizes error → VALIDATION
   - Calls extractFieldErrors() → Record<field, message>
   - Sets fieldErrors state
   ↓
6. useEffect triggers (depends on [apiError])
   - Checks is error a VALIDATION category?
   - Yes → extract fieldErrors (already done by hook)
   - Sets fieldErrors and formError state
   ↓
7. Component re-renders with fieldErrors
   - Amount input gets red border (fieldErrors.amount exists)
   - "must be > 0" text appears below
   - Participants chips get red border
   - "cannot be empty" text appears below
   - Toast shows "Please check highlighted fields"
   ↓
8. User notices error and edits amount field
   ↓
9. handleAmountChange() called
   - Updates amountInput
   - Deletes fieldErrors.amount
   - Component re-renders
   - Red border gone, error text gone
   ↓
10. User resubmits
    - Cycle repeats or succeeds
```

---

## Architecture Benefits

✅ **Separation of Concerns**
- Hook handles API + error extraction
- Component handles UI + error display
- Utilities handle logic (extractFieldErrors)

✅ **Reusable Pattern**
- Same code works in any form
- Just different field names

✅ **Automatic Error Clearing**
- No manual cache invalidation
- Errors cleared on edit = fast UX feedback

✅ **Handles Edge Cases**
- Multiple errors on same form
- Missing field details (form-level fallback)
- Different error categories

✅ **MVP-Friendly**
- No Redux, no complex state
- No Formik, no heavy libraries
- Just React hooks

---

## Files to Reference

1. **Pattern explanation:** `docs/FORM-VALIDATION-PATTERN.md`
2. **Concrete example:** `docs/CREATE-EXPENSE-FORM-EXAMPLE.md`
3. **Complete reference:** `docs/ERROR-HANDLING.md`
4. **Quick reference:** `docs/QUICK-REFERENCE.md`

---

## Next: Apply to Other Forms

Once this works in CreateExpenseScreen, apply the exact same pattern to:
- CreateGroupScreen
- CreateSettlementScreen
- EditExpenseScreen
- EditGroupScreen

Just change field names and API endpoints!
