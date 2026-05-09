# Backend Validation Error Handling Pattern

## Overview

When a user submits a form and the backend returns validation errors, the frontend needs to:
1. Parse the error details array
2. Map each detail to a form field
3. Display inline error messages
4. Clear errors as the user corrects them
5. Handle missing details gracefully

This guide shows the reusable architectural pattern using CreateExpenseScreen as the concrete example.

---

## Backend Error Structure

**Example: User submits expense with invalid data**

```json
{
  "code": "VALIDATION_ERROR",
  "message": "Invalid expense data",
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
    },
    {
      "field": "payerMemberId",
      "issue": "Payer member must belong to the group.",
      "value": "member-uuid"
    }
  ],
  "traceId": "00-123abc456def-xyz"
}
```

**Key Points:**
- `code` = "VALIDATION_ERROR" (not "INVALID_SPLIT_CONFIGURATION", different code)
- `details[]` contains field-by-field breakdown
- Each detail has `field`, `issue`, `value`
- May have 1 error, or multiple errors (show all at once)

---

## Frontend Validation State Structure

### Option 1: Simple String Map (Best for MVP)
```typescript
type FieldErrors = Record<string, string>;

// State
const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});

// Usage
fieldErrors.amount              // "Amount must be greater than zero."
fieldErrors.participants        // "Participants array cannot be empty."
fieldErrors.payerMemberId       // "Payer member must belong to the group."
fieldErrors.unknownField        // undefined (no error)
```

**Pros:**
- Simple to understand and maintain
- Fast lookups O(1)
- Works with destructuring

**Cons:**
- Only stores first error per field (ignores duplicates)
- No metadata about error severity

### Option 2: Rich Error Details (For future enhancement)
```typescript
type ValidationError = {
  issue: string;         // The error message
  value?: unknown;       // The rejected value
  severity?: 'error' | 'warning';
};

type FieldErrors = Record<string, ValidationError>;

// Usage
fieldErrors.amount?.issue        // "Amount must be greater than zero."
fieldErrors.amount?.value        // -50
fieldErrors.amount?.severity     // 'error'
```

**For MVP, stick with Option 1 (simple string map). Upgrade to Option 2 later if needed.**

---

## Mapping Logic: Backend Details → Field Errors

### Utility Function (Reusable)

```typescript
/**
 * Extract field errors from backend validation error.
 * Maps error details array to Record<fieldName, errorMessage>.
 * 
 * If multiple errors on same field, first one wins.
 */
export function extractFieldErrors(
  backendError: BackendError
): Record<string, string> {
  const fieldErrors: Record<string, string> = {};

  for (const detail of backendError.details) {
    // First error for this field wins
    // (don't overwrite if already set)
    if (!fieldErrors[detail.field]) {
      fieldErrors[detail.field] = detail.issue;
    }
  }

  return fieldErrors;
}
```

**Usage in Hook:**
```typescript
try {
  return await createExpense(groupId, request);
} catch (err) {
  if (isBackendError(err)) {
    const category = categorizeErrorCode(err.code);
    
    // VALIDATION errors get field extraction
    if (category === ErrorCategory.VALIDATION) {
      const errors = extractFieldErrors(err);
      setFieldErrors(errors);  // Map to state
    }
  }
  
  handleError(err);
  return null;
}
```

---

## Error Display: Inline Field Errors

### Pattern 1: Text Input with Error Message Below

```typescript
interface FieldInputProps {
  label: string;
  value: string;
  onChangeText: (text: string) => void;
  error?: string;                    // Error message from fieldErrors
  placeholder?: string;
  keyboardType?: KeyboardTypeOptions;
}

function FieldInput({
  label,
  value,
  onChangeText,
  error,
  placeholder,
  keyboardType = 'default',
}: FieldInputProps) {
  return (
    <View style={styles.fieldContainer}>
      <Text style={styles.label}>{label}</Text>
      
      <TextInput
        value={value}
        onChangeText={onChangeText}
        placeholder={placeholder}
        keyboardType={keyboardType}
        style={[
          styles.input,
          error ? styles.inputError : null,  // Red border if error
        ]}
      />
      
      {error ? (
        <Text style={styles.errorText}>{error}</Text>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  fieldContainer: {
    marginBottom: theme.spacing.lg,
  },
  label: {
    fontSize: theme.typography.fontSize.md,
    fontWeight: theme.typography.fontWeight.semibold,
    color: theme.colors.textPrimary,
    marginBottom: theme.spacing.sm,
  },
  input: {
    backgroundColor: theme.colors.surfaceMuted,
    borderRadius: theme.radius.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.sm,
    fontSize: theme.typography.fontSize.md,
    color: theme.colors.textPrimary,
  },
  inputError: {
    borderColor: theme.colors.danger,  // Red
    borderWidth: 2,
  },
  errorText: {
    fontSize: theme.typography.fontSize.sm,
    color: theme.colors.danger,        // Red
    marginTop: theme.spacing.xs,
  },
});
```

