import { EChartsOption } from 'echarts'
import { ApproachVolumeSummaryData } from './approachVolume/types'
import { ChartType, ToolType } from './common/types'

export interface ExtendedEChartsOption extends EChartsOption {
  displayProp?: {
    height: number
    description: string
  }
}

interface ApproachVolumeChart {
  chart: ExtendedEChartsOption
  table: ApproachVolumeSummaryData
}

export interface StandardChart {
  chart: ExtendedEChartsOption
}

export interface TransformedDefaultResponse {
  type: ChartType
  data: {
    charts: StandardChart[]
  }
}

export interface TransformedToolResponse {
  type: ToolType
  data: {
    charts: StandardChart[]
  }
}

export interface TransformedApproachVolumeResponse {
  type: ChartType.ApproachVolume
  data: {
    charts: ApproachVolumeChart[]
  }
}

export interface TransformedPreemptDetailsResponse {
  type: ChartType
  data: {
    charts: StandardChart[]
  }
}
export interface TransformedTurningMovementCountsResponse {
  type: ChartType
  data: {
    charts: StandardChart[]
    table: any
    peakHourFactor: number
    peakHour: { key: string; value: string }
  }
}

export interface TransformedTimingAndActuationResponse {
  type: ChartType
  data: {
    charts: StandardChart[]
    legends: EChartsOption[]
  }
}

export interface TransformedRampMeteringResponse {
  type: ChartType
  data: {
    charts: StandardChart[]
  }
}

export type TransformedChartResponse =
  | TransformedDefaultResponse
  | TransformedApproachVolumeResponse
  | TransformedPreemptDetailsResponse
  | TransformedTimingAndActuationResponse
  | TransformedTurningMovementCountsResponse
  | TransformedToolResponse

export type ChartDefaults = {
  abbreviation: string
  name: string
  id: number
  chartType: ChartType
  showOnWebsite: boolean
  showOnAggregationSite: boolean
  displayOrder: number
  measureOptions: Default[]
}

export type Default = {
  id: number
  option: string
  value: string | number | boolean | number[]
}
export interface MeasureType {
  id: number
  name: string
  abbreviation: string
  showOnWebsite: boolean
  showOnAggregationSite: boolean
  displayOrder: number
}
