import { useCallback, useEffect, useState } from 'react';

import { getMembersByGroupId } from '../api/membersApi';
import type { Member } from '../types';

export function useGroupMembers(groupId: string) {
	const [members, setMembers] = useState<Member[]>([]);
	const [isLoading, setIsLoading] = useState(true);
	const [errorMessage, setErrorMessage] = useState<string | null>(null);

	const loadMembers = useCallback(async () => {
		setIsLoading(true);
		setErrorMessage(null);

		try {
			const response = await getMembersByGroupId(groupId);
			setMembers(response);
		} catch {
			setErrorMessage('Could not load group members. Check backend status and API URL.');
		} finally {
			setIsLoading(false);
		}
	}, [groupId]);

	useEffect(() => {
		void loadMembers();
	}, [loadMembers]);

	return {
		members,
		isLoading,
		errorMessage,
		reload: loadMembers,
	};
}