### Pattern 2: Chip/Selection Error

```typescript
interface FieldChipsProps {
  label: string;
  values: string[];
  options: Array<{ id: string; label: string }>;
  onToggle: (id: string) => void;
  error?: string;
}

function FieldChips({
  label,
  values,
  options,
  onToggle,
  error,
}: FieldChipsProps) {
  return (
    <View style={styles.container}>
      <Text style={styles.label}>{label}</Text>
      
      <View style={[styles.chipsRow, error ? styles.chipsRowError : null]}>
        {options.map(option => (
          <Pressable
            key={option.id}
            onPress={() => onToggle(option.id)}
            style={[
              styles.chip,
              values.includes(option.id) ? styles.chipSelected : null,
            ]}
          >
            <Text style={styles.chipText}>{option.label}</Text>
          </Pressable>
        ))}
      </View>
      
      {error ? (
        <Text style={styles.errorText}>{error}</Text>
      ) : null}
    </View>
  );
}
```

---

## Error Clearing: Clear on Edit

### Strategy 1: Clear Field Error When User Edits

```typescript
function CreateExpenseScreen() {
  const [amountInput, setAmountInput] = useState('');
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  const handleAmountChange = (text: string) => {
    setAmountInput(text);
    
    // Clear error for this field when user starts editing
    if (fieldErrors.amount) {
      setFieldErrors(previous => {
        const updated = { ...previous };
        delete updated.amount;  // Remove error
        return updated;
      });
    }
  };

  return (
    <FieldInput
      label="Amount"
      value={amountInput}
      onChangeText={handleAmountChange}
      error={fieldErrors.amount}  // Error from backend
      keyboardType="decimal-pad"
    />
  );
}
```

### Strategy 2: Reusable Clear Helper

```typescript
/**
 * Clear error for a specific field.
 * Use when user starts editing the field.
 */
function clearFieldError(
  fieldErrors: Record<string, string>,
  fieldName: string
): Record<string, string> {
  if (!fieldErrors[fieldName]) {
    return fieldErrors;  // No error to clear
  }

  const updated = { ...fieldErrors };
  delete updated[fieldName];
  return updated;
}

// Usage
const handleFieldChange = (fieldName: string, value: string) => {
  setValue(value);
  setFieldErrors(previous => clearFieldError(previous, fieldName));
};
```

### Strategy 3: Generic Input Wrapper (Recommended)

```typescript
/**
 * Reusable input component that auto-clears errors on edit.
 */
interface ClearingFieldInputProps extends FieldInputProps {
  onChangeTextWithErrorClear: (text: string, fieldName: string) => void;
  fieldName: string;
}

function ClearingFieldInput({
  fieldName,
  onChangeTextWithErrorClear,
  ...props
}: ClearingFieldInputProps) {
  return (
    <FieldInput
      {...props}
      onChangeText={(text) => onChangeTextWithErrorClear(text, fieldName)}
    />
  );
}

// Usage
const handleFieldChange = (text: string, fieldName: string) => {
  // Update value
  if (fieldName === 'amount') setAmountInput(text);
  else if (fieldName === 'payerMemberId') setPayerMemberId(text);
  
  // Clear error
  setFieldErrors(prev => {
    const updated = { ...prev };
    delete updated[fieldName];
    return updated;
  });
};

// In render
<ClearingFieldInput
  fieldName="amount"
  value={amountInput}
  onChangeTextWithErrorClear={handleFieldChange}
  error={fieldErrors.amount}
/>
```

---

## Complete Submit Handler Pattern

```typescript
async function handleSubmit() {
  // 1. Clear any previous server-side validation errors
  setFieldErrors({});

  // 2. Quick client-side validation (optional, for UX)
  const amount = Number(amountInput);
  if (!amount || amount <= 0) {
    setFieldErrors(prev => ({
      ...prev,
      amount: 'Amount must be greater than zero.'
    }));
    return;
  }

  if (!payerMemberId) {
    setFieldErrors(prev => ({
      ...prev,
      payerMemberId: 'Please select a payer.'
    }));
    return;
  }

  if (selectedParticipantIds.length === 0) {
    setFieldErrors(prev => ({
      ...prev,
      participants: 'Select at least one participant.'
    }));
    return;
  }

  // 3. Submit to backend
  const createdExpense = await submit({
    payerMemberId,
    amount,
    splitType: 'equal',
    participants: selectedParticipantIds.map(id => ({ memberId: id })),
  });

  // 4. On success, navigate away
  if (createdExpense) {
    onBack();
    return;
  }

  // 5. On failure with validation errors, they're already extracted by hook
  //    and set in fieldErrors state automatically
  //    Just display them (which we do via render)
}
```

