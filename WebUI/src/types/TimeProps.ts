import { ChartType, ToolType } from '@/features/charts/common/types'

export interface DateTimeProps {
  startDateTime: Date
  endDateTime: Date
  changeStartDate(date: Date): void
  changeEndDate(date: Date): void
  chartType?: ChartType | ToolType
}

export interface TimeOnlyProps {
  startTime: Date
  endTime: Date
  changeStartTime(date: Date): void
  changeEndTime(date: Date): void
}
