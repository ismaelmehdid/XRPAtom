"use client"

import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { useAuth } from "@/contexts/auth-context"
import LoginForm from "@/components/LoginForm"
import { fetchApi } from "@/lib/api"
import { toast } from "sonner"
import { getAuthToken } from "@/lib/auth"
import { useEffect } from "react"

interface WalletConnectionResponse {
  deepLink: string
  qrUrl: string
  uuid: string
}

export default function ProfilePage() {
  const { isLoggedIn, userData } = useAuth()

  if (!isLoggedIn) {
    return (
      <LoginForm />
    )
  }

  const token = getAuthToken()

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetchApi("/wallet/verify-xumm-connection", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`,
          },
          body: JSON.stringify({
            "payloadId": localStorage.getItem("xumm_uuid"),
          }),
        });

        if (response.error) {
          console.log("finished checking xumm connection");
          return;
        }

        console.log("validated signature");
      } catch (error) {
        console.error("Error loading profile data:", error);
      }
    }

    if (isLoggedIn) {
      fetchData();
    }
  }, [isLoggedIn, token]);

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
            <Button variant="default" onClick={async () => {
              try {
                const response = await fetchApi<WalletConnectionResponse>("/wallet/connect-xumm", {
                  method: "POST",
                  headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`,
                  },
                });

                if (response.error) {
                  console.error("Failed to connect wallet:", response.error);
                  return;
                }

                if (response.data) {
                  // TODO: Open the qr code link in a new tab
                  localStorage.setItem("xumm_uuid", response.data.uuid);

                  window.open(response.data.deepLink, "_blank");
                  console.log("Wallet connection initiated:", response.data);

                } else {
                  toast.error("Failed to connect wallet, please try again later.");
                }
                
                // TODO: Handle the response (show QR code, etc.)
              } catch (error) {
                console.error("Error connecting wallet:", error);
              }
            }}>
              Connect Wallet
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
