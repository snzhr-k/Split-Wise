import { StyleSheet, Text, View } from 'react-native';

import { apiConfig, getApiBaseUrlHelpText } from '../config/env';
import { theme } from '../theme';

export function RootScreen() {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>FairSplit Mobile</Text>
      <Text style={styles.subtitle}>Expo app bootstrapped for local API integration.</Text>

      <View style={styles.card}>
        <Text style={styles.cardLabel}>API Base URL</Text>
        <Text style={styles.cardValue}>{apiConfig.baseUrl}</Text>
      </View>

      <Text style={styles.helpText}>{getApiBaseUrlHelpText()}</Text>
      <Text style={styles.todoText}>Business screens and API features are intentionally not implemented yet.</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    paddingHorizontal: theme.spacing.xl,
    paddingVertical: theme.spacing.xxl,
    gap: theme.spacing.lg,
  },
  title: {
    fontSize: theme.typography.fontSize.xxl,
    lineHeight: theme.typography.lineHeight.xxl,
    fontWeight: theme.typography.fontWeight.bold,
    color: theme.colors.textPrimary,
  },
  subtitle: {
    fontSize: theme.typography.fontSize.lg,
    lineHeight: theme.typography.lineHeight.lg,
    color: theme.colors.textSecondary,
  },
  card: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.md,
    padding: theme.spacing.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    gap: theme.spacing.xs + 2,
    ...theme.shadows.card,
  },
  cardLabel: {
    fontSize: theme.typography.fontSize.sm,
    lineHeight: theme.typography.lineHeight.sm,
    fontWeight: theme.typography.fontWeight.semibold,
    textTransform: 'uppercase',
    color: theme.colors.textMuted,
  },
  cardValue: {
    fontSize: theme.typography.fontSize.md,
    lineHeight: theme.typography.lineHeight.md,
    color: theme.colors.textPrimary,
  },
  helpText: {
    fontSize: theme.typography.fontSize.md,
    lineHeight: theme.typography.lineHeight.md,
    color: theme.colors.textSecondary,
  },
  todoText: {
    marginTop: theme.spacing.xs,
    fontSize: theme.typography.fontSize.md,
    lineHeight: theme.typography.lineHeight.md,
    color: theme.colors.textMuted,
  },
});
