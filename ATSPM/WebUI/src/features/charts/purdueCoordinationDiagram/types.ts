import {
  BaseChartData,
  BaseChartOptions,
  BasePlan,
  ChartType,
  DataPoint,
} from '@/features/charts/common/types'

export interface PurdueCoordinationDiagramChartOptions
  extends BaseChartOptions {
  binSize: number
  showPlanStatistics: boolean
  getVolume: boolean
}

export interface PurdueCoordinationDiagramChartOptionsDefaults {
  binSize: { id: number; value: string; option: string }
  showPlanStatistics: { id: number; value: boolean; option: string }
  getVolume: { id: number; value: boolean; option: string }
}

export interface purdueCoordinationDiagramPlan extends BasePlan {
  percentGreenTime: number
  percentArrivalOnGreen: number
  platoonRatio: number
}

export interface RawPurdueCoordinationDiagramData extends BaseChartData {
  approachId: number
  approachDescription: string
  phaseNumber: number
  phaseDescription: string
  totalOnGreenEvents: number
  totalDetectorHits: number
  percentArrivalOnGreen: number
  plans: purdueCoordinationDiagramPlan[]
  volumePerHour: DataPoint[]
  redSeries: DataPoint[]
  yellowSeries: DataPoint[]
  greenSeries: DataPoint[]
  detectorEvents: DataPoint[]
}

export interface RawPurdueCoordinationDiagramResponse {
  type: ChartType.PurdueCoordinationDiagram
  data: RawPurdueCoordinationDiagramData[]
}
