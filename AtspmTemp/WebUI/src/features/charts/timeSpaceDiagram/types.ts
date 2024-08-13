// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - types.ts
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
import { BaseChartData, ToolType } from '@/features/charts/common/types'
import { Cycle } from '../timingAndActuation/types'

export interface TimeSpaceDetectorEvent {
  initialX: string
  finalX: string
  isDetectorOn?: boolean | null
}

export interface LocationWithSequence {
  locationIdentifier: string
  sequence: number[][]
}

export interface LocationWithCoordPhases {
  locationIdentifier: string
  coordinatedPhases: number[]
}

export interface TimeSpaceAverageOptions {
  startDate: string
  endDate: string
  startTime: string
  endTime: string
  routeId: string
  speedLimit: number | null
  daysOfWeek: number[]
  sequence: LocationWithSequence[]
  coordinatedPhases: LocationWithCoordPhases[]
}

export interface TimeSpaceHistoricOptions {
  locationIdentifier: string
  extendStartStopSearch: number
  showAllLanesInfo: boolean
  routeId: string
  chartType: string
  speedLimit: number | null
  start: Date
  end: Date
}

export interface RawTimeSpaceBaseData extends BaseChartData {
  phaseNumber: number
  phaseNumberSort: string
  distanceToNextLocation: number
  speed: number
  approachId: number
  approachDescription: string
  phaseType: 'Primary' | 'Opposing'
}

export interface RawTimeSpaceAverageData extends RawTimeSpaceBaseData {
  offset: number
  cycleLength: number
  programmedSplit: number
  coordinatedPhases: boolean
  greenTimeEvents: TimeSpaceDetectorEvent[] | []
  cycleAllEvents: Cycle[] | null
}

export interface RawTimeSpaceHistoricData extends RawTimeSpaceBaseData {
  greenTimeEvents: TimeSpaceDetectorEvent[] | []
  laneByLaneCountDetectors: TimeSpaceDetectorEvent[] | []
  advanceCountDetectors: TimeSpaceDetectorEvent[] | []
  stopBarPresenceDetectors: TimeSpaceDetectorEvent[] | []
  cycleAllEvents: Cycle[] | null
}

export type TimeSpaceResponseData =
  | RawTimeSpaceHistoricData[]
  | RawTimeSpaceAverageData[]

export type TimeSpaceOptions =
  | TimeSpaceHistoricOptions
  | TimeSpaceAverageOptions

export interface RawTimeSpaceDiagramResponse {
  type: ToolType.TimeSpaceHistoric | ToolType.TimeSpaceAverage
  data: TimeSpaceResponseData
}
