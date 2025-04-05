"use client"

import type React from "react"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Slider } from "@/components/ui/slider"
import { Switch } from "@/components/ui/switch"
import { useToast } from "@/components/ui/use-toast"
import { Alert, AlertDescription } from "@/components/ui/alert"
import { Clock, Zap, BarChart3, Calendar, Info } from "lucide-react"

export default function CreateOfferPage() {
  const router = useRouter()
  const { toast } = useToast()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [pricePerKwh, setPricePerKwh] = useState(0.25)
  const [minKwh, setMinKwh] = useState(2)
  const [isRecurring, setIsRecurring] = useState(false)

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    setIsSubmitting(true)

    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 1500))

    toast({
      title: "Offer Created",
      description: "Your flexibility offer has been published to the marketplace.",
    })

    setIsSubmitting(false)
    router.push("/marketplace")
  }

  return (
    <div className="container mx-auto px-4 py-12">
      <div className="max-w-3xl mx-auto">
        <h1 className="text-3xl font-bold mb-2">Create Flexibility Offer</h1>
        <p className="text-muted-foreground mb-8">Define the parameters for your energy flexibility purchase offer</p>

        <Alert className="mb-6">
          <Info className="h-4 w-4" />
          <AlertDescription>
            As a Grid Operator/Energy Supplier, you can create offers to purchase flexibility from residential users.
          </AlertDescription>
        </Alert>

        <form onSubmit={handleSubmit}>
          <Card className="mb-6">
            <CardHeader>
              <CardTitle>Offer Details</CardTitle>
              <CardDescription>Basic information about your flexibility offer</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-2">
                <Label htmlFor="title">Offer Title</Label>
                <Input id="title" placeholder="e.g., Peak Demand Reduction" required />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="description">Description</Label>
                <Textarea
                  id="description"
                  placeholder="Describe the flexibility service you're looking to purchase"
                  className="min-h-[100px]"
                  required
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="type">Flexibility Type</Label>
                <Select defaultValue="peak_shaving">
                  <SelectTrigger id="type">
                    <SelectValue placeholder="Select flexibility type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="peak_shaving">Peak Shaving</SelectItem>
                    <SelectItem value="frequency_response">Frequency Response</SelectItem>
                    <SelectItem value="renewable_integration">Renewable Integration</SelectItem>
                    <SelectItem value="voltage_support">Voltage Support</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>Pricing & Requirements</CardTitle>
              <CardDescription>Define the economic terms of your offer</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <Label>Price per kWh (XRP)</Label>
                  <span className="font-medium">{pricePerKwh.toFixed(2)} XRP</span>
                </div>
                <Slider
                  value={[pricePerKwh]}
                  min={0.1}
                  max={1.0}
                  step={0.01}
                  onValueChange={(value) => setPricePerKwh(value[0])}
                />
                <p className="text-sm text-muted-foreground">
                  <Zap className="inline h-3 w-3 mr-1" />
                  The amount in XRP you'll pay per kilowatt-hour of curtailed energy
                </p>
              </div>

              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <Label>Minimum kWh per Event</Label>
                  <span className="font-medium">{minKwh.toFixed(1)} kWh</span>
                </div>
                <Slider value={[minKwh]} min={0.5} max={10} step={0.5} onValueChange={(value) => setMinKwh(value[0])} />
                <p className="text-sm text-muted-foreground">
                  <BarChart3 className="inline h-3 w-3 mr-1" />
                  Minimum energy reduction required to participate
                </p>
              </div>

              <div className="grid gap-2">
                <Label htmlFor="budget">Total Budget (XRP)</Label>
                <Input id="budget" type="number" min="1" placeholder="e.g., 1000" required />
                <p className="text-sm text-muted-foreground">Maximum amount of XRP allocated for this offer</p>
              </div>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>Scheduling</CardTitle>
              <CardDescription>Define when you need the flexibility</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center space-x-2">
                <Switch id="recurring" checked={isRecurring} onCheckedChange={setIsRecurring} />
                <Label htmlFor="recurring">Recurring Event</Label>
              </div>

              {isRecurring ? (
                <div className="space-y-4">
                  <div className="grid gap-2">
                    <Label htmlFor="days">Days of Week</Label>
                    <Select defaultValue="weekdays">
                      <SelectTrigger id="days">
                        <SelectValue placeholder="Select days" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="weekdays">Weekdays (Mon-Fri)</SelectItem>
                        <SelectItem value="weekends">Weekends (Sat-Sun)</SelectItem>
                        <SelectItem value="all">All Days</SelectItem>
                        <SelectItem value="custom">Custom Selection</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div className="grid gap-2">
                      <Label htmlFor="start-time">Start Time</Label>
                      <Input id="start-time" type="time" defaultValue="16:00" required />
                    </div>
                    <div className="grid gap-2">
                      <Label htmlFor="end-time">End Time</Label>
                      <Input id="end-time" type="time" defaultValue="19:00" required />
                    </div>
                  </div>

                  <div className="grid gap-2">
                    <Label htmlFor="duration">Event Duration (hours)</Label>
                    <Input id="duration" type="number" min="0.5" max="24" step="0.5" defaultValue="3" required />
                    <p className="text-sm text-muted-foreground">
                      <Clock className="inline h-3 w-3 mr-1" />
                      How long each curtailment event will last
                    </p>
                  </div>
                </div>
              ) : (
                <div className="space-y-4">
                  <div className="grid gap-2">
                    <Label htmlFor="event-date">Event Date</Label>
                    <Input id="event-date" type="date" required />
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div className="grid gap-2">
                      <Label htmlFor="single-start-time">Start Time</Label>
                      <Input id="single-start-time" type="time" defaultValue="16:00" required />
                    </div>
                    <div className="grid gap-2">
                      <Label htmlFor="single-end-time">End Time</Label>
                      <Input id="single-end-time" type="time" defaultValue="19:00" required />
                    </div>
                  </div>
                </div>
              )}

              <div className="grid gap-2">
                <Label htmlFor="notice">Advance Notice (hours)</Label>
                <Input id="notice" type="number" min="1" max="48" defaultValue="24" required />
                <p className="text-sm text-muted-foreground">
                  <Calendar className="inline h-3 w-3 mr-1" />
                  How much notice participants will receive before the event
                </p>
              </div>
            </CardContent>
          </Card>

          <div className="flex justify-end space-x-4">
            <Button variant="outline" type="button" onClick={() => router.push("/marketplace")}>
              Cancel
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Creating Offer..." : "Publish Offer"}
            </Button>
          </div>
        </form>
      </div>
    </div>
  )
}

