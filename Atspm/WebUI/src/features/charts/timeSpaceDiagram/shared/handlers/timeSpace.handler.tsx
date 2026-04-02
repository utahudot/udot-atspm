import { dateToTimestamp } from '@/utils/dateTime'
import { TIME_SPACE_LOCATION_AXIS_SERIES_ID } from '../transformers/timeSpaceTransformerBase'
import { ECharts, EChartsOption, SeriesOption } from 'echarts'
import { useEffect, useRef } from 'react'

const HIT_TOLERANCE = 10
const STEP_MS = 1000

type SeriesKind =
  | 'point'
  | 'point-with-gaps'
  | 'range'
  | 'offset'
  | 'location-axis'

const SERIES_ID_PREFIXES = [
  'Cycle Duration Labels',
  'Green Bands',
  'Left Turn',
  'Right Turn',
  'TSP Request',
  'TSP Service',
  'Early Green',
  'Extend Green',
  'Cycles',
  'LLC',
  'AC',
  'SBP',
  'SRM',
  'PI',
  'Offset',
]

const POINT_SERIES_PREFIXES = [
  'Cycle Duration Labels',
  'Green Bands',
  'SBP',
  'SRM',
  'Early Green',
  'Extend Green',
]

const POINT_WITH_GAPS_SERIES_PREFIXES = [
  'LLC',
  'AC',
  'Left Turn',
  'Right Turn',
  'TSP Request',
  'TSP Service',
]

const RANGE_SERIES_PREFIXES = ['PI']

function hasSeriesPrefix(id: string, prefix: string) {
  return id === prefix || id.startsWith(`${prefix} `)
}

function isSeriesOption(
  value: SeriesOption | null | undefined
): value is SeriesOption {
  return Boolean(value && typeof value === 'object')
}

function shiftTimeStr(t: string, offsetMs: number): string {
  const time = new Date(t).getTime()
  return dateToTimestamp(new Date(time + offsetMs))
}

function stripCategory(id: string) {
  const prefix = SERIES_ID_PREFIXES.find((candidate) =>
    hasSeriesPrefix(id, candidate)
  )

  if (!prefix) {
    return id.trim()
  }

  return id.slice(prefix.length).trim()
}

function getGroupKeyFromSeriesId(id: string) {
  const rest = stripCategory(id)
  const m = rest.match(/\b\d+\b/)
  if (m) return m[0]
  return rest.trim()
}

const getAllSeries = (chart: ECharts) => {
  const options = chart.getOption() as EChartsOption

  if (!options?.series) {
    return {
      base: [],
      points: [],
      pointWithGaps: [],
      ranges: [],
      offset: [],
      locationAxis: [],
    }
  }

  const rawSeries = Array.isArray(options.series) ? options.series : [options.series]
  const series = rawSeries.filter(isSeriesOption)
  const withId = series.filter((s) => typeof s.id === 'string')

  return {
    base: withId.filter((s) => hasSeriesPrefix(String(s.id), 'Cycles')),
    points: withId.filter((s) =>
      POINT_SERIES_PREFIXES.some((prefix) =>
        hasSeriesPrefix(String(s.id), prefix)
      )
    ),
    pointWithGaps: withId.filter((s) =>
      POINT_WITH_GAPS_SERIES_PREFIXES.some((prefix) =>
        hasSeriesPrefix(String(s.id), prefix)
      )
    ),
    ranges: withId.filter((s) =>
      RANGE_SERIES_PREFIXES.some((prefix) =>
        hasSeriesPrefix(String(s.id), prefix)
      )
    ),
    offset: withId.filter((s) => hasSeriesPrefix(String(s.id), 'Offset')),
    locationAxis: withId.filter(
      (s) => s.id === TIME_SPACE_LOCATION_AXIS_SERIES_ID
    ),
  }
}

