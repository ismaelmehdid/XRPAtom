"use client"

import { useEffect, useState } from "react"
import { Bar, BarChart, ResponsiveContainer, XAxis, YAxis, Tooltip, Legend, CartesianGrid } from "recharts"
import { Card, CardHeader, CardTitle, CardContent, CardDescription } from "@/components/ui/card"

const data = [
  {
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },  
  {
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
  {
    curtailed: Math.floor(Math.random() * 50) + 10,
    baseline: Math.floor(Math.random() * 100) + 50,
  },
]

export function LiveEnergyUsage() {
  const [liveData, setLiveData] = useState<number[]>([]);

  useEffect(() => {
    const web_socket = new WebSocket("wss://wss.zunix.systems/ws");

    web_socket.onopen = () => {
      console.log("✅ WebSocket connected");
    };

    web_socket.onmessage = (event) => {
      try {
        const newData = JSON.parse(event.data);
  
        setLiveData((prevData) => {
          const updatedData = [...prevData];
          if (updatedData.length >= 60) updatedData.shift();
          updatedData.push(newData);
          return updatedData;
        });
      } catch (err) {
        console.error("❌ Error parsing message:", event.data, err);
      }
    };

    web_socket.onclose = () => {
      console.log("WebSocket connection closed");
    };

  }, []);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Live Energy Usage</CardTitle>
        <CardDescription>Real-time energy consumption monitoring</CardDescription>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={350}>
          <BarChart data={liveData}>
            <CartesianGrid strokeDasharray="3 3" />
            <YAxis
              stroke="#888888"
              fontSize={12}
              tickLine={false}
              axisLine={false}
              tickFormatter={(value) => `${value} kWh`}
            />
            <Tooltip
              formatter={(value) => [`${value} kWh`]}
            />
            <Bar name="Energy Curtailed" dataKey="consumption" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}

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
