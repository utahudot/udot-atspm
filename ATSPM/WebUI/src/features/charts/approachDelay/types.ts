import {
  BaseChartData,
  BaseChartOptions,
  BasePlan,
  ChartType,
  DataPoint,
} from '@/features/charts/common/types'

export interface ApproachDelayChartOptions extends BaseChartOptions {
  binSize: number
  getPermissivePhase: boolean
  getVolume: boolean
}

export interface ApproachDelayChartOptionsDefaults {
  binSize: { id: number; value: string; option: string }
  getPermissivePhase: { id: number; value: string; option: string }
  getVolume: { id: number; value: string; option: string }
}

export interface ApproachDelayPlan extends BasePlan {
  averageDelay: number
  totalDelay: number
  planDescription: string
}

export interface RawApproachDelayData extends BaseChartData {
  phaseNumber: number
  phaseDescription: string
  averageDelayPerVehicle: number
  totalDelay: number
  plans: ApproachDelayPlan[]
  approachDelayDataPoints: DataPoint[]
  approachDelayPerVehicleDataPoints: DataPoint[]
  approachId: number
}

export interface RawApproachDelayReponse {
  type: ChartType.ApproachDelay
  data: RawApproachDelayData[]
}