**Flow:**
1. User submits form
2. Hook calls API
3. Backend returns validation error
4. Hook catches error
5. Hook extracts fieldErrors via `extractFieldErrors()`
6. Hook sets `setFieldErrors(errors)`
7. Component re-renders with errors displayed inline
8. User can edit any field and error disappears
9. User resubmits

---

## Handling Missing Field Details

### Scenario: Backend error has no details[]

```json
{
  "code": "VALIDATION_ERROR",
  "message": "Something is wrong with this expense",
  "details": [],  // Empty!
  "traceId": "abc123"
}
```

### Solution: Fall back to form-level error

```typescript
function CreateExpenseScreen() {
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [formError, setFormError] = useState<string | null>(null);

  useEffect(() => {
    if (error) {
      // If no field-level details, show as form-level error
      if (error.details.length === 0) {
        setFormError(error.message);
        setFieldErrors({});
      } else {
        // Extract to field errors
        setFieldErrors(extractFieldErrors(error));
        setFormError(null);
      }
    }
  }, [error]);

  return (
    <View>
      {/* Field-level errors */}
      <FieldInput
        label="Amount"
        value={amountInput}
        error={fieldErrors.amount}
      />

      {/* Form-level error */}
      {formError && (
        <View style={styles.formErrorContainer}>
          <Text style={styles.formErrorText}>{formError}</Text>
        </View>
      )}

      <Pressable onPress={handleSubmit}>
        <Text>Create Expense</Text>
      </Pressable>
    </View>
  );
}
```

---

## Reusable Pattern for Other Forms

### Step 1: Create Generic Form Hook

```typescript
/**
 * Generic form validation hook.
 * Handles field errors, error clearing, and submission.
 */
export function useFormValidation<T>(initialValues: T) {
  const [values, setValues] = useState(initialValues);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const setFieldValue = (field: keyof T, value: unknown) => {
    setValues(prev => ({
      ...prev,
      [field]: value,
    }));
    
    // Clear error when user edits
    setFieldErrors(prev => {
      const updated = { ...prev };
      delete updated[String(field)];
      return updated;
    });
  };

  const setFieldError = (field: keyof T, error: string) => {
    setFieldErrors(prev => ({
      ...prev,
      [String(field)]: error,
    }));
  };

  const setErrors = (errors: Record<string, string>) => {
    setFieldErrors(errors);
  };

  const resetErrors = () => {
    setFieldErrors({});
    setFormError(null);
  };

  return {
    values,
    setValues,
    fieldErrors,
    formError,
    setFormError,
    setFieldValue,
    setFieldError,
    setErrors,
    resetErrors,
    isSubmitting,
    setIsSubmitting,
  };
}
```

### Step 2: Use in CreateExpenseScreen

```typescript
function CreateExpenseScreen() {
  const form = useFormValidation({
    amountInput: '',
    payerMemberId: '',
    selectedParticipantIds: [] as string[],
  });

  const { error: apiError } = useCreateExpense(groupId);

  // Extract backend field errors when API error changes
  useEffect(() => {
    if (apiError && isBackendError(apiError)) {
      const category = categorizeErrorCode(apiError.code);

      if (category === ErrorCategory.VALIDATION) {
        // Extract to field errors
        form.setErrors(extractFieldErrors(apiError));
      } else if (apiError.details.length === 0) {
        // Fall back to form-level error
        form.setFormError(apiError.message);
      }
    }
  }, [apiError]);

  const handleSubmit = async () => {
    form.resetErrors();
    form.setIsSubmitting(true);

    const result = await submit({
      payerMemberId: form.values.payerMemberId,
      amount: Number(form.values.amountInput),
      splitType: 'equal',
      participants: form.values.selectedParticipantIds.map(id => ({ memberId: id })),
    });

    form.setIsSubmitting(false);

    if (result) {
      onBack();
    }
  };

  return (
    <View>
      {form.formError && (
        <Text style={styles.formError}>{form.formError}</Text>
      )}

      <FieldInput
        label="Amount"
        value={form.values.amountInput}
        onChangeText={text => form.setFieldValue('amountInput', text)}
        error={form.fieldErrors.amountInput}
      />

      <Pressable onPress={handleSubmit} disabled={form.isSubmitting}>
        <Text>{form.isSubmitting ? 'Submitting...' : 'Create Expense'}</Text>
      </Pressable>
    </View>
  );
}
```

