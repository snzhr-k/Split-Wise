import { Pressable, StyleSheet, Text, View } from 'react-native';

import { EmptyState } from '../../../components/common/EmptyState';
import { ErrorState } from '../../../components/common/ErrorState';
import { LoadingState } from '../../../components/common/LoadingState';
import { theme } from '../../../theme';
import type { Group } from '../../groups/types';
import { ExpenseCard } from '../components/ExpenseCard';
import { useExpenses } from '../hooks/useExpenses';

type ExpensesListScreenProps = {
	group: Group;
	onBack: () => void;
	onOpenExpenseDetails: (expenseId: string) => void;
	onOpenCreateExpense: () => void;
};

export function ExpensesListScreen({
	group,
	onBack,
	onOpenExpenseDetails,
	onOpenCreateExpense,
}: ExpensesListScreenProps) {
	const { expenses, isLoading, errorMessage, reload } = useExpenses(group.id);

	if (isLoading) {
		return <LoadingState label="Loading expenses..." />;
	}

	if (errorMessage) {
		return <ErrorState message={errorMessage} onRetry={reload} />;
	}

	return (
		<View style={styles.container}>
			<Pressable style={styles.backButton} onPress={onBack}>
				<Text style={styles.backText}>Back</Text>
			</Pressable>

			<Text style={styles.title}>Expenses</Text>
			<Text style={styles.subtitle}>{group.name}</Text>

			<Pressable style={styles.primaryAction} onPress={onOpenCreateExpense}>
				<Text style={styles.primaryActionText}>Create expense</Text>
			</Pressable>

			{expenses.length === 0 ? (
				<EmptyState
					title="No expenses yet"
					description="Add the first expense in this group to start tracking balances."
				/>
			) : (
				<View style={styles.list}>
					{expenses.map(expense => (
						<ExpenseCard
							key={expense.id}
							expense={expense}
							onPress={() => onOpenExpenseDetails(expense.id)}
						/>
					))}
				</View>
			)}
		</View>
	);
}

const styles = StyleSheet.create({
	container: {
		flex: 1,
		paddingHorizontal: theme.spacing.xl,
		paddingVertical: theme.spacing.xxl,
		gap: theme.spacing.lg,
		backgroundColor: theme.colors.background,
	},
	backButton: {
		alignSelf: 'flex-start',
		backgroundColor: theme.colors.surfaceMuted,
		borderRadius: theme.radius.round,
		paddingHorizontal: theme.spacing.lg,
		minHeight: theme.sizing.buttonHeight,
		justifyContent: 'center',
	},
	backText: {
		fontSize: theme.typography.fontSize.md,
		fontWeight: theme.typography.fontWeight.semibold,
		color: theme.colors.textSecondary,
	},
	title: {
		fontSize: theme.typography.fontSize.xxl,
		lineHeight: theme.typography.lineHeight.xxl,
		fontWeight: theme.typography.fontWeight.bold,
		color: theme.colors.textPrimary,
	},
	subtitle: {
		fontSize: theme.typography.fontSize.md,
		lineHeight: theme.typography.lineHeight.md,
		color: theme.colors.textSecondary,
	},
	list: {
		gap: theme.spacing.md,
	},
	primaryAction: {
		backgroundColor: theme.colors.brandPrimary,
		borderRadius: theme.radius.md,
		minHeight: theme.sizing.buttonHeight,
		paddingHorizontal: theme.spacing.lg,
		justifyContent: 'center',
		alignSelf: 'flex-start',
	},
	primaryActionText: {
		fontSize: theme.typography.fontSize.md,
		fontWeight: theme.typography.fontWeight.semibold,
		color: theme.colors.textInverse,
	},
});
