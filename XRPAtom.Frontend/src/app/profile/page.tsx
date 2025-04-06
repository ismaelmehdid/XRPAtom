"use client"

import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { useAuth } from "@/contexts/auth-context"
import LoginForm from "@/components/LoginForm"

export default function ProfilePage() {
  const { isLoggedIn, userData } = useAuth()

  if (!isLoggedIn) {
    return (
      <LoginForm />
    )
  }

  console.log(userData)
  // /api/wallet/connect-xumm

  return (
    <div className="container mx-auto px-4 py-8">
      <Card className="max-w-2xl mx-auto">
        <CardHeader>
          <CardTitle>Profile</CardTitle>
          <CardDescription>Manage your account and wallet connections</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between p-4 border rounded-lg">
            <div>
              <h3 className="font-medium">Xaman Wallet</h3>
              <p className="text-sm text-muted-foreground">Connect your Xaman wallet to interact with the platform</p>
            </div>
            <Button variant="default">
              Connect Wallet
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
