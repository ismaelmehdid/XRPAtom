"use client"

import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { useEffect, useState } from "react"
import { toast } from "sonner"
import { Button } from "../ui/button"
import { fetchApi } from "@/lib/api"
import { format } from "date-fns"
import { getAuthToken } from "@/lib/auth"

interface CurtailmentEvent {
  blockchainReference: string
  createdAt: string
  createdBy: string
  description: string
  duration: number
  endTime: string
  id: string
  participantCount: number
  rewardPerKwh: number
  startTime: string
  status: string
  title: string
  totalEnergySaved: number
  totalRewardsPaid: number
  updatedAt: string
  userIsParticipant: boolean
}

export function CurtailmentEvents() {
  const [events, setEvents] = useState<CurtailmentEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isConnecting, setIsConnecting] = useState(false);
  const token = getAuthToken()

  useEffect(() => {
    async function fetchEvents() {
      try {
        const response = await fetchApi(`/curtailment-events/upcoming?hours=10000&page=1&limit=10`, {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            "Accept": "application/json",
            "Authorization": `Bearer ${token}`,
          },
        });

        if (response.error) {
          toast.error(response.error);
        } else {
          console.log("RESPONSE: ", response)
        }

        const data: CurtailmentEvent[] = await response.data.events as CurtailmentEvent[];
        
        setEvents(data);
      } catch (error) {
        toast.error('Error fetching events:');
      } finally {
        setLoading(false);
      }
    }

    fetchEvents();
  }, []);

  return (
    <div className="space-y-4">
      {events.map((event) => (
        <Card key={event.id}>
          <CardContent className="pt-6">
            <div className="flex justify-between items-start">
              <div className="space-y-1">
                <h3 className="font-semibold">{event.title}</h3>
                <p className="text-sm text-muted-foreground">{event.description}</p>
                <div className="flex gap-4 text-sm text-muted-foreground">
                  <div>
                    <span className="font-medium">Start:</span>{" "}
                    {format(new Date(event.startTime), "MMM d, yyyy HH:mm")}
                  </div>
                  <div>
                    <span className="font-medium">Duration:</span> {event.duration} minutes
                  </div>
                  <div>
                    <span className="font-medium">Reward:</span> {event.rewardPerKwh} XRP/kWh
                  </div>
                  <div>
                    <span className="font-medium">Participants:</span> {event.participantCount}
                  </div>
                </div>
              </div>
              <Button variant="default" disabled={event.userIsParticipant} onClick={ async () => {
                try {
                  const response = await fetchApi(`/curtailment-events/register/${event.id}`, {
                    method: "POST",
                    headers: {
                      "Content-Type": "application/json",
                      "Authorization": `Bearer ${token}`,
                    },
                  });

                  if (response.error) {
                    toast.error(response.error);
                  } else {
                    toast.success("Successfully registered for event");
                  }
                } catch (error) {
                  toast.error("Error registering for event");
                }
              }}>
                {event.userIsParticipant ? "Participating" : "Participate"}
              </Button>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}

