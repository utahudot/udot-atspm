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

const directionAxis: Record<string, string> = {
  Northbound: 'NorthSouth',
  Southbound: 'NorthSouth',
  Eastbound: 'EastWest',
  Westbound: 'EastWest',
  NorthEast: 'NorthEastSouthWest',
  SouthWest: 'NorthEastSouthWest',
  NorthWest: 'NorthWestSouthEast',
  SouthEast: 'NorthWestSouthEast',
}

/**
 * The backend applies the all-day directions to every hour outside the AM and
 * PM windows, but the form hides that list while AM/PM directions are edited
 * separately. Derive it from the visible AM and PM picks so midday never uses
 * a stale street. Matching AM and PM picks are the user's all-day choice and
 * pass through as-is. When differing picks span more than one street, send none
 * and let the backend infer the strongest street.
 */
export const deriveAllDayPrimaryDirections = (
  amDirections: string[],
  pmDirections: string[]
) => {
  const directions = [...new Set([...amDirections, ...pmDirections])]
  const sameSelection =
    amDirections.length === pmDirections.length &&
    amDirections.every((direction) => pmDirections.includes(direction))
  if (sameSelection) return amDirections

  const axes = new Set(
    directions.map((direction) => directionAxis[direction] ?? direction)
  )

  return axes.size <= 1 ? directions : []
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
    allDayPrimaryDirections: deriveAllDayPrimaryDirections(
      options.amPrimaryDirections,
      options.pmPrimaryDirections
    ),
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
