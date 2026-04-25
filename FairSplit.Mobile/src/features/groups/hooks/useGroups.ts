import { useCallback, useEffect, useState } from 'react';

import { getGroups } from '../api/groupsApi';
import type { Group } from '../types';

export function useGroups() {
	const [groups, setGroups] = useState<Group[]>([]);
	const [isLoading, setIsLoading] = useState(true);
	const [errorMessage, setErrorMessage] = useState<string | null>(null);

	const loadGroups = useCallback(async () => {
		setIsLoading(true);
		setErrorMessage(null);

		try {
			const response = await getGroups();
			setGroups(response);
		} catch {
			setErrorMessage('Could not load groups from the API. Check backend status and API URL.');
		} finally {
			setIsLoading(false);
		}
	}, []);

	useEffect(() => {
		void loadGroups();
	}, [loadGroups]);

	return {
		groups,
		isLoading,
		errorMessage,
		reload: loadGroups,
	};
}
