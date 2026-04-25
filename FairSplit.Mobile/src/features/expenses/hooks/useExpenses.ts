import { useCallback, useEffect, useState } from 'react';

import { getExpensesByGroupId } from '../api/expensesApi';
import type { Expense } from '../types';

export function useExpenses(groupId: string) {
	const [expenses, setExpenses] = useState<Expense[]>([]);
	const [isLoading, setIsLoading] = useState(true);
	const [errorMessage, setErrorMessage] = useState<string | null>(null);

	const loadExpenses = useCallback(async () => {
		setIsLoading(true);
		setErrorMessage(null);

		try {
			const response = await getExpensesByGroupId(groupId);
			setExpenses(response);
		} catch {
			setErrorMessage('Could not load expenses for this group. Check backend status and API URL.');
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
		errorMessage,
		reload: loadExpenses,
	};
}
