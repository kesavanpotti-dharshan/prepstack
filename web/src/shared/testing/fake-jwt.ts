// Test-only helper: builds an unsigned JWT-shaped string so tests can exercise
// the claim-decoding path in shared/lib/jwt.ts without a real signing key.
export function createFakeAccessToken(claims: { sub: string; email: string }): string {
  const header = base64url(JSON.stringify({ alg: 'HS256', typ: 'JWT' }))
  const payload = base64url(JSON.stringify({ ...claims, exp: Math.floor(Date.now() / 1000) + 900 }))
  return `${header}.${payload}.signature`
}

function base64url(value: string): string {
  return btoa(value)
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '')
}
