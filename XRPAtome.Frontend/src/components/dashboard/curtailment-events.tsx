import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"

const events = [
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
  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Date</TableHead>
          <TableHead>Time</TableHead>
          <TableHead className="hidden md:table-cell">Energy Saved</TableHead>
          <TableHead>Reward</TableHead>
          <TableHead>Status</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {events.map((event) => (
          <TableRow key={event.id}>
            <TableCell>{event.date}</TableCell>
            <TableCell>{event.time}</TableCell>
            <TableCell className="hidden md:table-cell">{event.energySaved}</TableCell>
            <TableCell>{event.reward}</TableCell>
            <TableCell>
              <Badge
                variant={
                  event.status === "completed" ? "default" : event.status === "upcoming" ? "outline" : "destructive"
                }
              >
                {event.status}
              </Badge>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}