### Step 3: Reuse in Group Creation

```typescript
function CreateGroupScreen() {
  const form = useFormValidation({
    nameInput: '',
    descriptionInput: '',
    memberIds: [] as string[],
  });

  const { error: apiError } = useCreateGroup();

  useEffect(() => {
    if (apiError && isBackendError(apiError)) {
      const category = categorizeErrorCode(apiError.code);

      if (category === ErrorCategory.VALIDATION) {
        form.setErrors(extractFieldErrors(apiError));
      } else if (apiError.details.length === 0) {
        form.setFormError(apiError.message);
      }
    }
  }, [apiError]);

  const handleSubmit = async () => {
    form.resetErrors();
    form.setIsSubmitting(true);

    const result = await submit({
      name: form.values.nameInput,
      description: form.values.descriptionInput,
      memberIds: form.values.memberIds,
    });

    form.setIsSubmitting(false);

    if (result) {
      onBack();
    }
  };

  return (
    <View>
      {form.formError && <Text>{form.formError}</Text>}

      <FieldInput
        label="Group Name"
        value={form.values.nameInput}
        onChangeText={text => form.setFieldValue('nameInput', text)}
        error={form.fieldErrors.nameInput}
      />

      <FieldInput
        label="Description"
        value={form.values.descriptionInput}
        onChangeText={text => form.setFieldValue('descriptionInput', text)}
        error={form.fieldErrors.descriptionInput}
      />

      <Pressable onPress={handleSubmit}>
        <Text>Create Group</Text>
      </Pressable>
    </View>
  );
}

// Exact same pattern, just different field names!
```

---

## Complete Architecture Diagram

```
Backend Validation Error
    ↓
    └─ {code: "VALIDATION_ERROR", details: [{field, issue, value}]}
       ↓
Hook (useCreateExpense)
    ├─ Catches error
    ├─ isBackendError(err) → true
    ├─ categorizeErrorCode(err.code) → VALIDATION
    ├─ extractFieldErrors(err) → {amount: "...", participants: "..."}
    └─ setFieldErrors(extracted)
       ↓
Component State
    └─ fieldErrors: {amount: "Amount must be...", participants: "..."}
       ↓
Render
    ├─ <FieldInput error={fieldErrors.amount} />
    │   └─ Shows red border + error text below input
    └─ <FieldChips error={fieldErrors.participants} />
        └─ Shows red border + error text below chips
       ↓
User Interaction
    ├─ User edits amount field
    ├─ onChangeText called
    ├─ clearFieldError('amount')
    ├─ setFieldErrors removes 'amount' key
    └─ Re-render with error gone
       ↓
User Resubmits
    └─ Cycle repeats
```

---

## Summary: Reusable Pattern

### 1. Backend Response Structure
```typescript
type BackendError = {
  code: string;  // "VALIDATION_ERROR", etc.
  message: string;
  details: Array<{
    field: string;
    issue: string;
    value?: unknown;
  }>;
  traceId: string;
};
```

### 2. Extract to Field Map
```typescript
function extractFieldErrors(error: BackendError): Record<string, string> {
  const map: Record<string, string> = {};
  for (const detail of error.details) {
    if (!map[detail.field]) {
      map[detail.field] = detail.issue;
    }
  }
  return map;
}
```

### 3. Display Inline
```typescript
<FieldInput
  label="Amount"
  value={value}
  onChangeText={handleChange}
  error={fieldErrors.amount}  // Show error inline
/>
```

### 4. Clear on Edit
```typescript
const handleChange = (text: string) => {
  setValue(text);
  setFieldErrors(prev => {
    const updated = { ...prev };
    delete updated.amount;  // Clear error
    return updated;
  });
};
```

### 5. Fallback for Missing Details
```typescript
if (error.details.length === 0) {
  setFormError(error.message);
} else {
  setFieldErrors(extractFieldErrors(error));
}
```

### 6. Reuse Pattern in Multiple Forms
```typescript
// Same pattern for CreateGroupScreen, CreateSettlementScreen, etc.
// Just different field names and API endpoints
```

---

## Implementation Checklist

- [ ] `extractFieldErrors()` utility function created
- [ ] `FieldInput` component accepts error prop and styles it
- [ ] Error cleared when user edits field
- [ ] Form error fallback for missing details
- [ ] Hook extracts and sets field errors automatically
- [ ] Form-level error displayed above submit button
- [ ] Submit button disabled while submitting
- [ ] Successfully submitted form navigates away
- [ ] Pattern reusable in other form screens
- [ ] No Redux or Formik (MVP-friendly)

This pattern is now ready to be applied consistently across all FairSplit forms.
