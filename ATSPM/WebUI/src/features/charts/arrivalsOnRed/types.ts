import {
  BaseChartData,
  BaseChartOptions,
  BasePlan,
  ChartType,
  DataPoint,
} from '@/features/charts/common/types'

export interface ArrivalsOnRedChartOptions extends BaseChartOptions {
  binSize: number
  getPermissivePhase: boolean
}

export interface ArrivalsOnRedChartOptionsDefaults {
  binSize: { id: number; value: string; option: string }
  getPermissivePhase: { id: number; value: string; option: string }
}

export interface ArrivalsOnRedPlan extends BasePlan {
  percentArrivalOnRed: number
  percentRedTime: number
}

export interface RawArrivalsOnRedData extends BaseChartData {
  approachId: number
  phaseNumber: number
  phaseDescription: string
  totalDetectorHits: number
  totalArrivalOnRed: number
  percentArrivalOnRed: number
  plans: ArrivalsOnRedPlan[]
  percentArrivalsOnRed: DataPoint[]
  totalVehicles: DataPoint[]
  arrivalsOnRed: DataPoint[]
}

export interface RawArrivalsOnRedResponse {
  type: ChartType.ArrivalsOnRed
  data: RawArrivalsOnRedData[]
}
