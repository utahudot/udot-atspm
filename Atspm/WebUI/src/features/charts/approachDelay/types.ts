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

export interface ApproachDelayChartOptions extends BaseChartOptions {
  binSize: number
  getPermissivePhase: boolean
  getVolume: boolean
}

export interface ApproachDelayChartOptionsDefaults {
  binSize: { id: number; value: string; option: string }
  getPermissivePhase: { id: number; value: string; option: string }
  getVolume: { id: number; value: string; option: string }
}

export interface ApproachDelayPlan extends BasePlan {
  averageDelay: number
  totalDelay: number
  planDescription: string
}

export interface RawApproachDelayData extends BaseChartData {
  phaseNumber: number
  phaseDescription: string
  averageDelayPerVehicle: number
  totalDelay: number
  plans: ApproachDelayPlan[]
  approachDelayDataPoints: DataPoint[]
  approachDelayPerVehicleDataPoints: DataPoint[]
  approachId: number
}

export interface RawApproachDelayReponse {
  type: ChartType.ApproachDelay
  data: RawApproachDelayData[]
}
