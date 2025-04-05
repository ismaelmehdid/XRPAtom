"use client"

import { Bar, BarChart, ResponsiveContainer, XAxis, YAxis, Tooltip, Legend, CartesianGrid } from "recharts"

const data = [
  {
    name: "Jan",
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    name: "Feb",
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    name: "Mar",
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    name: "Apr",
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    name: "May",
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    name: "Jun",
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    name: "Jul",
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    name: "Aug",
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    name: "Sep",
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    name: "Oct",
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    name: "Nov",
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    name: "Dec",
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
]

export function EnergyOverview() {
  return (
    <ResponsiveContainer width="100%" height={350}>
      <BarChart data={data}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="name" stroke="#888888" fontSize={12} tickLine={false} axisLine={false} />
        <YAxis
          stroke="#888888"
          fontSize={12}
          tickLine={false}
          axisLine={false}
          tickFormatter={(value) => `${value} kWh`}
        />
        <Tooltip
          formatter={(value, name) => [`${value} kWh`, name === "curtailed" ? "Energy Curtailed" : "Baseline Usage"]}
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
  )
}

