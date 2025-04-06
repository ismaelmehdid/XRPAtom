// API utility functions to interact with the C# backend

import { getAuthToken } from "./auth"

type ApiResponse<T> = {
  data?: T
  error?: string
}

/**
 * Base function for making API requests to the C# backend
 */
export async function fetchPlugApi<T>(
  device: Device,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  if (!device) {
    return { error: "Device is not defined" }
  }

  const isSupported =
    device.manufacturer === "Meross" &&
    device.type === "smart_plug" &&
    device.model === "MSS210"

  if (!isSupported) {
    return { error: "Device is not supported" }
  }

  let endpoint = ""
  if (device.status === "Offline") {
    endpoint = "/turn_off"
  } else if (device.status === "Online") {
    endpoint = "/turn_on"
  } else {
    return { error: "Unsupported device status" }
  }

  const url = `https://wss.zunix.systems/device${endpoint}`

  console.log("Making request to:", url)
  try {
    const response = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        ...options.headers,
      },
    })

    if (!response.ok) {
      const errorData = await response.json()
      return { error: errorData.message || "An error occurred" }
    }

    const data = await response.json()
    return { data }
  } catch (error) {
    console.error("API request failed:", error)
    return { error: "Failed to connect to the server" }
  }
}

export async function fetchApi<T>(endpoint: string, options: RequestInit = {}): Promise<ApiResponse<T>> {
  const apiUrl = process.env.NEXT_PUBLIC_API_URL || "https://api.zunix.systems/api"
  const url = `${apiUrl}${endpoint}`
  const token = getAuthToken()
  const defaultHeaders = {
    "Content-Type": "application/json",
    "Authorization": "Bearer " + token,
  }

  try {
    const response = await fetch(url, {
      ...options,
      headers: {
        ...defaultHeaders,
        ...options.headers,
      },
    })

    if (!response.ok) {
      const errorData = await response.json()
      return { error: errorData.message || "An error occurred" }
    }

    const data = await response.json()
    return { data }
  } catch (error) {
    console.error("API request failed:", error)
    return { error: "Failed to connect to the server" }
  }
}

/**
 * Get user account details
 */
export async function getUserAccount(userId: string): Promise<ApiResponse<UserAccount>> {
  return fetchApi<UserAccount>(`/users/${userId}/account`)
}

/**
 * Get user's connected devices
 */
export async function getUserDevices(): Promise<ApiResponse<{ devices: Device[] }>> {
  return fetchApi<{ devices: Device[] }>(`/devices`)
}

/**
 * Update device settings
 */
export async function updateDeviceSettings(deviceId: string, settings: DeviceSettings): Promise<ApiResponse<Device>> {
  return fetchApi<Device>(`/devices/${deviceId}`, {
    method: "PUT",
    body: JSON.stringify(settings),
  })
}

/**
 * Get curtailment events
 */
export async function getCurtailmentEvents(
  userId: string,
  page = 1,
  limit = 10,
): Promise<ApiResponse<{ events: CurtailmentEvent[] }>> {
  return fetchApi<{ events: CurtailmentEvent[] }>(`/users/${userId}/events?page=${page}&limit=${limit}`)
}

/**
 * Get user's reward history
 */
export async function getRewardHistory(
  userId: string,
  page = 1,
  limit = 10,
): Promise<ApiResponse<{ rewards: Reward[] }>> {
  return fetchApi<{ rewards: Reward[] }>(`/users/${userId}/rewards?page=${page}&limit=${limit}`)
}

/**
 * Get marketplace listings
 */
export async function getMarketplaceListings(
  type: "buy" | "sell",
  page = 1,
  limit = 10,
): Promise<ApiResponse<{ listings: MarketplaceListing[] }>> {
  return fetchApi<{ listings: MarketplaceListing[] }>(`/marketplace/listings?type=${type}&page=${page}&limit=${limit}`)
}

/**
 * Create marketplace listing
 */
export async function createMarketplaceListing(
  listing: MarketplaceListingInput,
): Promise<ApiResponse<MarketplaceListing>> {
  return fetchApi<MarketplaceListing>("/marketplace/listings", {
    method: "POST",
    body: JSON.stringify(listing),
  })
}

/**
 * Register a new device
 */
export async function registerDevice(device: DeviceRegistration): Promise<ApiResponse<Device>> {
  return fetchApi<Device>("/devices", {
    method: "POST",
    body: JSON.stringify(device),
  })
}

// Types
export interface UserAccount {
  id: string
  name: string
  email: string
  xrpAddress: string
  totalRewards: number
  participationLevel: "basic" | "standard" | "premium"
  createdAt: string
}

export interface Device {
  id: string
  userId: string
  name: string
  type: string
  manufacturer: string
  model: string
  status: string
  enrolled: boolean
  curtailmentLevel: number
  lastSeen: string,
  location: string,
  energyCapacity: number
  createdAt: string
  preferences: string
}

export interface DeviceSettings {
  name?: string
  enrolled?: boolean
  curtailmentLevel?: number
  preferences?: Record<string, any>
}

export interface CurtailmentEvent {
  id: string
  startTime: string
  endTime: string
  duration: number
  status: "upcoming" | "active" | "completed" | "missed"
  energySaved: number
  reward: number
  devices: string[]
}

export interface Reward {
  id: string
  amount: number
  eventId: string
  timestamp: string
  status: "pending" | "completed"
  transactionId: string
}

export interface MarketplaceListing {
  id: string
  title: string
  description: string
  type: "buy" | "sell"
  provider: string
  providerType: "grid_operator" | "energy_supplier" | "residential" | "commercial"
  availabilityWindow: string
  pricePerKwh: number
  minKwh: number
  maxKwh: number
  status: "active" | "inactive" | "completed"
  createdAt: string
}

export interface MarketplaceListingInput {
  title: string
  description: string
  type: "buy" | "sell"
  availabilityWindow: string
  pricePerKwh: number
  minKwh: number
  maxKwh: number
}

export interface DeviceRegistration {
  userId: string
  name: string
  type: string
  manufacturer: string
  model: string
  enrolled: boolean
  curtailmentLevel: number
  location: string
  energyCapacity: number
  preferences: string
}

