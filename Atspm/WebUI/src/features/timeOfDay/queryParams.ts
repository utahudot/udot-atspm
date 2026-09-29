import {
  getLocationLocationsForSearch,
  type SearchLocation,
} from '@/api/config'
import { directionList } from '@/features/locations/types/DirectionType'
import type { TimeOfDayDataSourceOption } from '@/features/timeOfDay/types'
import { format, isValid, parseISO } from 'date-fns'
import { createParser } from 'nuqs'

const validDirections = new Set(directionList)

export const ymdDateParser = createParser<Date>({
  parse: (value) => {
    const parsedDate = parseISO(value)
    return isValid(parsedDate) ? parsedDate : null
  },
  serialize: (date) => format(date, 'yyyy-MM-dd'),
  eq: (a, b) => a.getTime() === b.getTime(),
})

export const dataSourceParser = createParser<TimeOfDayDataSourceOption>({
  parse: (value) =>
    value === 'IndianaEvents' || value === 'Aggregated' ? value : null,
  serialize: (value) => value,
  eq: (a, b) => a === b,
})

export const areStringArraysEqual = (a: string[], b: string[]) =>
  a.length === b.length && a.every((value, index) => value === b[index])

export const areDateArraysEqual = (a: Date[], b: Date[]) =>
  areStringArraysEqual(
    a.map((date) => format(date, 'yyyy-MM-dd')),
    b.map((date) => format(date, 'yyyy-MM-dd'))
  )

export const areLaneCountsEqual = (
  a: Record<string, number>,
  b: Record<string, number>
) => {
  const aEntries = Object.entries(a)
  const bEntries = Object.entries(b)

  return (
    aEntries.length === bEntries.length &&
    aEntries.every(([direction, count]) => b[direction] === count)
  )
}

const serializeLaneCounts = (laneCounts: Record<string, number>) =>
  Object.entries(laneCounts)
    .filter(([, count]) => Number.isFinite(count) && count > 0)
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([direction, count]) => `${direction}:${count}`)
    .join(',')

export const laneCountsParser = createParser<Record<string, number>>({
  parse: (value) =>
    value.split(',').reduce<Record<string, number>>((laneCounts, pair) => {
      const [direction, rawCount] = pair.split(':')
      const count = Number(rawCount)

      if (direction && Number.isFinite(count) && count > 0) {
        laneCounts[direction] = count
      }

      return laneCounts
    }, {}),
  serialize: serializeLaneCounts,
  eq: areLaneCountsEqual,
})

export const normalizeLocationIdentifiers = (identifiers: string[]) =>
  Array.from(
    new Set(
      identifiers
        .map((identifier) => identifier.trim())
        .filter((identifier): identifier is string => Boolean(identifier))
    )
  )

export const normalizeDates = (dates: Date[], fallbackDates: Date[]) => {
  const dateStrings = Array.from(
    new Set(
      dates
        .filter((date) => isValid(date))
        .map((date) => format(date, 'yyyy-MM-dd'))
    )
  ).sort()

  if (dateStrings.length === 0) {
    return fallbackDates
  }

  return dateStrings.map((dateString) => parseISO(dateString))
}

export const normalizeDirections = (
  directions: string[],
  fallback: string[]
) => {
  const normalizedDirections = directions.filter((direction) =>
    validDirections.has(direction)
  )

  return normalizedDirections.length ? normalizedDirections : fallback
}

export const normalizeLaneCounts = (laneCounts: Record<string, number>) =>
  Object.fromEntries(
    Object.entries(laneCounts).filter(
      ([direction, count]) =>
        validDirections.has(direction) && Number.isFinite(count) && count > 0
    )
  )

const odataString = (value: string) => `'${value.replace(/'/g, "''")}'`

export const createSearchLocationsFromIdentifiers = (
  identifiers: string[]
): SearchLocation[] =>
  normalizeLocationIdentifiers(identifiers).map((locationIdentifier) => ({
    locationIdentifier,
  }))

export async function resolveSearchLocationsByIdentifier(
  identifiers: string[]
) {
  const normalizedIdentifiers = normalizeLocationIdentifiers(identifiers)

  if (normalizedIdentifiers.length === 0) return []

  const filter = normalizedIdentifiers
    .map((identifier) => `locationIdentifier eq ${odataString(identifier)}`)
    .join(' or ')
  // The config API returns an OData envelope even though the generated client
  // currently types this endpoint as a bare array.
  const response = (await getLocationLocationsForSearch({ filter })) as
    | SearchLocation[]
    | { value?: SearchLocation[] }
  const locations = Array.isArray(response) ? response : (response.value ?? [])
  const byIdentifier = new Map(
    locations.map((location) => [location.locationIdentifier, location])
  )

  return normalizedIdentifiers.map(
    (identifier) =>
      byIdentifier.get(identifier) ?? { locationIdentifier: identifier }
  )
}
