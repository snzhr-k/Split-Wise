import { Pressable, StyleSheet, Text, View } from 'react-native';

import { theme } from '../../../theme';
import type { Group } from '../../groups/types';

type BalancesScreenProps = {
	group: Group;
	onBack: () => void;
};

export function BalancesScreen({ group, onBack }: BalancesScreenProps) {
	return (
		<View style={styles.container}>
			<Pressable style={styles.backButton} onPress={onBack}>
				<Text style={styles.backText}>Back to group details</Text>
			</Pressable>

			<Text style={styles.title}>Balances</Text>
			<Text style={styles.meta}>Group: {group.name}</Text>

			<View style={styles.card}>
				<Text style={styles.cardText}>Balances route is registered and ready for GET /api/groups/{'{groupId}'}/balances wiring.</Text>
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
