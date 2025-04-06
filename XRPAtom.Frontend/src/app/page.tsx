"use client"

import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import Link from "next/link"
import { ArrowRight, Battery, Lightbulb, Zap, BarChart3, Home, LogIn, Building } from "lucide-react"
import { useAuth } from "@/contexts/auth-context"
import { Alert, AlertDescription } from "@/components/ui/alert"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"

export default function HomePage() {
  const { isLoggedIn, login, isTSO, userData } = useAuth()

  return (
    <div className="container mx-auto px-4 py-12">
      {/* Login Status Alert */}
      {isLoggedIn ? (
        <Alert className="mb-8 bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-900">
          <Zap className="h-4 w-4 text-green-600 dark:text-green-400" />
          <AlertDescription className="text-green-600 dark:text-green-400">
            Welcome back, {userData?.name}! You are logged in as a {isTSO ? "Grid Operator" : "Residential User"}.
            {isTSO ? (
              <>
                {" "}
                You can{" "}
                <Link href="/create-offer" className="font-bold underline">
                  create offers
                </Link>{" "}
                to purchase energy flexibility.
              </>
            ) : (
              <>
                {" "}
                You can sell your energy flexibility on the{" "}
                <Link href="/marketplace" className="font-bold underline">
                  marketplace
                </Link>
                .
              </>
            )}
          </AlertDescription>
        </Alert>
      ) : (
        <Alert className="mb-8">
          <LogIn className="h-4 w-4" />
          <AlertDescription>
            <Link href="/login" className="font-bold underline">
              Login
            </Link>{" "}
            to access the dashboard and marketplace features.
          </AlertDescription>
        </Alert>
      )}

      <section className="py-12 md:py-24 lg:py-32">
        <div className="container px-4 md:px-6">
          <div className="flex flex-col items-center space-y-4 text-center">
            <div className="space-y-2">
              <h1 className="text-3xl font-bold tracking-tighter sm:text-4xl md:text-5xl lg:text-6xl/none">
                XRPAtom Energy Flexibility Platform
              </h1>
              <p className="mx-auto max-w-[700px] text-gray-500 md:text-xl dark:text-gray-400">
                Connecting grid operators with residential energy flexibility through the XRP Ledger
              </p>
            </div>
            <div className="space-x-4">
              {isLoggedIn ? (
                <Button asChild>
                  <Link href="/dashboard">
                    Go to Dashboard <ArrowRight className="ml-2 h-4 w-4" />
                  </Link>
                </Button>
              ) : (
                <Button asChild>
                  <Link href="/login">
                    Get Started <LogIn className="ml-2 h-4 w-4" />
                  </Link>
                </Button>
              )}
              {!isLoggedIn && (
              <Button variant="outline" asChild>
                <Link href="/how-it-works">Learn How It Works</Link>
              </Button>
              )}
            </div>
          </div>
        </div>
      </section>

      <section className="py-12 md:py-24 lg:py-32 bg-muted/50">
        <div className="container px-4 md:px-6">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold tracking-tight mb-4">For Grid Operators & Residential Users</h2>
            <p className="text-lg text-muted-foreground max-w-3xl mx-auto">
              XRPAtom serves both grid operators seeking flexibility and residential users looking to monetize their
              energy assets.
            </p>
          </div>

          <Tabs defaultValue="residential" className="w-full max-w-4xl mx-auto">
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="residential" className="text-center py-3">
                <Home className="h-4 w-4 mr-2 inline" />
                Residential Users
              </TabsTrigger>
              <TabsTrigger value="tso" className="text-center py-3">
                <Building className="h-4 w-4 mr-2 inline" />
                Grid Operators
              </TabsTrigger>
            </TabsList>

            <TabsContent value="residential" className="mt-8">
              <div className="grid gap-6 lg:grid-cols-3 lg:gap-12">
                <Card>
                  <CardHeader>
                    <Home className="h-8 w-8 text-primary mb-2" />
                    <CardTitle>Connect Your Home</CardTitle>
                    <CardDescription>Link your smart devices to the XRPAtom platform</CardDescription>
                  </CardHeader>
                  <CardContent>
                    <p>
                      Easily connect your smart thermostats, water heaters, EV chargers, and other devices to
                      participate in energy curtailment events.
                    </p>
                  </CardContent>
                  <CardFooter>
                    <Button variant="ghost" size="sm" asChild>
                      <Link href="/devices">Connect Devices</Link>
                    </Button>
                  </CardFooter>
                </Card>
                <Card>
                  <CardHeader>
                    <Lightbulb className="h-8 w-8 text-primary mb-2" />
                    <CardTitle>Reduce Consumption</CardTitle>
                    <CardDescription>Automatically reduce energy use during peak demand</CardDescription>
                  </CardHeader>
                  <CardContent>
                    <p>
                      When the grid is stressed, your devices will automatically reduce consumption with minimal impact
                      on your comfort.
                    </p>
                  </CardContent>
                  <CardFooter>
                    <Button variant="ghost" size="sm" asChild>
                      <Link href="/curtailment">Learn About Curtailment</Link>
                    </Button>
                  </CardFooter>
                </Card>
                <Card>
                  <CardHeader>
                    <Zap className="h-8 w-8 text-primary mb-2" />
                    <CardTitle>Earn Rewards</CardTitle>
                    <CardDescription>Receive XRP tokens for your energy flexibility</CardDescription>
                  </CardHeader>
                  <CardContent>
                    <p>
                      Get rewarded with tokenized incentives on the XRP Ledger for every kilowatt-hour of energy you
                      help save during peak periods.
                    </p>
                  </CardContent>
                  <CardFooter>
                    <Button variant="ghost" size="sm" asChild>
                      <Link href="/rewards">View Reward Structure</Link>
                    </Button>
                  </CardFooter>
                </Card>
              </div>
            </TabsContent>

            <TabsContent value="tso" className="mt-8">
              <div className="grid gap-6 lg:grid-cols-3 lg:gap-12">
                <Card>
                  <CardHeader>
                    <Building className="h-8 w-8 text-primary mb-2" />
                    <CardTitle>Create Flexibility Offers</CardTitle>
                    <CardDescription>Define your flexibility needs and pricing</CardDescription>
                  </CardHeader>
                  <CardContent>
                    <p>
                      Create customized offers specifying when you need flexibility, how much you need, and what you're
                      willing to pay.
                    </p>
                  </CardContent>
                  <CardFooter>
                    <Button variant="ghost" size="sm" asChild>
                      <Link href="/create-offer">Create Offer</Link>
                    </Button>
                  </CardFooter>
                </Card>
                <Card>
                  <CardHeader>
                    <BarChart3 className="h-8 w-8 text-primary mb-2" />
                    <CardTitle>Access Aggregated Flexibility</CardTitle>
                    <CardDescription>Tap into residential energy resources</CardDescription>
                  </CardHeader>
                  <CardContent>
                    <p>
                      Access a pool of residential flexibility resources that can be dispatched when needed to support
                      grid stability.
                    </p>
                  </CardContent>
                  <CardFooter>
                    <Button variant="ghost" size="sm" asChild>
                      <Link href="/marketplace">Browse Marketplace</Link>
                    </Button>
                  </CardFooter>
                </Card>
                <Card>
                  <CardHeader>
                    <Battery className="h-8 w-8 text-primary mb-2" />
                    <CardTitle>Transparent Verification</CardTitle>
                    <CardDescription>Blockchain-verified curtailment</CardDescription>
                  </CardHeader>
                  <CardContent>
                    <p>
                      All flexibility events are verified and recorded on the XRP Ledger, ensuring transparency and
                      trust.
                    </p>
                  </CardContent>
                  <CardFooter>
                    <Button variant="ghost" size="sm" asChild>
                      <Link href="/verification">Learn More</Link>
                    </Button>
                  </CardFooter>
                </Card>
              </div>
            </TabsContent>
          </Tabs>
        </div>
      </section>

      <section className="py-12 md:py-24">
        <div className="container px-4 md:px-6">
          <div className="grid gap-6 lg:grid-cols-2 lg:gap-12 items-center">
            <div>
              <h2 className="text-3xl font-bold tracking-tight mb-4">Why Join XRPAtom?</h2>
              <ul className="space-y-4">
                <li className="flex items-start">
                  <div className="mr-4 mt-1 bg-primary rounded-full p-1">
                    <Zap className="h-4 w-4 text-primary-foreground" />
                  </div>
                  <div>
                    <h3 className="font-bold">Tokenized Energy Flexibility</h3>
                    <p className="text-muted-foreground">
                      Energy flexibility is tokenized on the XRP Ledger, creating a transparent and efficient
                      marketplace.
                    </p>
                  </div>
                </li>
                <li className="flex items-start">
                  <div className="mr-4 mt-1 bg-primary rounded-full p-1">
                    <Battery className="h-4 w-4 text-primary-foreground" />
                  </div>
                  <div>
                    <h3 className="font-bold">Support Grid Stability</h3>
                    <p className="text-muted-foreground">
                      Help prevent blackouts and reduce the need for expensive peaker plants.
                    </p>
                  </div>
                </li>
                <li className="flex items-start">
                  <div className="mr-4 mt-1 bg-primary rounded-full p-1">
                    <BarChart3 className="h-4 w-4 text-primary-foreground" />
                  </div>
                  <div>
                    <h3 className="font-bold">Reduce Carbon Emissions</h3>
                    <p className="text-muted-foreground">
                      Peak demand is often met with the most polluting power plants. Your curtailment helps reduce
                      emissions.
                    </p>
                  </div>
                </li>
              </ul>
              {!isLoggedIn && (
                <Button className="mt-6" asChild>
                  <Link href="/register">Join XRPAtom Today</Link>
                </Button>
              )}
            </div>
            <div className="bg-muted rounded-lg p-6">
              <div className="text-center mb-4">
                <h3 className="text-xl font-bold">Platform Statistics</h3>
                <p className="text-sm text-muted-foreground">Real-time energy flexibility marketplace</p>
              </div>
              <div className="space-y-4">
                <div className="flex justify-between items-center">
                  <span>Residential Users</span>
                  <span className="font-bold">5,280+</span>
                </div>
                <div className="flex justify-between items-center">
                  <span>Grid Operators</span>
                  <span className="font-bold">24</span>
                </div>
                <div className="flex justify-between items-center">
                  <span>Energy Curtailed</span>
                  <span className="font-bold">128.5 MWh</span>
                </div>
                <div className="flex justify-between items-center">
                  <span>XRP Rewards Distributed</span>
                  <span className="font-bold">42,500 XRP</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  )
}

