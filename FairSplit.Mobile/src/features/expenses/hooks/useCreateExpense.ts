import { useState } from 'react';

import type { ApiError } from '../../../api/httpClient';
import { createExpense } from '../api/expensesApi';
import type { CreateExpenseRequest, Expense } from '../types';

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
			setErrorMessage(apiError.message || 'Could not create expense.');
			return null;
		} finally {
			setIsSubmitting(false);
		}
	};

	return {
		isSubmitting,
		errorMessage,
		submit,
	};
}
