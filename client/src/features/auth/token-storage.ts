// =============================================================================
// Token storage - wrapper quanh localStorage de de swap (sessionStorage / cookie)
// =============================================================================

const TOKEN_KEY = 'pharmaintel.access_token'

export function getToken(): string | null {
  try {
    return localStorage.getItem(TOKEN_KEY)
  } catch {
    return null
  }
}

export function setToken(token: string): void {
  try {
    localStorage.setItem(TOKEN_KEY, token)
  } catch {
    /* ignore quota / privacy mode */
  }
}

export function clearToken(): void {
  try {
    localStorage.removeItem(TOKEN_KEY)
  } catch {
    /* ignore */
  }
}
