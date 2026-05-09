# FairSplit Mobile Error Handling - Implementation Checklist

## ✅ Completed Implementation

### Core Services Created

#### 1. `src/services/errorTypes.ts`
- Defines `BackendError` interface matching backend response shape
- Defines error category enum (VALIDATION, NOT_FOUND, BUSINESS_RULE, CONFLICT, INTERNAL, NETWORK, UNKNOWN)
- Implements `categorizeErrorCode()` to map error codes to categories
- Provides type guards: `isBackendError()`, `isNetworkError()`
- Provides helpers: `getValidationErrorsForField()`, `getFirstValidationErrorForField()`

**Key Types:**
```typescript
type BackendError = {
  code: string;
  message: string;
  details: ValidationDetail[];
  traceId: string;
};

enum ErrorCategory {
  VALIDATION, NOT_FOUND, BUSINESS_RULE, CONFLICT, INTERNAL, NETWORK, UNKNOWN
}
```

#### 2. `src/services/toastService.ts`
- Event-driven toast system (no context providers needed)
- Methods: `show()`, `success()`, `error()`, `warning()`, `info()`, `dismiss()`, `dismissAll()`
- Auto-dismiss based on type-specific durations
- Singleton pattern for app-wide access
- Subscribe/unsubscribe for component rendering

**Usage:**
```typescript
toastService.error('Something went wrong');
toastService.success('Expense created!');
```

#### 3. `src/services/errorHandler.ts`
- Central error handling logic with categorical decision-making
- Handles 7 error types with appropriate user feedback
- Validates errors -> form field extraction
- Not found -> navigation callback
- Business rules -> show backend message
- Internal -> generic message with traceId
- Network -> connection suggestion
- Exports utilities for form binding: `extractValidationErrorsByField()`

**Usage:**
```typescript
handleError(error, {
  skipToast: false,
  onNavigationRequired: (reason) => {
    if (reason === 'resource_not_found') navigation.goBack();
  },
  onValidationError: (error) => {
    setFieldErrors(extractValidationErrorsByField(error));
  }
});
```

### API Layer Updated

#### 4. `src/api/httpClient.ts` [UPDATED]
- **Removed:** Old `ApiError` type with only status and message
- **Added:** Structured error parsing via `parseBackendError()`
- **Now Throws:** `BackendError` with code, message, details, traceId
- **Handles:** Network errors separately (code: 'NETWORK_ERROR')
- All requests now return structured errors

**Error Transformation:**
```
Raw HTTP 400 Response
    ↓
parseBackendError(response)
    ↓
BackendError object with code, message, details[], traceId
    ↓
Thrown to caller
```

### Hooks Updated

#### 5. `src/features/expenses/hooks/useExpenses.ts` [UPDATED]
- **Added:** `error: BackendError | null` state
- **Added:** Error handler call with automatic toast
- **Added:** `shouldNavigateBack` computed property
- **Kept:** `errorMessage` for backward compatibility
- Now supports structured error handling throughout app

#### 6. `src/features/expenses/hooks/useCreateExpense.ts` [UPDATED]
- **Added:** `error: BackendError | null` state
- **Added:** `fieldErrors: Record<string, string>` for form validation
- **Added:** Validation error extraction logic
- **Added:** Error categorization to handle different types
- Calls error handler for appropriate toast/navigation
- Backward compatible with `errorMessage` property

### Components Created

#### 7. `src/components/common/ToastContainer.tsx` [NEW]
- Renders all active toasts with animations
- Spring animation on appearance
- Type-specific background colors (green, red, orange, blue)
- Dismiss button on each toast
- Auto-dismisses based on duration
- Should be placed at app root for visibility

**Usage in RootScreen:**
```tsx
function App() {
  return (
    <>
      <Navigation />
      <ToastContainer />
    </>
  );
}
```

### Screens Updated

#### 8. `src/features/expenses/screens/CreateExpenseScreen.tsx` [UPDATED]
- **Updated destructuring:** Now gets `fieldErrors` from hook
- **Added input styling:** `fieldErrors.amount ? styles.inputError : null`
- **Added field error display:** Shows error below amount input when validation fails
- **Added new styles:** `inputError`, `fieldErrorText`
- Form now shows inline validation errors per field
- Maintains existing UI/UX

