import { useMemo, useState } from 'react';

import { BalancesScreen } from '../../features/balances/screens/BalancesScreen';
import { CreateExpenseScreen } from '../../features/expenses/screens/CreateExpenseScreen';
import { ExpenseDetailsScreen } from '../../features/expenses/screens/ExpenseDetailsScreen';
import { ExpensesListScreen } from '../../features/expenses/screens/ExpensesListScreen';
import { GroupDetailsScreen } from '../../features/groups/screens/GroupHomeScreen';
import { GroupsListScreen } from '../../features/groups/screens/GroupsListScreen';
import type { AppRoute } from '../../types/navigation';

export function RootNavigator() {
	const [stack, setStack] = useState<AppRoute[]>([{ name: 'GroupsList' }]);

	const currentRoute = stack[stack.length - 1];

	const routeRegistry = useMemo(
		() => ({
			GroupsList: 'GroupsListScreen',
			GroupDetails: 'GroupDetailsScreen',
			ExpensesList: 'ExpensesListScreen',
			ExpenseDetails: 'ExpenseDetailsScreen',
			CreateExpense: 'CreateExpenseScreen',
			Balances: 'BalancesScreen',
		}),
		[],
	);

	void routeRegistry;

	const push = (route: AppRoute) => {
		setStack(previous => [...previous, route]);
	};

	const pop = () => {
		setStack(previous => {
			if (previous.length <= 1) {
				return previous;
			}

			return previous.slice(0, -1);
		});
	};

	if (currentRoute.name === 'GroupsList') {
		return (
			<GroupsListScreen
				onOpenGroupDetails={group => push({ name: 'GroupDetails', params: { group } })}
				onOpenGroupExpenses={group => push({ name: 'ExpensesList', params: { group } })}
			/>
		);
	}

	if (currentRoute.name === 'GroupDetails') {
		return (
			<GroupDetailsScreen
				group={currentRoute.params.group}
				onBack={pop}
				onOpenExpenses={() => push({ name: 'ExpensesList', params: { group: currentRoute.params.group } })}
				onOpenBalances={() => push({ name: 'Balances', params: { group: currentRoute.params.group } })}
			/>
		);
	}

	if (currentRoute.name === 'ExpensesList') {
		return (
			<ExpensesListScreen
				group={currentRoute.params.group}
				onBack={pop}
				onOpenExpenseDetails={expenseId =>
					push({ name: 'ExpenseDetails', params: { group: currentRoute.params.group, expenseId } })
				}
				onOpenCreateExpense={() => push({ name: 'CreateExpense', params: { group: currentRoute.params.group } })}
			/>
		);
	}

	if (currentRoute.name === 'ExpenseDetails') {
		return (
			<ExpenseDetailsScreen
				group={currentRoute.params.group}
				expenseId={currentRoute.params.expenseId}
				onBack={pop}
			/>
		);
	}

	if (currentRoute.name === 'CreateExpense') {
		return <CreateExpenseScreen group={currentRoute.params.group} onBack={pop} />;
	}

	return <BalancesScreen group={currentRoute.params.group} onBack={pop} />;
}
