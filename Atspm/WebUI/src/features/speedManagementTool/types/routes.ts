interface Geometry {
  type: 'LineString'
  coordinates: [number, number][]
}

export interface HistoricalDataOptions {
  routeId: number
  startDate: string
  endDate: string
  daysOfWeek: string
  startTime: string
  endTime: string
}

export interface HistoricalDataResponse {
  routeId: number
  historicalRouteData: SourceData[]
}

interface SourceData {
  sourceId: number
  monthlyAverages: MonthlyAverage[]
}

interface MonthlyAverage {
  month: number
  year: number
  averageSpeed: number
}

interface Properties {
  // route_name: string
  route_id: number
  startdate: string | null
  enddate: string | null
  avg: number
  percentilespd_85: number
  percentilespd_95: number
  // percentilespd_99: number
  averageSpeedAboveSpeedLimit: number
  estimatedViolations: number
  flow: number
  speedLimit: number
  // dataSource: string
}

export interface SpeedManagementRoute {
  type: 'Feature'
  geometry: Geometry
  properties: Properties
  coordinates: [number, number][]
  color?: string
}

export type RoutesResponse = {
  type: 'FeatureCollection'
  features: SpeedManagementRoute[]
}
