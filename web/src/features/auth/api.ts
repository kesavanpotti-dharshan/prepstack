import { useMutation } from '@tanstack/react-query'
import type { components } from '../../shared/api/schema'
import { apiFetch } from '../../shared/lib/http'

export type LoginRequest = components['schemas']['LoginCommand']
export type RegisterRequest = components['schemas']['RegisterCommand']
export type AuthResponse = components['schemas']['AuthResponse']

export function login(request: LoginRequest): Promise<AuthResponse> {
  return apiFetch<AuthResponse>('/auth/login', { method: 'POST', body: request })
}

export function register(request: RegisterRequest): Promise<AuthResponse> {
  return apiFetch<AuthResponse>('/auth/register', { method: 'POST', body: request })
}

export function logout(): Promise<void> {
  return apiFetch<void>('/auth/logout', { method: 'POST' })
}

export function useLoginMutation() {
  return useMutation({ mutationFn: login })
}

export function useRegisterMutation() {
  return useMutation({ mutationFn: register })
}

export function useLogoutMutation() {
  return useMutation({ mutationFn: logout })
}
