import type { Metadata } from "next"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import Link from "next/link"
import { ArrowRight, Lightbulb, Zap, BarChart3, Clock, Wallet } from "lucide-react"

export const metadata: Metadata = {
  title: "How It Works | XRPAtom",
  description: "Learn how XRPAtom enables residential energy curtailment",
}

export default function HowItWorksPage() {
  return (
    <div className="container mx-auto px-4 py-12">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-4xl font-bold mb-6">How XRPAtom Works</h1>

        <div className="prose dark:prose-invert max-w-none mb-12">
          <p className="text-xl mb-8">
            XRPAtom is a decentralized platform that enables residential energy consumers to participate in grid
            balancing by reducing their consumption during peak demand periods, earning tokenized rewards on the XRP
            Ledger.
          </p>
        </div>

        <div className="space-y-12">
          <section>
            <h2 className="text-2xl font-bold mb-6 flex items-center">
              <span className="bg-primary text-primary-foreground rounded-full w-8 h-8 inline-flex items-center justify-center mr-2">
                1
              </span>
              Connect Your Smart Devices
            </h2>
            <div className="grid md:grid-cols-2 gap-6">
              <div>
                <p className="mb-4">XRPAtom integrates with popular smart home devices including:</p>
                <ul className="list-disc pl-6 space-y-2 mb-6">
                  <li>Smart thermostats (Nest, Ecobee, etc.)</li>
                  <li>Smart water heaters</li>
                  <li>EV chargers</li>
                  <li>Smart plugs and switches</li>
                  <li>Home battery systems</li>
                </ul>
                <p>Our platform uses secure APIs to communicate with your devices, ensuring privacy and security.</p>
              </div>
              <Card>
                <CardHeader>
                  <CardTitle>Device Compatibility</CardTitle>
                  <CardDescription>XRPAtom works with most major smart home ecosystems</CardDescription>
                </CardHeader>
                <CardContent>
                  <ul className="space-y-2">
                    <li className="flex items-center">
                      <Lightbulb className="h-4 w-4 mr-2 text-primary" />
                      Google Nest
                    </li>
                    <li className="flex items-center">
                      <Lightbulb className="h-4 w-4 mr-2 text-primary" />
                      Apple HomeKit
                    </li>
                    <li className="flex items-center">
                      <Lightbulb className="h-4 w-4 mr-2 text-primary" />
                      Amazon Alexa
                    </li>
                    <li className="flex items-center">
                      <Lightbulb className="h-4 w-4 mr-2 text-primary" />
                      Samsung SmartThings
                    </li>
                    <li className="flex items-center">
                      <Lightbulb className="h-4 w-4 mr-2 text-primary" />
                      And many more...
                    </li>
                  </ul>
                </CardContent>
              </Card>
            </div>
          </section>

          <section>
            <h2 className="text-2xl font-bold mb-6 flex items-center">
              <span className="bg-primary text-primary-foreground rounded-full w-8 h-8 inline-flex items-center justify-center mr-2">
                2
              </span>
              Set Your Preferences
            </h2>
            <div className="grid md:grid-cols-2 gap-6">
              <Card>
                <CardHeader>
                  <CardTitle>Customizable Curtailment</CardTitle>
                  <CardDescription>You control how much and when your devices participate</CardDescription>
                </CardHeader>
                <CardContent>
                  <ul className="space-y-2">
                    <li className="flex items-center">
                      <Clock className="h-4 w-4 mr-2 text-primary" />
                      Set preferred times for participation
                    </li>
                    <li className="flex items-center">
                      <Zap className="h-4 w-4 mr-2 text-primary" />
                      Define minimum and maximum curtailment levels
                    </li>
                    <li className="flex items-center">
                      <BarChart3 className="h-4 w-4 mr-2 text-primary" />
                      Adjust settings per device
                    </li>
                  </ul>
                </CardContent>
              </Card>
              <div>
                <p className="mb-4">You remain in control of your energy usage. Set preferences for:</p>
                <ul className="list-disc pl-6 space-y-2 mb-6">
                  <li>Comfort thresholds (e.g., minimum/maximum temperatures)</li>
                  <li>Time windows when curtailment is acceptable</li>
                  <li>Maximum duration of curtailment events</li>
                  <li>Device-specific settings</li>
                </ul>
                <p>Our platform respects your preferences and will never curtail beyond your specified limits.</p>
              </div>
            </div>
          </section>

          <section>
            <h2 className="text-2xl font-bold mb-6 flex items-center">
              <span className="bg-primary text-primary-foreground rounded-full w-8 h-8 inline-flex items-center justify-center mr-2">
                3
              </span>
              Participate in Curtailment Events
            </h2>
            <div className="grid md:grid-cols-2 gap-6">
              <div>
                <p className="mb-4">
                  When the grid is under stress, XRPAtom receives signals from grid operators requesting curtailment.
                </p>
                <p className="mb-4">Your devices automatically respond based on your preferences:</p>
                <ul className="list-disc pl-6 space-y-2 mb-6">
                  <li>Thermostats adjust by 1-3°F temporarily</li>
                  <li>Water heaters delay heating cycles</li>
                  <li>EV charging slows or pauses briefly</li>
                  <li>Other devices reduce consumption as configured</li>
                </ul>
                <p>Most curtailment events last 1-3 hours and occur during peak demand periods.</p>
              </div>
              <Card>
                <CardHeader>
                  <CardTitle>Automated & Seamless</CardTitle>
                  <CardDescription>No manual intervention required</CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  <p>The XRPAtom platform handles everything automatically:</p>
                  <ul className="space-y-2">
                    <li className="flex items-center">
                      <Zap className="h-4 w-4 mr-2 text-primary" />
                      Receives grid signals
                    </li>
                    <li className="flex items-center">
                      <Zap className="h-4 w-4 mr-2 text-primary" />
                      Dispatches commands to your devices
                    </li>
                    <li className="flex items-center">
                      <Zap className="h-4 w-4 mr-2 text-primary" />
                      Measures and verifies energy reduction
                    </li>
                    <li className="flex items-center">
                      <Zap className="h-4 w-4 mr-2 text-primary" />
                      Returns devices to normal after the event
                    </li>
                  </ul>
                </CardContent>
              </Card>
            </div>
          </section>

          <section>
            <h2 className="text-2xl font-bold mb-6 flex items-center">
              <span className="bg-primary text-primary-foreground rounded-full w-8 h-8 inline-flex items-center justify-center mr-2">
                4
              </span>
              Earn XRP Rewards
            </h2>
            <div className="grid md:grid-cols-2 gap-6">
              <Card>
                <CardHeader>
                  <CardTitle>Transparent Rewards</CardTitle>
                  <CardDescription>Tokenized compensation on the XRP Ledger</CardDescription>
                </CardHeader>
                <CardContent>
                  <ul className="space-y-2">
                    <li className="flex items-center">
                      <Wallet className="h-4 w-4 mr-2 text-primary" />
                      Rewards based on actual kWh curtailed
                    </li>
                    <li className="flex items-center">
                      <Wallet className="h-4 w-4 mr-2 text-primary" />
                      Payments in XRP tokens
                    </li>
                    <li className="flex items-center">
                      <Wallet className="h-4 w-4 mr-2 text-primary" />
                      Transparent blockchain verification
                    </li>
                    <li className="flex items-center">
                      <Wallet className="h-4 w-4 mr-2 text-primary" />
                      Automatic deposits to your XRP wallet
                    </li>
                  </ul>
                </CardContent>
              </Card>
              <div>
                <p className="mb-4">After each curtailment event:</p>
                <ul className="list-disc pl-6 space-y-2 mb-6">
                  <li>Your energy reduction is measured against your baseline</li>
                  <li>The curtailed kWh is calculated and verified</li>
                  <li>XRP tokens are awarded based on the current rate</li>
                  <li>Rewards are deposited directly to your XRP wallet</li>
                </ul>
                <p>All transactions are recorded on the XRP Ledger, ensuring transparency and immutability.</p>
              </div>
            </div>
          </section>
        </div>

        <div className="mt-12 text-center">
          <Button size="lg" asChild>
            <Link href="/register">
              Get Started Today <ArrowRight className="ml-2 h-4 w-4" />
            </Link>
          </Button>
          <p className="mt-4 text-muted-foreground">
            Join thousands of households already earning rewards for energy flexibility
          </p>
        </div>
      </div>
    </div>
  )
}

