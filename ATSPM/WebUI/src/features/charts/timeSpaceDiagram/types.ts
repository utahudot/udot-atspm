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
