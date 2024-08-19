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
