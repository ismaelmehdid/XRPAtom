"use client"

import { useEffect, useState } from "react"
import { Bar, BarChart, ResponsiveContainer, XAxis, YAxis, Tooltip, Legend, CartesianGrid, TooltipProps } from "recharts"
import { NameType, ValueType } from "recharts/types/component/DefaultTooltipContent"
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card"
import { yearlyData } from "./energy-data"

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
          updatedData.push(newData); // <- ou newData.consumption si c'est un objet
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

export function LastYearConsumption() {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Last Year Consumption</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="h-[300px]">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={yearlyData}>
              <YAxis />
              <Tooltip
                content={({ active, payload }: TooltipProps<ValueType, NameType>) => {
                  if (active && payload && payload.length) {
                    const consumption = payload[0].value as number
                    const baseline = payload[1].value as number
                    return (
                      <div className="rounded-lg border bg-background p-2 shadow-sm">
                        <div className="grid grid-cols-2 gap-2">
                          <div className="flex flex-col">
                            <span className="text-[0.70rem] uppercase text-muted-foreground">
                              Consumption
                            </span>
                            <span className="font-bold text-muted-foreground">
                              {consumption.toFixed(2)} kWh
                            </span>
                          </div>
                          <div className="flex flex-col">
                            <span className="text-[0.70rem] uppercase text-muted-foreground">
                              Baseline
                            </span>
                            <span className="font-bold">
                              {baseline.toFixed(2)} kWh
                            </span>
                          </div>
                        </div>
                      </div>
                    )
                  }
                  return null
                }}
              />
              <Bar
                dataKey="curtailed"
                name="Energy Usage"
                fill="currentColor"
                className="fill-primary"
                radius={[4, 4, 0, 0]}
              />
              <Bar
                dataKey="baseline"
                name="Baseline"
                fill="currentColor"
                className="fill-muted"
                radius={[4, 4, 0, 0]}
                opacity={0.3}
              />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  )
}