### Hooks for Components

#### 9. `src/app/hooks/useToasts.ts` [NEW]
- React hook to subscribe to toast notifications
- Returns array of current toasts
- Used by ToastContainer to render
- Unsubscribes on unmount

**Usage:**
```typescript
const toasts = useToasts();
return (
  <>
    {toasts.map(toast => <Toast key={toast.id} toast={toast} />)}
  </>
);
```

### Documentation Created

#### 10. `docs/ERROR-HANDLING.md` [NEW]
- Comprehensive 200+ line guide covering:
  - Error categories and frontend behavior
  - Architecture diagram (5-layer)
  - Usage examples for each error type
  - Integration checklist
  - API error code reference
  - Testing scenarios
  - Troubleshooting guide
  - Future enhancement suggestions

#### 11. `docs/ERROR-INTEGRATION-SUMMARY.md` [NEW]
- Before/after examples for 4 key changes
- Architecture flow diagram
- File structure overview
- Error code mapping table
- Testing checklist
- Summary table comparing old vs new

---

## 📊 Changes Summary

| File | Status | Changes |
|------|--------|---------|
| `src/services/errorTypes.ts` | ✅ NEW | 200+ lines - error types, categorization, helpers |
| `src/services/toastService.ts` | ✅ NEW | 120+ lines - event-driven toast system |
| `src/services/errorHandler.ts` | ✅ NEW | 130+ lines - central error handler |
| `src/api/httpClient.ts` | ✅ UPDATED | 40 lines - structured error parsing |
| `src/api/types.ts` | ✅ UPDATED | 2 lines - export error types |
| `src/features/expenses/hooks/useExpenses.ts` | ✅ UPDATED | 15 lines - add error handling |
| `src/features/expenses/hooks/useCreateExpense.ts` | ✅ UPDATED | 20 lines - add fieldErrors extraction |
| `src/components/common/ToastContainer.tsx` | ✅ NEW | 130+ lines - toast UI component |
| `src/app/hooks/useToasts.ts` | ✅ NEW | 20 lines - toast subscription hook |
| `src/features/expenses/screens/CreateExpenseScreen.tsx` | ✅ UPDATED | 10 lines - inline error display |
| `docs/ERROR-HANDLING.md` | ✅ NEW | 240+ lines - comprehensive guide |
| `docs/ERROR-INTEGRATION-SUMMARY.md` | ✅ NEW | 350+ lines - before/after examples |

**Total New Code:** ~600 lines of new services/types  
**Total Updated:** ~100 lines of hook/component updates  
**Total Documentation:** ~600 lines  

---

## 🔄 Error Flow Examples

### Example 1: Validation Error (User enters -50 for amount)
```
User Input: amount = -50
    ↓
Form submission
    ↓
createExpense API call
    ↓
Backend returns 400 with code="INVALID_SPLIT_CONFIGURATION"
    ↓
httpClient.parseBackendError()
    ↓
Throws BackendError with details=[{field: "amount", issue: "must be greater than 0", value: -50}]
    ↓
useCreateExpense catches → sets fieldErrors.amount = "must be greater than 0"
    ↓
CreateExpenseScreen displays error below input
    ↓
handleError() shows toast: "Please check the highlighted fields"
    ↓
User sees: red border on amount input + error text + toast notification
```

### Example 2: Not Found Error (Delete group, then fetch expenses)
```
User navigates to expenses screen
    ↓
useExpenses(groupId) calls getExpensesByGroupId
    ↓
Backend returns 404 with code="GROUP_NOT_FOUND"
    ↓
httpClient throws BackendError
    ↓
useExpenses catches → hook.shouldNavigateBack = true
    ↓
handleError() categorizes as NOT_FOUND
    ↓
Toast shows: "Group not found. It may have been deleted."
    ↓
Screen can check shouldNavigateBack and navigate.goBack()
    ↓
User is returned to groups list
```

