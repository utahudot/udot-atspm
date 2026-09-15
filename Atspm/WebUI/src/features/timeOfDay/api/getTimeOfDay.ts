import type {
  TimeOfDayOptions as ApiTimeOfDayOptions,
  ProblemDetails,
  TimeOfDayResult,
} from '@/api/reports'
import { getTimeOfDayReportData } from '@/api/reports'
import { useMutation, UseMutationOptions } from 'react-query'
import type { TimeOfDayOptions } from '../types'

const apiDataSourceByName: Record<
  TimeOfDayOptions['dataSource'],
  ApiTimeOfDayOptions['dataSource']
> = {
  IndianaEvents: 0 as ApiTimeOfDayOptions['dataSource'],
  Aggregated: 1 as ApiTimeOfDayOptions['dataSource'],
}

export const toApiTimeOfDayOptions = (
  options: TimeOfDayOptions
): ApiTimeOfDayOptions => {
  const directionLaneCounts = Object.fromEntries(
    Object.entries(options.directionLaneCounts ?? {}).filter(
      ([, count]) => Number.isFinite(count) && count > 0
    )
  )

  return {
    locationIdentifiers: options.locationIdentifiers,
    selectedDates: options.selectedDates,
    binSizeMinutes: options.binSizeMinutes,
    dataSource: apiDataSourceByName[options.dataSource],
    allDayPrimaryDirections: options.allDayPrimaryDirections,
    amPrimaryDirections: options.amPrimaryDirections,
    pmPrimaryDirections: options.pmPrimaryDirections,
    amEntryPctOfPeak: options.amEntryPctOfPeak,
    amExitPctOfPeak: options.amExitPctOfPeak,
    pmEntryPctOfPeak: options.pmEntryPctOfPeak,
    pmExitPctOfPeak: options.pmExitPctOfPeak,
    freeEntryPctOfDailyPeak: options.freeEntryPctOfDailyPeak,
    freeEntryPctOfDynamicRange: options.freeEntryPctOfDynamicRange,
    entrySustainedBins: options.entrySustainedBins,
    freeSustainedBins: options.freeSustainedBins,
    freeFallbackTime: options.freeFallbackTime,
    maxAmEndTime: options.maxAmEndTime,
    maxPmEndTime: options.maxPmEndTime,
    laneCapacityVehiclesPerHour: options.laneCapacityVehiclesPerHour,
    approachVolumeAssumedLanes: options.approachVolumeAssumedLanes,
    splitReviewThresholdPercent: options.splitReviewThresholdPercent,
    shoulderReviewThresholdPercent: options.shoulderReviewThresholdPercent,
    directionLaneCounts: Object.keys(directionLaneCounts).length
      ? directionLaneCounts
      : undefined,
  }
}

export const getTimeOfDay = (options: TimeOfDayOptions) =>
  getTimeOfDayReportData(toApiTimeOfDayOptions(options))

export const useTimeOfDayReport = (
  options?: UseMutationOptions<
    TimeOfDayResult,
    ProblemDetails,
    TimeOfDayOptions
  >
) =>
  useMutation<TimeOfDayResult, ProblemDetails, TimeOfDayOptions>(
    getTimeOfDay,
    options
  )
