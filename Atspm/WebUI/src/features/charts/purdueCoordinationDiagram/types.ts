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

export interface PurdueCoordinationDiagramChartOptions
  extends BaseChartOptions {
  binSize: number
  showPlanStatistics: boolean
  getVolume: boolean
}

export interface PurdueCoordinationDiagramChartOptionsDefaults {
  binSize: { id: number; value: string; option: string }
  yAxisDefault: { id: number; value: string; option: string }
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
