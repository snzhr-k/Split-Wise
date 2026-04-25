import { Pressable, StyleSheet, Text, View } from 'react-native';

import { EmptyState } from '../../../components/common/EmptyState';
import { ErrorState } from '../../../components/common/ErrorState';
import { LoadingState } from '../../../components/common/LoadingState';
import { theme } from '../../../theme';
import { useGroups } from '../hooks/useGroups';
import type { Group } from '../types';

type GroupsListScreenProps = {
	onOpenGroupDetails: (group: Group) => void;
	onOpenGroupExpenses: (group: Group) => void;
};

export function GroupsListScreen({ onOpenGroupDetails, onOpenGroupExpenses }: GroupsListScreenProps) {
	const { groups, isLoading, errorMessage, reload } = useGroups();

	if (isLoading) {
		return <LoadingState label="Loading groups..." />;
	}

	if (errorMessage) {
		return <ErrorState message={errorMessage} onRetry={reload} />;
	}

	if (groups.length === 0) {
		return (
			<EmptyState
				title="No groups yet"
				description="Create your first group from the backend or seed data, then pull to refresh later."
			/>
		);
	}

	return (
		<View style={styles.container}>
			<Text style={styles.title}>Your Groups</Text>
			<Text style={styles.subtitle}>Choose a group to open its workspace.</Text>

			<View style={styles.list}>
				{groups.map(group => (
					<View key={group.id} style={styles.groupCard}>
						<Pressable onPress={() => onOpenGroupDetails(group)}>
							<Text style={styles.groupName}>{group.name}</Text>
							<Text style={styles.groupId}>ID: {group.id}</Text>
						</Pressable>

						<Pressable style={styles.expensesButton} onPress={() => onOpenGroupExpenses(group)}>
							<Text style={styles.expensesButtonText}>Open expenses</Text>
						</Pressable>
					</View>
				))}
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
	groupCard: {
		backgroundColor: theme.colors.surface,
		borderRadius: theme.radius.md,
		borderWidth: 1,
		borderColor: theme.colors.border,
		padding: theme.spacing.lg,
		gap: theme.spacing.xs,
		...theme.shadows.card,
	},
	expensesButton: {
		marginTop: theme.spacing.sm,
		alignSelf: 'flex-start',
		backgroundColor: theme.colors.brandPrimary,
		borderRadius: theme.radius.md,
		minHeight: theme.sizing.buttonHeight,
		paddingHorizontal: theme.spacing.lg,
		justifyContent: 'center',
	},
	expensesButtonText: {
		fontSize: theme.typography.fontSize.md,
		fontWeight: theme.typography.fontWeight.semibold,
		color: theme.colors.textInverse,
	},
	groupName: {
		fontSize: theme.typography.fontSize.lg,
		lineHeight: theme.typography.lineHeight.lg,
		fontWeight: theme.typography.fontWeight.semibold,
		color: theme.colors.textPrimary,
	},
	groupId: {
		fontSize: theme.typography.fontSize.sm,
		lineHeight: theme.typography.lineHeight.sm,
		color: theme.colors.textMuted,
	},
});
