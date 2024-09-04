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
}
