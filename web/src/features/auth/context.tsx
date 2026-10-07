import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { refreshAccessToken } from '../../shared/lib/http'
import { decodeAccessToken } from '../../shared/lib/jwt'
import { tokenStore } from '../../shared/lib/token-store'
import { useLoginMutation, useLogoutMutation, useRegisterMutation, type LoginRequest, type RegisterRequest } from './api'
import { AuthContext, type AuthContextValue, type AuthStatus, type AuthUser } from './auth-context'

function userFromToken(token: string | null): AuthUser | null {
  if (!token) {
    return null
  }
  const claims = decodeAccessToken(token)
  return claims ? { id: claims.sub, email: claims.email } : null
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<AuthStatus>('loading')
  const [user, setUser] = useState<AuthUser | null>(() => userFromToken(tokenStore.get()))

  const loginMutation = useLoginMutation()
  const registerMutation = useRegisterMutation()
  const logoutMutation = useLogoutMutation()

  useEffect(() => tokenStore.subscribe((token) => setUser(userFromToken(token))), [])

  // Silent refresh on load: exchange the httpOnly refresh cookie for an access token.
  useEffect(() => {
    let active = true
    void refreshAccessToken().then((refreshed) => {
      if (active) {
        setStatus(refreshed ? 'authenticated' : 'unauthenticated')
      }
    })
    return () => {
      active = false
    }
  }, [])

  const login = useCallback(
    async (request: LoginRequest) => {
      const response = await loginMutation.mutateAsync(request)
      tokenStore.set(response.accessToken)
      setStatus('authenticated')
    },
    [loginMutation],
  )

  const register = useCallback(
    async (request: RegisterRequest) => {
      const response = await registerMutation.mutateAsync(request)
      tokenStore.set(response.accessToken)
      setStatus('authenticated')
    },
    [registerMutation],
  )

  const logout = useCallback(async () => {
    try {
      await logoutMutation.mutateAsync()
    } finally {
      tokenStore.set(null)
      setStatus('unauthenticated')
    }
  }, [logoutMutation])

  const value = useMemo<AuthContextValue>(
    () => ({ status, user, login, register, logout }),
    [status, user, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
