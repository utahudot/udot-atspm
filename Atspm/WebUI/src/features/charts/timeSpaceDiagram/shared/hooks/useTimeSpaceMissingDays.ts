import { getEventLogDaysWithDataFromLocationIdentifierAndDataType } from '@/api/data'
import { dateToTimestamp } from '@/utils/dateTime'
import {
  eachDayOfInterval,
  endOfMonth,
  endOfWeek,
  format,
  isAfter,
  isValid,
  startOfMonth,
  startOfToday,
  startOfWeek,
} from 'date-fns'
import { useEffect, useMemo, useState } from 'react'

const TIME_SPACE_DATA_TYPE = 'IndianaEvent'

interface TimeSpaceDayAvailabilityParams {
  locationIdentifiers: string[]
  availableDaysByLocation: string[][]
  startDate: Date
  endDate: Date
  today?: Date
  includedDaysOfWeek?: number[]
}

export interface TimeSpaceLocationDayAvailability {
  locationIdentifier: string
  hasData: boolean
}

export interface TimeSpaceDayAvailability {
  date: Date
  availableLocationCount: number
  totalLocationCount: number
  locations: TimeSpaceLocationDayAvailability[]
}

const normalizeLocationIdentifiers = (locationIdentifiers: string[]) =>
  Array.from(
    new Set(
      locationIdentifiers
        .map((locationIdentifier) => locationIdentifier.trim())
        .filter((locationIdentifier) => locationIdentifier.length > 0)
    )
  )

const normalizeIncludedDaysOfWeek = (includedDaysOfWeek?: number[]) => {
  if (!includedDaysOfWeek) return undefined

  return Array.from(
    new Set(
      includedDaysOfWeek.filter(
        (dayOfWeek) =>
          Number.isInteger(dayOfWeek) && dayOfWeek >= 0 && dayOfWeek <= 6
      )
    )
  )
}

export const getTimeSpaceCalendarRange = (date: Date) => ({
  start: startOfWeek(startOfMonth(date)),
  end: endOfWeek(endOfMonth(date)),
})

export const getTimeSpaceDayAvailabilityFromLocationData = ({
  locationIdentifiers,
  availableDaysByLocation,
  startDate,
  endDate,
  today = startOfToday(),
  includedDaysOfWeek,
}: TimeSpaceDayAvailabilityParams): TimeSpaceDayAvailability[] => {
  const normalizedLocationIdentifiers =
    normalizeLocationIdentifiers(locationIdentifiers)

  if (
    normalizedLocationIdentifiers.length === 0 ||
    normalizedLocationIdentifiers.length !== availableDaysByLocation.length ||
    availableDaysByLocation.length === 0 ||
    !isValid(startDate) ||
    !isValid(endDate) ||
    isAfter(startDate, endDate)
  ) {
    return []
  }

  const includedDays = normalizeIncludedDaysOfWeek(includedDaysOfWeek)
  const availableDayKeysByLocation = availableDaysByLocation.map(
    (availableDays) => new Set(availableDays)
  )
  const allDays = eachDayOfInterval({ start: startDate, end: endDate })

  return allDays
    .filter((day) => {
      if (isAfter(day, today)) return false
      if (includedDays && !includedDays.includes(day.getDay())) return false

      return true
    })
    .map((day) => {
      const dayKey = format(day, 'yyyy-MM-dd')
      const locations = normalizedLocationIdentifiers.map(
        (locationIdentifier, index) => ({
          locationIdentifier,
          hasData: availableDayKeysByLocation[index]?.has(dayKey) ?? false,
        })
      )
      const availableLocationCount = locations.filter(
        (location) => location.hasData
      ).length

      return {
        date: day,
        availableLocationCount,
        totalLocationCount: locations.length,
        locations,
      }
    })
}

export const useTimeSpaceDayAvailability = (
  locationIdentifiers: string[],
  startDate: Date,
  endDate: Date,
  includedDaysOfWeek?: number[]
): TimeSpaceDayAvailability[] => {
  const [dayAvailability, setDayAvailability] = useState<
    TimeSpaceDayAvailability[]
  >([])
  const normalizedLocationIdentifiers = useMemo(
    () => normalizeLocationIdentifiers(locationIdentifiers),
    [locationIdentifiers]
  )
  const normalizedIncludedDaysOfWeek = useMemo(
    () => normalizeIncludedDaysOfWeek(includedDaysOfWeek),
    [includedDaysOfWeek]
  )

  useEffect(() => {
    if (
      normalizedLocationIdentifiers.length === 0 ||
      !isValid(startDate) ||
      !isValid(endDate) ||
      isAfter(startDate, endDate)
    ) {
      setDayAvailability([])
      return
    }

    const abortController = new AbortController()

    const computeDayAvailability = async () => {
      try {
        const availableDaysByLocation = await Promise.all(
          normalizedLocationIdentifiers.map((locationIdentifier) =>
            getEventLogDaysWithDataFromLocationIdentifierAndDataType(
              locationIdentifier,
              TIME_SPACE_DATA_TYPE,
              {
                start: dateToTimestamp(startDate),
                end: dateToTimestamp(endDate),
              },
              abortController.signal
            )
          )
        )

        if (abortController.signal.aborted) return

        setDayAvailability(
          getTimeSpaceDayAvailabilityFromLocationData({
            locationIdentifiers: normalizedLocationIdentifiers,
            availableDaysByLocation,
            startDate,
            endDate,
            includedDaysOfWeek: normalizedIncludedDaysOfWeek,
          })
        )
      } catch (error) {
        if (abortController.signal.aborted) return

        console.error('Error computing time-space day availability:', error)
        setDayAvailability([])
      }
    }

    computeDayAvailability()

    return () => abortController.abort()
  }, [
    normalizedLocationIdentifiers,
    startDate,
    endDate,
    normalizedIncludedDaysOfWeek,
  ])

  return dayAvailability
}
