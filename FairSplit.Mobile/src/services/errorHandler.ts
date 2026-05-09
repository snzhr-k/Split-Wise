/**
 * Centralized error handling strategy for FairSplit mobile.
 * Decides what to do based on error code and category.
 */

import {
  BackendError,
  categorizeErrorCode,
  ErrorCategory,
  isBackendError,
  isNetworkError,
  NetworkError,
  UnknownError,
} from './errorTypes';
import { toastService } from './toastService';

export type ErrorHandlerOptions = {
  /**
   * Skip the automatic toast for this error.
   * Useful when the caller wants custom handling.
   */
  skipToast?: boolean;

  /**
   * Callback when error requires navigation (e.g., resource not found).
   * Caller can use this to pop screens or navigate back.
   */
  onNavigationRequired?: (reason: 'resource_not_found' | 'forbidden') => void;

  /**
   * Callback for validation errors to allow screen/form to extract details.
   */
  onValidationError?: (error: BackendError) => void;
};

/**
 * Central error handler.
 * Categorizes errors and shows appropriate toasts.
 */
export function handleError(error: unknown, options: ErrorHandlerOptions = {}): void {
  const { skipToast = false, onNavigationRequired, onValidationError } = options;

  // Network error - no response from backend
  if (isNetworkError(error)) {
    if (!skipToast) {
      toastService.error('Network error. Please check your connection and try again.', 5000);
    }
    return;
  }

  // Backend structured error
  if (isBackendError(error)) {
    const category = categorizeErrorCode(error.code);

    // Validation errors - don't show toast, let form handle it
    if (category === ErrorCategory.VALIDATION) {
      onValidationError?.(error);
      if (!skipToast) {
        toastService.warning('Please check the highlighted fields and try again.', 3000);
      }
      return;
    }

    // Not found - resource doesn't exist
    if (category === ErrorCategory.NOT_FOUND) {
      if (!skipToast) {
        const resource = extractResourceFromNotFoundCode(error.code);
        toastService.warning(`${resource} not found. It may have been deleted.`, 4000);
      }
      onNavigationRequired?.('resource_not_found');
      return;
    }

    // Business rule violation - show backend message directly
    if (category === ErrorCategory.BUSINESS_RULE) {
      if (!skipToast) {
        toastService.warning(error.message, 4000);
      }
      return;
    }

    // Conflict - state/uniqueness constraint violation
    if (category === ErrorCategory.CONFLICT) {
      if (!skipToast) {
        toastService.warning(error.message, 4000);
      }
      return;
    }

    // Internal server error - show generic message with debug info
    if (category === ErrorCategory.INTERNAL) {
      if (!skipToast) {
        const debugInfo = `[${error.traceId.slice(0, 8)}]`;
        toastService.error(
          `Something went wrong. Please try again or contact support. ${debugInfo}`,
          5000
        );
      }
      return;
    }

    // Unknown error category - fallback
    if (!skipToast) {
      toastService.error('An unexpected error occurred. Please try again.', 4000);
    }
    return;
  }

  // Unknown error - defensive fallback
  if (!skipToast) {
    toastService.error('An unexpected error occurred. Please try again.', 4000);
  }
}

/**
 * Extract readable resource name from NOT_FOUND error code.
 * Examples: GROUP_NOT_FOUND → "Group", EXPENSE_NOT_FOUND → "Expense"
 */
function extractResourceFromNotFoundCode(code: string): string {
  const match = code.match(/^(\w+)_NOT_FOUND$/);
  if (!match) {
    return 'Resource';
  }

  const resourceName = match[1];
  // Convert MEMBER_ID to Member Id
  return resourceName
    .split('_')
    .map(word => word.charAt(0) + word.slice(1).toLowerCase())
    .join(' ');
}

/**
 * Special handler for validation errors in forms.
 * Returns a map of field -> error message for rendering inline errors.
 */
export function extractValidationErrorsByField(error: BackendError): Record<string, string> {
  const errorsByField: Record<string, string> = {};

  for (const detail of error.details) {
    // If already have an error for this field, skip (first error wins)
    if (!errorsByField[detail.field]) {
      errorsByField[detail.field] = detail.issue;
    }
  }

  return errorsByField;
}

/**
 * Get appropriate navigation action based on error.
 */
export function shouldNavigateOnError(error: unknown): 'back' | null {
  if (isBackendError(error)) {
    const category = categorizeErrorCode(error.code);
    if (category === ErrorCategory.NOT_FOUND) {
      return 'back';
    }
  }

  return null;
}
