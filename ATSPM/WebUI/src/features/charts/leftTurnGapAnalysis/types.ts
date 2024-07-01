import {
  BaseChartData,
  BaseChartOptions,
  ChartType,
  DataPoint,
} from '@/features/charts/common/types'

export interface LeftTurnGapAnalysisChartOptions extends BaseChartOptions {
  binSize: number
  gap1Min: number
  gap1Max: number
  gap2Min: number
  gap2Max: number
  gap3Min: number
  gap3Max: number
  gap4Min: number
  trendLineGapThreshold: number
}

export interface LeftTurnGapAnalysisChartOptionsDefaults {
  binSize: { id: number; value: string; option: string }
  gap1Min: { id: number; value: string; option: string }
  gap1Max: { id: number; value: string; option: string }
  gap2Min: { id: number; value: string; option: string }
  gap2Max: { id: number; value: string; option: string }
  gap3Min: { id: number; value: string; option: string }
  gap3Max: { id: number; value: string; option: string }
  gap4Min: { id: number; value: string; option: string }
  trendLineGapThreshold: { id: number; value: string; option: string }
}

export interface RawLeftTurnGapData extends BaseChartData {
  approachId: number
  approachDescription: string
  phaseNumber: number
  phaseDescription: string
  detectionTypeDescription: string
  gap1Min: number
  gap1Max: number
  gap1Count: DataPoint[]
  gap2Min: number
  gap2Max: number
  gap2Count: DataPoint[]
  gap3Min: number
  gap3Max: number
  gap3Count: DataPoint[]
  gap4Min: number
  gap4Max: number | null
  gap4Count: DataPoint[]
  gap5Min: number | null
  gap5Max: number | null
  gap5Count: DataPoint[]
  gap6Min: number | null
  gap6Max: number | null
  gap6Count: DataPoint[]
  gap7Min: number | null
  gap7Max: number | null
  gap7Count: DataPoint[]
  gap8Min: number | null
  gap8Max: number | null
  gap8Count: DataPoint[]
  gap9Min: number | null
  gap9Max: number | null
  gap9Count: DataPoint[]
  gap10Min: number | null
  gap10Max: number | null
  gap10Count: DataPoint[]
  gap11Min: number | null
  gap11Max: number | null
  gap11Count: DataPoint[]
  trendingLineGapThreshold: number
  sumDuration1: number
  sumDuration2: number
  sumDuration3: number
  sumGreenTime: number
  highestTotal: number
  detectionTypeStr: string
  percentTurnableSeries: DataPoint[]
  trendLineGapThreshold: number
}

export interface RawLeftTurnGapAnalysisResponse {
  type: ChartType.LeftTurnGapAnalysis
  data: RawLeftTurnGapData[]
}
