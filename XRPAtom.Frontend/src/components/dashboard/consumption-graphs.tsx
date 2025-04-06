"use client"

import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Bar, BarChart, ResponsiveContainer, XAxis, YAxis, Tooltip, Legend, CartesianGrid } from "recharts"
import { Leaf, Coins } from "lucide-react"
import { last30DaysData, last7DaysData } from "./energy-data"
import { yearlyData } from "./energy-data"
import { NameType } from "recharts/types/component/DefaultTooltipContent"

export function Last30DaysConsumption() {
  const totalCo2Saved = last30DaysData.reduce((sum, day) => sum + day.co2Saved, 0)
  const totalPriceSaved = last30DaysData.reduce((sum, day) => sum + parseFloat(day.priceSaved), 0)

  return (
    <Card>
      <CardHeader>
        <CardTitle>Last 30 Days Consumption</CardTitle>
        <CardDescription>Your energy consumption and savings over the past month</CardDescription>
      </CardHeader>
      <CardContent>
        <div className="grid gap-4 md:grid-cols-2 mb-4">
          <div className="flex items-center space-x-4 rounded-md border p-4">
            <Leaf className="h-6 w-6 text-green-500" />
            <div>
              <p className="text-sm font-medium">CO2 Saved</p>
              <p className="text-2xl font-bold">{totalCo2Saved} kg</p>
            </div>
          </div>
          <div className="flex items-center space-x-4 rounded-md border p-4">
            <Coins className="h-6 w-6 text-yellow-500" />
            <div>
              <p className="text-sm font-medium">Price Saved</p>
              <p className="text-2xl font-bold">${totalPriceSaved.toFixed(2)}</p>
            </div>
          </div>
        </div>
        <ResponsiveContainer width="100%" height={350}>
          <BarChart data={last30DaysData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="date" stroke="#888888" fontSize={12} tickLine={false} axisLine={false} />
            <YAxis
              stroke="#888888"
              fontSize={12}
              tickLine={false}
              axisLine={false}
              tickFormatter={(value) => `${value} kWh`}
            />
            <Tooltip
              formatter={(value, name) => [`${value} kWh`, name === "curtailed" ? "Energy Curtailed" : "Baseline Usage"]}
              labelFormatter={(label) => `Date: ${label}`}
            />
            <Legend />
            <Bar name="Energy Curtailed" dataKey="curtailed" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
            <Bar
              name="Baseline Usage"
              dataKey="baseline"
              fill="hsl(var(--muted-foreground))"
              radius={[4, 4, 0, 0]}
              opacity={0.5}
            />
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}

export function Last7DaysConsumption() {
  const totalCo2Saved = last7DaysData.reduce((sum, day) => sum + day.co2Saved, 0)
  const totalPriceSaved = last7DaysData.reduce((sum, day) => sum + parseFloat(day.priceSaved), 0)

  return (
    <Card>
      <CardHeader>
        <CardTitle>Last 7 Days Consumption</CardTitle>
        <CardDescription>Your energy consumption and savings over the past week</CardDescription>
      </CardHeader>
      <CardContent>
        <div className="grid gap-4 md:grid-cols-2 mb-4">
          <div className="flex items-center space-x-4 rounded-md border p-4">
            <Leaf className="h-6 w-6 text-green-500" />
            <div>
              <p className="text-sm font-medium">CO2 Saved</p>
              <p className="text-2xl font-bold">{totalCo2Saved} kg</p>
            </div>
          </div>
          <div className="flex items-center space-x-4 rounded-md border p-4">
            <Coins className="h-6 w-6 text-yellow-500" />
            <div>
              <p className="text-sm font-medium">Price Saved</p>
              <p className="text-2xl font-bold">${totalPriceSaved.toFixed(2)}</p>
            </div>
          </div>
        </div>
        <ResponsiveContainer width="100%" height={350}>
          <BarChart data={last7DaysData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="date" stroke="#888888" fontSize={12} tickLine={false} axisLine={false} />
            <YAxis
              stroke="#888888"
              fontSize={12}
              tickLine={false}
              axisLine={false}
              tickFormatter={(value) => `${value} kWh`}
            />
            <Tooltip
              formatter={(value, name) => [`${value} kWh`, name === "curtailed" ? "Energy Curtailed" : "Baseline Usage"]}
              labelFormatter={(label) => `Date: ${label}`}
            />
            <Legend />
            <Bar name="Energy Curtailed" dataKey="curtailed" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
            <Bar
              name="Baseline Usage"
              dataKey="baseline"
              fill="hsl(var(--muted-foreground))"
              radius={[4, 4, 0, 0]}
              opacity={0.5}
            />
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}

export function LastYearConsumption() {
  const totalCo2Saved = yearlyData.reduce((sum, month) => sum + month.co2Saved, 0)
  const totalPriceSaved = yearlyData.reduce((sum, month) => sum + parseFloat(month.priceSaved), 0)

  return (
    <Card>
      <CardHeader>
        <CardTitle>Last Year Consumption</CardTitle>
        <CardDescription>Monthly energy consumption overview</CardDescription>
      </CardHeader>
      <CardContent>
        <div className="grid gap-4 md:grid-cols-2 mb-4">
          <div className="flex items-center space-x-4 rounded-md border p-4">
            <Leaf className="h-6 w-6 text-green-500" />
            <div>
              <p className="text-sm font-medium">CO2 Saved</p>
              <p className="text-2xl font-bold">{totalCo2Saved} kg</p>
            </div>
          </div>
          <div className="flex items-center space-x-4 rounded-md border p-4">
            <Coins className="h-6 w-6 text-yellow-500" />
            <div>
              <p className="text-sm font-medium">Price Saved</p>
              <p className="text-2xl font-bold">${totalPriceSaved.toFixed(2)}</p>
            </div>
          </div>
        </div>
        <ResponsiveContainer width="100%" height={350}>
          <BarChart data={yearlyData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" stroke="#888888" fontSize={12} tickLine={false} axisLine={false} />
            <YAxis
              stroke="#888888"
              fontSize={12}
              tickLine={false}
              axisLine={false}
              tickFormatter={(value) => `${Math.round(value)} kWh`}
            />
            <Tooltip
              formatter={(value, name) => [`${Math.round(value as number)} kWh`, name === "curtailed" ? "Energy Curtailed" : "Baseline Usage"]}
              labelFormatter={(label) => `Month: ${label}`}
            />
            <Legend />
            <Bar name="Energy Curtailed" dataKey="curtailed" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
            <Bar
              name="Baseline Usage"
              dataKey="baseline"
              fill="hsl(var(--muted-foreground))"
              radius={[4, 4, 0, 0]}
              opacity={0.5}
            />
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}

