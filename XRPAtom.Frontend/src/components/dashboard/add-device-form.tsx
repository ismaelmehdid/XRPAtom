"use client"

import type React from "react"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Switch } from "@/components/ui/switch"
import { Thermometer, Droplet, Car, Plug, Lightbulb, Battery } from "lucide-react"
import { useToast } from "@/components/ui/use-toast"

type DeviceType = "thermostat" | "water_heater" | "ev_charger" | "smart_plug" | "smart_light" | "battery"

interface DeviceTypeOption {
  value: DeviceType
  label: string
  icon: React.ReactNode
  manufacturers: string[]
}

const deviceTypes: DeviceTypeOption[] = [
  {
    value: "thermostat",
    label: "Smart Thermostat",
    icon: <Thermometer className="h-5 w-5" />,
    manufacturers: ["Nest", "Ecobee", "Honeywell", "Emerson", "Wyze"],
  },
  {
    value: "water_heater",
    label: "Water Heater",
    icon: <Droplet className="h-5 w-5" />,
    manufacturers: ["Rheem", "A.O. Smith", "Bradford White", "Rinnai", "Stiebel Eltron"],
  },
  {
    value: "ev_charger",
    label: "EV Charger",
    icon: <Car className="h-5 w-5" />,
    manufacturers: ["Tesla", "ChargePoint", "JuiceBox", "Wallbox", "Blink"],
  },
  {
    value: "smart_plug",
    label: "Smart Plug",
    icon: <Plug className="h-5 w-5" />,
    manufacturers: ["TP-Link Kasa", "Wemo", "Amazon", "Wyze", "Philips Hue"],
  },
  {
    value: "smart_light",
    label: "Smart Light",
    icon: <Lightbulb className="h-5 w-5" />,
    manufacturers: ["Philips Hue", "LIFX", "Nanoleaf", "Sengled", "Wyze"],
  },
  {
    value: "battery",
    label: "Home Battery",
    icon: <Battery className="h-5 w-5" />,
    manufacturers: ["Tesla Powerwall", "LG Chem", "Enphase", "Generac", "Sonnen"],
  },
]

export function AddDeviceForm() {
  const { toast } = useToast()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [selectedType, setSelectedType] = useState<DeviceType | null>(null)
  const [formData, setFormData] = useState({
    name: "",
    manufacturer: "",
    model: "",
    location: "",
    enrollImmediately: true,
  })

  const handleTypeChange = (value: DeviceType) => {
    setSelectedType(value)
    setFormData((prev) => ({
      ...prev,
      manufacturer: "",
    }))
  }

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }))
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedType) return

    setIsSubmitting(true)

    try {
      // This would be replaced with actual API call to your C# backend
      // const response = await fetch('/api/devices', {
      //   method: 'POST',
      //   headers: { 'Content-Type': 'application/json' },
      //   body: JSON.stringify({
      //     ...formData,
      //     type: selectedType
      //   }),
      // })

      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 1500))

      toast({
        title: "Device Added Successfully",
        description: `${formData.name} has been added to your account.`,
      })

      // Reset form
      setFormData({
        name: "",
        manufacturer: "",
        model: "",
        location: "",
        enrollImmediately: true,
      })
      setSelectedType(null)
    } catch (error) {
      toast({
        title: "Error Adding Device",
        description: "There was a problem adding your device. Please try again.",
        variant: "destructive",
      })
    } finally {
      setIsSubmitting(false)
    }
  }

  const selectedTypeOption = selectedType ? deviceTypes.find((dt) => dt.value === selectedType) : null
  const manufacturers = selectedTypeOption?.manufacturers || []

  return (
    <Card>
      <CardHeader>
        <CardTitle>Add New Device</CardTitle>
      </CardHeader>
      <form onSubmit={handleSubmit}>
        <CardContent className="space-y-4">
          <div className="grid gap-2">
            <Label htmlFor="device-type">Device Type</Label>
            <Select value={selectedType || ""} onValueChange={(value) => handleTypeChange(value as DeviceType)}>
              <SelectTrigger id="device-type">
                <SelectValue placeholder="Select device type" />
              </SelectTrigger>
              <SelectContent>
                {deviceTypes.map((deviceType) => (
                  <SelectItem key={deviceType.value} value={deviceType.value} className="flex items-center">
                    <div className="flex items-center">
                      <span className="mr-2">{deviceType.icon}</span>
                      <span>{deviceType.label}</span>
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {selectedType && (
            <>
              <div className="grid gap-2">
                <Label htmlFor="name">Device Name</Label>
                <Input
                  id="name"
                  name="name"
                  placeholder={`e.g., Living Room ${selectedTypeOption?.label}`}
                  value={formData.name}
                  onChange={handleInputChange}
                  required
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="manufacturer">Manufacturer</Label>
                <Select
                  value={formData.manufacturer}
                  onValueChange={(value) => setFormData((prev) => ({ ...prev, manufacturer: value }))}
                >
                  <SelectTrigger id="manufacturer">
                    <SelectValue placeholder="Select manufacturer" />
                  </SelectTrigger>
                  <SelectContent>
                    {manufacturers.map((manufacturer) => (
                      <SelectItem key={manufacturer} value={manufacturer}>
                        {manufacturer}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="grid gap-2">
                <Label htmlFor="model">Model</Label>
                <Input
                  id="model"
                  name="model"
                  placeholder="e.g., Model X123"
                  value={formData.model}
                  onChange={handleInputChange}
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="location">Location</Label>
                <Input
                  id="location"
                  name="location"
                  placeholder="e.g., Living Room"
                  value={formData.location}
                  onChange={handleInputChange}
                />
              </div>

              <div className="flex items-center space-x-2">
                <Switch
                  id="enrollImmediately"
                  checked={formData.enrollImmediately}
                  onCheckedChange={(checked) => setFormData((prev) => ({ ...prev, enrollImmediately: checked }))}
                />
                <Label htmlFor="enrollImmediately">Enroll in curtailment immediately</Label>
              </div>
            </>
          )}
        </CardContent>
        <CardFooter>
          <Button type="submit" className="w-full" disabled={!selectedType || isSubmitting}>
            {isSubmitting ? "Adding Device..." : "Add Device"}
          </Button>
        </CardFooter>
      </form>
    </Card>
  )
}

