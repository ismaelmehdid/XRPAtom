"use client"

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
  login: (role: UserRole) => void
  logout: () => void
  userRole: UserRole | null
  isTSO: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

// Sample user data for each role
const sampleUsers: Record<UserRole, UserData> = {
  tso: {
    id: "tso-123",
    name: "Grid Operator",
    email: "operator@gridco.com",
    role: "tso",
    organization: "Pacific Grid Solutions",
  },
  residential: {
    id: "res-456",
    name: "John Doe",
    email: "john@example.com",
    role: "residential",
  },
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const [userData, setUserData] = useState<UserData | null>(null)

  // Check localStorage on initial load
  useEffect(() => {
    const storedAuthState = localStorage.getItem("xrpatom-auth")
    const storedUserRole = localStorage.getItem("xrpatom-user-role")

    if (storedAuthState && JSON.parse(storedAuthState)) {
      setIsLoggedIn(true)
      if (storedUserRole && (storedUserRole === "tso" || storedUserRole === "residential")) {
        setUserData(sampleUsers[storedUserRole as UserRole])
      }
    }
  }, [])

  // Update localStorage when auth state changes
  useEffect(() => {
    localStorage.setItem("xrpatom-auth", JSON.stringify(isLoggedIn))
    if (userData) {
      localStorage.setItem("xrpatom-user-role", userData.role)
    } else {
      localStorage.removeItem("xrpatom-user-role")
    }
  }, [isLoggedIn, userData])

  const login = (role: UserRole) => {
    setIsLoggedIn(true)
    setUserData(sampleUsers[role])
  }

  const logout = () => {
    setIsLoggedIn(false)
    setUserData(null)
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

