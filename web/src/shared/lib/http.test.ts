import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ApiError } from './api-error'
import { apiFetch } from './http'
import { tokenStore } from './token-store'

function jsonResponse(body: unknown, init: ResponseInit = {}): Response {
  return new Response(JSON.stringify(body), {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  })
}

function problemResponse(body: unknown, status: number): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/problem+json' },
  })
}

describe('apiFetch', () => {
  beforeEach(() => {
    tokenStore.set(null)
    vi.unstubAllGlobals()
  })

  it('attaches the bearer token from the token store', async () => {
    tokenStore.set('access-token')
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse({ ok: true }))
    vi.stubGlobal('fetch', fetchMock)

    await apiFetch('/topics')

    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit]
    expect((init.headers as Headers).get('Authorization')).toBe('Bearer access-token')
  })

  it('retries once via silent refresh on a 401 for non-auth endpoints', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(
        jsonResponse({ accessToken: 'new-token', accessTokenExpiresAt: '2030-01-01T00:00:00Z' }),
      )
      .mockResolvedValueOnce(jsonResponse({ ok: true }))
    vi.stubGlobal('fetch', fetchMock)

    const result = await apiFetch<{ ok: boolean }>('/topics')

    expect(result).toEqual({ ok: true })
    expect(fetchMock).toHaveBeenCalledTimes(3)
    expect(tokenStore.get()).toBe('new-token')
  })

  it('gives up without retrying when the silent refresh also fails', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
    vi.stubGlobal('fetch', fetchMock)

    await expect(apiFetch('/topics')).rejects.toBeInstanceOf(ApiError)
    expect(fetchMock).toHaveBeenCalledTimes(2)
    expect(tokenStore.get()).toBeNull()
  })

  it('does not attempt a silent refresh for auth endpoints', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      problemResponse({ title: 'Invalid email or password.', code: 'auth.invalid_credentials' }, 401),
    )
    vi.stubGlobal('fetch', fetchMock)

    await expect(
      apiFetch('/auth/login', { method: 'POST', body: { email: 'a@b.com', password: 'wrong' } }),
    ).rejects.toBeInstanceOf(ApiError)
    expect(fetchMock).toHaveBeenCalledTimes(1)
  })

  it('throws an ApiError carrying field errors from a validation problem', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      problemResponse(
        { title: 'One or more validation errors occurred.', errors: { Email: ['Email is required.'] } },
        400,
      ),
    )
    vi.stubGlobal('fetch', fetchMock)

    const error = (await apiFetch('/auth/register', { method: 'POST', body: {} }).catch((e) => e)) as ApiError

    expect(error).toBeInstanceOf(ApiError)
    expect(error.fieldErrors).toEqual({ Email: ['Email is required.'] })
  })

  it('returns undefined for a 204 No Content response', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 204 }))
    vi.stubGlobal('fetch', fetchMock)

    const result = await apiFetch('/auth/logout', { method: 'POST' })

    expect(result).toBeUndefined()
  })
})
