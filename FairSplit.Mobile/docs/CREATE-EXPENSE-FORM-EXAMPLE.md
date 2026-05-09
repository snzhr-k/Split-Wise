# CreateExpenseScreen: Concrete Validation Implementation

This file shows the complete, working implementation of CreateExpenseScreen with proper validation error handling from the backend.

---

## Complete CreateExpenseScreen Implementation

```typescript
import { useMemo, useState, useEffect } from 'react';
import { Pressable, ScrollView, StyleSheet, Text, TextInput, View } from 'react-native';

import { theme } from '../../../theme';
import type { Group } from '../../groups/types';
import { useGroupMembers } from '../../members/hooks/useGroupMembers';
import type { Member } from '../../members/types';
import { useCreateExpense } from '../hooks/useCreateExpense';
import { isBackendError, categorizeErrorCode, ErrorCategory } from '../../../services/errorTypes';
import { extractFieldErrors } from '../../../services/errorHandler';

type CreateExpenseScreenProps = {
  group: Group;
  onBack: () => void;
};

export function CreateExpenseScreen({
  group,
  onBack,
}: CreateExpenseScreenProps) {
  // ─────────────────────────────────────────────────────────────
  // 1. Form Input State
  // ─────────────────────────────────────────────────────────────
  const [amountInput, setAmountInput] = useState('');
  const [payerMemberId, setPayerMemberId] = useState('');
  const [selectedParticipantIds, setSelectedParticipantIds] = useState<string[]>([]);

  // ─────────────────────────────────────────────────────────────
  // 2. Validation State
  // ─────────────────────────────────────────────────────────────
  // Map of field name -> error message from backend
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  
  // Fallback error if backend returns no field details
  const [formError, setFormError] = useState<string | null>(null);

  // ─────────────────────────────────────────────────────────────
  // 3. API and Dependencies
  // ─────────────────────────────────────────────────────────────
  const {
    members,
    isLoading: membersLoading,
    errorMessage: membersErrorMessage,
    reload: reloadMembers,
  } = useGroupMembers(group.id);

  const {
    isSubmitting,
    error: apiError,
    submit,
  } = useCreateExpense(group.id);

  // ─────────────────────────────────────────────────────────────
  // 4. Effect: Extract Backend Validation Errors
  // ─────────────────────────────────────────────────────────────
  useEffect(() => {
    if (!apiError) {
      return;
    }

    if (!isBackendError(apiError)) {
      // Network error or other - already handled by toast
      return;
    }

    const category = categorizeErrorCode(apiError.code);

    // Only VALIDATION errors get field-level extraction
    if (category === ErrorCategory.VALIDATION) {
      // Extract details array → Record<field, message>
      const errors = extractFieldErrors(apiError);
      setFieldErrors(errors);
      setFormError(null);
    } else {
      // Non-validation errors: show as form-level message
      // (e.g., "Member not in group" is a business rule, not field validation)
      if (apiError.details.length === 0) {
        setFormError(apiError.message);
      }
      setFieldErrors({});
    }
  }, [apiError]);

  // ─────────────────────────────────────────────────────────────
  // 5. Field Change Handlers
  // ─────────────────────────────────────────────────────────────
  /**
   * When user edits amount:
   * 1. Update the value
   * 2. Clear any error for this field
   */
  const handleAmountChange = (text: string) => {
    setAmountInput(text);
    
    // Clear error when user starts fixing it
    if (fieldErrors.amount) {
      setFieldErrors(prev => {
        const updated = { ...prev };
        delete updated.amount;
        return updated;
      });
    }
  };

  /**
   * When user selects/deselects a participant:
   * 1. Toggle the participant in selection
   * 2. Clear participants error if it exists
   */
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

  /**
   * When user selects payer:
   * 1. Set the payer
   * 2. Auto-add to participants if not already selected
   * 3. Clear payerMemberId error if exists
   */
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

  // ─────────────────────────────────────────────────────────────
  // 6. Computed Properties
  // ─────────────────────────────────────────────────────────────
  const normalizedPayerId = payerMemberId.trim();
  const hasMembers = members.length > 0;

  const canSubmit = useMemo(() => {
    if (isSubmitting) return false;
    if (!hasMembers) return false;

    const amount = Number(amountInput);
    if (!amount || Number.isNaN(amount) || amount <= 0) return false;

    if (!normalizedPayerId) return false;
    if (selectedParticipantIds.length === 0) return false;

    return true;
  }, [amountInput, hasMembers, isSubmitting, normalizedPayerId, selectedParticipantIds.length]);

  // ─────────────────────────────────────────────────────────────
  // 7. Submit Handler
  // ─────────────────────────────────────────────────────────────
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

    // On success, navigate away
    if (createdExpense) {
      onBack();
    }

    // On failure, errors are extracted by the useEffect above
    // and displayed inline near each field
  };

  // ─────────────────────────────────────────────────────────────
  // 8. Render
  // ─────────────────────────────────────────────────────────────
  return (
    <ScrollView
      contentContainerStyle={styles.container}
      keyboardShouldPersistTaps="handled"
    >
      <Pressable style={styles.backButton} onPress={onBack}>
        <Text style={styles.backText}>Back to expenses</Text>
      </Pressable>

      <Text style={styles.title}>Create Expense</Text>
      <Text style={styles.subtitle}>Group: {group.name}</Text>

      <View style={styles.card}>
        {/* ─────────────────────────────────────────────────────────────
            Form-Level Error (if no field details)
            ─────────────────────────────────────────────────────────────
        */}
        {formError && (
          <View style={styles.formErrorContainer}>
            <Text style={styles.formErrorText}>{formError}</Text>
          </View>
        )}

        {/* ─────────────────────────────────────────────────────────────
            Amount Field
            ─────────────────────────────────────────────────────────────
        */}
        <Text style={styles.label}>Amount</Text>
        <TextInput
          value={amountInput}
          onChangeText={handleAmountChange}
          placeholder="e.g. 120.50"
          keyboardType="decimal-pad"
          // Red border if validation error
          style={[
            styles.input,
            fieldErrors.amount ? styles.inputError : null,
          ]}
        />
        {/* Error message below input */}
        {fieldErrors.amount && (
          <Text style={styles.fieldErrorText}>{fieldErrors.amount}</Text>
        )}

        {/* ─────────────────────────────────────────────────────────────
            Split Type (hardcoded to Equal for MVP)
            ─────────────────────────────────────────────────────────────
        */}
        <Text style={styles.label}>Split type</Text>
        <Text style={styles.helperText}>Equal split (current MVP scope)</Text>

        {/* ─────────────────────────────────────────────────────────────
            Payer Selection
            ─────────────────────────────────────────────────────────────
        */}
        <Text style={styles.label}>Payer</Text>
        {membersLoading ? (
          <Text style={styles.helperText}>Loading members...</Text>
        ) : membersErrorMessage ? (
          <View style={styles.errorRow}>
            <Text style={styles.errorText}>{membersErrorMessage}</Text>
            <Pressable style={styles.retryButton} onPress={reloadMembers}>
              <Text style={styles.retryButtonText}>Retry</Text>
            </Pressable>
          </View>
        ) : members.length === 0 ? (
          <Text style={styles.helperText}>No members in this group yet.</Text>
        ) : (
          <>
            {/* Red border if payer validation error */}
            <View
              style={[
                styles.chipsRow,
                fieldErrors.payerMemberId ? styles.chipsRowError : null,
              ]}
            >
              {members.map(member => {
                const isSelected = normalizedPayerId === member.id;
                return (
                  <Pressable
                    key={member.id}
                    onPress={() => handleSelectPayer(member.id)}
                    style={[
                      styles.chip,
                      isSelected ? styles.chipSelected : null,
                    ]}
                  >
                    <Text
                      style={[
                        styles.chipText,
                        isSelected ? styles.chipTextSelected : null,
                      ]}
                    >
                      {member.displayName}
                    </Text>
                  </Pressable>
                );
              })}
            </View>
            {/* Error message below chips */}
            {fieldErrors.payerMemberId && (
              <Text style={styles.fieldErrorText}>{fieldErrors.payerMemberId}</Text>
            )}
          </>
        )}

        {/* ─────────────────────────────────────────────────────────────
            Participants Selection
            ─────────────────────────────────────────────────────────────
        */}
        <Text style={styles.label}>Participants</Text>
        {membersLoading ? (
          <Text style={styles.helperText}>Loading members...</Text>
        ) : membersErrorMessage ? (
          <Text style={styles.errorText}>Could not load members.</Text>
        ) : members.length === 0 ? (
          <Text style={styles.helperText}>No members available.</Text>
        ) : (
          <>
            {/* Red border if participants validation error */}
            <View
              style={[
                styles.chipsRow,
                fieldErrors.participants ? styles.chipsRowError : null,
              ]}
            >
              {members.map(member => {
                const isSelected = selectedParticipantIds.includes(member.id);
                return (
                  <Pressable
                    key={member.id}
                    onPress={() => handleToggleParticipant(member.id)}
                    style={[
                      styles.chip,
                      isSelected ? styles.chipSelected : null,
                    ]}
                  >
                    <Text
                      style={[
                        styles.chipText,
                        isSelected ? styles.chipTextSelected : null,
                      ]}
                    >
                      {member.displayName}
                    </Text>
                  </Pressable>
                );
              })}
            </View>
            {/* Error message below chips */}
            {fieldErrors.participants && (
              <Text style={styles.fieldErrorText}>{fieldErrors.participants}</Text>
            )}
          </>
        )}

        {/* ─────────────────────────────────────────────────────────────
            Submit Button
            ─────────────────────────────────────────────────────────────
        */}
        <Pressable
          style={[
            styles.submitButton,
            !canSubmit ? styles.submitButtonDisabled : null,
          ]}
          onPress={handleSubmit}
          disabled={!canSubmit}
        >
          <Text style={styles.submitButtonText}>
            {isSubmitting ? 'Submitting...' : 'Create expense'}
          </Text>
        </Pressable>
      </View>
    </ScrollView>
  );
}

// ─────────────────────────────────────────────────────────────
// Styles
// ─────────────────────────────────────────────────────────────
const styles = StyleSheet.create({
  container: {
    flexGrow: 1,
    paddingHorizontal: theme.spacing.xl,
    paddingVertical: theme.spacing.xxl,
    gap: theme.spacing.lg,
    backgroundColor: theme.colors.background,
  },
  backButton: {
    alignSelf: 'flex-start',
    backgroundColor: theme.colors.surfaceMuted,
    borderRadius: theme.radius.round,
    paddingHorizontal: theme.spacing.lg,
    minHeight: theme.sizing.buttonHeight,
    justifyContent: 'center',
  },
  backText: {
    fontSize: theme.typography.fontSize.md,
    fontWeight: theme.typography.fontWeight.semibold,
    color: theme.colors.textSecondary,
  },
  title: {
    fontSize: theme.typography.fontSize.xxl,
    lineHeight: theme.typography.lineHeight.xxl,
    fontWeight: theme.typography.fontWeight.bold,
    color: theme.colors.textPrimary,
  },
  subtitle: {
    fontSize: theme.typography.fontSize.md,
    lineHeight: theme.typography.lineHeight.md,
    color: theme.colors.textSecondary,
  },
  card: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.lg,
    gap: theme.spacing.md,
  },
  // ─ Form-level error
  formErrorContainer: {
    backgroundColor: `${theme.colors.danger}15`,  // 15% opacity
    borderRadius: theme.radius.md,
    borderLeftWidth: 4,
    borderLeftColor: theme.colors.danger,
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.sm,
  },
  formErrorText: {
    fontSize: theme.typography.fontSize.md,
    color: theme.colors.danger,
    lineHeight: theme.typography.lineHeight.md,
  },
  // ─ Labels
  label: {
    fontSize: theme.typography.fontSize.md,
    fontWeight: theme.typography.fontWeight.semibold,
    color: theme.colors.textPrimary,
  },
  helperText: {
    fontSize: theme.typography.fontSize.sm,
    color: theme.colors.textSecondary,
  },
  // ─ Input field
  input: {
    backgroundColor: theme.colors.surfaceMuted,
    borderRadius: theme.radius.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.sm,
    fontSize: theme.typography.fontSize.md,
    lineHeight: theme.typography.lineHeight.md,
    color: theme.colors.textPrimary,
  },
  // ─ Input with error
  inputError: {
    borderColor: theme.colors.danger,
    borderWidth: 2,
    backgroundColor: `${theme.colors.danger}08`,  // Very light red bg
  },
  // ─ Field error text
  fieldErrorText: {
    fontSize: theme.typography.fontSize.sm,
    color: theme.colors.danger,
    marginTop: -theme.spacing.xs,  // Bring closer to field
    marginBottom: theme.spacing.sm,
  },
  // ─ Chips container
  chipsRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: theme.spacing.xs,
    borderRadius: theme.radius.md,
    paddingHorizontal: theme.spacing.sm,
    paddingVertical: theme.spacing.sm,
  },
  // ─ Chips container with error
  chipsRowError: {
    borderWidth: 2,
    borderColor: theme.colors.danger,
    backgroundColor: `${theme.colors.danger}08`,
  },
  // ─ Individual chip
  chip: {
    backgroundColor: theme.colors.surfaceMuted,
    borderRadius: theme.radius.round,
    borderWidth: 1,
    borderColor: theme.colors.border,
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.xs,
  },
  chipSelected: {
    backgroundColor: theme.colors.brandPrimary,
    borderColor: theme.colors.brandPrimary,
  },
  chipText: {
    fontSize: theme.typography.fontSize.sm,
    lineHeight: theme.typography.lineHeight.sm,
    color: theme.colors.textSecondary,
  },
  chipTextSelected: {
    color: theme.colors.textInverse,
    fontWeight: theme.typography.fontWeight.semibold,
  },
  // ─ Error rows
  errorRow: {
    gap: theme.spacing.sm,
  },
  errorText: {
    fontSize: theme.typography.fontSize.sm,
    color: theme.colors.danger,
  },
  retryButton: {
    backgroundColor: theme.colors.surfaceMuted,
    borderRadius: theme.radius.md,
    minHeight: theme.sizing.buttonHeight,
    paddingHorizontal: theme.spacing.lg,
    justifyContent: 'center',
    alignSelf: 'flex-start',
  },
  retryButtonText: {
    fontSize: theme.typography.fontSize.md,
    fontWeight: theme.typography.fontWeight.semibold,
    color: theme.colors.textSecondary,
  },
  // ─ Submit button
  submitButton: {
    marginTop: theme.spacing.md,
    backgroundColor: theme.colors.brandPrimary,
    borderRadius: theme.radius.md,
    minHeight: theme.sizing.buttonHeight,
    paddingHorizontal: theme.spacing.lg,
    justifyContent: 'center',
    alignSelf: 'flex-start',
  },
  submitButtonDisabled: {
    opacity: 0.5,
  },
  submitButtonText: {
    fontSize: theme.typography.fontSize.md,
    fontWeight: theme.typography.fontWeight.semibold,
    color: theme.colors.textInverse,
  },
});
```

