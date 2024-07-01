import {
  BaseChartData,
  BaseChartOptions,
  BasePlan,
  ChartType,
  DataPoint,
} from '@/features/charts/common/types'

export interface PurdueSplitFailureChartOptions extends BaseChartOptions {
  firstSecondsOfRed: number
  metricTypeId: number
  getPermissivePhase: boolean
}

export interface PurdueSplitFailureChartOptionsDefaults {
  firstSecondsOfRed: { id: number; value: string; option: string }
  metricTypeId: { id: number; value: string; option: string }
  getPermissivePhase: { id: number; value: boolean; option: string }
}

export interface PurdueSplitFailurePlan extends BasePlan {
  totalCycles: number
  failsInPlan: number
  percentFails: number
}

interface FailLine {
  timestamp: string
}

export interface RawPurdueSplitFailureData extends BaseChartData {
  approachId: number
  approachDescription: string
  phaseNumber: number
  totalSplitFails: number
  plans: PurdueSplitFailurePlan[]
  failLines: FailLine[]
  gapOutGreenOccupancies: DataPoint[]
  gapOutRedOccupancies: DataPoint[]
  forceOffGreenOccupancies: DataPoint[]
  forceOffRedOccupancies: DataPoint[]
  averageGor: DataPoint[]
  averageRor: DataPoint[]
  percentFails: DataPoint[]
}

export interface RawPurdueSplitFailureResponse {
  type: ChartType.PurdueSplitFailure
  data: RawPurdueSplitFailureData[]
}
