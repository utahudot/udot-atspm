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
    table: RawTurningMovementCountTableRow[]
    peakHourFactor: number | null
    peakHour: { key: string; value: number } | null
  }
}

interface RawTurningMovementCountTableRow {
  direction: string
  movementType: string
  laneType: string
  volume: { value: number; timestamp: string }[]
}
