import type { Group } from '../features/groups/types';

export type AppRoute =
	| { name: 'GroupsList' }
	| { name: 'GroupDetails'; params: { group: Group } }
	| { name: 'ExpensesList'; params: { group: Group } }
	| { name: 'ExpenseDetails'; params: { group: Group; expenseId: string } }
	| { name: 'CreateExpense'; params: { group: Group } }
	| { name: 'Balances'; params: { group: Group } };
