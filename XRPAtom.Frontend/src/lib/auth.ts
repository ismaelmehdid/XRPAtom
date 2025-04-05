import { jwtDecode } from "jwt-decode"

interface TokenPayload {
  exp: number
  [key: string]: any
}

export const setAuthToken = (token: string) => {
  localStorage.setItem("auth_token", token)
}

export const getAuthToken = (): string | null => {
  return localStorage.getItem("auth_token")
}

export const removeAuthToken = () => {
  localStorage.removeItem("auth_token")
}

export const isTokenValid = (): boolean => {
  const token = getAuthToken()
  if (!token) return false

  try {
    const decoded = jwtDecode<TokenPayload>(token)
    const currentTime = Date.now() / 1000
    return decoded.exp > currentTime
  } catch {
    return false
  }
}

export const getTokenPayload = (): TokenPayload | null => {
  const token = getAuthToken()
  if (!token) return null

  try {
    return jwtDecode<TokenPayload>(token)
  } catch {
    return null
  }
} 