---

## Key Implementation Details

### 1. State Management
```typescript
// Input values
const [amountInput, setAmountInput] = useState('');
const [payerMemberId, setPayerMemberId] = useState('');
const [selectedParticipantIds, setSelectedParticipantIds] = useState<string[]>([]);

// Validation state
const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
const [formError, setFormError] = useState<string | null>(null);
```

### 2. Backend Error Extraction
```typescript
useEffect(() => {
  if (!apiError) return;

  if (!isBackendError(apiError)) return;

  const category = categorizeErrorCode(apiError.code);

  if (category === ErrorCategory.VALIDATION) {
    // VALIDATION errors → extract field details
    const errors = extractFieldErrors(apiError);
    setFieldErrors(errors);
    setFormError(null);
  } else {
    // Other errors → show as form-level message
    if (apiError.details.length === 0) {
      setFormError(apiError.message);
    }
    setFieldErrors({});
  }
}, [apiError]);
```

### 3. Error Clearing on Edit
```typescript
const handleAmountChange = (text: string) => {
  setAmountInput(text);
  
  // Clear error when user edits
  if (fieldErrors.amount) {
    setFieldErrors(prev => {
      const updated = { ...prev };
      delete updated.amount;
      return updated;
    });
  }
};
```

### 4. Inline Error Display
```typescript
<TextInput
  style={[
    styles.input,
    fieldErrors.amount ? styles.inputError : null,  // Red border
  ]}
/>
{fieldErrors.amount && (
  <Text style={styles.fieldErrorText}>{fieldErrors.amount}</Text>
)}
```

