import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { ArrowUpRight, ArrowDownRight, Wallet, RefreshCw } from "lucide-react"

export function AccountSummary() {
  return (
    <>
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">Total Balance</CardTitle>
          <Wallet className="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">5,231.89 XRP</div>
          <p className="text-xs text-muted-foreground">≈ $2,615.95 USD</p>
        </CardContent>
      </Card>
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">Sent (30d)</CardTitle>
          <ArrowUpRight className="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">1,245.00 XRP</div>
          <p className="text-xs text-muted-foreground">+12.5% from last month</p>
        </CardContent>
      </Card>
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">Received (30d)</CardTitle>
          <ArrowDownRight className="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">1,732.50 XRP</div>
          <p className="text-xs text-muted-foreground">+18.2% from last month</p>
        </CardContent>
      </Card>
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">Transaction Count</CardTitle>
          <RefreshCw className="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">42</div>
          <p className="text-xs text-muted-foreground">+8 from last month</p>
        </CardContent>
      </Card>
    </>
  )
}

