import {
  BaseChartData,
  BaseChartOptions,
  BasePlan,
  ChartType,
  DataPoint,
} from '@/features/charts/common/types'

export interface PedestrianDelayChartOptions extends BaseChartOptions {
  showPedRecall: boolean
  showCycleLength: boolean
  showPercentDelay: boolean
  showPedBeginWalk: boolean
  timeBuffer: number
  pedRecallThreshold: number
}

export interface PedestrianDelayChartOptionsDefaults {
  showPedRecall: { id: number; value: boolean; option: string }
  showCycleLength: { id: number; value: boolean; option: string }
  showPercentDelay: { id: number; value: boolean; option: string }
  showPedBeginWalk: { id: number; value: boolean; option: string }
  timeBuffer: { id: number; value: string; option: string }
  pedRecallThreshold: { id: number; value: string; option: string }
}

export interface pedestrianDelayPlan extends BasePlan {
  pedRecallMessage: string
  cyclesWithPedRequests: number
  uniquePedDetections: number
  averageDelaySeconds: number
  averageCycleLengthSeconds: number
  pedPresses: number
}

export interface RawPedestrianDelayData extends BaseChartData {
  approachId: number
  approachDescription: string
  phaseNumber: number
  phaseDescription: string
  pedPresses: number
  cyclesWithPedRequests: number
  timeBuffered: number
  uniquePedestrianDetections: number
  minDelay: number
  maxDelay: number
  averageDelay: number
  plans: pedestrianDelayPlan[]
  cycleLengths: DataPoint[]
  pedestrianDelay: DataPoint[]
  startOfWalk: DataPoint[]
  percentDelayByCycleLength: DataPoint[]
}

export interface RawPedestrianDelayResponse {
  type: ChartType.PedestrianDelay
  data: RawPedestrianDelayData[]
}
