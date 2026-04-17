import { ToolType } from '@/features/charts/common/types'
import { TimeSpaceHistoricOptions } from '@/features/charts/timeSpaceDiagram/shared/types'
import { Route } from '@/features/routes/types'
import { DateTimeProps } from '@/types/TimeProps'
import { parseBool, parseDate, parseNum } from '@/utils/searchParams'
import { set, subDays } from 'date-fns'
import type { ReadonlyURLSearchParams } from 'next/navigation'
import { useMemo, useState } from 'react'

export interface TSBaseHandler {
  routes: Route[]
  routeId: string
  speedLimit: number | null

  setRouteId(routeId: string): void
  setSpeedLimit(speedLimit: number | null): void
}

export interface TSHistoricHandler extends TSBaseHandler, DateTimeProps {
  // state <-> request payload
  toOptions(): TimeSpaceHistoricOptions
  applyFromOptions(options: Partial<TimeSpaceHistoricOptions>): void

  // state <-> URL
  toSearchParams(): URLSearchParams
  applyFromSearchParams(params: ReadonlyURLSearchParams): void

  resetToDefaults(): void
}

interface Props {
  routes: Route[]
}

export const useHistoricOptionsHandler = ({
  routes,
}: Props): TSHistoricHandler => {
  const yesterday = subDays(new Date(), 1)

  const defaultStart = useMemo(
    () =>
      set(yesterday, {
        hours: 16,
        minutes: 0,
        seconds: 0,
        milliseconds: 0,
      }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  )
  const defaultEnd = useMemo(
    () =>
      set(yesterday, {
        hours: 16,
        minutes: 20,
        seconds: 0,
        milliseconds: 0,
      }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  )

  const [routeId, setRouteId] = useState('')
  const [speedLimit, setSpeedLimit] = useState<number | null>(null)

  const [startDateTime, setStartDateTime] = useState<Date>(defaultStart)
  const [endDateTime, setEndDateTime] = useState<Date>(defaultEnd)

  // optional historic-only fields (if your UI supports them)
  const [extendStartStopSearch, setExtendStartStopSearch] = useState<number>(2)
  const [showAllLanesInfo, setShowAllLanesInfo] = useState<boolean>(true)
  const [chartType, setChartType] = useState<string>('')
  const [locationIdentifier, setLocationIdentifier] = useState<string>('')

  const resetToDefaults = () => {
    setSpeedLimit(null)
    setStartDateTime(defaultStart)
    setEndDateTime(defaultEnd)
    setExtendStartStopSearch(2)
    setShowAllLanesInfo(true)
    setChartType('')
    setLocationIdentifier('')
  }

  const applyFromOptions = (options: Partial<TimeSpaceHistoricOptions>) => {
    if (options.routeId != null) setRouteId(String(options.routeId))
    if (options.speedLimit !== undefined)
      setSpeedLimit(options.speedLimit ?? null)

    if (options.extendStartStopSearch != null)
      setExtendStartStopSearch(options.extendStartStopSearch)
    if (options.showAllLanesInfo != null)
      setShowAllLanesInfo(options.showAllLanesInfo)

    if (typeof options.chartType === 'string') setChartType(options.chartType)
    if (typeof options.locationIdentifier === 'string')
      setLocationIdentifier(options.locationIdentifier)

    if (options.start instanceof Date) setStartDateTime(options.start)
    if (options.end instanceof Date) setEndDateTime(options.end)

    // tolerate string inputs (ISO)
    if (typeof options.start === 'string') {
      const d = new Date(options.start)
      if (!Number.isNaN(d.getTime())) setStartDateTime(d)
    }
    if (typeof options.end === 'string') {
      const d = new Date(options.end)
      if (!Number.isNaN(d.getTime())) setEndDateTime(d)
    }
  }

  const toOptions = (): TimeSpaceHistoricOptions => ({
    extendStartStopSearch,
    showAllLanesInfo,
    start: startDateTime,
    end: endDateTime,
    routeId,
    chartType,
    speedLimit,
    locationIdentifier,
  })

  const toSearchParams = () => {
    const o = toOptions()
    const p = new URLSearchParams()

    p.set('toolType', String(ToolType.TimeSpaceHistoric))
    p.set('routeId', o.routeId ?? '')

    if (o.speedLimit != null) p.set('speedLimit', String(o.speedLimit))

    if (o.start instanceof Date) p.set('start', o.start.toISOString())
    if (o.end instanceof Date) p.set('end', o.end.toISOString())

    // include these so links reproduce exactly
    if (o.extendStartStopSearch != null)
      p.set('extendStartStopSearch', String(o.extendStartStopSearch))
    if (o.showAllLanesInfo != null)
      p.set('showAllLanesInfo', String(o.showAllLanesInfo))
    if (o.chartType) p.set('chartType', o.chartType)
    if (o.locationIdentifier) p.set('locationIdentifier', o.locationIdentifier)

    return p
  }

  const applyFromSearchParams = (params: ReadonlyURLSearchParams) => {
    // Only apply if URL is for this toolType OR toolType is absent (back compat)
    const toolType = params.get('toolType')
    if (toolType && toolType !== String(ToolType.TimeSpaceHistoric)) return

    const nextRouteId = params.get('routeId') ?? ''
    const nextSpeedLimit = parseNum(params.get('speedLimit'))

    const nextStart = parseDate(params.get('start'))
    const nextEnd = parseDate(params.get('end'))

    const nextExtend = parseNum(params.get('extendStartStopSearch'))
    const nextShowAll = parseBool(params.get('showAllLanesInfo'))

    const nextChartType = params.get('chartType')
    const nextLocationIdentifier = params.get('locationIdentifier')

    applyFromOptions({
      routeId: nextRouteId,
      speedLimit: nextSpeedLimit ?? undefined,
      start: nextStart ?? undefined,
      end: nextEnd ?? undefined,
      extendStartStopSearch: nextExtend ?? undefined,
      showAllLanesInfo: nextShowAll ?? undefined,
      chartType: nextChartType ?? undefined,
      locationIdentifier: nextLocationIdentifier ?? undefined,
    } as Partial<TimeSpaceHistoricOptions>)
  }

  return {
    routes,
    routeId,
    speedLimit,

    // DateTimeProps
    startDateTime,
    endDateTime,
    changeStartDate: (d: Date) => setStartDateTime(d),
    changeEndDate: (d: Date) => setEndDateTime(d),

    // base setters
    setRouteId: (id: string) => setRouteId(id),
    setSpeedLimit: (v: number | null) => setSpeedLimit(v),

    toOptions,
    applyFromOptions,
    toSearchParams,
    applyFromSearchParams,
    resetToDefaults,
  }
}
