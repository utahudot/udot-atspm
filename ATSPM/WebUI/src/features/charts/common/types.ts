import {
  ApproachDelayChartOptions,
  RawApproachDelayReponse,
} from '@/features/charts/approachDelay/types'
import {
  ApproachSpeedChartOptions,
  RawApproachSpeedReponse,
} from '@/features/charts/approachSpeed/types'
import {
  ApproachVolumeChartOptions,
  RawApproachVolumeResponse,
} from '@/features/charts/approachVolume/types'
import {
  ArrivalsOnRedChartOptions,
  RawArrivalsOnRedResponse,
} from '@/features/charts/arrivalsOnRed/types'
import {
  GreenTimeUtilizationChartOptions,
  RawGreenTimeUtilizationResponse,
} from '@/features/charts/greenTimeUtilization/types'
import {
  LeftTurnGapAnalysisChartOptions,
  RawLeftTurnGapAnalysisResponse,
} from '@/features/charts/leftTurnGapAnalysis/types'
import {
  PedestrianDelayChartOptions,
  RawPedestrianDelayResponse,
} from '@/features/charts/pedestrianDelay/types'
import {
  PreemptionDetailsChartOptions,
  RawPreemptionDetailsResponse,
} from '@/features/charts/preemptionDetails/types'
import {
  PurdueCoordinationDiagramChartOptions,
  RawPurdueCoordinationDiagramResponse,
} from '@/features/charts/purdueCoordinationDiagram/types'
import {
  PurduePhaseTerminationChartOptions,
  RawPurduePhaseTerminationResponse,
} from '@/features/charts/purduePhaseTermination/types'
import {
  PurdueSplitFailureChartOptions,
  RawPurdueSplitFailureResponse,
} from '@/features/charts/purdueSplitFailure/types'
import {
  RawSplitMonitorResponse,
  SplitMonitorChartOptions,
} from '@/features/charts/splitMonitor/types'
import {
  RawTimingAndActuationResponse,
  TimingAndActuationChartOptions,
} from '@/features/charts/timingAndActuation/types'
import {
  RawTurningMovementCountsResponse,
  TurningMovementCountsChartOptions,
} from '@/features/charts/turningMovementCounts/types'
import { Color } from '@/features/charts/utils'
import {
  RawWaitTimeResponse,
  WaitTimeChartOptions,
} from '@/features/charts/waitTime/types'
import {
  RawYellowAndRedActuationsResponse,
  YellowAndRedActuationsChartOptions,
} from '@/features/charts/yellowAndRedActuations/types'
import {
  LinkPivotOptions,
  RawLinkPivotPcdResponse,
} from '@/features/tools/link-pivot/types'
import { RawRampMeteringResponse } from '../rampMetering/types'
import {
  RawTimeSpaceDiagramResponse,
  TimeSpaceOptions,
} from '../timeSpaceDiagram/types'

export interface BaseChartOptions {
  locationIdentifier: string
  start: Date
  end: Date
}

export interface BasePlan {
  planNumber: string
  planDescription: string
  start: string
  end: string
}

export type PlanData = [string, 1, string]

export type MarkAreaData = [
  {
    xAxis: string
    itemStyle: {
      color: Color
    }
  },
  {
    xAxis: string
  },
]

export type PlanOptions<T> = {
  [K in keyof Omit<T, keyof BasePlan>]: (value: T[K]) => string | number
}

export interface BaseChartData {
  locationIdentifier: string
  locationDescription: string
  start: string
  end: string
}

export interface DataPoint {
  timestamp: string
  value: number
}

export type RawChartResponse =
  | RawApproachDelayReponse
  | RawApproachSpeedReponse
  | RawApproachVolumeResponse
  | RawArrivalsOnRedResponse
  | RawGreenTimeUtilizationResponse
  | RawLeftTurnGapAnalysisResponse
  | RawPedestrianDelayResponse
  | RawPreemptionDetailsResponse
  | RawPurdueCoordinationDiagramResponse
  | RawPurduePhaseTerminationResponse
  | RawPurdueSplitFailureResponse
  | RawSplitMonitorResponse
  | RawTimingAndActuationResponse
  | RawTurningMovementCountsResponse
  | RawWaitTimeResponse
  | RawYellowAndRedActuationsResponse
  | RawRampMeteringResponse

