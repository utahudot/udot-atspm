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

export interface SplitMonitorChartOptions extends BaseChartOptions {
  percentileSplit: number
}

export interface SplitMonitorChartOptionsDefaults {
  percentileSplit: { id: number; value: string; option: string }
  yAxisDefault: { id: number; value: string; option: string }
}
export interface SplitMonitorPlan extends BasePlan {
  percentSkips: number
  percentGapOuts: number
  percentMaxOuts: number | null
  percentForceOffs: number | null
  averageSplit: number
  percentileSplit: number
  minTime: number
  programmedSplit: number
  percentileSplit85th: number
  percentileSplit50th: number
}

export interface RawSplitMonitorData extends BaseChartData {
  phaseNumber: number
  yAxisDefault: string
  percentileSplit: number
  phaseDescription: string
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
