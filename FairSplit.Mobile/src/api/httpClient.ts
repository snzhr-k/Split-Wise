import type { BackendError, ValidationDetail, NetworkError } from '../services/errorTypes';
import { apiConfig } from '../config/env';

/**
 * Backend error response structure matching our backend error taxonomy.
 */
type BackendErrorResponse = {
  code?: string;
  message?: string;
  details?: Array<{
    field: string;
    issue: string;
    value?: unknown;
  }>;
  traceId?: string;
};

/**
 * Parse backend error response into structured BackendError.
 * Handles both new structured errors and legacy error responses.
 */
async function parseBackendError(response: Response): Promise<BackendError | NetworkError> {
  let errorBody: BackendErrorResponse | null = null;

  try {
    errorBody = (await response.json()) as BackendErrorResponse;
  } catch {
    // Response is not valid JSON - treat as network error
    return {
      code: 'NETWORK_ERROR',
      message: `Request failed with status ${response.status}`,
      originalError: new Error(`HTTP ${response.status}`),
    };
  }

  // If we got valid JSON, structure it as BackendError
  const code = errorBody?.code || 'UNKNOWN_ERROR';
  const message = errorBody?.message || `Request failed: ${response.status}`;
  const details = (errorBody?.details || []) as ValidationDetail[];
  const traceId = errorBody?.traceId || 'unknown';

  return {
    code,
    message,
    details,
    traceId,
  };
}

function buildHeaders(contentType?: string) {
  const headers: Record<string, string> = {
    Accept: 'application/json',
  };

  if (contentType) {
    headers['Content-Type'] = contentType;
  }

  if (apiConfig.authToken) {
    headers.Authorization = `Bearer ${apiConfig.authToken}`;
  }

  return headers;
}

export async function apiGet<T>(path: string): Promise<T> {
  try {
    const response = await fetch(`${apiConfig.baseUrl}${path}`, {
      method: 'GET',
      headers: buildHeaders(),
    });

    if (!response.ok) {
      throw await parseBackendError(response);
    }

    return (await response.json()) as T;
  } catch (error) {
    // If already structured error, re-throw
    if (typeof error === 'object' && error !== null && 'code' in error) {
      throw error;
    }

    // Network error (fetch failed, no response)
    throw {
      code: 'NETWORK_ERROR',
      message: 'Failed to connect to server. Please check your connection.',
      originalError: error,
    } as NetworkError;
  }
}

export async function apiPost<TResponse, TBody>(path: string, body: TBody): Promise<TResponse> {
  try {
    const response = await fetch(`${apiConfig.baseUrl}${path}`, {
      method: 'POST',
      headers: buildHeaders('application/json'),
      body: JSON.stringify(body),
    });

    if (!response.ok) {
      throw await parseBackendError(response);
    }

    return (await response.json()) as TResponse;
  } catch (error) {
    // If already structured error, re-throw
    if (typeof error === 'object' && error !== null && 'code' in error) {
      throw error;
    }

    // Network error (fetch failed, no response)
    throw {
      code: 'NETWORK_ERROR',
      message: 'Failed to connect to server. Please check your connection.',
      originalError: error,
    } as NetworkError;
  }
}
