import { Pressable, StyleSheet, Text, View } from 'react-native';

import { theme } from '../../../theme';
import type { Group } from '../../groups/types';

type ExpenseDetailsScreenProps = {
	group: Group;
	expenseId: string;
	onBack: () => void;
};

export function ExpenseDetailsScreen({ group, expenseId, onBack }: ExpenseDetailsScreenProps) {
	return (
		<View style={styles.container}>
			<Pressable style={styles.backButton} onPress={onBack}>
				<Text style={styles.backText}>Back to expenses</Text>
			</Pressable>

			<Text style={styles.title}>Expense Details</Text>
			<Text style={styles.meta}>Group: {group.name}</Text>
			<Text style={styles.meta}>Expense ID: {expenseId}</Text>

			<View style={styles.card}>
				<Text style={styles.cardText}>Details view route is registered for MVP navigation.</Text>
			</View>
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
	meta: {
		fontSize: theme.typography.fontSize.md,
		lineHeight: theme.typography.lineHeight.md,
		color: theme.colors.textSecondary,
	},
	card: {
		backgroundColor: theme.colors.surface,
		borderRadius: theme.radius.md,
		borderWidth: 1,
		borderColor: theme.colors.border,
		padding: theme.spacing.lg,
		...theme.shadows.card,
	},
	cardText: {
		fontSize: theme.typography.fontSize.md,
		lineHeight: theme.typography.lineHeight.md,
		color: theme.colors.textSecondary,
	},
});
