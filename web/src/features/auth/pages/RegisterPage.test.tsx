import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createFakeAccessToken } from '../../../shared/testing/fake-jwt'
import { tokenStore } from '../../../shared/lib/token-store'
import { AuthProvider } from '../context'
import { RegisterPage } from './RegisterPage'

function renderRegisterPage() {
  const queryClient = new QueryClient()
  return render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <MemoryRouter initialEntries={['/register']}>
          <Routes>
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/" element={<div>Home</div>} />
          </Routes>
        </MemoryRouter>
      </AuthProvider>
    </QueryClientProvider>,
  )
}

describe('RegisterPage', () => {
  beforeEach(() => {
    tokenStore.set(null)
  })

  it('registers and redirects home on success', async () => {
    const user = userEvent.setup()
    const token = createFakeAccessToken({ sub: 'user-1', email: 'new@prepstack.dev' })
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

    renderRegisterPage()

    await user.type(screen.getByLabelText('Display name'), 'New User')
    await user.type(screen.getByLabelText('Email'), 'new@prepstack.dev')
    await user.type(screen.getByLabelText('Password'), 'correct-password')
    await user.click(screen.getByRole('button', { name: 'Create account' }))

    expect(await screen.findByText('Home')).toBeInTheDocument()

    vi.unstubAllGlobals()
  })

  it('shows the conflict error when the email is already registered', async () => {
    const user = userEvent.setup()
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            title: 'An account with this email already exists.',
            code: 'auth.email_taken',
          }),
          { status: 409, headers: { 'Content-Type': 'application/problem+json' } },
        ),
      )
    vi.stubGlobal('fetch', fetchMock)

    renderRegisterPage()

    await user.type(screen.getByLabelText('Display name'), 'Existing User')
    await user.type(screen.getByLabelText('Email'), 'taken@prepstack.dev')
    await user.type(screen.getByLabelText('Password'), 'correct-password')
    await user.click(screen.getByRole('button', { name: 'Create account' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('An account with this email already exists.')

    vi.unstubAllGlobals()
  })
})
