"use client"

import type React from "react"

import { useEffect } from "react"
import { useRouter } from "next/navigation"
import { useAuth } from "@/contexts/auth-context"
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert"
import { Button } from "@/components/ui/button"
import { RoleSelector } from "@/components/auth/role-selector"
import { ArrowLeft } from "lucide-react"
import LoginForm from "../LoginForm"

interface ProtectedRouteProps {
  children: React.ReactNode
  allowedRoles?: Array<"tso" | "residential"> // Optional: specify which roles can access this route
}

export function ProtectedRoute({ children, allowedRoles }: ProtectedRouteProps) {
  const { isLoggedIn, userRole } = useAuth()
  const router = useRouter()

  const hasAccess = isLoggedIn && (!allowedRoles || allowedRoles.includes(userRole!))

  useEffect(() => {
    // If not logged in, scroll to top to show the alert
    if (!isLoggedIn) {
      window.scrollTo(0, 0)
    }
  }, [isLoggedIn])

  if (!isLoggedIn) {
    return (
      <LoginForm />
    );
  }

  if (isLoggedIn && !hasAccess) {
    return (
      <div className="container mx-auto px-4 py-12">
        <Alert variant="destructive" className="mb-6">
          <AlertTitle>Access Denied</AlertTitle>
          <AlertDescription>Your user role does not have permission to access this page.</AlertDescription>
        </Alert>

        <div className="flex flex-col items-center justify-center space-y-4 p-8 border rounded-lg bg-muted/50">
          <h2 className="text-2xl font-bold">Insufficient Permissions</h2>
          <p className="text-muted-foreground text-center max-w-md">
            This area is restricted to {allowedRoles?.join(" or ")} users only.
          </p>
          <Button onClick={() => router.push("/dashboard")} className="mt-4">
            Return to Dashboard
          </Button>
        </div>
      </div>
    )
  }

  return <>{children}</>
}

