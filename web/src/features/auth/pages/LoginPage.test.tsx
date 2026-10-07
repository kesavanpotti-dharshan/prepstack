import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { ReactElement } from 'react'
import { MemoryRouter, Route, Routes } from 'react-router'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createFakeAccessToken } from '../../../shared/testing/fake-jwt'
import { tokenStore } from '../../../shared/lib/token-store'
import { AuthProvider } from '../context'
import { LoginPage } from './LoginPage'

function renderLoginPage(extraRoute: ReactElement = <div>Home</div>) {
  const queryClient = new QueryClient()
  return render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <MemoryRouter initialEntries={['/login']}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/" element={extraRoute} />
          </Routes>
        </MemoryRouter>
      </AuthProvider>
    </QueryClientProvider>,
  )
}

function problemResponse(body: unknown, status: number) {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/problem+json' },
  })
}

describe('LoginPage', () => {
  beforeEach(() => {
    tokenStore.set(null)
  })

  it('logs in and redirects home on valid credentials', async () => {
    const user = userEvent.setup()
    const token = createFakeAccessToken({ sub: 'user-1', email: 'me@prepstack.dev' })
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(new Response(null, { status: 401 })) // silent refresh on mount
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ accessToken: token, accessTokenExpiresAt: '2030-01-01T00:00:00Z' }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      )
    vi.stubGlobal('fetch', fetchMock)

    renderLoginPage()

    await user.type(screen.getByLabelText('Email'), 'me@prepstack.dev')
    await user.type(screen.getByLabelText('Password'), 'correct-password')
    await user.click(screen.getByRole('button', { name: 'Log in' }))

    expect(await screen.findByText('Home')).toBeInTheDocument()
    expect(tokenStore.get()).toBe(token)

    vi.unstubAllGlobals()
  })

  it('shows the server error on invalid credentials', async () => {
    const user = userEvent.setup()
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(
        problemResponse({ title: 'Invalid email or password.', code: 'auth.invalid_credentials' }, 401),
      )
    vi.stubGlobal('fetch', fetchMock)

    renderLoginPage()

    await user.type(screen.getByLabelText('Email'), 'me@prepstack.dev')
    await user.type(screen.getByLabelText('Password'), 'wrong-password')
    await user.click(screen.getByRole('button', { name: 'Log in' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Invalid email or password.')

    vi.unstubAllGlobals()
  })

  it('shows a client-side validation error without calling the API', async () => {
    const user = userEvent.setup()
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 401 }))
    vi.stubGlobal('fetch', fetchMock)

    renderLoginPage()
    await screen.findByRole('heading', { name: 'Log in' })
    fetchMock.mockClear()

    await user.type(screen.getByLabelText('Password'), 'correct-password')
    await user.click(screen.getByRole('button', { name: 'Log in' }))

    expect(await screen.findByText('Email is required')).toBeInTheDocument()
    expect(fetchMock).not.toHaveBeenCalled()

    vi.unstubAllGlobals()
  })
})
