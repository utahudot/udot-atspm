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
} from '@/features/charts/common/types'
import { EChartsOption } from 'echarts'

export interface TimingAndActuationChartOptions extends BaseChartOptions {
  showPedestrianIntervals: boolean
  showPedestrianActuation: boolean
  extendStartStopSearch: number
  showStopBarPresence: boolean
  showLaneByLaneCount: boolean
  showAdvancedDilemmaZone: boolean
  showAdvancedCount: boolean
  showAllLanesInfo: boolean
  globalEventCounter: number
  phaseEventCodesList: number[]
  globalEventCodesList: number[]
  globalEventParamsList: number[]
}

export interface TimingAndActuationChartOptionsDefaults {
  showPedestrianIntervals: { id: number; value: string; option: string }
  showPedestrianActuation: { id: number; value: string; option: string }
  extendStartStopSearch: { id: number; value: string; option: string }
  showStopBarPresence: { id: number; value: string; option: string }
  showLaneByLaneCount: { id: number; value: string; option: string }
  showAdvancedDilemmaZone: { id: number; value: string; option: string }
  showAdvancedCount: { id: number; value: string; option: string }
  showAllLanesInfo: { id: number; value: string; option: string }
  globalEventCounter: { id: number; value: string; option: string }
  phaseEventCodesList: { id: number; value: string; option: string }
  globalEventCodesList: { id: number; value: string; option: string }
  globalEventParamsList: { id: number; value: string; option: string }
}

export interface TimingAndActuationEChartsOption extends EChartsOption {
  amountOfSegments: number
}

export interface DetectorEvent {
  detectorOn: string
  detectorOff: string
}

export interface Cycle {
  start: string
  value: number
}

export interface PedestrianInterval {
  start: string
  value: number
}

export interface BasicDetectors {
  name: string
  events: DetectorEvent[]
}

export interface AdvancedDetectors extends BasicDetectors {
  isOffset: boolean
}

export interface RawTimingAndActuationData extends BaseChartData {
  phaseNumber: number
  isPhaseOverLap: boolean
  phaseNumberSort: string
  getPermissivePhase: boolean
  pedestrianIntervals: PedestrianInterval[] | []
  pedestrianEvents: BasicDetectors[] | []
  stopBarDetectors: BasicDetectors[] | []
  laneByLanesDetectors: BasicDetectors[] | []
  advanceCountDetectors: AdvancedDetectors[] | []
  advancePresenceDetectors: AdvancedDetectors[] | []
  cycleAllEvents: Cycle[] | null
  phaseCustomEvents: unknown
  approachId: number
  approachDescription: string
  phaseType: string
}

export interface RawTimingAndActuationResponse {
  type: ChartType.TimingAndActuation
  data: RawTimingAndActuationData[]
}
