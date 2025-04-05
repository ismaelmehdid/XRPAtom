"use client"

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { EnergyOverview, LiveEnergyUsage } from "@/components/dashboard/energy-overview"
import { CurtailmentEvents } from "@/components/dashboard/curtailment-events"
import { DeviceStatus } from "@/components/dashboard/device-status"
import { AddDeviceForm } from "@/components/dashboard/add-device-form"
import { RewardsSummary } from "@/components/dashboard/rewards-summary"
import { Button } from "@/components/ui/button"
import { CalendarIcon } from "lucide-react"
import { useAuth } from "@/contexts/auth-context"
import { useState } from "react"

export default function DashboardPage() {
  const { isTSO } = useAuth()
  const [refreshDevices, setRefreshDevices] = useState(0)

  const handleDeviceAdded = () => {
    setRefreshDevices(prev => prev + 1)
  }

  return (
    <div className="flex min-h-screen flex-col">
      <div className="flex-1 space-y-4 p-8 pt-6">
        <div className="flex items-center justify-between space-y-2">
          <h2 className="text-3xl font-bold tracking-tight">Energy Dashboard</h2>
          <div className="flex items-center space-x-2">
            <Button variant="outline" size="sm">
              <CalendarIcon className="mr-2 h-4 w-4" />
              Last 30 Days
            </Button>
          </div>
        </div>
        <Tabs defaultValue="overview" className="space-y-4">
          <TabsList>
            <TabsTrigger value="overview">Overview</TabsTrigger>
            {!isTSO && <TabsTrigger value="devices">Devices</TabsTrigger>}
            <TabsTrigger value="events">Curtailment Events</TabsTrigger>
            <TabsTrigger value="rewards">Rewards</TabsTrigger>
          </TabsList>
          <TabsContent value="overview" className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
              <RewardsSummary />
            </div>
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-7">
              <Card className="col-span-4">
                <CardHeader>
                  <CardTitle>Energy Curtailment Overview</CardTitle>
                  <CardDescription>Your energy savings and curtailment participation</CardDescription>
                </CardHeader>
                <CardContent className="pl-2">
                  <EnergyOverview />
                </CardContent>
              </Card>
              <Card className="col-span-4">
                <CardHeader>
                  <CardTitle>Live Energy Usage</CardTitle>
                  <CardDescription>Your energy usage in real time</CardDescription>
                </CardHeader>
                <CardContent className="pl-2">
                  <LiveEnergyUsage />
                </CardContent>
              </Card>
              <Card className="col-span-3">
                <CardHeader>
                  <CardTitle>Recent Curtailment Events</CardTitle>
                  <CardDescription>Your participation in recent grid events</CardDescription>
                </CardHeader>
                <CardContent>
                  <CurtailmentEvents />
                </CardContent>
              </Card>
            </div>
          </TabsContent>

          {!isTSO && (
            <TabsContent value="devices" className="space-y-4">
              <div className="grid gap-6 md:grid-cols-2">
                <div>
                  <Card>
                    <CardHeader>
                      <CardTitle>Connected Devices</CardTitle>
                      <CardDescription>Manage your smart devices and curtailment settings</CardDescription>
                    </CardHeader>
                    <CardContent>
                      <DeviceStatus key={refreshDevices} />
                    </CardContent>
                  </Card>
                </div>
                <div>
                  <AddDeviceForm onDeviceAdded={handleDeviceAdded} />
                </div>
              </div>
            </TabsContent>
          )}

          <TabsContent value="events" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Curtailment Events</CardTitle>
                <CardDescription>History and upcoming scheduled grid events</CardDescription>
              </CardHeader>
              <CardContent>
                <p>Detailed curtailment event history will be displayed here.</p>
              </CardContent>
            </Card>
          </TabsContent>
          <TabsContent value="rewards" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>XRP Rewards</CardTitle>
                <CardDescription>Track your earnings from energy curtailment</CardDescription>
              </CardHeader>
              <CardContent>
                <p>Detailed reward history and transaction records will be displayed here.</p>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </div>
    </div>
  )
}

