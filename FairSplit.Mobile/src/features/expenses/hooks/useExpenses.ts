import { useCallback, useEffect, useState } from 'react';

import { getExpensesByGroupId } from '../api/expensesApi';
import type { Expense } from '../types';
import { handleError, shouldNavigateOnError } from '../../../services/errorHandler';
import type { BackendError } from '../../../services/errorTypes';

export function useExpenses(groupId: string) {
	const [expenses, setExpenses] = useState<Expense[]>([]);
	const [isLoading, setIsLoading] = useState(true);
	const [error, setError] = useState<BackendError | null>(null);

	const loadExpenses = useCallback(async () => {
		setIsLoading(true);
		setError(null);

		try {
			const response = await getExpensesByGroupId(groupId);
			setExpenses(response);
		} catch (err) {
			setError(err as BackendError);
			handleError(err, { skipToast: false });
		} finally {
			setIsLoading(false);
		}
	}, [groupId]);

	useEffect(() => {
		void loadExpenses();
	}, [loadExpenses]);

	return {
		expenses,
		isLoading,
		error,
		// Keep errorMessage for backward compatibility
		errorMessage: error ? error.message : null,
		reload: loadExpenses,
		shouldNavigateBack: error ? shouldNavigateOnError(error) === 'back' : false,
	};
}
