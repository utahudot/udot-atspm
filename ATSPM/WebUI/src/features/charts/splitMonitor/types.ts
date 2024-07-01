import {
  BaseChartData,
  BaseChartOptions,
  BasePlan,
  ChartType,
  DataPoint,
} from '@/features/charts/common/types'

export interface SplitMonitorChartOptions extends BaseChartOptions {
  percentileSplit: number
}

export interface SplitMonitorChartOptionsDefaults {
  percentileSplit: { id: number; value: string; option: string }
}
export interface SplitMonitorPlan extends BasePlan {
  percentSkips: number
  percentGapOuts: number
  percentMaxOuts: number
  percentForceOffs: number
  averageSplit: number
  percentileSplit: number
  minTime: number
  programmedSplit: number
  percentileSplit85th: number
  percentileSplit50th: number
}

export interface RawSplitMonitorData extends BaseChartData {
  phaseNumber: number
  percentileSplit: number
  phaseDescription:string
  plans: SplitMonitorPlan[]
  programmedSplits: DataPoint[]
  gapOuts: DataPoint[]
  maxOuts: DataPoint[]
  forceOffs: DataPoint[]
  unknowns: DataPoint[]
  peds: DataPoint[]
}

export interface RawSplitMonitorResponse {
  type: ChartType.SplitMonitor
  data: RawSplitMonitorData[]
}
