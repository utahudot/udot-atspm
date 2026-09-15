import {
  getAggregationDaysWithDataFromLocationIdentifierAndDataType,
  getEventLogDaysWithDataFromLocationIdentifierAndDataType,
} from '@/api/data'
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
import type { CalendarDayAvailability } from './types'

type EventLogDataType = Parameters<
  typeof getEventLogDaysWithDataFromLocationIdentifierAndDataType
>[1]
type AggregationDataType = Parameters<
  typeof getAggregationDaysWithDataFromLocationIdentifierAndDataType
>[1]

export type DayAvailabilityDataSource =
  | {
      dataCategory: 'raw'
      dataType: EventLogDataType
    }
  | {
      dataCategory: 'aggregation'
      dataType: AggregationDataType
    }

const defaultDataSource: DayAvailabilityDataSource = {
  dataCategory: 'raw',
  dataType: 'IndianaEvent',
}

interface DayAvailabilityParams {
  locationIdentifiers: string[]
  availableDaysByLocation: string[][]
  startDate: Date
  endDate: Date
  today?: Date
  includedDaysOfWeek?: number[]
}

interface DayAvailabilityResult {
  requestKey: string
  days: CalendarDayAvailability[]
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

export const getDayAvailabilityCalendarRange = (date: Date) => ({
  start: startOfWeek(startOfMonth(date)),
  end: endOfWeek(endOfMonth(date)),
})

export const getDayAvailabilityFromLocationData = ({
  locationIdentifiers,
  availableDaysByLocation,
  startDate,
  endDate,
  today = startOfToday(),
  includedDaysOfWeek,
}: DayAvailabilityParams): CalendarDayAvailability[] => {
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

export const useDayAvailability = (
  locationIdentifiers: string[],
  startDate: Date,
  endDate: Date,
  includedDaysOfWeek?: number[],
  dataSource: DayAvailabilityDataSource = defaultDataSource
): CalendarDayAvailability[] => {
  const [result, setResult] = useState<DayAvailabilityResult>({
    requestKey: '',
    days: [],
  })
  const normalizedLocationIdentifiers = useMemo(
    () => normalizeLocationIdentifiers(locationIdentifiers),
    [locationIdentifiers]
  )
  const normalizedIncludedDaysOfWeek = useMemo(
    () => normalizeIncludedDaysOfWeek(includedDaysOfWeek),
    [includedDaysOfWeek]
  )
  const requestKey = JSON.stringify([
    normalizedLocationIdentifiers,
    startDate.getTime(),
    endDate.getTime(),
    normalizedIncludedDaysOfWeek,
    dataSource.dataCategory,
    dataSource.dataType,
  ])

  useEffect(() => {
    if (
      normalizedLocationIdentifiers.length === 0 ||
      !isValid(startDate) ||
      !isValid(endDate) ||
      isAfter(startDate, endDate)
    ) {
      setResult({ requestKey, days: [] })
      return
    }

    const abortController = new AbortController()

    const computeDayAvailability = async () => {
      try {
        const availableDaysByLocation = await Promise.all(
          normalizedLocationIdentifiers.map((locationIdentifier) => {
            const params = {
              start: dateToTimestamp(startDate),
              end: dateToTimestamp(endDate),
            }

            return dataSource.dataCategory === 'aggregation'
              ? getAggregationDaysWithDataFromLocationIdentifierAndDataType(
                  locationIdentifier,
                  dataSource.dataType,
                  params,
                  abortController.signal
                )
              : getEventLogDaysWithDataFromLocationIdentifierAndDataType(
                  locationIdentifier,
                  dataSource.dataType,
                  params,
                  abortController.signal
                )
          })
        )

        if (abortController.signal.aborted) return

        setResult({
          requestKey,
          days: getDayAvailabilityFromLocationData({
            locationIdentifiers: normalizedLocationIdentifiers,
            availableDaysByLocation,
            startDate,
            endDate,
            includedDaysOfWeek: normalizedIncludedDaysOfWeek,
          }),
        })
      } catch (error) {
        if (abortController.signal.aborted) return

        console.error('Error computing day availability:', error)
        setResult({ requestKey, days: [] })
      }
    }

    computeDayAvailability()

    return () => abortController.abort()
  }, [
    normalizedLocationIdentifiers,
    startDate,
    endDate,
    normalizedIncludedDaysOfWeek,
    dataSource.dataCategory,
    dataSource.dataType,
    requestKey,
  ])

  return result.requestKey === requestKey ? result.days : []
}
