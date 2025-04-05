"use client"

import { useEffect } from "react"
import { useRouter } from "next/navigation"
import { useAuth } from "@/contexts/auth-context"
import LoginForm from "@/components/LoginForm"

export default function LoginPage() {
  const { isLoggedIn } = useAuth()
  const router = useRouter()

  useEffect(() => {
    if (isLoggedIn) {
      router.push("/dashboard")
    }
  }, [isLoggedIn, router])

  return <LoginForm />
}
