import {
  AggClassification,
  SpeedDataType,
  TimePeriodFilter,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'

export interface HotSpotForReportMap {
  segmentId: string
  coordinates: [number, number][]
}

export interface ImpactHotspotForReportMap {
  impactId: string
  impactedSegments: HotSpotForReportMap[]
}

export const AggClassificationMapping = {
  [AggClassification.Total]: 'All Days',
  [AggClassification.Weekend]: 'Weekend',
  [AggClassification.Weekday]: 'Weekdays',
} as const

export const TimePeriodFilterMapping = {
  [TimePeriodFilter.AllDay]: 'All Hours',
  [TimePeriodFilter.OffPeak]: 'Off Peak',
  [TimePeriodFilter.AmPeak]: 'AM Peak',
  [TimePeriodFilter.PmPeak]: 'PM Peak',
  [TimePeriodFilter.MidDay]: 'Mid Day',
  [TimePeriodFilter.Evening]: 'Evening',
  [TimePeriodFilter.EarlyMorning]: 'Early Morning',
} as const

export const SourceIdToNamingMapping = {
  1: 'ATSPM',
  2: 'Pems',
  3: 'Clearguide',
}

export const SpeedDataTypeMapping = {
  [SpeedDataType.H]: 'Hourly',
  [SpeedDataType.M]: 'Monthly',
}
