"use client"

import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { useEffect, useState } from "react"
import { toast } from "sonner"
import { Button } from "../ui/button"
import { fetchApi } from "@/lib/api"


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
        const response = await fetchApi(`/curtailment-events`, {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            "Accept": "application/json",
          },
        });

        if (response.error) {
          throw new Error(response.error)
        }

        const data = await response.data
        if (!Array.isArray(data)) {
          return new Error("Invalid data format");
        }
        let events = [];
        events = data.map((event) => ({
            date: event.date,
            duration: event.duration,
            energySaved: event.energySaved,
            status: event.status,
          }));
        console.log("Fetched events:", events);
        //setEvents(events);
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

