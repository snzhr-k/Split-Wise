import { Platform } from 'react-native';

const envApiBaseUrl = process.env.EXPO_PUBLIC_API_BASE_URL?.trim();
const envAuthToken = process.env.EXPO_PUBLIC_DEV_AUTH_TOKEN?.trim();

function getFallbackBaseUrl() {
  if (Platform.OS === 'android') {
    // Android emulator cannot use localhost for host machine services.
    return 'http://10.0.2.2:5000';
  }

  return 'http://localhost:5000';
}

function normalizeBaseUrl(url: string) {
  return url.replace(/\/$/, '');
}

export const apiConfig = {
  baseUrl: normalizeBaseUrl(envApiBaseUrl || getFallbackBaseUrl()),
  authToken: envAuthToken || '',
};

export function getApiBaseUrlHelpText() {
  return 'Set EXPO_PUBLIC_API_BASE_URL in .env for real device testing (use your computer LAN IP).';
}