const findClosestGroup = (
  chart: any,
  baseSeries: SeriesOption[],
  mouseY: number
) => {
  let closest: { groupKey: string; distance: number } | null = null

  for (const s of baseSeries) {
    const id = String(s.id ?? '')
    const yValue = (s.data as any[])?.[0]?.[1]
    if (yValue == null) continue

    const [, yPixel] = chart.convertToPixel({ xAxisIndex: 0, yAxisIndex: 0 }, [
      0,
      yValue,
    ])
    const distance = Math.abs(mouseY - yPixel)

    if (distance < HIT_TOLERANCE && (!closest || distance < closest.distance)) {
      closest = { groupKey: getGroupKeyFromSeriesId(id), distance }
    }
  }

  return closest
}

function shiftTimeLike(value: unknown, offsetMs: number) {
  if (typeof value === 'number' && Number.isFinite(value)) {
    return value + offsetMs
  }

  if (typeof value === 'string') {
    return shiftTimeStr(value, offsetMs)
  }

  return value
}

function shiftPointDatum(datum: any[], offsetMs: number) {
  return [shiftTimeLike(datum[0], offsetMs), ...datum.slice(1)]
}

function buildShiftedData(kind: SeriesKind, original: any[], offsetMs: number) {
  switch (kind) {
    case 'point':
      return original.map((d) => shiftPointDatum(d, offsetMs))
    case 'point-with-gaps':
      return original.map((d) => (d === null ? null : shiftPointDatum(d, offsetMs)))
    case 'range':
      return original.map((d) => [
        shiftTimeLike(d[0], offsetMs),
        shiftTimeLike(d[1], offsetMs),
        ...d.slice(2),
      ])
    case 'offset':
      return original.map((d) => [d[0], d[1], d[2], offsetMs / 1000])
    case 'location-axis':
      return original
    default:
      return original
  }
}

function buildLocationAxisOffsetData(
  original: any[],
  offsetsByGroup: Record<string, number>
) {
  return original.map((datum) => {
    if (!Array.isArray(datum)) {
      return datum
    }

    const nextDatum = [...datum]
    const locationId = String(nextDatum[2] ?? '')
    const baseOffsetSeconds =
      typeof nextDatum[5] === 'number' && Number.isFinite(nextDatum[5])
        ? nextDatum[5]
        : 0
    const dragOffsetSeconds = (offsetsByGroup[locationId] ?? 0) / 1000

    nextDatum[5] = baseOffsetSeconds + dragOffsetSeconds
    return nextDatum
  })
}

