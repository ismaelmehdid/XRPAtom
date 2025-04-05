import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"

const transactions = [
  {
    id: "TR-1234",
    amount: "250.00",
    status: "completed",
    date: "2023-04-01",
    destination: "rHb9CJAWyB4rj91VRWn96DkukG4bwdtyTh",
  },
  {
    id: "TR-1235",
    amount: "1000.00",
    status: "pending",
    date: "2023-04-02",
    destination: "rPT1Sjq2YGrBMTttX4GZHjKu9dyfzbpAYe",
  },
  {
    id: "TR-1236",
    amount: "150.00",
    status: "completed",
    date: "2023-04-03",
    destination: "rUCzEr6jrEyMpjhs4wSdQdz4g8Y382NxfM",
  },
  {
    id: "TR-1237",
    amount: "500.00",
    status: "failed",
    date: "2023-04-04",
    destination: "rJb5KsHsDHF1YS5B5DU6QCkH5NsPaKQTcy",
  },
  {
    id: "TR-1238",
    amount: "750.00",
    status: "completed",
    date: "2023-04-05",
    destination: "rLHzPsX6oXkzU2qL12kHCH8G8cnZv1rBJh",
  },
]

export function RecentTransactions() {
  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Transaction ID</TableHead>
          <TableHead>Amount (XRP)</TableHead>
          <TableHead>Status</TableHead>
          <TableHead className="hidden md:table-cell">Date</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {transactions.map((transaction) => (
          <TableRow key={transaction.id}>
            <TableCell className="font-medium">{transaction.id}</TableCell>
            <TableCell>{transaction.amount}</TableCell>
            <TableCell>
              <Badge
                variant={
                  transaction.status === "completed"
                    ? "success"
                    : transaction.status === "pending"
                      ? "outline"
                      : "destructive"
                }
              >
                {transaction.status}
              </Badge>
            </TableCell>
            <TableCell className="hidden md:table-cell">{transaction.date}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}

