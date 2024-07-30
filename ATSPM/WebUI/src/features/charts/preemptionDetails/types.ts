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
