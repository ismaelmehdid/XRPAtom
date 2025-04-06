// Base data for a single day with hourly consumption
const generateDailyData = (date: Date) => {
  const data = []
  for (let hour = 0; hour < 24; hour++) {
    const isPeakHour = hour >= 8 && hour <= 20
    const isWeekday = date.getDay() >= 1 && date.getDay() <= 5
    const isSummer = date.getMonth() >= 5 && date.getMonth() <= 8

    // Base consumption values (in kWh)
    let baseline = 2.5 // Base hourly consumption for a typical household
    if (isPeakHour) baseline *= 1.4 // 40% higher during peak hours
    if (isWeekday) baseline *= 1.2 // 20% higher on weekdays
    if (isSummer) baseline *= 1.3 // 30% higher in summer

    // Add some randomness (±10%)
    baseline += (Math.random() - 0.5) * baseline * 0.2

    // Curtailed consumption is typically 15-25% lower than baseline
    const savingsPercentage = 0.2 + (Math.random() * 0.1) // 20-30% savings
    const curtailed = baseline * (1 - savingsPercentage)

    data.push({
      timestamp: new Date(date.getFullYear(), date.getMonth(), date.getDate(), hour).getTime(),
      hour: hour,
      baseline,
      curtailed,
      co2Saved: Math.floor((baseline - curtailed) * 0.4), // 0.4 kg CO2 per kWh saved (typical grid emission factor)
      priceSaved: ((baseline - curtailed) * 0.12).toFixed(2) // $0.12 per kWh (typical residential rate)
    })
  }
  return data
}

// Generate data for the last 7 days
const generate7DaysData = () => {
  const data = []
  const today = new Date()
  
  for (let i = 6; i >= 0; i--) {
    const date = new Date(today)
    date.setDate(date.getDate() - i)
    const dailyData = generateDailyData(date)
    
    // Aggregate daily data into a single point
    const aggregated = {
      date: date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
      baseline: dailyData.reduce((sum, hour) => sum + hour.baseline, 0),
      curtailed: dailyData.reduce((sum, hour) => sum + hour.curtailed, 0),
      co2Saved: dailyData.reduce((sum, hour) => sum + hour.co2Saved, 0),
      priceSaved: dailyData.reduce((sum, hour) => sum + parseFloat(hour.priceSaved), 0).toFixed(2)
    }
    
    data.push(aggregated)
  }
  
  return data
}

// Generate data for the last 30 days
const generate30DaysData = () => {
  const data = []
  const today = new Date()
  
  for (let i = 29; i >= 0; i--) {
    const date = new Date(today)
    date.setDate(date.getDate() - i)
    const dailyData = generateDailyData(date)
    
    // Aggregate daily data into a single point
    const aggregated = {
      date: date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
      baseline: dailyData.reduce((sum, hour) => sum + hour.baseline, 0),
      curtailed: dailyData.reduce((sum, hour) => sum + hour.curtailed, 0),
      co2Saved: dailyData.reduce((sum, hour) => sum + hour.co2Saved, 0),
      priceSaved: dailyData.reduce((sum, hour) => sum + parseFloat(hour.priceSaved), 0).toFixed(2)
    }
    
    data.push(aggregated)
  }
  
  return data
}

// Generate data for the last year
const generateYearlyData = () => {
  const data = []
  const today = new Date()
  
  for (let i = 11; i >= 0; i--) {
    const date = new Date(today.getFullYear(), today.getMonth() - i, 1)
    const monthData = []
    
    // Generate data for each day in the month
    const daysInMonth = new Date(date.getFullYear(), date.getMonth() + 1, 0).getDate()
    for (let day = 1; day <= daysInMonth; day++) {
      const dayDate = new Date(date.getFullYear(), date.getMonth(), day)
      monthData.push(...generateDailyData(dayDate))
    }
    
    // Aggregate monthly data
    const aggregated = {
      name: date.toLocaleDateString('en-US', { month: 'short' }),
      baseline: monthData.reduce((sum, hour) => sum + hour.baseline, 0),
      curtailed: monthData.reduce((sum, hour) => sum + hour.curtailed, 0),
      co2Saved: monthData.reduce((sum, hour) => sum + hour.co2Saved, 0),
      priceSaved: monthData.reduce((sum, hour) => sum + parseFloat(hour.priceSaved), 0).toFixed(2)
    }
    
    data.push(aggregated)
  }
  
  return data
}

// Generate live data (last 60 minutes)
const generateLiveData = () => {
  const data = []
  const now = new Date()
  
  for (let i = 59; i >= 0; i--) {
    const date = new Date(now.getTime() - i * 60000) // Subtract i minutes
    const hourlyData = generateDailyData(date)[date.getHours()]
    
    data.push({
      timestamp: date.getTime(),
      consumption: hourlyData.curtailed,
      baseline: hourlyData.baseline
    })
  }
  
  return data
}

export const yearlyData = generateYearlyData()
export const last30DaysData = generate30DaysData()
export const last7DaysData = generate7DaysData()
export const liveData = generateLiveData() 