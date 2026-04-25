import { StatusBar } from 'expo-status-bar';
import { SafeAreaView, StyleSheet } from 'react-native';

import { RootNavigator } from './src/app/navigation/RootNavigator';
import { theme } from './src/theme';

export default function App() {
  return (
    <SafeAreaView style={styles.container}>
      <RootNavigator />
      <StatusBar style="dark" />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: theme.colors.background,
  },
});
