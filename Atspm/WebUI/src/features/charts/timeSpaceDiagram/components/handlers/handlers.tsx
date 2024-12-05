import { Route, RouteLocation } from '@/features/routes/types'
import { DateTimeProps, TimeOnlyProps } from '@/types/TimeProps'
import { set, subDays, subMonths } from 'date-fns'
import { useEffect, useState } from 'react'
import {
  LocationWithCoordPhases,
  LocationWithSequence,
  TimeSpaceAverageOptions,
  TimeSpaceHistoricOptions,
} from '../../types'

interface TSContainerPresenterProps {
  routes: Route[]
  handleToolOptions: (
    options: Partial<TimeSpaceAverageOptions | TimeSpaceHistoricOptions>
  ) => void
  routeId: string
  setRouteId: (routeId: string) => void
}
const defaultSequence = [
  [1, 2, 3, 4],
  [5, 6, 7, 8],
]

export interface TSBaseHandler {
  routes: Route[]
  routeId: string
  setRouteId(routeId: string): void
  setSpeedLimit(speedLimit: number): void
}

export interface TSHistoricHandler extends TSBaseHandler, DateTimeProps {}

export interface TSAverageHandler
  extends TSBaseHandler,
    DateTimeProps,
    TimeOnlyProps {
  selectedDays: number[]
  updateDaysOfWeek(daysOfWeek: number[]): void
  routeLocationWithSequence: LocationWithSequence[]
  routeLocationWithCoordPhases: LocationWithCoordPhases[]
  updateLocationWithCoordPhases(
    locationWithCoordPhases: LocationWithCoordPhases
  ): void
  updateLocationWithSequence(locationWithSequence: LocationWithSequence): void
  routeLocationsForSelectedRoute: RouteLocation[]
}

export const useHistoricOptionsHandler = ({
  routes,
  handleToolOptions,
  routeId,
  setRouteId,
}: TSContainerPresenterProps) => {
  const yesterday = subDays(new Date(), 1)
  const [startDateTime, setStartDateTime] = useState(
    set(yesterday, { hours: 16, minutes: 0 })
  )
  const [endDateTime, setEndDateTime] = useState(
    set(yesterday, { hours: 16, minutes: 20 })
  )

  const componentHandler: TSHistoricHandler = {
    startDateTime,
    endDateTime,
    routeId,
    routes,
    setSpeedLimit: (speedLimit: number) => {
      handleToolOptions({ speedLimit })
    },
    setRouteId: (newRouteId: string) => {
      handleToolOptions({ routeId: newRouteId })
      setRouteId(newRouteId)
    },
    changeStartDate: (date: Date) => {
      handleToolOptions({ start: date })
      setStartDateTime(date)
    },
    changeEndDate: (date: Date) => {
      handleToolOptions({ end: date })
      setEndDateTime(date)
    },
  }

  return componentHandler
}

