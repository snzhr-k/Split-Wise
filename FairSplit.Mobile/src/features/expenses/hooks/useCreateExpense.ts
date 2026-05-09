import { useState } from 'react';

import { createExpense } from '../api/expensesApi';
import type { CreateExpenseRequest, Expense } from '../types';
import { handleError, extractValidationErrorsByField } from '../../../services/errorHandler';
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
					setFieldErrors(extractValidationErrorsByField(err));
				}
			}

			// Let central handler show appropriate toast
			handleError(err, {
				skipToast: false,
				onValidationError: backendError => {
					// Already handled above
				},
			});

			return null;
		} finally {
			setIsSubmitting(false);
		}
	};

	return {
		isSubmitting,
		error,
		fieldErrors,
		// Keep errorMessage for backward compatibility
		errorMessage: error ? error.message : null,
		submit,
	};
}
