import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createBrowserRouter, RouterProvider } from 'react-router'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { tokenStore } from '../shared/lib/token-store'
import { createFakeAccessToken } from '../shared/testing/fake-jwt'
import { AppProviders } from './providers'
import { routes } from './router'

function authResponse(accessToken: string) {
  return new Response(JSON.stringify({ accessToken, accessTokenExpiresAt: '2030-01-01T00:00:00Z' }), {
    status: 200,
    headers: { 'Content-Type': 'application/json' },
  })
}

function jsonResponse(body: unknown) {
  return new Response(JSON.stringify(body), { status: 200, headers: { 'Content-Type': 'application/json' } })
}

function renderAppAt(path: string) {
  window.history.pushState({}, '', path)
  // A fresh router per test avoids carrying navigation state from a previous test.
  const router = createBrowserRouter(routes)
  return render(
    <AppProviders>
      <RouterProvider router={router} />
    </AppProviders>,
  )
}

describe('App', () => {
  beforeEach(() => {
    tokenStore.set(null)
  })

  it('redirects an unauthenticated visitor to the login page', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 401 })))

    renderAppAt('/')

    expect(await screen.findByRole('heading', { name: 'Log in' })).toBeInTheDocument()

    vi.unstubAllGlobals()
  })

  it('shows the authenticated layout after a successful silent refresh, and logging out returns to login', async () => {
    const user = userEvent.setup()
    const token = createFakeAccessToken({ sub: 'user-1', email: 'me@prepstack.dev' })
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(authResponse(token)) // silent refresh on load
      .mockResolvedValueOnce(jsonResponse([])) // GET /topics (empty tree)
      .mockResolvedValueOnce(new Response(null, { status: 204 })) // logout
    vi.stubGlobal('fetch', fetchMock)

    renderAppAt('/')

    expect(await screen.findByRole('heading', { name: 'Topics' })).toBeInTheDocument()
    expect(screen.getByText('me@prepstack.dev')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Log out' }))

    expect(await screen.findByRole('heading', { name: 'Log in' })).toBeInTheDocument()

    vi.unstubAllGlobals()
  })
})
