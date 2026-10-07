import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createFakeAccessToken } from '../../../shared/testing/fake-jwt'
import { tokenStore } from '../../../shared/lib/token-store'
import { AuthProvider } from '../context'
import { RequireAuth } from './RequireAuth'

function renderProtected(initialEntry: string) {
  const queryClient = new QueryClient()
  return render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <MemoryRouter initialEntries={[initialEntry]}>
          <Routes>
            <Route path="/login" element={<div>Login page</div>} />
            <Route element={<RequireAuth />}>
              <Route path="/" element={<div>Protected home</div>} />
            </Route>
          </Routes>
        </MemoryRouter>
      </AuthProvider>
    </QueryClientProvider>,
  )
}

describe('RequireAuth', () => {
  beforeEach(() => {
    tokenStore.set(null)
  })

  it('redirects to /login when the silent refresh fails', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 401 })))

    renderProtected('/')

    expect(await screen.findByText('Login page')).toBeInTheDocument()

    vi.unstubAllGlobals()
  })

  it('renders the protected content once the silent refresh succeeds', async () => {
    const token = createFakeAccessToken({ sub: 'user-1', email: 'me@prepstack.dev' })
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify({ accessToken: token, accessTokenExpiresAt: '2030-01-01T00:00:00Z' }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      ),
    )

    renderProtected('/')

    expect(await screen.findByText('Protected home')).toBeInTheDocument()

    vi.unstubAllGlobals()
  })
})
