import { apiConfig } from '../config/env';

export type ApiError = {
  status: number;
  message: string;
};

export async function apiGet<T>(path: string): Promise<T> {
  const response = await fetch(`${apiConfig.baseUrl}${path}`, {
    method: 'GET',
    headers: {
      Accept: 'application/json',
    },
  });

  if (!response.ok) {
    throw {
      status: response.status,
      message: `Request failed: ${response.status}`,
    } as ApiError;
  }

  return (await response.json()) as T;
}
