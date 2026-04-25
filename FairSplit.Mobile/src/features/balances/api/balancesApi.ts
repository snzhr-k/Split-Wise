import { apiGet } from '../../../api/httpClient';

import type { Balance } from '../types';

export async function getBalancesByGroupId(groupId: string): Promise<Balance[]> {
	return apiGet<Balance[]>(`/api/groups/${groupId}/balances`);
}
