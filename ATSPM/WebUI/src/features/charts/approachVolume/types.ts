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

export interface ApproachVolumeChartOptions extends BaseChartOptions {
  binSize: number
  getVolume: boolean
  showDirectionalSplits: boolean
  showTotalVolume: boolean
  showNbEbVolume: boolean
  showSbWbVolume: boolean
  showTMCDetection: boolean
  showAdvanceDetection: boolean
}

export interface ApproachVolumeChartOptionsDefaults {
  binSize: { id: number; value: string; option: string }
  getVolume: { id: number; value: string; option: string }
  showDirectionalSplits: { id: number; value: string; option: string }
  showTotalVolume: { id: number; value: string; option: string }
  showNbEbVolume: { id: number; value: string; option: string }
  showSbWbVolume: { id: number; value: string; option: string }
  showTMCDetection: { id: number; value: string; option: string }
  showAdvanceDetection: { id: number; value: string; option: string }
}

export interface ApproachVolumeSummaryData {
  primaryDirectionName: string
  opposingDirectionName: string
  peakHour: string
  kFactor: number
  peakHourVolume: number
  peakHourFactor: number
  totalVolume: number
  primaryPeakHour: string
  primaryKFactor: number
  primaryPeakHourVolume: number
  primaryPeakHourFactor: number
  primaryTotalVolume: number
  primaryDFactor: number
  opposingPeakHour: string
  opposingKFactor: number
  opposingPeakHourVolume: number
  opposingPeakHourFactor: number
  opposingTotalVolume: number
  opposingDFactor: number
}

export interface RawApproachVolumeData extends BaseChartData {
  primaryDirectionName: string
  opposingDirectionName: string
  distanceFromStopBar: number
  detectorType: string
  primaryDirectionVolumes: DataPoint[]
  opposingDirectionVolumes: DataPoint[]
  combinedDirectionVolumes: DataPoint[]
  primaryDFactors: DataPoint[]
  opposingDFactors: DataPoint[]
  summaryData: ApproachVolumeSummaryData
}

export interface RawApproachVolumeResponse {
  type: ChartType.ApproachVolume
  data: RawApproachVolumeData[]
}
