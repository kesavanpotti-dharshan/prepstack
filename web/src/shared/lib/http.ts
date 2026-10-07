import type { components } from '../api/schema'
import { ApiError } from './api-error'
import { tokenStore } from './token-store'

// Proxied to the API by vite.config.ts in dev so the refresh cookie is same-origin.
const API_BASE_URL = '/api/v1'

type ProblemDetails = components['schemas']['ProblemDetails'] & { code?: string }
type ValidationProblemDetails = components['schemas']['HttpValidationProblemDetails'] & { code?: string }

export interface ApiFetchOptions extends Omit<RequestInit, 'body'> {
  body?: unknown
}

let refreshPromise: Promise<boolean> | null = null

/** Exchanges the httpOnly refresh cookie for a new access token. */
export async function refreshAccessToken(): Promise<boolean> {
  refreshPromise ??= performRefresh().finally(() => {
    refreshPromise = null
  })
  return refreshPromise
}

async function performRefresh(): Promise<boolean> {
  try {
    const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
      method: 'POST',
      credentials: 'include',
    })
    if (!response.ok) {
      tokenStore.set(null)
      return false
    }
    const body = (await response.json()) as components['schemas']['AuthResponse']
    tokenStore.set(body.accessToken)
    return true
  } catch {
    tokenStore.set(null)
    return false
  }
}

async function rawFetch(path: string, options: ApiFetchOptions): Promise<Response> {
  const headers = new Headers(options.headers)
  const token = tokenStore.get()
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  let body: BodyInit | undefined
  if (options.body !== undefined) {
    headers.set('Content-Type', 'application/json')
    body = JSON.stringify(options.body)
  }

  return fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
    body,
    credentials: 'include',
  })
}

const AUTH_PATH_PREFIX = '/auth/'

/**
 * Typed fetch wrapper. Retries once via silent refresh on a 401, except for
 * auth endpoints themselves — a 401 there means bad credentials/token, not
 * an expired access token.
 */
export async function apiFetch<T>(path: string, options: ApiFetchOptions = {}): Promise<T> {
  let response = await rawFetch(path, options)

  if (response.status === 401 && !path.startsWith(AUTH_PATH_PREFIX)) {
    const refreshed = await refreshAccessToken()
    if (refreshed) {
      response = await rawFetch(path, options)
    }
  }

  if (!response.ok) {
    throw await toApiError(response)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

async function toApiError(response: Response): Promise<ApiError> {
  let problem: (ProblemDetails & ValidationProblemDetails) | undefined
  try {
    problem = await response.json()
  } catch {
    problem = undefined
  }

  return new ApiError(response.status, problem?.title ?? response.statusText, {
    code: problem?.code,
    fieldErrors: problem?.errors,
  })
}
