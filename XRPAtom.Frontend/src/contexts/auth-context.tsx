"use client"

import { removeAuthToken, setAuthToken, getAuthToken, isTokenValid, getTokenPayload } from "@/lib/auth"
import { createContext, useContext, useState, useEffect, type ReactNode } from "react"

export type UserRole = "tso" | "residential"

interface UserData {
  id: string
  name: string
  email: string
  role: UserRole
  organization?: string
}

interface AuthContextType {
  isLoggedIn: boolean
  userData: UserData | null
  login: (token: string, userData: UserData) => void
  logout: () => void
  userRole: UserRole | null
  isTSO: boolean
  isLoading: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const [userData, setUserData] = useState<UserData | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  // Check token and initialize auth state on mount
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        const token = getAuthToken()
        if (token && isTokenValid()) {
          const payload = getTokenPayload()
          if (payload) {
            // You might want to fetch the user data from your API here
            // For now, we'll use the data from the token
            const userData: UserData = {
              id: payload.userId,
              name: payload.name,
              email: payload.email,
              role: payload.role,
              organization: payload.organization
            }
            setUserData(userData)
            setIsLoggedIn(true)
          }
        }
      } catch (error) {
        console.error('Error initializing auth:', error)
        logout()
      } finally {
        setIsLoading(false)
      }
    }
    initializeAuth()
  }, [])

  const login = (token: string, userData: UserData) => {
    setAuthToken(token)
    setUserData(userData)
    setIsLoggedIn(true)
  }

  const logout = () => {
    setIsLoggedIn(false)
    setUserData(null)
    removeAuthToken()
  }

  const userRole = userData?.role || null
  const isTSO = userRole === "tso"

  return (
    <AuthContext.Provider
      value={{
        isLoggedIn,
        userData,
        login,
        logout,
        userRole,
        isTSO,
        isLoading
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider")
  }
  return context
}

