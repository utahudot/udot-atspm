import {
  BaseChartData,
  BaseChartOptions,
  BasePlan,
  ChartType,
  DataPoint,
} from '@/features/charts/common/types'

export interface TurningMovementCountsChartOptions extends BaseChartOptions {
  binSize: number
}

export interface TurningMovementCountsChartOptionsDefaults {
  binSize: { id: number; value: string; option: string }
}

export type Plan = BasePlan

interface Lane {
  laneNumber: number
  movementType: string
  volume: DataPoint[]
  laneType: number
}

export interface RawTurningMovementCountsData extends BaseChartData {
  direction: string
  laneType: string
  movementType: string
  plans: Plan[]
  lanes: Lane[]
  TotalHourlyVolumes: DataPoint[]
  totalVolume: number
  peakHour: string
  peakHourVolume: number
  peakHourFactor: number
  laneUtilizationFactor: number
}

export interface RawTurningMovementCountsResponse {
  type: ChartType.TurningMovementCounts
  data: {
    charts: RawTurningMovementCountsData[]
    table: any
    peakHourFactor: number
    peakHour: { key: string; value: string }
  }
}
