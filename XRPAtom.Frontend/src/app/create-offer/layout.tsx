import type React from "react"
import { ProtectedRoute } from "@/components/auth/protected-route"

export default function CreateOfferLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return <ProtectedRoute allowedRoles={["tso"]}>{children}</ProtectedRoute>
}

