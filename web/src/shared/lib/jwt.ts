export interface AccessTokenClaims {
  sub: string
  email: string
  exp: number
}

/** Decodes the JWT payload for display purposes only — the server already verified the signature. */
export function decodeAccessToken(token: string): AccessTokenClaims | null {
  try {
    const payload = token.split('.')[1]
    if (!payload) {
      return null
    }
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
    const decoded = atob(normalized)
    const json = decodeURIComponent(
      decoded
        .split('')
        .map((char) => `%${char.charCodeAt(0).toString(16).padStart(2, '0')}`)
        .join(''),
    )
    return JSON.parse(json) as AccessTokenClaims
  } catch {
    return null
  }
}
