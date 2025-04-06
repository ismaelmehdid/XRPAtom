import { useState, useEffect } from "react"
import { Button } from "@/components/ui/button"
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"
import { Badge } from "@/components/ui/badge"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Slider } from "@/components/ui/slider"
import { Thermometer, Droplet, Car, Plug, Lightbulb, Battery, Settings, RefreshCw } from "lucide-react"
import { Device, fetchApi, getUserDevices } from "@/lib/api"
import { DeviceType } from "./add-device-form"
import { toast } from "sonner"

const deviceIcons: Record<DeviceType, React.ReactNode> = {
  thermostat: <Thermometer className="h-8 w-8" />,
  water_heater: <Droplet className="h-8 w-8" />,
  ev_charger: <Car className="h-8 w-8" />,
  smart_plug: <Plug className="h-8 w-8" />,
  smart_light: <Lightbulb className="h-8 w-8" />,
  battery: <Battery className="h-8 w-8" />,
}

const convertToDeviceArray = (data: any[]): Device[] => data.map(item => ({
  id: item.id,
  userId: item.userId,
  name: item.name,
  type: item.type,
  manufacturer: item.manufacturer,
  model: item.model,
  status: item.status,
  enrolled: item.enrolled,
  curtailmentLevel: item.curtailmentLevel,
  lastSeen: item.lastSeen,
  location: item.location,
  energyCapacity: item.energyCapacity,
  createdAt: item.createdAt,
  preferences: item.preferences,
}))

