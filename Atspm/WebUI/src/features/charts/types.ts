// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - types.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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

export type TableRow = (string | number)[]
export type ColumnGroup = { title: string | null; columns: string[] }

export interface Labels {
  columnGroups: ColumnGroup[]
  flatColumns: string[]
}

export interface TransformedTurningMovementCountsResponse {
  type: ChartType
  data: {
    labels: Labels
    table: TableRow[]
    charts: StandardChart[]
    peakHour?: {
      peakHourFactor: number | null
      peakHourData: TableRow[]
    } | null
  }
}

export interface TransformedTimingAndActuationResponse {
  type: ChartType
  data: {
    title: EChartsOption
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
