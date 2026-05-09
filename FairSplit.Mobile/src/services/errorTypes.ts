/**
 * Structured error type matching backend error responses.
 * All backend errors are parsed into this shape by httpClient.
 */
export type BackendError = {
  code: string;
  message: string;
  details: ValidationDetail[];
  traceId: string;
};

/**
 * Field-level validation error detail from backend.
 */
export type ValidationDetail = {
  field: string;
  issue: string;
  value?: unknown;
};

/**
 * Error categories for consistent frontend handling.
 */
export enum ErrorCategory {
  VALIDATION = 'VALIDATION',
  NOT_FOUND = 'NOT_FOUND',
  BUSINESS_RULE = 'BUSINESS_RULE',
  CONFLICT = 'CONFLICT',
  INTERNAL = 'INTERNAL',
  NETWORK = 'NETWORK',
  UNKNOWN = 'UNKNOWN',
}

/**
 * Categorized error for frontend decision-making.
 */
export type CategorizedError = {
  category: ErrorCategory;
  error: BackendError | NetworkError | UnknownError;
};

/**
 * Network-level error (no response from backend).
 */
export type NetworkError = {
  code: 'NETWORK_ERROR';
  message: string;
  originalError: unknown;
};

/**
 * Catch-all for unexpected errors.
 */
export type UnknownError = {
  code: 'UNKNOWN_ERROR';
  message: string;
  originalError: unknown;
};

/**
 * Error code patterns for categorization.
 */
const ERROR_CODE_PATTERNS = {
  validation: [/^VALIDATION/, /^INVALID_/, /^DUPLICATE_/],
  notFound: [/^.*_NOT_FOUND$/],
  businessRule: [
    /^MEMBER_NOT_IN_GROUP$/,
    /^INVALID_SPLIT_CONFIGURATION$/,
    /^INVALID_SPLIT_TYPE$/,
    /^INVALID_CUSTOM_SHARE$/,
    /^INVALID_SPLIT_SUM$/,
    /^BALANCE_CONFLICT$/,
    /^BUSINESS_RULE_VIOLATION$/,
  ],
  conflict: [/^CONFLICT$/, /^DUPLICATE_/, /^ALREADY_/],
  internal: [/^INTERNAL_ERROR$/, /^DATA_ACCESS_ERROR$/],
};

/**
 * Categorize a backend error code.
 */
export function categorizeErrorCode(code: string): ErrorCategory {
  if (ERROR_CODE_PATTERNS.validation.some(pattern => pattern.test(code))) {
    return ErrorCategory.VALIDATION;
  }

  if (ERROR_CODE_PATTERNS.notFound.some(pattern => pattern.test(code))) {
    return ErrorCategory.NOT_FOUND;
  }

  if (ERROR_CODE_PATTERNS.businessRule.some(pattern => pattern.test(code))) {
    return ErrorCategory.BUSINESS_RULE;
  }

  if (ERROR_CODE_PATTERNS.conflict.some(pattern => pattern.test(code))) {
    return ErrorCategory.CONFLICT;
  }

  if (ERROR_CODE_PATTERNS.internal.some(pattern => pattern.test(code))) {
    return ErrorCategory.INTERNAL;
  }

  return ErrorCategory.UNKNOWN;
}

/**
 * Check if error is a network error (no response from backend).
 */
export function isNetworkError(error: unknown): error is NetworkError {
  return typeof error === 'object' && error !== null && 'code' in error && error.code === 'NETWORK_ERROR';
}

/**
 * Check if error is a backend error (has error code).
 */
export function isBackendError(error: unknown): error is BackendError {
  return (
    typeof error === 'object' &&
    error !== null &&
    'code' in error &&
    'message' in error &&
    'details' in error &&
    'traceId' in error
  );
}

/**
 * Get validation details by field name (for form binding).
 * Returns all validation errors for a specific field.
 */
export function getValidationErrorsForField(error: BackendError, fieldName: string): ValidationDetail[] {
  return error.details.filter(detail => detail.field === fieldName);
}

/**
 * Get first validation error message for a field.
 * Useful for displaying single error per field inline.
 */
export function getFirstValidationErrorForField(error: BackendError, fieldName: string): string | null {
  const errors = getValidationErrorsForField(error, fieldName);
  return errors.length > 0 ? errors[0].issue : null;
}
