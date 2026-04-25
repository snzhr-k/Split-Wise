import { apiGet } from '../../../api/httpClient';

import type { Group } from '../types';

export async function getGroups(): Promise<Group[]> {
	return apiGet<Group[]>('/api/groups');
}
