import { apiConfig } from '../config/env';

export type ApiError = {
  status: number;
  message: string;
};

type ApiErrorResponse = {
  message?: string;
};

async function buildApiError(response: Response): Promise<ApiError> {
  let message = `Request failed: ${response.status}`;

  try {
    const errorBody = (await response.json()) as ApiErrorResponse;
    if (typeof errorBody.message === 'string' && errorBody.message.trim().length > 0) {
      message = errorBody.message;
    }
  } catch {
    // Keep default message when response body is missing or invalid JSON.
  }

  return {
    status: response.status,
    message,
  };
}

export async function apiGet<T>(path: string): Promise<T> {
  const response = await fetch(`${apiConfig.baseUrl}${path}`, {
    method: 'GET',
    headers: {
      Accept: 'application/json',
    },
  });

  if (!response.ok) {
    throw await buildApiError(response);
  }

  return (await response.json()) as T;
}

export async function apiPost<TResponse, TBody>(path: string, body: TBody): Promise<TResponse> {
  const response = await fetch(`${apiConfig.baseUrl}${path}`, {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw await buildApiError(response);
  }

  return (await response.json()) as TResponse;
}
