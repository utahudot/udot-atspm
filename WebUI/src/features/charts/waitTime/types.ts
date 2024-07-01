import {
  BaseChartData,
  BaseChartOptions,
  BasePlan,
  ChartType,
  DataPoint,
} from '@/features/charts/common/types'

export interface WaitTimeChartOptions extends BaseChartOptions {
  binSize: number
}

export interface WaitTimeChartOptionsDefaults {
  binSize: { id: number; value: string; option: string }
}

export interface WaitTimePlan extends BasePlan {
  averageWaitTime: number
  maxWaitTime: number
}

export interface RawWaitTimeData extends BaseChartData {
  approachId: number
  approachDescription: string
  phaseNumber: number
  detectionTypes: string
  plans: WaitTimePlan[]
  gapOuts: DataPoint[]
  maxOuts: DataPoint[]
  forceOffs: DataPoint[]
  unknowns: DataPoint[]
  average: DataPoint[]
  volumes: DataPoint[]
  planSplits: DataPoint[]
}

export interface RawWaitTimeResponse {
  type: ChartType.WaitTime
  data: RawWaitTimeData[]
}
