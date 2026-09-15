import type { SearchLocation } from '@/api/config'

export type TimeOfDayDataSourceOption = 'IndianaEvents' | 'Aggregated'

export interface TimeOfDayTuningOptions {
  amEntryPctOfPeak: number
  amExitPctOfPeak: number
  pmEntryPctOfPeak: number
  pmExitPctOfPeak: number
  freeEntryPctOfDailyPeak: number
  freeEntryPctOfDynamicRange: number
  entrySustainedBins: number
  freeSustainedBins: number
  freeFallbackTime: string
  maxAmEndTime: string
  maxPmEndTime: string
  laneCapacityVehiclesPerHour: number
  approachVolumeAssumedLanes: number
  splitReviewThresholdPercent: number
  shoulderReviewThresholdPercent: number
}

export type TimeOfDayTuningOptionKey = keyof TimeOfDayTuningOptions

export const timeOfDayTuningOptionKeys: TimeOfDayTuningOptionKey[] = [
  'amEntryPctOfPeak',
  'amExitPctOfPeak',
  'pmEntryPctOfPeak',
  'pmExitPctOfPeak',
  'freeEntryPctOfDailyPeak',
  'freeEntryPctOfDynamicRange',
  'entrySustainedBins',
  'freeSustainedBins',
  'freeFallbackTime',
  'maxAmEndTime',
  'maxPmEndTime',
  'laneCapacityVehiclesPerHour',
  'approachVolumeAssumedLanes',
  'splitReviewThresholdPercent',
  'shoulderReviewThresholdPercent',
]

export const timeOfDayDefaultTuningOptions: TimeOfDayTuningOptions = {
  amEntryPctOfPeak: 0.55,
  amExitPctOfPeak: 0.4,
  pmEntryPctOfPeak: 0.68,
  pmExitPctOfPeak: 0.38,
  freeEntryPctOfDailyPeak: 0.22,
  freeEntryPctOfDynamicRange: 0.18,
  entrySustainedBins: 2,
  freeSustainedBins: 4,
  freeFallbackTime: '23:30',
  maxAmEndTime: '10:00',
  maxPmEndTime: '20:00',
  laneCapacityVehiclesPerHour: 800,
  approachVolumeAssumedLanes: 2,
  splitReviewThresholdPercent: 35,
  shoulderReviewThresholdPercent: 45,
}

export interface TimeOfDayOptions extends TimeOfDayTuningOptions {
  locationIdentifiers: string[]
  selectedDates: string[]
  binSizeMinutes: 15
  dataSource: TimeOfDayDataSourceOption
  allDayPrimaryDirections: string[]
  amPrimaryDirections: string[]
  pmPrimaryDirections: string[]
  directionLaneCounts?: Record<string, number>
}

export interface TimeOfDayFormState extends TimeOfDayTuningOptions {
  selectedLocations: SearchLocation[]
  selectedDates: Date[]
  dataSource: TimeOfDayDataSourceOption
  allDayPrimaryDirections: string[]
  amPrimaryDirections: string[]
  pmPrimaryDirections: string[]
  directionLaneCounts: Record<string, number>
}

export const timeOfDayDataSourceLabels: Record<
  TimeOfDayDataSourceOption,
  string
> = {
  IndianaEvents: 'Indiana Events',
  Aggregated: 'Aggregated',
}

export const defaultPrimaryDirections = ['Northbound', 'Southbound']
