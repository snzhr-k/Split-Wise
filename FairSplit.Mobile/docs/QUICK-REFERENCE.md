# FairSplit Mobile Error Handling - Quick Reference

## One-Minute Overview

**What:** Centralized error handling for consistent backend error responses  
**Why:** Users see appropriate feedback based on error type (inline validation, toasts, navigation)  
**Where:** Services handle categorization, components show feedback  
**How:** Import helpers, call `handleError()`, check `fieldErrors`

---

## Common Tasks

### Show a Toast
```typescript
import { toastService } from '../services/toastService';

toastService.success('Done!');
toastService.error('Something failed', 5000);
toastService.warning('Be careful');
toastService.info('FYI');
```

### Handle API Error
```typescript
import { handleError } from '../services/errorHandler';

try {
  await createExpense();
} catch (err) {
  handleError(err);  // Automatic toast + categorization
}
```

### Check Error Type
```typescript
import { isBackendError, categorizeErrorCode, ErrorCategory } from '../services/errorTypes';

if (isBackendError(error)) {
  const category = categorizeErrorCode(error.code);
  
  if (category === ErrorCategory.VALIDATION) {
    // Show form errors
  } else if (category === ErrorCategory.NOT_FOUND) {
    // Navigate back
  }
}
```

### Extract Field Errors for Form
```typescript
import { extractValidationErrorsByField } from '../services/errorHandler';
import { categorizeErrorCode, ErrorCategory } from '../services/errorTypes';

const { fieldErrors } = useCreateExpense(groupId);

// fieldErrors is already extracted and ready to use:
// fieldErrors.amount, fieldErrors.participants, etc.

<TextInput
  style={[styles.input, fieldErrors.amount ? styles.inputError : null]}
/>
{fieldErrors.amount ? <Text>{fieldErrors.amount}</Text> : null}
```

### Handle Specific Error Category
```typescript
try {
  await getExpenses(groupId);
} catch (err) {
  handleError(err, {
    skipToast: false,
    
    onNavigationRequired: (reason) => {
      if (reason === 'resource_not_found') {
        navigation.goBack();  // Custom navigation
      }
    },
    
    onValidationError: (error) => {
      // Already handled by hook, but if custom logic needed:
      console.log('Validation failed for:', error.details);
    }
  });
}
```

---

## Hook Return Values

### useExpenses
```typescript
const { 
  expenses,           // Expense[]
  isLoading,          // boolean
  error,              // BackendError | null
  errorMessage,       // string | null (backward compatible)
  reload,             // () => Promise<void>
  shouldNavigateBack  // boolean (true if NOT_FOUND)
} = useExpenses(groupId);
```

### useCreateExpense
```typescript
const {
  isSubmitting,       // boolean
  error,              // BackendError | null
  fieldErrors,        // { fieldName: "error message" }
  errorMessage,       // string | null (backward compatible)
  submit              // (request) => Promise<Expense | null>
} = useCreateExpense(groupId);
```

---

## Error Codes You'll See

| Code | Type | Toast Message |
|------|------|---------------|
| `INVALID_SPLIT_CONFIGURATION` | Validation | "Please check highlighted fields" |
| `INVALID_SPLIT_TYPE` | Validation | "Please check highlighted fields" |
| `INVALID_CUSTOM_SHARE` | Validation | "Please check highlighted fields" |
| `INVALID_SPLIT_SUM` | Validation | "Please check highlighted fields" |
| `GROUP_NOT_FOUND` | Not Found | "Group not found. It may have been deleted." |
| `EXPENSE_NOT_FOUND` | Not Found | "Expense not found. It may have been deleted." |
| `MEMBER_NOT_FOUND` | Not Found | "Member not found. It may have been deleted." |
| `MEMBER_NOT_IN_GROUP` | Business Rule | "Not all participants belong to this group." |
| `BALANCE_CONFLICT` | Business Rule | (shown message from backend) |
| `DUPLICATE_EXPENSE_PARTICIPANT` | Conflict | (shown message from backend) |
| `DUPLICATE_MEMBER` | Conflict | (shown message from backend) |
| `INTERNAL_ERROR` | Internal | "Something went wrong. [abc123]" |
| `NETWORK_ERROR` | Network | "Failed to connect. Check your connection." |

---

## Component Patterns

### Form with Inline Validation
```typescript
function MyForm() {
  const { fieldErrors, errorMessage, submit } = useCreateExpense(groupId);
  
  return (
    <>
      <TextInput
        value={amount}
        style={[styles.input, fieldErrors.amount && styles.inputError]}
      />
      {fieldErrors.amount && <Text style={styles.error}>{fieldErrors.amount}</Text>}
      
      {errorMessage && <Text style={styles.error}>{errorMessage}</Text>}
      
      <Button onPress={() => submit(data)} />
    </>
  );
}
```

### List with Auto-Navigation on Not Found
```typescript
function ListScreen() {
  const { items, shouldNavigateBack } = useItems(id);
  
  React.useEffect(() => {
    if (shouldNavigateBack) {
      navigation.goBack();
    }
  }, [shouldNavigateBack, navigation]);
  
  return <FlatList data={items} />;
}
```

### Try-Catch with Custom Error Handling
```typescript
async function handleAction() {
  try {
    const result = await apiCall();
    toastService.success('Done!');
  } catch (err) {
    handleError(err, {
      skipToast: true,  // Don't show automatic toast
      onValidationError: (backendErr) => {
        setCustomErrors(extractValidationErrorsByField(backendErr));
      }
    });
  }
}
```

