"use client"

import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { ArrowRight, Clock, Zap, BarChart3, Building, Home } from "lucide-react"
import { useAuth } from "@/contexts/auth-context"
import Link from "next/link"

export default function MarketplacePage() {
  const { isTSO } = useAuth()

  return (
    <div className="container mx-auto px-4 py-12">
      <div className="max-w-6xl mx-auto">
        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-8">
          <div>
            <h1 className="text-3xl font-bold mb-2">Flexibility Marketplace</h1>
            <p className="text-muted-foreground">
              {isTSO
                ? "Find and purchase energy flexibility from residential users"
                : "Sell your energy flexibility to grid operators"}
            </p>
          </div>
          {isTSO && (
            <Button asChild>
              <Link href="/create-offer">
                Create New Offer <ArrowRight className="ml-2 h-4 w-4" />
              </Link>
            </Button>
          )}
        </div>

        <Tabs defaultValue={isTSO ? "buy" : "sell"} className="space-y-8">
          <TabsList className="grid w-full grid-cols-2 md:w-auto">
            {isTSO && <TabsTrigger value="buy">Buy Flexibility</TabsTrigger>}
            <TabsTrigger value="sell">Sell Flexibility</TabsTrigger>
            <TabsTrigger value="active">Active Deals</TabsTrigger>
          </TabsList>

          {isTSO && (
            <TabsContent value="buy" className="space-y-8">
              <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                <Card>
                  <CardHeader>
                    <div className="flex justify-between items-start">
                      <div className="space-y-1">
                        <CardTitle>Residential Bundle</CardTitle>
                        <CardDescription>10 Households</CardDescription>
                      </div>
                      <Badge>Available</Badge>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      <div className="flex items-center">
                        <Home className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>Residential Aggregation</span>
                      </div>
                      <div className="flex items-center">
                        <Clock className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>5-9 PM Weekdays</span>
                      </div>
                      <div className="flex items-center">
                        <Zap className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>Asking 0.28 XRP per kWh</span>
                      </div>
                      <div className="flex items-center">
                        <BarChart3 className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>Up to 50 kWh available</span>
                      </div>
                    </div>
                  </CardContent>
                  <CardFooter>
                    <Button className="w-full">Purchase Flexibility</Button>
                  </CardFooter>
                </Card>

                <Card>
                  <CardHeader>
                    <div className="flex justify-between items-start">
                      <div className="space-y-1">
                        <CardTitle>EV Charging Network</CardTitle>
                        <CardDescription>25 Charging Stations</CardDescription>
                      </div>
                      <Badge>Available</Badge>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      <div className="flex items-center">
                        <Home className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>Commercial Aggregation</span>
                      </div>
                      <div className="flex items-center">
                        <Clock className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>24/7 Availability</span>
                      </div>
                      <div className="flex items-center">
                        <Zap className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>Asking 0.32 XRP per kWh</span>
                      </div>
                      <div className="flex items-center">
                        <BarChart3 className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>Up to 200 kWh available</span>
                      </div>
                    </div>
                  </CardContent>
                  <CardFooter>
                    <Button className="w-full">Purchase Flexibility</Button>
                  </CardFooter>
                </Card>

                <Card>
                  <CardHeader>
                    <div className="flex justify-between items-start">
                      <div className="space-y-1">
                        <CardTitle>Smart Thermostat Group</CardTitle>
                        <CardDescription>50 Households</CardDescription>
                      </div>
                      <Badge>Available</Badge>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      <div className="flex items-center">
                        <Home className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>Residential Aggregation</span>
                      </div>
                      <div className="flex items-center">
                        <Clock className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>2-6 PM Weekdays</span>
                      </div>
                      <div className="flex items-center">
                        <Zap className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>Asking 0.25 XRP per kWh</span>
                      </div>
                      <div className="flex items-center">
                        <BarChart3 className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>Up to 75 kWh available</span>
                      </div>
                    </div>
                  </CardContent>
                  <CardFooter>
                    <Button className="w-full">Purchase Flexibility</Button>
                  </CardFooter>
                </Card>
              </div>
            </TabsContent>
          )}

          <TabsContent value="sell" className="space-y-8">
            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
              {!isTSO && (
                <Card>
                  <CardHeader>
                    <div className="flex justify-between items-start">
                      <div className="space-y-1">
                        <CardTitle>Your Flexibility</CardTitle>
                        <CardDescription>Active Listing</CardDescription>
                      </div>
                      <Badge>Active</Badge>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      <div className="flex items-center">
                        <Home className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>Your Home</span>
                      </div>
                      <div className="flex items-center">
                        <Clock className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>5-9 PM Weekdays</span>
                      </div>
                      <div className="flex items-center">
                        <Zap className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>Asking 0.28 XRP per kWh</span>
                      </div>
                      <div className="flex items-center">
                        <BarChart3 className="h-4 w-4 mr-2 text-muted-foreground" />
                        <span>Up to 5 kWh available</span>
                      </div>
                    </div>
                  </CardContent>
                  <CardFooter>
                    <Button variant="outline" className="w-full">
                      Edit Listing
                    </Button>
                  </CardFooter>
                </Card>
              )}

              <Card>
                <CardHeader>
                  <div className="flex justify-between items-start">
                    <div className="space-y-1">
                      <CardTitle>Peak Shaving</CardTitle>
                      <CardDescription>Grid Operator</CardDescription>
                    </div>
                    <Badge>Open</Badge>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="flex items-center">
                      <Building className="h-4 w-4 mr-2 text-muted-foreground" />
                      <span>Pacific Grid Solutions</span>
                    </div>
                    <div className="flex items-center">
                      <Clock className="h-4 w-4 mr-2 text-muted-foreground" />
                      <span>4-7 PM Weekdays</span>
                    </div>
                    <div className="flex items-center">
                      <Zap className="h-4 w-4 mr-2 text-muted-foreground" />
                      <span>Offering 0.25 XRP per kWh</span>
                    </div>
                    <div className="flex items-center">
                      <BarChart3 className="h-4 w-4 mr-2 text-muted-foreground" />
                      <span>Min 2 kWh per event</span>
                    </div>
                  </div>
                </CardContent>
                <CardFooter>
                  <Button className="w-full" disabled={isTSO}>
                    {isTSO ? "Your Offer" : "Sell Flexibility"}
                  </Button>
                </CardFooter>
              </Card>

              <Card>
                <CardHeader>
                  <div className="flex justify-between items-start">
                    <div className="space-y-1">
                      <CardTitle>Frequency Response</CardTitle>
                      <CardDescription>Grid Operator</CardDescription>
                    </div>
                    <Badge>Open</Badge>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="flex items-center">
                      <Building className="h-4 w-4 mr-2 text-muted-foreground" />
                      <span>ISO New England</span>
                    </div>
                    <div className="flex items-center">
                      <Clock className="h-4 w-4 mr-2 text-muted-foreground" />
                      <span>24/7 Availability</span>
                    </div>
                    <div className="flex items-center">
                      <Zap className="h-4 w-4 mr-2 text-muted-foreground" />
                      <span>Offering 0.35 XRP per kWh</span>
                    </div>
                    <div className="flex items-center">
                      <BarChart3 className="h-4 w-4 mr-2 text-muted-foreground" />
                      <span>Min 1 kWh per event</span>
                    </div>
                  </div>
                </CardContent>
                <CardFooter>
                  <Button className="w-full" disabled={isTSO}>
                    {isTSO ? "Your Offer" : "Sell Flexibility"}
                  </Button>
                </CardFooter>
              </Card>
            </div>
          </TabsContent>

          <TabsContent value="active" className="space-y-8">
            <div>
              <h2 className="text-xl font-semibold mb-4">Your Active Deals</h2>
              <Card>
                <CardHeader>
                  <CardTitle>Transaction History</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="flex justify-between items-center pb-2 border-b">
                      <div>
                        <p className="font-medium">
                          {isTSO ? "Purchased from Residential Group" : "Sold to Pacific Grid Solutions"}
                        </p>
                        <p className="text-sm text-muted-foreground">Apr 15, 2023 • 2.5 kWh</p>
                      </div>
                      <div className="text-right">
                        <p className="font-medium">0.625 XRP</p>
                        <p className="text-sm text-green-600">Completed</p>
                      </div>
                    </div>
                    <div className="flex justify-between items-center pb-2 border-b">
                      <div>
                        <p className="font-medium">{isTSO ? "Purchased from EV Network" : "Sold to ISO New England"}</p>
                        <p className="text-sm text-muted-foreground">Apr 12, 2023 • 1.8 kWh</p>
                      </div>
                      <div className="text-right">
                        <p className="font-medium">0.63 XRP</p>
                        <p className="text-sm text-green-600">Completed</p>
                      </div>
                    </div>
                    <div className="flex justify-between items-center pb-2 border-b">
                      <div>
                        <p className="font-medium">
                          {isTSO ? "Purchased from Smart Thermostat Group" : "Sold to Green Mountain Energy"}
                        </p>
                        <p className="text-sm text-muted-foreground">Apr 10, 2023 • 3.2 kWh</p>
                      </div>
                      <div className="text-right">
                        <p className="font-medium">0.96 XRP</p>
                        <p className="text-sm text-green-600">Completed</p>
                      </div>
                    </div>
                  </div>
                </CardContent>
                <CardFooter>
                  <Button variant="outline" className="w-full">
                    View All Transactions
                  </Button>
                </CardFooter>
              </Card>
            </div>
          </TabsContent>
        </Tabs>
      </div>
    </div>
  )
}