export const useTimeSpaceHandler = (chart: ECharts | null, syncVersion = 0) => {
  const draggingRef = useRef(false)
  const draggingGroupKeyRef = useRef<string | null>(null)
  const lastXRef = useRef<number | null>(null)

  const offsetsByGroupRef = useRef<Record<string, number>>({})
  const originalDataByIdRef = useRef<Record<string, any[]>>({})
  const kindByIdRef = useRef<Record<string, SeriesKind>>({})

  useEffect(() => {
    if (!chart) return

    const zr = chart.getZr()

    const resetRefs = () => {
      draggingRef.current = false
      draggingGroupKeyRef.current = null
      lastXRef.current = null

      // This is the big one: prevents “jump back” after restore
      offsetsByGroupRef.current = {}
    }

    const captureOriginal = () => {
      const { base, points, pointWithGaps, ranges, offset, locationAxis } =
        getAllSeries(chart)

      const put = (arr: SeriesOption[], kind: SeriesKind) => {
        for (const s of arr) {
          const id = String(s.id ?? '')
          if (!id) continue

          // Always recapture everything when we call captureOriginal()
          originalDataByIdRef.current[id] = Array.isArray(s.data)
            ? (s.data as any[]).map((d) => (Array.isArray(d) ? [...d] : d))
            : []

          kindByIdRef.current[id] = kind
        }
      }

      put(base, 'point')
      put(points, 'point')
      put(pointWithGaps, 'point-with-gaps')
      put(ranges, 'range')
      put(offset, 'offset')
      put(locationAxis, 'location-axis')

      return { base }
    }

    const recaptureAll = () => {
      // wipe caches so we don’t shift from stale “originals”
      originalDataByIdRef.current = {}
      kindByIdRef.current = {}
      resetRefs()
      captureOriginal()
    }

    originalDataByIdRef.current = {}
    kindByIdRef.current = {}
    resetRefs()

    // initial capture
    const { base } = captureOriginal()
    if (!base.length) return

    const updateGroupFromOriginal = (groupKey: string, offsetMs: number) => {
      const rounded = Math.round(offsetMs / STEP_MS) * STEP_MS
      const updates: Array<{ id: string; data: any[] }> = []

      for (const [id, original] of Object.entries(
        originalDataByIdRef.current
      )) {
        const kind = kindByIdRef.current[id]
        if (!kind) continue

        if (kind === 'location-axis') {
          updates.push({
            id,
            data: buildLocationAxisOffsetData(original, offsetsByGroupRef.current),
          })
          continue
        }

        if (getGroupKeyFromSeriesId(id) !== groupKey) continue

        updates.push({ id, data: buildShiftedData(kind, original, rounded) })
      }

      if (!updates.length) return

      chart.setOption({ series: updates }, false, false)
    }

    const onMouseDown = (e: any) => {
      const [xData] = chart.convertFromPixel({ xAxisIndex: 0, yAxisIndex: 0 }, [
        e.offsetX,
        e.offsetY,
      ])
      if (xData == null) return

      const latestSeries = Array.isArray(chart.getOption()?.series)
        ? (chart.getOption()?.series as Array<SeriesOption | null | undefined>).filter(
            isSeriesOption
          )
        : []
      const latestBase = latestSeries.filter(
        (s) => typeof s.id === 'string' && s.id.includes('Cycles')
      )
      const activeBase = latestBase.length ? latestBase : base

      const closest = findClosestGroup(chart, activeBase, e.offsetY)
      if (!closest) return

      draggingRef.current = true
      draggingGroupKeyRef.current = closest.groupKey
      lastXRef.current = Number(xData)

      offsetsByGroupRef.current[closest.groupKey] ??= 0
    }

    const onMouseMove = (e: any) => {
      if (
        !draggingRef.current ||
        !draggingGroupKeyRef.current ||
        lastXRef.current == null
      )
        return

      const [xData] = chart.convertFromPixel({ xAxisIndex: 0, yAxisIndex: 0 }, [
        e.offsetX,
        e.offsetY,
      ])
      if (xData == null) return

      const dx = Number(xData) - lastXRef.current
      lastXRef.current = Number(xData)

      const snappedDx = Math.round(dx / STEP_MS) * STEP_MS

      const key = draggingGroupKeyRef.current
      offsetsByGroupRef.current[key] =
        (offsetsByGroupRef.current[key] ?? 0) + snappedDx
      offsetsByGroupRef.current[key] =
        Math.round(offsetsByGroupRef.current[key] / STEP_MS) * STEP_MS

      updateGroupFromOriginal(key, offsetsByGroupRef.current[key])
    }

    const onMouseUp = () => {
      draggingRef.current = false
      draggingGroupKeyRef.current = null
      lastXRef.current = null
    }

    // 🔥 handle toolbox restore
    const onRestore = () => {
      // restore resets series/zoom/etc — we need to reset our refs too
      recaptureAll()
    }

    // Optional but recommended:
    // if something else calls setOption with full new data, recapture after render completes
    // so “originals” match the new chart state.
    const onFinished = () => {
      // don’t trash offsets on every finished; only recapture originals if you need it
      // If your app replaces the whole chart option often, uncomment:
      // captureOriginal()
    }

    zr.on('mousedown', onMouseDown)
    zr.on('mousemove', onMouseMove)
    zr.on('mouseup', onMouseUp)

    chart.on('restore', onRestore)
    chart.on('finished', onFinished)

    return () => {
      zr.off('mousedown', onMouseDown)
      zr.off('mousemove', onMouseMove)
      zr.off('mouseup', onMouseUp)

      chart.off('restore', onRestore)
      chart.off('finished', onFinished)
    }
  }, [chart, syncVersion])
}
