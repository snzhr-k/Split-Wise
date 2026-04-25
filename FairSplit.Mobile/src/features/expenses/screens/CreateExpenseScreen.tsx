import { useMemo, useState } from 'react';
import { Pressable, ScrollView, StyleSheet, Text, TextInput, View } from 'react-native';

import { theme } from '../../../theme';
import type { Group } from '../../groups/types';
import { useGroupMembers } from '../../members/hooks/useGroupMembers';
import type { Member } from '../../members/types';
import { useCreateExpense } from '../hooks/useCreateExpense';

type CreateExpenseScreenProps = {
	group: Group;
	onBack: () => void;
};

export function CreateExpenseScreen({ group, onBack }: CreateExpenseScreenProps) {
	const [amountInput, setAmountInput] = useState('');
	const [payerMemberId, setPayerMemberId] = useState('');
	const [selectedParticipantIds, setSelectedParticipantIds] = useState<string[]>([]);
	const [validationError, setValidationError] = useState<string | null>(null);

	const { members, isLoading, errorMessage: membersErrorMessage, reload } = useGroupMembers(group.id);

	const { isSubmitting, errorMessage, submit } = useCreateExpense(group.id);

	const normalizedPayerId = payerMemberId.trim();
	const hasMembers = members.length > 0;

	const canSubmit = useMemo(() => {
		if (isSubmitting) {
			return false;
		}

		if (!hasMembers) {
			return false;
		}

		const amount = Number(amountInput);
		if (!amount || Number.isNaN(amount) || amount <= 0) {
			return false;
		}

		if (!normalizedPayerId) {
			return false;
		}

		if (selectedParticipantIds.length === 0) {
			return false;
		}

		return true;
	}, [amountInput, hasMembers, isSubmitting, normalizedPayerId, selectedParticipantIds.length]);

	const toggleParticipant = (memberId: string) => {
		setSelectedParticipantIds(previous => {
			if (previous.includes(memberId)) {
				return previous.filter(id => id !== memberId);
			}

			return [...previous, memberId];
		});
	};

	const selectPayer = (memberId: string) => {
		setPayerMemberId(memberId);

		if (!selectedParticipantIds.includes(memberId)) {
			setSelectedParticipantIds(previous => [...previous, memberId]);
		}
	};

	const getMemberLabel = (member: Member) => {
		return `${member.displayName} (${member.id.slice(0, 8)})`;
	};

	const handleSubmit = async () => {
		setValidationError(null);

		const amount = Number(amountInput);
		if (!amount || Number.isNaN(amount) || amount <= 0) {
			setValidationError('Amount must be a number greater than zero.');
			return;
		}

		if (!hasMembers) {
			setValidationError('No group members found. Add members first before creating an expense.');
			return;
		}

		if (!normalizedPayerId) {
			setValidationError('Please select a payer.');
			return;
		}

		if (selectedParticipantIds.length === 0) {
			setValidationError('Select at least one participant.');
			return;
		}

		const createdExpense = await submit({
			payerMemberId: normalizedPayerId,
			amount,
			splitType: 'equal',
			participants: selectedParticipantIds.map(memberId => ({ memberId })),
		});

		if (!createdExpense) {
			return;
		}

		onBack();
	};

	return (
		<ScrollView contentContainerStyle={styles.container} keyboardShouldPersistTaps="handled">
			<Pressable style={styles.backButton} onPress={onBack}>
				<Text style={styles.backText}>Back to expenses</Text>
			</Pressable>

			<Text style={styles.title}>Create Expense</Text>
			<Text style={styles.meta}>Group: {group.name}</Text>

			<View style={styles.card}>
				<Text style={styles.sectionLabel}>Amount</Text>
				<TextInput
					value={amountInput}
					onChangeText={setAmountInput}
					placeholder="e.g. 120.50"
					keyboardType="decimal-pad"
					style={styles.input}
				/>

				<Text style={styles.sectionLabel}>Split type</Text>
				<Text style={styles.helperText}>Equal split (current MVP scope)</Text>

				<Text style={styles.sectionLabel}>Payer</Text>
				{isLoading ? (
					<Text style={styles.helperText}>Loading members...</Text>
				) : membersErrorMessage ? (
					<View style={styles.errorRow}>
						<Text style={styles.errorText}>{membersErrorMessage}</Text>
						<Pressable style={styles.retryButton} onPress={reload}>
							<Text style={styles.retryButtonText}>Retry</Text>
						</Pressable>
					</View>
				) : members.length === 0 ? (
					<Text style={styles.helperText}>No members in this group yet.</Text>
				) : (
					<View style={styles.chipsRow}>
						{members.map(member => {
							const isSelected = normalizedPayerId === member.id;
							return (
								<Pressable
									key={member.id}
									onPress={() => selectPayer(member.id)}
									style={[styles.chip, isSelected ? styles.chipSelected : null]}
								>
									<Text style={[styles.chipText, isSelected ? styles.chipTextSelected : null]}>{getMemberLabel(member)}</Text>
								</Pressable>
							);
						})}
					</View>
				)}

				<Text style={styles.sectionLabel}>Participants</Text>
				{isLoading ? (
					<Text style={styles.helperText}>Loading members...</Text>
				) : membersErrorMessage ? (
					<Text style={styles.errorText}>Could not load members for participant selection.</Text>
				) : members.length === 0 ? (
					<Text style={styles.helperText}>No members available.</Text>
				) : (
					<View style={styles.chipsRow}>
						{members.map(member => {
							const isSelected = selectedParticipantIds.includes(member.id);
							return (
								<Pressable
									key={member.id}
									onPress={() => toggleParticipant(member.id)}
									style={[styles.chip, isSelected ? styles.chipSelected : null]}
								>
									<Text style={[styles.chipText, isSelected ? styles.chipTextSelected : null]}>{getMemberLabel(member)}</Text>
								</Pressable>
							);
						})}
					</View>
				)}

				{validationError ? <Text style={styles.errorText}>{validationError}</Text> : null}
				{errorMessage ? <Text style={styles.errorText}>{errorMessage}</Text> : null}

				<Pressable
					style={[styles.submitButton, !canSubmit ? styles.submitButtonDisabled : null]}
					onPress={handleSubmit}
					disabled={!canSubmit}
				>
					<Text style={styles.submitButtonText}>{isSubmitting ? 'Submitting...' : 'Create expense'}</Text>
				</Pressable>
			</View>
		</ScrollView>
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
		gap: theme.spacing.sm,
		...theme.shadows.card,
	},
	sectionLabel: {
		fontSize: theme.typography.fontSize.md,
		lineHeight: theme.typography.lineHeight.md,
		fontWeight: theme.typography.fontWeight.semibold,
		color: theme.colors.textPrimary,
	},
	helperText: {
		fontSize: theme.typography.fontSize.md,
		lineHeight: theme.typography.lineHeight.md,
		color: theme.colors.textSecondary,
	},
	input: {
		backgroundColor: theme.colors.surfaceMuted,
		borderRadius: theme.radius.md,
		borderWidth: 1,
		borderColor: theme.colors.border,
		paddingHorizontal: theme.spacing.md,
		paddingVertical: theme.spacing.sm,
		fontSize: theme.typography.fontSize.md,
		lineHeight: theme.typography.lineHeight.md,
		color: theme.colors.textPrimary,
	},
	chipsRow: {
		flexDirection: 'row',
		flexWrap: 'wrap',
		gap: theme.spacing.xs,
	},
	chip: {
		backgroundColor: theme.colors.surfaceMuted,
		borderRadius: theme.radius.round,
		borderWidth: 1,
		borderColor: theme.colors.border,
		paddingHorizontal: theme.spacing.md,
		paddingVertical: theme.spacing.xs,
	},
	chipSelected: {
		backgroundColor: theme.colors.brandPrimary,
		borderColor: theme.colors.brandPrimary,
	},
	chipText: {
		fontSize: theme.typography.fontSize.sm,
		lineHeight: theme.typography.lineHeight.sm,
		color: theme.colors.textSecondary,
	},
	chipTextSelected: {
		color: theme.colors.textInverse,
	},
	errorRow: {
		gap: theme.spacing.sm,
	},
	retryButton: {
		backgroundColor: theme.colors.surfaceMuted,
		borderRadius: theme.radius.md,
		minHeight: theme.sizing.buttonHeight,
		paddingHorizontal: theme.spacing.lg,
		justifyContent: 'center',
		alignSelf: 'flex-start',
	},
	retryButtonText: {
		fontSize: theme.typography.fontSize.md,
		fontWeight: theme.typography.fontWeight.semibold,
		color: theme.colors.textSecondary,
	},
	errorText: {
		fontSize: theme.typography.fontSize.sm,
		lineHeight: theme.typography.lineHeight.sm,
		color: theme.colors.danger,
	},
	submitButton: {
		marginTop: theme.spacing.sm,
		backgroundColor: theme.colors.brandPrimary,
		borderRadius: theme.radius.md,
		minHeight: theme.sizing.buttonHeight,
		paddingHorizontal: theme.spacing.lg,
		justifyContent: 'center',
		alignSelf: 'flex-start',
	},
	submitButtonDisabled: {
		opacity: 0.5,
	},
	submitButtonText: {
		fontSize: theme.typography.fontSize.md,
		fontWeight: theme.typography.fontWeight.semibold,
		color: theme.colors.textInverse,
	},
});