### Example 3: Business Rule Violation (Member not in group)
```
User creates expense with member from other group
    ↓
createExpense API call
    ↓
Backend validates and returns 403 with code="MEMBER_NOT_IN_GROUP"
    ↓
httpClient throws BackendError
    ↓
useCreateExpense catches → categorized as BUSINESS_RULE
    ↓
handleError() calls categorizeErrorCode
    ↓
Matches BUSINESS_RULE pattern
    ↓
Toast shows: "Not all participants belong to this group."
    ↓
No field errors (not a validation error)
    ↓
User sees message and can navigate back to select correct members
```

---

## 🎯 MVP Scope Features

✅ **Centralized Error Handling**
- Single source of truth for error logic
- Easy to extend with new error codes

✅ **Field-Level Validation**
- Forms show inline errors
- User immediately knows which field needs fixing

✅ **User-Friendly Toasts**
- Non-intrusive notifications
- Auto-dismiss based on importance
- Manual dismiss available

✅ **Network Error Handling**
- Graceful handling of connection failures
- Clear messaging to user

✅ **No Complex Dependencies**
- No Redux required
- No context providers needed (event-driven)
- Lightweight side effects

✅ **Backward Compatible**
- Existing `errorMessage` property still available
- Gradual migration possible

---

## 📋 Next Steps Required

### Phase 1: Finalization (Required for working system)
- [ ] Add `<ToastContainer />` to RootScreen.tsx
- [ ] Run app and test with backend

### Phase 2: Extend to Other Features (Recommended)
- [ ] Update `useGroupMembers` hook
- [ ] Update balance-related hooks
- [ ] Update settlement screens
- [ ] Test all major user flows

### Phase 3: Refinement (Optional)
- [ ] Add haptic feedback on error
- [ ] Add retry button for network errors
- [ ] Add debug mode to show traceId
- [ ] Error analytics tracking

---

## 🧪 Quick Test Checklist

| Test Case | Expected Behavior |
|-----------|-------------------|
| Enter negative amount | Red border on input, error text below, warning toast |
| Delete group, fetch expenses | Warning toast + auto-pop to group list |
| Add deleted member to expense | Warning toast: "Member not found" |
| Network disconnected | Error toast: "Check your connection" |
| Create expense successfully | Success toast: "Expense created" |
| Validation + business rule errors | Field errors inline + separate toasts for rules |

---

## 📚 Documentation

Both detailed guides are in FairSplit.Mobile/docs/:
- `ERROR-HANDLING.md` - Complete reference guide with examples
- `ERROR-INTEGRATION-SUMMARY.md` - Before/after integration patterns

---

## Key Design Decisions

1. **Event-driven toast system** instead of Context
   - Reason: MVP simplicity, works from anywhere
   - Could upgrade to Context later if needed

2. **Automatic error categorization** by code patterns
   - Reason: New error codes automatically work
   - No need to register every code in mapping

3. **Field-level error details** in validation object
   - Reason: Forms can show exactly which field failed
   - Better UX than "something is wrong"

4. **Backward compatible hooks**
   - Reason: Gradual migration possible
   - Existing screens still work

5. **Lightweight, no state management**
   - Reason: MVP scope
   - Can add Redux/Zustand if needed later

---

## Architecture Benefits

| Benefit | How Achieved |
|---------|------------|
| Single source of truth | Centralized errorHandler.ts |
| Consistent UX | All errors go through same categorization |
| Easy to test | Pure functions, no dependencies |
| Easy to extend | Just add new error code patterns |
| Form-friendly | Field-level details extraction |
| No prop drilling | Event-driven toast system |
| MVP-appropriate | Lightweight, no complex libraries |

---

## Files Ready to Add to RootScreen.tsx

```typescript
// At the root of your app (inside navigation stack)
import { ToastContainer } from './src/components/common/ToastContainer';

export function App() {
  return (
    <>
      <NavigationStack />
      <ToastContainer />  // Add this line
    </>
  );
}
```

That's it! System is now ready to use app-wide.
