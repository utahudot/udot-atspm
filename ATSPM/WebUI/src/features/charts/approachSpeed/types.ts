import {
  BaseChartData,
  BaseChartOptions,
  BasePlan,
  ChartType,
  DataPoint,
} from '@/features/charts/common/types'

export interface ApproachSpeedChartOptions extends BaseChartOptions {
  binSize: number
}

export interface ApproachSpeedChartOptionsDefaults {
  binSize: { id: number; value: string; option: string }
}

export interface ApproachSpeedPlan extends BasePlan {
  averageSpeed: number
  standardDeviation: number
  eightyFifthPercentile: number
  fifteenthPercentile: number
}

export interface RawApproachSpeedData extends BaseChartData {
  phaseNumber: number
  phaseDescription: string
  detectionType: string
  distanceFromStopBar: number
  postedSpeed: number
  plans: ApproachSpeedPlan[]
  averageSpeeds: DataPoint[]
  eightyFifthSpeeds: DataPoint[]
  fifteenthSpeeds: DataPoint[]
  approachId: number
  approachDescription: string
}

export interface RawApproachSpeedReponse {
  type: ChartType.ApproachSpeed
  data: RawApproachSpeedData[]
}
