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

export interface ApproachSpeedChartOptions extends BaseChartOptions {
  binSize: number
}

export interface ApproachSpeedChartOptionsDefaults {
  binSize: { id: number; value: string; option: string }
}

export interface ApproachSpeedPlan extends BasePlan {
  averageSpeed: number
  standardDeviation: number
  eightyFifthPercentile: number
  fifteenthPercentile: number
}

export interface RawApproachSpeedData extends BaseChartData {
  phaseNumber: number
  phaseDescription: string
  detectionType: string
  distanceFromStopBar: number
  postedSpeed: number
  plans: ApproachSpeedPlan[]
  averageSpeeds: DataPoint[]
  eightyFifthSpeeds: DataPoint[]
  fifteenthSpeeds: DataPoint[]
  approachId: number
  approachDescription: string
}

export interface RawApproachSpeedReponse {
  type: ChartType.ApproachSpeed
  data: RawApproachSpeedData[]
}
