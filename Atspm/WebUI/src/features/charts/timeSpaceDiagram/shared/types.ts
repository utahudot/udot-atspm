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
import { DetectorEventDto, IndianaEvent } from '@/api/reports'
import { BaseChartData, ToolType } from '@/features/charts/common/types'
import {
  Cycle,
  PedestrianInterval,
} from '@/features/charts/timingAndActuation/types'
import { GpxPoint } from './gpxFileParser'
import type { SrmEntityTrack } from './srmFileParser'

// export interface TimeSpaceDetectorEvent {
//   initialX: string
//   finalX: string
//   isDetectorOn?: boolean | null
// }

export interface TimeSpaceEvent {
  initialX: string
  finalX: string
  isDetectorOn?: boolean | null
}

export interface TimeSpaceDetectorEvent {
  initialX: string
  isDetectorOn?: boolean | null
}

export interface TimeSpaceDetectorEventWithDistanceDTO {
  distanceToStopBar: number
  detectorOn: Date
  detectorOff: Date
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

export interface TimeSpaceSrmOptions {
  routeId: string
  start: Date
  end: Date
  srmCsvContentBase64: string
}

export interface RawTimeSpaceBaseData extends BaseChartData {
  phaseNumber: number
  phaseNumberSort: string
  distanceToNextLocation: number
  distanceToPreviousLocation: number
  speed: number
  approachId: number
  approachDescription: string
  phaseType: 'Primary' | 'Opposing'
}

export interface TimeSpaceBaseData extends RawTimeSpaceBaseData {
  calculatedDistanceToNext: number
  calculatedDistanceToPrevious: number
  isIgnoredLocation: boolean
}

export interface RawTimeSpaceAverageData extends TimeSpaceBaseData {
  offset: number
  cycleLength: number
  programmedSplit: number
  coordinatedPhases: boolean
  greenTimeEvents: TimeSpaceDetectorEvent[] | []
  cycleAllEvents: Cycle[] | null
}

export interface RawTimeSpaceHistoricData extends TimeSpaceBaseData {
  greenTimeEvents: TimeSpaceDetectorEvent[] | []
  laneByLaneCountDetectors: TimeSpaceDetectorEventWithDistanceDTO[] | []
  advanceCountDetectors: TimeSpaceDetectorEventWithDistanceDTO[] | []
  stopBarPresenceDetectors: TimeSpaceDetectorEventWithDistanceDTO[] | []
  cycleAllEvents: Cycle[] | null
  pedestrianIntervals: PedestrianInterval[] | []
  percentArrivalOnGreen: number
  tmcForPhase: TmcForPhaseDto

  order: number
  cycleLength: number
  isPhaseOverLap: boolean

  tspNumberCheckins: number
  tspNumberCheckouts: number
  tspNumberEarlyGreens: number
  tspNumberExtendedGreens: number

  tspEvents?: IndianaEvent[] | null
  priorityAndPreemptionEvents?: DetectorEventDto[] | null
  srmEntityTracks?: SrmHistoricEntityTrack[] | null
}

export interface SrmHistoricPoint {
  time: string
  distance: number
  timestampMs: number
}

export interface SrmHistoricEntityTrack {
  entityId: string
  points: SrmHistoricPoint[]
  startingIntersection?: string
  headingDirection?: number | string
}

export interface TimeSpaceSrmPhaseOverlay {
  locationIdentifier: string
  phaseType: 'Primary' | 'Opposing'
  order: number
  srmEntityTracks: SrmHistoricEntityTrack[]
}

export interface TmcForPhaseDto {
  leftTurnEvents: TmcEventDto[]
  rightTurnEvents: TmcEventDto[]
}

export interface TmcEventDto {
  start: string
  value: number
  isRightTurnEvent: boolean
  isLeftTurnEvent: boolean
  laneType: string
  directionTypes: string
}

// Wrapper type that matches C# TimeSpaceDiagramPhaseResult
export interface TimeSpaceDiagramPhaseResult<T extends TimeSpaceBaseData> {
  error: string | null
  result: T | null
  isSuccess: boolean
}

// API response contains wrapped results
export type TimeSpaceResponseData =
  | TimeSpaceDiagramPhaseResult<RawTimeSpaceHistoricData>[]
  | TimeSpaceDiagramPhaseResult<RawTimeSpaceAverageData>[]

// Unwrapped data for processing
export type TimeSpaceUnwrappedData =
  | RawTimeSpaceHistoricData[]
  | RawTimeSpaceAverageData[]

export type TimeSpaceOptions =
  | TimeSpaceHistoricOptions
  | TimeSpaceAverageOptions

export interface RawTimeSpaceDiagramResponse {
  type: ToolType.TimeSpaceHistoric | ToolType.TimeSpaceAverage
  data: TimeSpaceResponseData
}

export interface GpxUploadOptions {
  id: string
  file?: File
  parsedData?: GpxPoint[]
  parsedEntityData?: SrmEntityTrack[]
  startLocation: string
  endLocation: string
  primary?: boolean
  error?: string | null
}
