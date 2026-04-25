import { apiGet, apiPost } from '../../../api/httpClient';

import type { CreateExpenseRequest, Expense } from '../types';

export async function getExpensesByGroupId(groupId: string): Promise<Expense[]> {
	return apiGet<Expense[]>(`/api/groups/${groupId}/expenses`);
}

export async function createExpense(groupId: string, request: CreateExpenseRequest): Promise<Expense> {
  return apiPost<Expense, CreateExpenseRequest>(`/api/groups/${groupId}/expenses`, request);
}
