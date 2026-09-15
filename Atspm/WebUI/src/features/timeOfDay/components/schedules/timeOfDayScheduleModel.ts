import type {
  Plan,
  TimeOfDayLocationResult,
  TimeOfDayResult,
} from '@/api/reports'
import {
  formatPlanNumber,
  freeSchedulePlanColor,
  getPlanIntervalMinutes,
  getSchedulePlanColorMap,
} from '../../schedule'

export interface ScheduleLocation {
  identifier: string
  description?: string | null
  schedule?: Plan[] | null
}

export interface ScheduleException {
  location: ScheduleLocation
  schedule: Plan[]
}

export interface TimeOfDaySchedulesModel {
  proposedSchedule: Plan[]
  commonSchedule: Plan[]
  commonLocations: ScheduleLocation[]
  exceptions: ScheduleException[]
  unavailableLocations: ScheduleLocation[]
  totalLocations: number
}

export interface ScheduleInterval {
  plan: Plan
  startMinutes: number
  endMinutes: number
}

export const freePlanColor = freeSchedulePlanColor

export const formatLocationLabel = ({
  identifier,
  description,
}: ScheduleLocation) => {
  if (!description) return identifier

  return description.includes(identifier)
    ? description
    : `${identifier} - ${description}`
}

export const getScheduleIntervals = (schedule?: Plan[] | null) =>
  (schedule ?? [])
    .map((plan): ScheduleInterval | null => {
      const interval = getPlanIntervalMinutes(plan)
      return interval
        ? {
            plan,
            startMinutes: interval.start,
            endMinutes: interval.end,
          }
        : null
    })
    .filter((interval): interval is ScheduleInterval => Boolean(interval))
    .sort((left, right) => left.startMinutes - right.startMinutes)

const getScheduleSignature = (schedule?: Plan[] | null) =>
  getScheduleIntervals(schedule)
    .map(
      ({ plan, startMinutes, endMinutes }) =>
        `${formatPlanNumber(plan.planNumber).toLowerCase()}:${startMinutes}:${endMinutes}`
    )
    .join('|')

const getUniqueLocations = (result: TimeOfDayResult): ScheduleLocation[] => {
  const resultLocations = result.locations ?? []
  const locationByIdentifier = new Map<string, TimeOfDayLocationResult>()
  resultLocations.forEach((location) => {
    if (location.locationIdentifier) {
      locationByIdentifier.set(location.locationIdentifier, location)
    }
  })

  const identifiers = [
    ...(result.locationIdentifiers?.filter(Boolean) ?? []),
    ...resultLocations
      .map((location) => location.locationIdentifier)
      .filter((identifier): identifier is string => Boolean(identifier)),
  ]

  return [...new Set(identifiers)].map((identifier) => {
    const location = locationByIdentifier.get(identifier)

    return {
      identifier,
      description: location?.locationDescription,
      schedule: location?.currentPlanSchedule,
    }
  })
}

const deriveCommonSchedule = (locations: ScheduleLocation[]) => {
  const schedulesBySignature = new Map<
    string,
    { schedule: Plan[]; count: number }
  >()

  locations.forEach((location) => {
    const signature = getScheduleSignature(location.schedule)
    if (!signature || !location.schedule?.length) return

    const current = schedulesBySignature.get(signature)
    schedulesBySignature.set(signature, {
      schedule: location.schedule,
      count: (current?.count ?? 0) + 1,
    })
  })

  return [...schedulesBySignature.values()].sort(
    (left, right) => right.count - left.count
  )[0]?.schedule
}

export const buildTimeOfDaySchedulesModel = (
  result: TimeOfDayResult
): TimeOfDaySchedulesModel => {
  const locations = getUniqueLocations(result)
  const exceptionIdentifiers = new Set(
    result.planComparison?.exceptionLocationIdentifiers?.filter(Boolean) ?? []
  )
  const commonSchedule = result.planComparison?.commonCurrentSchedule?.length
    ? result.planComparison.commonCurrentSchedule
    : deriveCommonSchedule(locations)
  const commonSignature = getScheduleSignature(commonSchedule)
  const commonLocations: ScheduleLocation[] = []
  const exceptions: ScheduleException[] = []
  const unavailableLocations: ScheduleLocation[] = []

  locations.forEach((location) => {
    const scheduleSignature = getScheduleSignature(location.schedule)
    const matchesCommonSchedule =
      Boolean(commonSignature) && scheduleSignature === commonSignature
    const inferredCommonSchedule =
      Boolean(commonSignature) &&
      !scheduleSignature &&
      !exceptionIdentifiers.has(location.identifier)

    if (matchesCommonSchedule || inferredCommonSchedule) {
      commonLocations.push(location)
      return
    }

    if (location.schedule?.length && scheduleSignature) {
      exceptions.push({ location, schedule: location.schedule })
      return
    }

    unavailableLocations.push(location)
  })

  return {
    proposedSchedule: result.recommendation?.recommendedSchedule ?? [],
    commonSchedule: commonSchedule ?? [],
    commonLocations,
    exceptions,
    unavailableLocations,
    totalLocations: locations.length,
  }
}

export const formatClockTime = (minutes: number) => {
  const normalizedMinutes = minutes === 1440 ? 0 : minutes
  const hours = Math.floor(normalizedMinutes / 60)
  const displayHours = hours % 12 || 12
  const minuteText = String(Math.round(normalizedMinutes % 60)).padStart(2, '0')

  return `${displayHours}:${minuteText} ${hours < 12 ? 'AM' : 'PM'}`
}

export const getScheduleSummary = (schedule: Plan[]) =>
  getScheduleIntervals(schedule)
    .map(
      ({ plan, startMinutes, endMinutes }) =>
        `${formatPlanNumber(plan.planNumber)} from ${formatClockTime(
          startMinutes
        )} to ${formatClockTime(endMinutes)}`
    )
    .join(', ')

export const getScheduleColorMap = getSchedulePlanColorMap
