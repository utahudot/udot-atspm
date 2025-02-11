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

export interface YellowAndRedActuationsChartOptions extends BaseChartOptions {
  severeLevelSeconds: number
}

export interface YellowAndRedActuationsChartOptionsDefaults {
  severeLevelSeconds: { id: number; value: string; option: string }
  yAxisDefault: { id: number; value: string; option: string }
}

export interface YellowAndRedActuationsPlan extends BasePlan {
  totalViolations: number
  severeViolations: number
  percentViolations: number
  percentSevereViolations: number
  averageTimeViolations: number
}

export interface RawYellowAndRedActuationsData extends BaseChartData {
  approachId: number
  approachDescription: string
  phaseNumber: number
  totalViolations: number
  severeViolations: number
  isPermissivePhase: boolean
  yellowLightOccurences: number
  plans: YellowAndRedActuationsPlan[]
  redEvents: DataPoint[]
  yellowEvents: DataPoint[]
  redClearanceEvents: DataPoint[]
  detectorEvents: DataPoint[]
}

export interface RawYellowAndRedActuationsResponse {
  type: ChartType.YellowAndRedActuations
  data: RawYellowAndRedActuationsData[]
}