---

## Toast Usage Examples

### Success
```typescript
// Auto-dismisses after 2 seconds (default)
toastService.success('Expense created!');

// Custom duration
toastService.success('Saved!', 1000);

// Persistent (must dismiss manually)
toastService.success('Check this out!', 0);
```

### Error
```typescript
// Auto-dismisses after 4 seconds (default for errors)
toastService.error('Failed to create expense');

// Custom
toastService.error('Try again...', 6000);
```

### Warning
```typescript
// Auto-dismisses after 3.5 seconds
toastService.warning('This will delete the expense');
```

### Info
```typescript
// No auto-dismiss (must be manual)
toastService.info('Welcome to FairSplit!', 0);

// Or auto-dismiss
toastService.info('Syncing...', 3000);
```

---

## Error Flow Diagram

```
User Action (create expense, fetch list, etc)
    ↓
API Call via httpClient
    ↓
Backend Response
    ├─ Success (200) → Return data
    └─ Error (4xx/5xx) → Parse error
       ├─ Extract code, message, details, traceId
       └─ Throw BackendError
    ↓
Catch in Hook
    ├─ Set state (error, fieldErrors, errorMessage)
    └─ Call handleError()
    ↓
handleError()
    ├─ Categorize by error code
    └─ Execute appropriate handler
       ├─ VALIDATION → Extract fields, optional warning toast
       ├─ NOT_FOUND → Warning toast + call onNavigationRequired
       ├─ BUSINESS_RULE → Warning toast with message
       ├─ CONFLICT → Warning toast with message
       └─ INTERNAL → Error toast with traceId
    ↓
Display to User
    ├─ Inline field errors (validation)
    ├─ Toast notification (all error types)
    └─ Optional navigation (not found)
```

---

## Testing Queries

### Test Validation Error
```bash
curl -X POST http://localhost:5000/api/groups/{groupId}/expenses \
  -H Content-Type:application/json \
  -d '{"amount":-50,"splitType":"equal",...}'
```
Expect: Red input border + inline error

### Test Not Found
```bash
# Delete group first via DB
curl http://localhost:5000/api/groups/{deletedGroupId}/expenses
```
Expect: "Group not found" toast + auto-pop

### Test Business Rule
```bash
curl -X POST http://localhost:5000/api/groups/{groupId}/expenses \
  -H Content-Type:application/json \
  -d '{"payerMemberId":"{memberFromOtherGroup}",...}'
```
Expect: "Not all participants belong" toast

---

## Styling Guide

### Error Input (TextField)
```typescript
const styles = StyleSheet.create({
  input: {
    borderColor: theme.colors.border,
  },
  inputError: {
    borderColor: theme.colors.danger,  // Red
  },
});

// Usage
<TextInput style={[styles.input, fieldErrors.amount && styles.inputError]} />
```

### Error Text
```typescript
const styles = StyleSheet.create({
  fieldErrorText: {
    fontSize: theme.typography.fontSize.sm,
    color: theme.colors.danger,  // Red
    marginTop: theme.spacing.xs,
  },
});

// Usage
{fieldErrors.amount && <Text style={styles.fieldErrorText}>{fieldErrors.amount}</Text>}
```

---

## Troubleshooting

**Q: Toast doesn't show**
- Missing `<ToastContainer />` in RootScreen? Add it.
- Error creation different? Run console.clear and try again.

**Q: Inline errors not showing**
- Hook returning `fieldErrors`? Check hook is returning it.
- Assigning to state? Check `setFieldErrors()` is called.
- Displaying in JSX? Check render logic.

**Q: Field errors showing but input not red**
- Apply error style? Add `fieldErrors.field && styles.inputError` to style prop.
- Style defined? Check `inputError` in StyleSheet.

**Q: Error being thrown but not caught**
- Try-catch block? Wrap in try-catch or return from hook.
- Async/await? Remember to await the call.

**Q: Same error showing multiple times**
- Multiple error handlers? Only call `handleError()` once per error.
- Multiple toasts stacking? That's expected - they auto-dismiss.

---

## Performance Notes

- Toast service: O(1) operations (no rendering until display)
- Error categorization: O(1) regex matching
- Field extraction: O(n) where n = number of errors (usually 1-3)
- No debouncing needed (errors not spammy)

---

## See Also

- Full guide: `docs/ERROR-HANDLING.md`
- Integration examples: `docs/ERROR-INTEGRATION-SUMMARY.md`
- Implementation checklist: `docs/IMPLEMENTATION-CHECKLIST.md`

---

## Copy-Paste Template: New Screen with Error Handling

```typescript
import { useCreateExpense } from '../hooks/useCreateExpense';
import { handleError } from '../../../services/errorHandler';

export function MyScreen({ groupId, onBack }: Props) {
  const [input, setInput] = useState('');
  const { fieldErrors, errorMessage, isSubmitting, submit } = useCreateExpense(groupId);

  const handleSubmit = async () => {
    const result = await submit({
      // your data
    });

    if (result) {
      onBack();
    }
  };

  return (
    <View>
      <TextInput
        value={input}
        style={[styles.input, fieldErrors.myField && styles.inputError]}
      />
      {fieldErrors.myField && <Text style={styles.fieldError}>{fieldErrors.myField}</Text>}

      {errorMessage && <Text style={styles.error}>{errorMessage}</Text>}

      <Button onPress={handleSubmit} disabled={!canSubmit} />
    </View>
  );
}
```

Done! Copy, rename, and adapt.
