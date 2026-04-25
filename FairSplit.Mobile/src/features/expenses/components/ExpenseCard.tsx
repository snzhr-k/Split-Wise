import { Pressable, StyleSheet, Text, View } from 'react-native';

import { theme } from '../../../theme';
import { formatCurrency } from '../../../utils/formatters/currency';
import { formatDateTime } from '../../../utils/formatters/dateTime';
import type { Expense } from '../types';

type ExpenseCardProps = {
  expense: Expense;
  onPress: () => void;
};

export function ExpenseCard({ expense, onPress }: ExpenseCardProps) {
  return (
    <Pressable style={styles.card} onPress={onPress}>
      <View style={styles.topRow}>
        <Text style={styles.amount}>{formatCurrency(expense.amount)}</Text>
        <Text style={styles.date}>{formatDateTime(expense.createdAtUtc)}</Text>
      </View>

      <Text style={styles.meta}>Payer member: {expense.payerMemberId}</Text>
      <Text style={styles.link}>Open details</Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.lg,
    gap: theme.spacing.xs,
    ...theme.shadows.card,
  },
  topRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    gap: theme.spacing.md,
  },
  amount: {
    fontSize: theme.typography.fontSize.lg,
    lineHeight: theme.typography.lineHeight.lg,
    fontWeight: theme.typography.fontWeight.semibold,
    color: theme.colors.textPrimary,
  },
  date: {
    fontSize: theme.typography.fontSize.sm,
    lineHeight: theme.typography.lineHeight.sm,
    color: theme.colors.textMuted,
  },
  meta: {
    fontSize: theme.typography.fontSize.sm,
    lineHeight: theme.typography.lineHeight.sm,
    color: theme.colors.textSecondary,
  },
  link: {
    marginTop: theme.spacing.xs,
    fontSize: theme.typography.fontSize.md,
    fontWeight: theme.typography.fontWeight.semibold,
    color: theme.colors.brandPrimary,
  },
});
