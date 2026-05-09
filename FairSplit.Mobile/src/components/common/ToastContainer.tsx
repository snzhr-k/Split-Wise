/**
 * Toast container component for displaying notifications.
 * Place this at the root of your navigation stack to show toasts app-wide.
 */

import { Animated, Pressable, StyleSheet, Text, View } from 'react-native';
import { useEffect, useRef } from 'react';

import { useToasts } from '../../app/hooks/useToasts';
import { toastService, type Toast } from '../../services/toastService';
import { theme } from '../../theme';

function ToastItem({ toast }: { toast: Toast }) {
  const bounceAnim = useRef(new Animated.Value(0)).current;

  useEffect(() => {
    Animated.spring(bounceAnim, {
      toValue: 1,
      useNativeDriver: true,
      tension: 50,
      friction: 7,
    }).start();
  }, [bounceAnim]);

  const backgroundColor = getToastBackgroundColor(toast.type);
  const borderColor = getToastBorderColor(toast.type);

  return (
    <Animated.View
      style={[
        styles.toast,
        {
          transform: [
            {
              translateY: bounceAnim.interpolate({
                inputRange: [0, 1],
                outputRange: [-100, 0],
              }),
            },
          ],
        },
      ]}
    >
      <View style={[styles.toastContent, { backgroundColor, borderLeftColor: borderColor }]}>
        <Text style={styles.toastMessage}>{toast.message}</Text>
        <Pressable
          style={styles.dismissButton}
          onPress={() => toastService.dismiss(toast.id)}
          hitSlop={8}
        >
          <Text style={styles.dismissButtonText}>✕</Text>
        </Pressable>
      </View>
    </Animated.View>
  );
}

export function ToastContainer() {
  const toasts = useToasts();

  if (toasts.length === 0) {
    return null;
  }

  return (
    <View style={styles.container}>
      {toasts.map(toast => (
        <ToastItem key={toast.id} toast={toast} />
      ))}
    </View>
  );
}

function getToastBackgroundColor(type: Toast['type']): string {
  switch (type) {
    case 'success':
      return theme.colors.success;
    case 'error':
      return theme.colors.danger;
    case 'warning':
      return '#FFF3CD';
    case 'info':
      return theme.colors.brandPrimary;
    default:
      return theme.colors.surfaceMuted;
  }
}

function getToastBorderColor(type: Toast['type']): string {
  switch (type) {
    case 'success':
      return '#28A745';
    case 'error':
      return '#DC3545';
    case 'warning':
      return '#FFA500';
    case 'info':
      return theme.colors.brandPrimary;
    default:
      return theme.colors.border;
  }
}

const styles = StyleSheet.create({
  container: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.md,
    zIndex: 9999,
    pointerEvents: 'box-none',
  },
  toast: {
    marginBottom: theme.spacing.sm,
    pointerEvents: 'auto',
  },
  toastContent: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.sm,
    borderRadius: theme.radius.md,
    borderLeftWidth: 4,
    minHeight: 48,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 3,
    elevation: 3,
  },
  toastMessage: {
    flex: 1,
    fontSize: theme.typography.fontSize.md,
    lineHeight: theme.typography.lineHeight.md,
    color: theme.colors.textPrimary,
    marginRight: theme.spacing.md,
  },
  dismissButton: {
    paddingHorizontal: theme.spacing.sm,
    paddingVertical: theme.spacing.xs,
  },
  dismissButtonText: {
    fontSize: theme.typography.fontSize.lg,
    color: theme.colors.textSecondary,
    fontWeight: '600',
  },
});
