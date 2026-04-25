import { StyleSheet, Text, View } from 'react-native';

import { apiConfig, getApiBaseUrlHelpText } from '../config/env';

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
    paddingHorizontal: 20,
    paddingVertical: 24,
    gap: 16,
  },
  title: {
    fontSize: 30,
    fontWeight: '700',
    color: '#1f2a37',
  },
  subtitle: {
    fontSize: 16,
    color: '#4b5563',
  },
  card: {
    backgroundColor: '#ffffff',
    borderRadius: 12,
    padding: 16,
    borderWidth: 1,
    borderColor: '#e5e7eb',
    gap: 6,
  },
  cardLabel: {
    fontSize: 13,
    fontWeight: '600',
    textTransform: 'uppercase',
    color: '#6b7280',
  },
  cardValue: {
    fontSize: 15,
    color: '#111827',
  },
  helpText: {
    fontSize: 14,
    color: '#374151',
  },
  todoText: {
    marginTop: 4,
    fontSize: 14,
    color: '#6b7280',
  },
});
