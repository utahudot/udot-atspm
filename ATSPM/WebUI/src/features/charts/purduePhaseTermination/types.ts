import {
  BaseChartData,
  BaseChartOptions,
  BasePlan,
  ChartType,
} from '@/features/charts/common/types'

export interface PurduePhaseTerminationChartOptions extends BaseChartOptions {
  selectedConsecutiveCount: number
}

export interface PurduePhaseTerminationChartOptionsDefaults {
  selectedConsecutiveCount: { id: number; value: string; option: string }
}

export interface Phase {
  phaseNumber: number
  gapOuts: string[]
  maxOuts: string[]
  forceOffs: string[]
  pedWalkBegins: string[]
  unknownTerminations: string[]
}

export type PurduePhaseTerminationPlan = BasePlan

export interface RawPurduePhaseTerminationData extends BaseChartData {
  consecutiveCount: number
  plans: PurduePhaseTerminationPlan[]
  phases: Phase[]
}

export interface RawPurduePhaseTerminationResponse {
  type: ChartType.PurduePhaseTermination
  data: RawPurduePhaseTerminationData
}
