import {
  AnalysisPeriod,
  DataSource,
} from '@/features/speedManagementTool/enums'

export interface RouteSpeedRequest {
  sourceId: DataSource
  startDate: string
  endDate: string
  daysOfWeek: number[]
  analysisPeriod: AnalysisPeriod
  violationThreshold: number
  customStartTime?: Date
  customEndTime?: Date
  region: string | null
  county: string | null
  city: string | null
  accessCategory: string | null
  functionalType: string | null
}
