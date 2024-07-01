import { BaseChartData, ChartType, DataPoint } from '../common/types'
import { DetectorEvent } from '../timingAndActuation/types'

export interface DescriptionWithDataPoints {
  description: string
  value: DataPoint[]
}

export interface DescriptionWithDetectorEvents {
  description: string
  value: DetectorEvent[]
}

export interface RampMeteringData extends BaseChartData {
  mainlineAvgFlow: DataPoint[]
  mainlineAvgOcc: DataPoint[]
  mainlineAvgSpeed: DataPoint[]
  lanesActiveRate: DescriptionWithDataPoints[]
  lanesBaseRate: DescriptionWithDataPoints[]
  startUpWarning: DataPoint[]
  shutdownWarning: DataPoint[]
  // lanesQueueEvents: DescriptionWithDetectorEvents[]
  lanesQueueOnEvents: DescriptionWithDataPoints[]
  lanesQueueOffEvents: DescriptionWithDataPoints[]
}

export interface RawRampMeteringResponse {
  type: ChartType.RampMetering
  data: RampMeteringData[]
}
