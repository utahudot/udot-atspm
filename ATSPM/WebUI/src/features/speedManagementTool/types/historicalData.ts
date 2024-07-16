interface MonthlyAverage {
  month: number
  year: number
  averageSpeed: number
}

interface DailyAverage {
  date: string
  averageSpeed: number
}

interface MonthlyHistoricalData {
  sourceId: number
  monthlyAverages: MonthlyAverage[]
  dailyAverages: null
}

interface DailyHistoricalData {
  sourceId: number
  dailyAverages: DailyAverage[]
}

export interface HistoricalDataResponse {
  routeId: number
  monthlyHistoricalRouteData: MonthlyHistoricalData[]
  dailyHistoricalRouteData: DailyHistoricalData[]
}
