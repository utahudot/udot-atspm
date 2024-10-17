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

export interface ArrivalsOnRedChartOptions extends BaseChartOptions {
  binSize: number
  getPermissivePhase: boolean
}

export interface ArrivalsOnRedChartOptionsDefaults {
  binSize: { id: number; value: string; option: string }
  getPermissivePhase: { id: number; value: string; option: string }
}

export interface ArrivalsOnRedPlan extends BasePlan {
  percentArrivalOnRed: number
  percentRedTime: number
}

export interface RawArrivalsOnRedData extends BaseChartData {
  approachId: number
  phaseNumber: number
  phaseDescription: string
  totalDetectorHits: number
  totalArrivalOnRed: number
  percentArrivalOnRed: number
  plans: ArrivalsOnRedPlan[]
  percentArrivalsOnRed: DataPoint[]
  totalVehicles: DataPoint[]
  arrivalsOnRed: DataPoint[]
}

export interface RawArrivalsOnRedResponse {
  type: ChartType.ArrivalsOnRed
  data: RawArrivalsOnRedData[]
}
