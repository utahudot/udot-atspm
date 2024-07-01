import {
  BaseChartData,
  BaseChartOptions,
  ChartType,
  DataPoint,
} from '@/features/charts/common/types'
import { Plan } from '../turningMovementCounts/types'

export interface GreenTimeUtilizationChartOptions extends BaseChartOptions {
  xAxisBinSize: number
  yAxisBinSize: number
}

export interface GreenTimeUtilizationChartOptionsDefaults {
  xAxisBinSize: { id: number; value: string; option: string }
  yAxisBinSize: { id: number; value: string; option: string }
}

interface Layer {
  dataValue: number
  lowerEnd: number
}

export interface Stack {
  layers: Layer[]
  timestamp: string
}

export interface Bin {
  x: number
  y: number
  value: number
}

export interface RawGreenTimeUtilizationData extends BaseChartData {
  approachId: number
  approachDescription: string
  bins: Bin[]
  averageSplits: DataPoint[]
  programmedSplits: DataPoint[]
  phaseNumber: number
  yAxisBinSize: number
  xAxisBinSize: number
  plans: Plan[]
}

export interface RawGreenTimeUtilizationResponse {
  type: ChartType.GreenTimeUtilization
  data: RawGreenTimeUtilizationData[]
}
