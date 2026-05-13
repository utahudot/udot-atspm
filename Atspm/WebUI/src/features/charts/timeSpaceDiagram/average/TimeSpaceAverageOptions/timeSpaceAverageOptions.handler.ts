import { LocationWithCoordPhases, LocationWithSequence } from '@/api/config'
import { ToolType } from '@/features/charts/common/types'
import type { TSBaseHandler } from '@/features/charts/timeSpaceDiagram/shared/options/timeSpaceBaseHandler'
import { TimeSpaceAverageOptions } from '@/features/charts/timeSpaceDiagram/shared/types'
import { Route, RouteLocation } from '@/features/routes/types'
import { DateTimeProps, TimeOnlyProps } from '@/types/TimeProps'
import {
  formatTime,
  formatUtcDateToYYYYMMDD,
  parseNum,
  parseTimeOfDayToDate,
  parseYYYYMMDDToUtcDate,
  safeJsonParse,
} from '@/utils/searchParams'
import { set, subDays, subMonths } from 'date-fns'
import type { ReadonlyURLSearchParams } from 'next/navigation'
import { useEffect, useMemo, useState } from 'react'

const defaultSequence: number[][] = [
  [1, 2, 3, 4],
  [5, 6, 7, 8],
]

const SEQUENCE_PARAM = 'sequence'
const COORD_PARAM = 'coordinatedPhases'

export interface TSAverageHandler
  extends TSBaseHandler,
    DateTimeProps,
    TimeOnlyProps {
  selectedDays: number[]
  updateDaysOfWeek(daysOfWeek: number[]): void

  routeLocationWithSequence: LocationWithSequence[]
  routeLocationWithCoordPhases: LocationWithCoordPhases[]
  updateLocationWithSequence(locationWithSequence: LocationWithSequence): void
  updateLocationWithCoordPhases(
    locationWithCoordPhases: LocationWithCoordPhases
  ): void

  routeLocationsForSelectedRoute: RouteLocation[]

  toOptions(): TimeSpaceAverageOptions
  applyFromOptions(options: Partial<TimeSpaceAverageOptions>): void

  toSearchParams(): URLSearchParams
  applyFromSearchParams(params: ReadonlyURLSearchParams): void

  resetToDefaults(): void
}

interface Props {
  routes: Route[]
}

