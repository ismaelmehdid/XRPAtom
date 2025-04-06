"use client"

import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { useEffect, useState } from "react"
import { toast } from "sonner"
import { Button } from "../ui/button"

const mockEvents = [
  {
    id: "EVT-1234",
    date: "2023-04-01",
    time: "14:00-16:00",
    duration: "2 hours",
    energySaved: "2.5 kWh",
    reward: "5.2 XRP",
    status: "completed",
  },
  {
    id: "EVT-1235",
    date: "2023-04-05",
    time: "17:30-19:30",
    duration: "2 hours",
    energySaved: "3.1 kWh",
    reward: "6.8 XRP",
    status: "completed",
  },
  {
    id: "EVT-1236",
    date: "2023-04-10",
    time: "13:00-15:00",
    duration: "2 hours",
    energySaved: "1.8 kWh",
    reward: "3.5 XRP",
    status: "completed",
  },
  {
    id: "EVT-1237",
    date: "2023-04-15",
    time: "18:00-20:00",
    duration: "2 hours",
    energySaved: "0 kWh",
    reward: "0 XRP",
    status: "missed",
  },
  {    
    id: "EVT-1238",
    date: "2023-04-22",
    time: "15:00-17:00",
    duration: "2 hours",
    energySaved: "2.9 kWh",
    reward: "6.1 XRP",
    status: "completed",
  },
]

export function CurtailmentEvents() {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isConnecting, setIsConnecting] = useState(false);

  const connectXummWallet = async () => {
    try {
      setIsConnecting(true);
      const response = await fetch("https://api.zunix.systems/api/wallet/connect-xumm", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
        },
        credentials: "include"
      });

      if (!response.ok) {
        throw new Error('Failed to connect to XUMM wallet');
      }

      const data = await response.json();
      toast.success("Successfully connected to XUMM wallet");
      // Handle the response data as needed
      console.log("XUMM connection response:", data);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to connect to XUMM wallet');
    } finally {
      setIsConnecting(false);
    }
  };

  useEffect(() => {
    async function fetchEvents() {
      try {
        const response = await fetch("https://api.zunix.systems/api/curtailment-events", {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            "Accept": "application/json",
          },
          credentials: "include"
        });

        if (!response.ok) {
          throw new Error('Network response was not ok');
        }

        const data = await response.json();
        setEvents(data.events);
      } catch (error) {
        toast.error('Error fetching events:');
      } finally {
        setLoading(false);
      }
    }

    fetchEvents();
  }, []);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Recent Curtailment Events</CardTitle>
        <CardDescription>Your participation in recent grid events</CardDescription>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Date</TableHead>
              <TableHead>Duration</TableHead>
              <TableHead>Energy Saved</TableHead>
              <TableHead>Status</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            <TableRow>
              <TableCell>2024-03-15</TableCell>
              <TableCell>2 hours</TableCell>
              <TableCell>15 kWh</TableCell>
              <TableCell>Completed</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>2024-03-10</TableCell>
              <TableCell>1 hour</TableCell>
              <TableCell>8 kWh</TableCell>
              <TableCell>Completed</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  )
}

