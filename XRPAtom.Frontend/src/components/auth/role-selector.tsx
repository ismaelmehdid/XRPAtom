"use client"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group"
import { Label } from "@/components/ui/label"
import { useAuth, type UserRole } from "@/contexts/auth-context"
import { Building, Home, LogIn } from "lucide-react"

export function RoleSelector() {
  const { login } = useAuth()
  const [selectedRole, setSelectedRole] = useState<UserRole>("residential")

  const handleLogin = () => {
    login(selectedRole)
  }

  return (
    <Card className="w-full max-w-md mx-auto">
      <CardHeader>
        <CardTitle>Select User Type</CardTitle>
        <CardDescription>Choose your role to access the appropriate features</CardDescription>
      </CardHeader>
      <CardContent>
        <RadioGroup
          value={selectedRole}
          onValueChange={(value) => setSelectedRole(value as UserRole)}
          className="space-y-4"
        >
          <div className="flex items-start space-x-3 space-y-0">
            <RadioGroupItem value="residential" id="residential" />
            <div className="grid gap-1.5 leading-none">
              <Label htmlFor="residential" className="text-base font-medium flex items-center">
                <Home className="h-4 w-4 mr-2" />
                Residential User
              </Label>
              <p className="text-sm text-muted-foreground">Individual homeowners who can sell energy flexibility</p>
            </div>
          </div>
          <div className="flex items-start space-x-3 space-y-0">
            <RadioGroupItem value="tso" id="tso" />
            <div className="grid gap-1.5 leading-none">
              <Label htmlFor="tso" className="text-base font-medium flex items-center">
                <Building className="h-4 w-4 mr-2" />
                Grid Operator / Energy Supplier
              </Label>
              <p className="text-sm text-muted-foreground">
                Organizations that can buy flexibility and create market offers
              </p>
            </div>
          </div>
        </RadioGroup>
      </CardContent>
      <CardFooter>
        <Button onClick={handleLogin} className="w-full">
          <LogIn className="mr-2 h-4 w-4" />
          Continue as {selectedRole === "tso" ? "Grid Operator" : "Residential User"}
        </Button>
      </CardFooter>
    </Card>
  )
}