export type RawToolResponse =
  | RawTimeSpaceDiagramResponse
  | RawLinkPivotPcdResponse

export type ChartOptions =
  | BaseChartOptions
  | ApproachDelayChartOptions
  | ApproachSpeedChartOptions
  | ApproachVolumeChartOptions
  | ArrivalsOnRedChartOptions
  | GreenTimeUtilizationChartOptions
  | LeftTurnGapAnalysisChartOptions
  | PedestrianDelayChartOptions
  | PreemptionDetailsChartOptions
  | PurdueCoordinationDiagramChartOptions
  | PurduePhaseTerminationChartOptions
  | PurdueSplitFailureChartOptions
  | SplitMonitorChartOptions
  | TimingAndActuationChartOptions
  | TurningMovementCountsChartOptions
  | WaitTimeChartOptions
  | YellowAndRedActuationsChartOptions

export type ToolOptions = TimeSpaceOptions | LinkPivotOptions

export type ChartOptionType =
  | 'ApproachDelay'
  | 'ApproachSpeed'
  | 'ApproachVolume'
  | 'ArrivalsOnRed'
  | 'GreenTimeUtilization'
  | 'LeftTurnGapAnalysis'
  | 'PedestrianDelay'
  | 'PreemptionDetails'
  | 'PurdueCoordinationDiagram'
  | 'PurduePhaseTermination'
  | 'PurdueSplitFailure'
  | 'SplitMonitor'
  | 'TimingAndActuation'
  | 'TurningMovementCounts'
  | 'WaitTime'
  | 'YellowAndRedActuations'
  | 'RampMetering'

export enum ChartType {
  ApproachDelay = 'ApproachDelay',
  ApproachSpeed = 'ApproachSpeed',
  ApproachVolume = 'ApproachVolume',
  ArrivalsOnRed = 'ArrivalsOnRed',
  PurdueCoordinationDiagram = 'PurdueCoordinationDiagram',
  GreenTimeUtilization = 'GreenTimeUtilization',
  LeftTurnGapAnalysis = 'LeftTurnGapAnalysis',
  PedestrianDelay = 'PedestrianDelay',
  PurduePhaseTermination = 'PurduePhaseTermination',
  PreemptionDetails = 'PreemptionDetails',
  PurdueSplitFailure = 'PurdueSplitFailure',
  SplitMonitor = 'SplitMonitor',
  TimingAndActuation = 'TimingAndActuation',
  TurningMovementCounts = 'TurningMovementCounts',
  WaitTime = 'WaitTime',
  YellowAndRedActuations = 'YellowAndRedActuations',
  RampMetering = 'RampMetering',
}

export enum ToolType {
  TimeSpaceHistoric = 'TimeSpaceHistoric',
  TimeSpaceAverage = 'TimeSpaceAverage',
  LinkPivot = 'LinkPivot',
  LpPcd = 'LpPcd',
}

export const chartTypeToString = (chartType: ChartType) => {
  switch (chartType) {
    case ChartType.ApproachDelay:
      return 'Approach Delay'
    case ChartType.ApproachSpeed:
      return 'Approach Speed'
    case ChartType.ApproachVolume:
      return 'Approach Volume'
    case ChartType.ArrivalsOnRed:
      return 'Arrivals on Red'
    case ChartType.PurdueCoordinationDiagram:
      return 'Purdue Coordination Diagram'
    case ChartType.GreenTimeUtilization:
      return 'Green Time Utilization'
    case ChartType.LeftTurnGapAnalysis:
      return 'Left Turn Gap Analysis'
    case ChartType.PedestrianDelay:
      return 'Pedestrian Delay'
    case ChartType.PurduePhaseTermination:
      return 'Purdue Phase Termination'
    case ChartType.PreemptionDetails:
      return 'Preemption Details'
    case ChartType.PurdueSplitFailure:
      return 'Purdue Split Failure'
    case ChartType.SplitMonitor:
      return 'Split Monitor'
    case ChartType.TimingAndActuation:
      return 'Timing and Actuation'
    case ChartType.TurningMovementCounts:
      return 'Turning Movement Counts'
    case ChartType.WaitTime:
      return 'Wait Time'
    case ChartType.YellowAndRedActuations:
      return 'Yellow and Red Actuations'
    case ChartType.RampMetering:
      return 'Ramp Metering'
  }
}