### 5. Error-Aware Styling
```typescript
// Input with error
inputError: {
  borderColor: theme.colors.danger,
  borderWidth: 2,
  backgroundColor: `${theme.colors.danger}08`,  // Light red bg
},

// Chips container with error
chipsRowError: {
  borderWidth: 2,
  borderColor: theme.colors.danger,
  backgroundColor: `${theme.colors.danger}08`,
},
```

---

## Usage Flow

1. **User fills form** → Enter amount, select payer, select participants
2. **User submits** → `handleSubmit()` calls `submit()` hook
3. **Backend returns error** → Hook catches and sets `apiError` state
4. **useEffect triggers** → Categorizes error, extracts fieldErrors
5. **Component re-renders** → Shows red borders and error messages inline
6. **User edits field** → Error for that field cleared immediately
7. **User resubmits** → Cycle repeats or succeeds

---

## Error Scenarios

### Scenario 1: Multiple Related Validation Errors
```json
{
  "code": "VALIDATION_ERROR",
  "details": [
    {"field": "amount", "issue": "must be > 0", "value": -50},
    {"field": "participants", "issue": "cannot be empty", "value": null},
    {"field": "payerMemberId", "issue": "must belong to group", "value": "xyz"}
  ]
}
```

**Result:** 3 separate field errors shown inline simultaneously

### Scenario 2: Single Field Error
```json
{
  "code": "VALIDATION_ERROR",
  "details": [
    {"field": "amount", "issue": "must be > 0", "value": -50}
  ]
}
```

**Result:** Only amount field shows error

### Scenario 3: No Field Details (Generic Validation)
```json
{
  "code": "VALIDATION_ERROR",
  "details": [],
  "message": "Expense configuration is invalid"
}
```

**Result:** Form-level error shown above submit button

### Scenario 4: Business Rule Error (Not Validation)
```json
{
  "code": "MEMBER_NOT_IN_GROUP",
  "details": [],
  "message": "Not all participants belong to this group."
}
```

**Result:** Shown as form-level message (not field errors) + toast

---

## This Pattern is Now Ready to Reuse

Once this is working in CreateExpenseScreen, the exact same pattern can be applied to:
- CreateGroupScreen
- CreateSettlementScreen
- EditGroupScreen
- EditExpenseScreen
- Any other form

Just change the field names and dependencies!