export const useAverageOptionsHandler = ({
  routes,
}: Props): TSAverageHandler => {
  const yesterday = subDays(new Date(), 1)

  const defaultStartDate = useMemo(() => subMonths(yesterday, 1), [yesterday])
  const defaultEndDate = useMemo(() => yesterday, [yesterday])
  const defaultStartTime = useMemo(
    () =>
      set(new Date(), { hours: 16, minutes: 0, seconds: 0, milliseconds: 0 }),
    []
  )
  const defaultEndTime = useMemo(
    () =>
      set(new Date(), { hours: 16, minutes: 20, seconds: 0, milliseconds: 0 }),
    []
  )

  const [routeId, setRouteId] = useState('')
  const [speedLimit, setSpeedLimit] = useState<number | null>(null)

  const [startDate, setStartDate] = useState<Date>(defaultStartDate)
  const [endDate, setEndDate] = useState<Date>(defaultEndDate)
  const [startTime, setStartTime] = useState<Date>(defaultStartTime)
  const [endTime, setEndTime] = useState<Date>(defaultEndTime)

  const [routeLocationsForSelectedRoute, setRouteLocationsForSelectedRoute] =
    useState<RouteLocation[]>([])

  const [routeLocationWithSequence, setRouteLocationWithSequence] = useState<
    LocationWithSequence[]
  >([])
  const [routeLocationWithCoordPhases, setRouteLocationWithCoordPhases] =
    useState<LocationWithCoordPhases[]>([])

  const [selectedDays, setSelectedDays] = useState<number[]>([1, 2, 3, 4, 5])

  const resetToDefaults = () => {
    setSpeedLimit(null)
    setStartDate(defaultStartDate)
    setEndDate(defaultEndDate)
    setStartTime(defaultStartTime)
    setEndTime(defaultEndTime)
    setSelectedDays([1, 2, 3, 4, 5])
  }

  useEffect(() => {
    const route = routes.find((r) => r.id === Number.parseInt(routeId))
    if (!route) return

    setRouteLocationsForSelectedRoute(route.routeLocations)

    const locationToSequence: LocationWithSequence[] = []
    const locationWithCoordPhases: LocationWithCoordPhases[] = []

    route.routeLocations?.forEach((rl) => {
      locationToSequence.push({
        locationIdentifier: rl.locationIdentifier,
        sequence: defaultSequence,
      })
      locationWithCoordPhases.push({
        locationIdentifier: rl.locationIdentifier,
        coordinatedPhases: [
          Number.parseInt(rl.primaryPhase),
          Number.parseInt(rl.opposingPhase),
        ],
      })
    })

    setRouteLocationWithSequence(locationToSequence)
    setRouteLocationWithCoordPhases(locationWithCoordPhases)
  }, [routeId, routes])

  const applyFromOptions = (options: Partial<TimeSpaceAverageOptions>) => {
    if (options.routeId != null) setRouteId(String(options.routeId))
    if (options.speedLimit !== undefined)
      setSpeedLimit(options.speedLimit ?? null)

    if (typeof options.startDate === 'string') {
      const d = parseYYYYMMDDToUtcDate(options.startDate)
      if (d) setStartDate(d)
    }
    if (typeof options.endDate === 'string') {
      const d = parseYYYYMMDDToUtcDate(options.endDate)
      if (d) setEndDate(d)
    }

    if (typeof options.startTime === 'string') {
      const d = parseTimeOfDayToDate(options.startTime)
      if (d) setStartTime(d)
    }
    if (typeof options.endTime === 'string') {
      const d = parseTimeOfDayToDate(options.endTime)
      if (d) setEndTime(d)
    }

    if (Array.isArray(options.daysOfWeek)) setSelectedDays(options.daysOfWeek)

    if (Array.isArray(options.sequence))
      setRouteLocationWithSequence(options.sequence)
    if (Array.isArray(options.coordinatedPhases))
      setRouteLocationWithCoordPhases(options.coordinatedPhases)
  }

  const toOptions = (): TimeSpaceAverageOptions => ({
    startDate: formatUtcDateToYYYYMMDD(startDate),
    endDate: formatUtcDateToYYYYMMDD(endDate),
    startTime: formatTime(startTime),
    endTime: formatTime(endTime),

    routeId,
    speedLimit,

    daysOfWeek: selectedDays,
    sequence: routeLocationWithSequence,
    coordinatedPhases: routeLocationWithCoordPhases,
  })

  const toSearchParams = () => {
    const o = toOptions()
    const p = new URLSearchParams()

    p.set('toolType', String(ToolType.TimeSpaceAverage))
    p.set('routeId', o.routeId ?? '')

    if (o.speedLimit != null) p.set('speedLimit', String(o.speedLimit))

    if (o.startDate) p.set('startDate', o.startDate)
    if (o.endDate) p.set('endDate', o.endDate)

    if (o.startTime) p.set('startTime', o.startTime)
    if (o.endTime) p.set('endTime', o.endTime)

    if (Array.isArray(o.daysOfWeek)) {
      o.daysOfWeek.forEach((d) => p.append('daysOfWeek', String(d)))
    }

    if (o.sequence?.length) p.set(SEQUENCE_PARAM, JSON.stringify(o.sequence))
    if (o.coordinatedPhases?.length)
      p.set(COORD_PARAM, JSON.stringify(o.coordinatedPhases))

    return p
  }

  const applyFromSearchParams = (params: ReadonlyURLSearchParams) => {
    const toolType = params.get('toolType')
    if (toolType && toolType !== String(ToolType.TimeSpaceAverage)) return

    const nextRouteId = params.get('routeId') ?? ''
    const nextSpeedLimit = parseNum(params.get('speedLimit'))

    const nextStartDate = params.get('startDate')
    const nextEndDate = params.get('endDate')
    const nextStartTime = params.get('startTime')
    const nextEndTime = params.get('endTime')

    const days = params
      .getAll('daysOfWeek')
      .map((x) => Number(x))
      .filter((n) => Number.isFinite(n))

    const seq = safeJsonParse<LocationWithSequence[]>(
      params.get(SEQUENCE_PARAM)
    )
    const coord = safeJsonParse<LocationWithCoordPhases[]>(
      params.get(COORD_PARAM)
    )

    applyFromOptions({
      routeId: nextRouteId,
      speedLimit: nextSpeedLimit ?? undefined,
      startDate: nextStartDate ?? undefined,
      endDate: nextEndDate ?? undefined,
      startTime: nextStartTime ?? undefined,
      endTime: nextEndTime ?? undefined,
      daysOfWeek: days.length ? days : undefined,
      sequence: seq ?? undefined,
      coordinatedPhases: coord ?? undefined,
    } as Partial<TimeSpaceAverageOptions>)
  }

  return {
    routes,
    routeId,
    speedLimit,

    // DateTimeProps
    startDateTime: startDate,
    endDateTime: endDate,
    changeStartDate: (d: Date) => setStartDate(d),
    changeEndDate: (d: Date) => setEndDate(d),

    // TimeOnlyProps
    startTime,
    endTime,
    changeStartTime: (d: Date) => setStartTime(d),
    changeEndTime: (d: Date) => setEndTime(d),

    // base setters
    setRouteId: (id: string) => setRouteId(id),
    setSpeedLimit: (v: number | null) => setSpeedLimit(v),

    selectedDays,
    updateDaysOfWeek: (daysOfWeek: number[]) => setSelectedDays(daysOfWeek),

    routeLocationsForSelectedRoute,
    routeLocationWithSequence,
    routeLocationWithCoordPhases,

    updateLocationWithSequence: (
      locationWithSequence: LocationWithSequence
    ) => {
      setRouteLocationWithSequence((prev) =>
        prev.map((item) =>
          item.locationIdentifier === locationWithSequence.locationIdentifier
            ? { ...item, sequence: locationWithSequence.sequence }
            : item
        )
      )
    },

    updateLocationWithCoordPhases: (
      locationWithCoordPhases: LocationWithCoordPhases
    ) => {
      setRouteLocationWithCoordPhases((prev) =>
        prev.map((item) =>
          item.locationIdentifier === locationWithCoordPhases.locationIdentifier
            ? {
                ...item,
                coordinatedPhases: locationWithCoordPhases.coordinatedPhases,
              }
            : item
        )
      )
    },

    toOptions,
    applyFromOptions,
    toSearchParams,
    applyFromSearchParams,
    resetToDefaults,
  }
}
