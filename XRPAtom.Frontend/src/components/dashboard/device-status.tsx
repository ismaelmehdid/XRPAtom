"use client"

import type React from "react"

import { useState, useEffect } from "react"
import { Button } from "@/components/ui/button"
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"
import { Badge } from "@/components/ui/badge"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Slider } from "@/components/ui/slider"
import { Thermometer, Droplet, Car, Plug, Settings, RefreshCw } from "lucide-react"
import { useAuth } from "@/contexts/auth-context"
import { getUserDevices } from "@/lib/api"
import { useToast } from "@/components/ui/use-toast"

type Device = {
  id: string
  name: string
  type: string
  icon: React.ReactNode
  status: "online" | "offline"
  enrolled: boolean
  curtailmentLevel?: number
  details?: string
}

const deviceIcons: Record<string, React.ReactNode> = {
  thermostat: <Thermometer className="h-8 w-8" />,
  water_heater: <Droplet className="h-8 w-8" />,
  ev_charger: <Car className="h-8 w-8" />,
  smart_plug: <Plug className="h-8 w-8" />,
  smart_light: <Settings className="h-8 w-8" />,
  battery: <Settings className="h-8 w-8" />,
}

export function DeviceStatus() {
  const { userData } = useAuth()
  const { toast } = useToast()
  const [devices, setDevices] = useState<Device[]>([])
  const [isLoading, setIsLoading] = useState(true)


  const handleEnrollmentChange = (deviceId: string, enrolled: boolean) => {
    setDevices(devices.map((device) => (device.id === deviceId ? { ...device, enrolled } : device)))
  }

  const handleCurtailmentLevelChange = (deviceId: string, level: number) => {
    setDevices(devices.map((device) => (device.id === deviceId ? { ...device, curtailmentLevel: level } : device)))
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <Button variant="outline" size="sm" disabled={isLoading}>
          <RefreshCw className="mr-2 h-4 w-4" />
          Refresh
        </Button>
      </div>
      <div className="grid gap-4">
        {devices.map((device) => (
          <Card key={device.id} className={device.status === "offline" ? "opacity-70" : ""}>
            <CardHeader className="flex flex-row items-start justify-between space-y-0">
              <div>
                <CardTitle className="flex items-center">{device.name}</CardTitle>
                <CardDescription>{device.details}</CardDescription>
              </div>
              <div className="flex items-center space-x-2">
                <Badge variant={device.status === "online" ? "default" : "outline"}>{device.status}</Badge>
              </div>
            </CardHeader>
            <CardContent>
              <div className="flex items-center justify-center py-4">
                <div className="rounded-full bg-muted p-6">{device.icon}</div>
              </div>
              <div className="space-y-4 mt-4">
                <div className="flex items-center justify-between">
                  <Label htmlFor={`enroll-${device.id}`}>Enrolled in Curtailment</Label>
                  <Switch
                    id={`enroll-${device.id}`}
                    checked={device.enrolled}
                    onCheckedChange={(checked) => handleEnrollmentChange(device.id, checked)}
                    disabled={device.status === "offline"}
                  />
                </div>

                {device.enrolled && device.status === "online" && (
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <Label>Curtailment Level</Label>
                      <span className="text-sm font-medium">
                        {device.curtailmentLevel === 1
                          ? "Minimal"
                          : device.curtailmentLevel === 2
                            ? "Moderate"
                            : device.curtailmentLevel === 3
                              ? "Maximum"
                              : "None"}
                      </span>
                    </div>
                    <Slider
                      value={[device.curtailmentLevel || 0]}
                      min={0}
                      max={3}
                      step={1}
                      onValueChange={(value) => handleCurtailmentLevelChange(device.id, value[0])}
                    />
                  </div>
                )}
              </div>
            </CardContent>
            <CardFooter>
              <Button variant="outline" size="sm" className="w-full">
                <Settings className="mr-2 h-4 w-4" />
                Device Settings
              </Button>
            </CardFooter>
          </Card>
        ))}
      </div>
    </div>
  )
}