export const useAverageOptionsHandler = ({
  routes,
  handleToolOptions,
  routeId,
  setRouteId,
}: TSContainerPresenterProps) => {
  const yesterday = subDays(new Date(), 1)
  const [startDate, setStartDate] = useState(subMonths(yesterday, 1))
  const [endDate, setEndDate] = useState(yesterday)
  const [startTime, setStartTime] = useState(
    set(new Date(), { hours: 16, minutes: 0 })
  )
  const [endTime, setEndTime] = useState(
    set(new Date(), { hours: 16, minutes: 20 })
  )
  const [routeLocationsForSelectedRoute, setRouteLocationsForSelectedRoute] =
    useState<RouteLocation[]>([])
  const [routeLocationWithSequence, setRouteLocationWithSequence] = useState<
    LocationWithSequence[]
  >([])
  const [routeLocationWithCoordPhases, setRouteLocationWithCoordPhases] =
    useState<LocationWithCoordPhases[]>([])
  const [selectedDays, setSelectedDays] = useState<number[]>([1, 2, 3, 4, 5])

  useEffect(() => {
    const route = routes.find((route) => route.id === Number.parseInt(routeId))
    if (route !== undefined) {
      setRouteLocationsForSelectedRoute(route.routeLocations)
      const locationToSequence: LocationWithSequence[] = []
      const locationWithCoordPhases: LocationWithCoordPhases[] = []
      route.routeLocations?.forEach((routeLocation) => {
        locationToSequence.push({
          locationIdentifier: routeLocation.locationIdentifier,
          sequence: defaultSequence,
        })
        locationWithCoordPhases.push({
          locationIdentifier: routeLocation.locationIdentifier,
          coordinatedPhases: [
            Number.parseInt(routeLocation.primaryPhase),
            Number.parseInt(routeLocation.opposingPhase),
          ],
        })
      })

      setRouteLocationWithSequence(locationToSequence)
      setRouteLocationWithCoordPhases(locationWithCoordPhases)
      handleToolOptions({ sequence: locationToSequence })
      handleToolOptions({ coordinatedPhases: locationWithCoordPhases })
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [routeId, routes])

  const componentHandler: TSAverageHandler = {
    routeLocationWithCoordPhases,
    routeLocationWithSequence,
    selectedDays,
    routes,
    startDateTime: startDate,
    endDateTime: endDate,
    startTime,
    endTime,
    routeId,
    routeLocationsForSelectedRoute,
    changeStartDate: (date: Date) => {
      const year = date.getUTCFullYear()
      const month = String(date.getUTCMonth() + 1).padStart(2, '0')
      const day = String(date.getUTCDate()).padStart(2, '0')

      const formattedDate = `${year}-${month}-${day}`
      handleToolOptions({ startDate: formattedDate })
      setStartDate(date)
    },
    changeEndDate: (date: Date) => {
      const year = date.getUTCFullYear()
      const month = String(date.getUTCMonth() + 1).padStart(2, '0')
      const day = String(date.getUTCDate()).padStart(2, '0')

      const formattedDate = `${year}-${month}-${day}`
      handleToolOptions({ endDate: formattedDate })
      setEndDate(date)
    },
    changeStartTime: (date: Date) => {
      const options: Intl.DateTimeFormatOptions = {
        hour12: false,
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
      }

      const formattedTime = date.toLocaleString('en-US', options)
      handleToolOptions({ startTime: formattedTime })
      setStartTime(date)
    },
    changeEndTime: (date: Date) => {
      const options: Intl.DateTimeFormatOptions = {
        hour12: false,
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
      }

      const formattedTime = date.toLocaleString('en-US', options)
      handleToolOptions({ endTime: formattedTime })
      setEndTime(date)
    },
    setSpeedLimit: (speedLimit: number) => {
      handleToolOptions({ speedLimit })
    },
    setRouteId: (newRouteId: string) => {
      handleToolOptions({ routeId: newRouteId })
      setRouteId(newRouteId)
    },
    updateDaysOfWeek: (daysOfWeek: number[]) => {
      handleToolOptions({ daysOfWeek })
      setSelectedDays(daysOfWeek)
    },
    updateLocationWithSequence: (
      locationWithSequence: LocationWithSequence
    ) => {
      const newLocationWithSequences = routeLocationWithSequence.map((item) =>
        item.locationIdentifier === locationWithSequence.locationIdentifier
          ? { ...item, sequence: locationWithSequence.sequence }
          : item
      )
      setRouteLocationWithSequence(newLocationWithSequences)
      handleToolOptions({ sequence: newLocationWithSequences })
    },
    updateLocationWithCoordPhases: (
      locationWithCoordPhases: LocationWithCoordPhases
    ) => {
      const newLocationWithCoordPhases = routeLocationWithCoordPhases.map(
        (item) =>
          item.locationIdentifier === locationWithCoordPhases.locationIdentifier
            ? {
                ...item,
                coordinatedPhases: locationWithCoordPhases.coordinatedPhases,
              }
            : item
      )
      setRouteLocationWithCoordPhases(newLocationWithCoordPhases)
      handleToolOptions({ coordinatedPhases: newLocationWithCoordPhases })
    },
  }

  return componentHandler
}
