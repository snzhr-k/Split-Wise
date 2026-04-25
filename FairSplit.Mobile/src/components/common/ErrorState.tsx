import { Pressable, StyleSheet, Text, View } from 'react-native';

import { theme } from '../../theme';

type ErrorStateProps = {
  message: string;
  onRetry?: () => void;
};

export function ErrorState({ message, onRetry }: ErrorStateProps) {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Something went wrong</Text>
      <Text style={styles.message}>{message}</Text>
      {onRetry ? (
        <Pressable onPress={onRetry} style={styles.retryButton}>
          <Text style={styles.retryText}>Try again</Text>
        </Pressable>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    gap: theme.spacing.md,
    padding: theme.spacing.xl,
  },
  title: {
    fontSize: theme.typography.fontSize.lg,
    lineHeight: theme.typography.lineHeight.lg,
    fontWeight: theme.typography.fontWeight.semibold,
    color: theme.colors.textPrimary,
  },
  message: {
    textAlign: 'center',
    fontSize: theme.typography.fontSize.md,
    lineHeight: theme.typography.lineHeight.md,
    color: theme.colors.textSecondary,
  },
  retryButton: {
    minHeight: theme.sizing.buttonHeight,
    paddingHorizontal: theme.spacing.lg,
    borderRadius: theme.radius.md,
    justifyContent: 'center',
    backgroundColor: theme.colors.brandPrimary,
  },
  retryText: {
    color: theme.colors.textInverse,
    fontSize: theme.typography.fontSize.md,
    fontWeight: theme.typography.fontWeight.semibold,
  },
});
