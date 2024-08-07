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
import { BaseChartOptions, ChartType } from '@/features/charts/common/types'

export type PreemptionDetailsChartOptions = BaseChartOptions

interface PreemptRequestService {
  preemptionNumber: number
  requests: string[]
  services: string[]
}

export interface Cycle {
  inputOff: string
  inputOn: string
  gateDown: string | null
  callMaxOut: string | null
  delay: number | null
  timeToService: number | null
  dwellTime: number | null
  trackClear: number | null
}
export interface LocationDetail {
  locationIdentifer: string
  locationDescription: string
  start: string
  end: string
  cycles: Cycle[]
  preemptionNumber: number
}

export interface PreemptServiceSummary {
  locationIdentifer: string
  locationDescription: string
  start: string
  end: string
  requestAndServices: PreemptRequestService[]
}

export interface RawPreemptionDetailsResponse {
  type: ChartType.PreemptionDetails
  data: {
    details: LocationDetail[]
    summary: PreemptServiceSummary
  }
}