export function DeviceStatus() {
  const [devices, setDevices] = useState<Device[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selectedDevice, setSelectedDevice] = useState<Device | null>(null)
  const [isSettingsOpen, setIsSettingsOpen] = useState(false)

  const fetchDevices = async () => {
    try {
      setIsLoading(true)
      const response = await getUserDevices()
      console.log("Fetched devices:", response.data)
      if (Array.isArray(response.data)) {
        setDevices(convertToDeviceArray(response.data))
      } else {
        setError("No devices found")
      }
    } catch (err) {
      setError("Failed to fetch devices")
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    fetchDevices()
  }, [])

  const handleEnrollmentChange = async (deviceId: string, enrolled: boolean) => {
    try {
      const status = await fetchApi(`/devices/${deviceId}/enrollment`, {
        method: "PATCH",
        body: JSON.stringify({ enrolled }),
      })
  
      if (status.error) {
        toast.error("Failed to update enrollment status")
        throw new Error(status.error)
      }
  
      setDevices(devices.map(device => device.id === deviceId ? { ...device, enrolled } : device))
  
    } catch (error) {
      console.error("Error updating enrollment status:", error)
    }
  }
  

  const handleCurtailmentLevelChange = async (deviceId: string, level: number) => {
    try {
      const response = await fetchApi(`/devices/${deviceId}/curtailment`, {
        method: "PATCH",
        body: JSON.stringify({ curtailmentLevel: level }),
      })
  
      if (response.error) {
        toast.error("Failed to update curtailment level")
        throw new Error(response.error)
      }
  
      setDevices(devices.map(device => device.id === deviceId ? { ...device, curtailmentLevel: level } : device))
  
    } catch (error) {
      console.error("Error updating curtailment level:", error)
    }
  }
  

  const handleRefresh = async () => {
    setDevices([])
    setIsLoading(true)
    await fetchDevices()
  }
  
  const renderRefreshButton = () => (
    <div className="flex justify-end">
      <Button variant="outline" size="sm" onClick={handleRefresh} disabled={isLoading}>
        <RefreshCw className="mr-2 h-4 w-4" />
        Refresh
      </Button>
    </div>
  )

  const renderErrorMessage = () => error && <div className="text-red-500">{error}</div>

  const renderDevice = (device: Device) => (
    <Card key={device.id} className={device.status === "offline" ? "opacity-70" : ""}>
      <CardHeader className="flex items-start justify-between space-y-0">
        {renderDeviceHeader(device)}
      </CardHeader>
      <CardContent>
        {renderDeviceContent(device)}
      </CardContent>
      <CardFooter>
        {renderDeviceSettingsButton(device)}
      </CardFooter>
    </Card>
  )

  const renderDeviceHeader = (device: Device) => (
    <div>
      <CardTitle className="flex items-center">{device.name}</CardTitle>
      <CardDescription>{device.location}</CardDescription>
      <Badge variant={device.status === "online" ? "default" : "outline"}>{device.status}</Badge>
    </div>
  )

  const renderDeviceContent = (device: Device) => (
    <>
      <div className="flex justify-center py-4">
        <div className="rounded-full bg-muted p-6">
          {deviceIcons[device.type.toLowerCase() as DeviceType]}
        </div>
      </div>
      <div className="space-y-4 mt-4">
        {renderEnrollmentSwitch(device)}
        {device.enrolled && device.status === "online" && renderCurtailmentLevel(device)}
      </div>
      <div className="space-y-4 mt-4">
        {renderCurtailmentLevel(device)}
      </div>
    </>
  )

  const renderEnrollmentSwitch = (device: Device) => (
    <div className="flex items-center justify-between">
      <Label htmlFor={`enroll-${device.id}`}>Enrolled in Curtailment</Label>
      <Switch
        id={`enroll-${device.id}`}
        checked={device.enrolled}
        onCheckedChange={(checked) => handleEnrollmentChange(device.id, checked)}
        disabled={device.status === "offline"}
      />
    </div>
  )

  const renderCurtailmentLevel = (device: Device) => (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <Label>Curtailment Level</Label>
        <span className="text-sm font-medium">
          {device.curtailmentLevel === 1 ? "Minimal" :
           device.curtailmentLevel === 2 ? "Moderate" :
           device.curtailmentLevel === 3 ? "Maximum" : "None"}
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
  )

  const handleSettingsClick = (device: Device) => {
    setSelectedDevice(device)
    setIsSettingsOpen(true)
  }

  const renderDeviceSettingsModal = () => {
    if (!selectedDevice) return null
  
    return (
      <div className="fixed inset-0 flex items-center justify-center z-50 bg-black bg-opacity-50">
        <div className="bg-white p-8 rounded-lg w-96 shadow-lg transition-transform transform">
          <h2 className="text-2xl font-semibold text-center mb-6 text-gray-800">
            Device Settings for {selectedDevice.name}
          </h2>
          <div className="space-y-4 text-sm text-gray-700">
            <div className="flex justify-between">
              <span className="font-semibold">Manufacturer:</span>
              <span>{selectedDevice.manufacturer}</span>
            </div>
            <div className="flex justify-between">
              <span className="font-semibold">Model:</span>
              <span>{selectedDevice.model}</span>
            </div>
            <div className="flex justify-between">
              <span className="font-semibold">Location:</span>
              <span>{selectedDevice.location}</span>
            </div>
            <div className="flex justify-between">
              <span className="font-semibold">Energy Capacity:</span>
              <span>{selectedDevice.energyCapacity} kWh</span>
            </div>
            <div className="flex justify-between">
              <span className="font-semibold">Last Seen:</span>
              <span>{new Date(selectedDevice.lastSeen).toLocaleString()}</span>
            </div>
            <div className="flex justify-between">
              <span className="font-semibold">Preferences:</span>
              <span>{selectedDevice.preferences}</span>
            </div>
          </div>
          <div className="flex justify-end mt-6">
            <Button 
              variant="outline" 
              size="sm" 
              className="text-gray-600 hover:bg-gray-200" 
              onClick={() => {
                setIsSettingsOpen(false)
                setSelectedDevice(null)
              }}>
              Close
            </Button>
          </div>
        </div>
      </div>
    )
  }  
  
  const renderDeviceSettingsButton = (device: Device) => (
    <CardFooter>
      <Button variant="outline" size="sm" className="w-full" onClick={() => handleSettingsClick(device)}>
        <Settings className="mr-2 h-4 w-4" />
        Device Settings
      </Button>
    </CardFooter>
  )

  return (
    <div className="space-y-4">
      {renderRefreshButton()}
      {renderErrorMessage()}
      <div className="grid gap-4">
        {devices.length ? devices.map(renderDevice) : <div>No devices found</div>}
      </div>
      {isSettingsOpen && renderDeviceSettingsModal()}
    </div>
  )
}
