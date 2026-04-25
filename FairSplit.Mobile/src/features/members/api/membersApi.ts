import { apiGet } from '../../../api/httpClient';

import type { Member } from '../types';

export async function getMembersByGroupId(groupId: string): Promise<Member[]> {
	return apiGet<Member[]>(`/api/groups/${groupId}/members`);
}