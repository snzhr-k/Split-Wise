import { Pressable, StyleSheet, Text, View } from 'react-native';

import { theme } from '../../../theme';
import type { Group } from '../types';

type GroupDetailsScreenProps = {
	group: Group;
	onBack: () => void;
	onOpenBalances: () => void;
	onOpenExpenses: () => void;
};

export function GroupDetailsScreen({
	group,
	onBack,
	onOpenBalances,
	onOpenExpenses,
}: GroupDetailsScreenProps) {
	return (
		<View style={styles.container}>
			<Pressable style={styles.backButton} onPress={onBack}>
				<Text style={styles.backText}>Back to groups</Text>
			</Pressable>

			<Text style={styles.title}>{group.name}</Text>
			<Text style={styles.subtitle}>Group details entry point</Text>

			<View style={styles.card}>
				<Text style={styles.cardTitle}>Current MVP routes</Text>
				<Text style={styles.cardText}>Navigate to expenses or balances for this group.</Text>
				<Text style={styles.cardMeta}>Group ID: {group.id}</Text>

				<View style={styles.actionsRow}>
					<Pressable style={styles.primaryAction} onPress={onOpenExpenses}>
						<Text style={styles.primaryActionText}>Expenses</Text>
					</Pressable>
					<Pressable style={styles.secondaryAction} onPress={onOpenBalances}>
						<Text style={styles.secondaryActionText}>Balances</Text>
					</Pressable>
				</View>
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
	subtitle: {
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
		gap: theme.spacing.sm,
		...theme.shadows.card,
	},
	cardTitle: {
		fontSize: theme.typography.fontSize.lg,
		lineHeight: theme.typography.lineHeight.lg,
		fontWeight: theme.typography.fontWeight.semibold,
		color: theme.colors.textPrimary,
	},
	cardText: {
		fontSize: theme.typography.fontSize.md,
		lineHeight: theme.typography.lineHeight.md,
		color: theme.colors.textSecondary,
	},
	cardMeta: {
		fontSize: theme.typography.fontSize.sm,
		lineHeight: theme.typography.lineHeight.sm,
		color: theme.colors.textMuted,
	},
	actionsRow: {
		flexDirection: 'row',
		gap: theme.spacing.sm,
		marginTop: theme.spacing.sm,
	},
	primaryAction: {
		backgroundColor: theme.colors.brandPrimary,
		borderRadius: theme.radius.md,
		minHeight: theme.sizing.buttonHeight,
		paddingHorizontal: theme.spacing.lg,
		justifyContent: 'center',
	},
	primaryActionText: {
		fontSize: theme.typography.fontSize.md,
		fontWeight: theme.typography.fontWeight.semibold,
		color: theme.colors.textInverse,
	},
	secondaryAction: {
		backgroundColor: theme.colors.surfaceMuted,
		borderRadius: theme.radius.md,
		minHeight: theme.sizing.buttonHeight,
		paddingHorizontal: theme.spacing.lg,
		justifyContent: 'center',
	},
	secondaryActionText: {
		fontSize: theme.typography.fontSize.md,
		fontWeight: theme.typography.fontWeight.semibold,
		color: theme.colors.textSecondary,
	},
});
