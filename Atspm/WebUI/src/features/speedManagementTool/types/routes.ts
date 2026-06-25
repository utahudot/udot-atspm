// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - routes.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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
  createdDate: string
  binStartTime: string
  route_id: string
  sourceId: number
  name: string
  speedLimit: number
  region: string
  city: string
  county: string
  averageSpeed: number
  averageEightyFifthSpeed: number
  violations: number
  extremeViolations: number
  flow: number
  minSpeed: number
  maxSpeed: number
  variability: number
  percentViolations: number
  percentExtremeViolations: number
  avgSpeedVsSpeedLimit: number
  eightyFifthSpeedVsSpeedLimit: number
  percentObserved: number
}

// interface Properties {
//   name: string
//   route_id: number
//   startdate: string | null
//   enddate: string | null
//   avg: number
//   percentilespd_85: number
//   percentilespd_95: number
//   // percentilespd_99: number
//   averageSpeedAboveSpeedLimit: number
//   estimatedViolations: number
//   flow: number
//   speedLimit: number
//   // dataSource: string
// }

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
