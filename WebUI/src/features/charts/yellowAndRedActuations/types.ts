import {
  BaseChartData,
  BaseChartOptions,
  BasePlan,
  ChartType,
  DataPoint,
} from '@/features/charts/common/types'

export interface YellowAndRedActuationsChartOptions extends BaseChartOptions {
  severeLevelSeconds: number
}

export interface YellowAndRedActuationsChartOptionsDefaults {
  severeLevelSeconds: { id: number; value: string; option: string }
}

export interface YellowAndRedActuationsPlan extends BasePlan {
  totalViolations: number
  severeViolations: number
  percentViolations: number
  percentSevereViolations: number
  averageTimeViolations: number
}

export interface RawYellowAndRedActuationsData extends BaseChartData {
  approachId: number
  approachDescription: string
  phaseNumber: number
  totalViolations: number
  severeViolations: number
  isPermissivePhase: boolean
  yellowLightOccurences: number
  plans: YellowAndRedActuationsPlan[]
  redEvents: DataPoint[]
  yellowEvents: DataPoint[]
  redClearanceEvents: DataPoint[]
  detectorEvents: DataPoint[]
}

export interface RawYellowAndRedActuationsResponse {
  type: ChartType.YellowAndRedActuations
  data: RawYellowAndRedActuationsData[]
}