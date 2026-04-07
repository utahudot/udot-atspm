import { dateToTimestamp } from '@/utils/dateTime'
import {
  formatSignedOffsetSeconds,
  getTimeSpaceLocationOffsetBadgeLayout,
  TIME_SPACE_LOCATION_AXIS_SERIES_ID,
} from '../transformers/timeSpaceTransformerBase'
import { ECharts, EChartsOption, SeriesOption } from 'echarts'
import { useEffect, useRef } from 'react'

const HIT_TOLERANCE = 10
const STEP_MS = 1000
const DEFAULT_CYCLE_DRAG_LIMIT_MS = (180 - 1) * STEP_MS

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

function getGroupKey(value: string) {
  const trimmed = value.trim()
  const m = trimmed.match(/\b\d+\b/)
  if (m) return m[0]
  return trimmed
}

function getGroupKeyFromSeriesId(id: string) {
  return getGroupKey(stripCategory(id))
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
    const locationId = getGroupKey(String(nextDatum[2] ?? ''))
    const baseOffsetSeconds =
      typeof nextDatum[5] === 'number' && Number.isFinite(nextDatum[5])
        ? nextDatum[5]
        : 0
    const dragOffsetSeconds = (offsetsByGroup[locationId] ?? 0) / 1000

    nextDatum[5] = baseOffsetSeconds + dragOffsetSeconds
    return nextDatum
  })
}

function getCycleDragLimitMs(cycleLengthValue: unknown) {
  const cycleLengthSeconds =
    typeof cycleLengthValue === 'number'
      ? cycleLengthValue
      : Number(cycleLengthValue)

  if (!Number.isFinite(cycleLengthSeconds) || cycleLengthSeconds <= 0) {
    return DEFAULT_CYCLE_DRAG_LIMIT_MS
  }

  return Math.max(0, (Math.ceil(cycleLengthSeconds) - 1) * STEP_MS)
}

function clampOffsetToCycleLength(
  offsetMs: number,
  cycleDragLimitMs: number | undefined
) {
  if (!Number.isFinite(cycleDragLimitMs)) {
    return clampOffsetToCycleLength(offsetMs, DEFAULT_CYCLE_DRAG_LIMIT_MS)
  }

  return Math.max(-cycleDragLimitMs, Math.min(cycleDragLimitMs, offsetMs))
}

function getPrimaryGridLeft(option: EChartsOption) {
  const grid = Array.isArray(option.grid) ? option.grid[0] : option.grid

  if (typeof grid?.left === 'number' && Number.isFinite(grid.left)) {
    return grid.left
  }

  if (typeof grid?.left === 'string') {
    const parsed = Number.parseFloat(grid.left)
    if (Number.isFinite(parsed)) {
      return parsed
    }
  }

  return 0
}

function getSeriesDatumValue(datum: unknown) {
  if (Array.isArray(datum)) {
    return datum
  }

  if (
    datum &&
    typeof datum === 'object' &&
    Array.isArray((datum as { value?: unknown[] }).value)
  ) {
    return (datum as { value: unknown[] }).value
  }

  return null
}

export const useTimeSpaceHandler = (chart: ECharts | null, syncVersion = 0) => {
  const draggingRef = useRef(false)
  const draggingGroupKeyRef = useRef<string | null>(null)
  const lastXRef = useRef<number | null>(null)

  const offsetsByGroupRef = useRef<Record<string, number>>({})
  const cycleDragLimitByGroupRef = useRef<Record<string, number>>({})
  const originalDataByIdRef = useRef<Record<string, any[]>>({})
  const kindByIdRef = useRef<Record<string, SeriesKind>>({})

  useEffect(() => {
    if (!chart) return

    let zrRef: ReturnType<ECharts['getZr']> | null = null
    let zrRetryRafId = 0

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
      cycleDragLimitByGroupRef.current = {}

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

      for (const series of locationAxis) {
        const data = Array.isArray(series.data) ? series.data : []

        for (const datum of data) {
          if (!Array.isArray(datum)) continue

          const groupKey = getGroupKey(String(datum[2] ?? ''))
          if (!groupKey) continue

          const cycleDragLimitMs = getCycleDragLimitMs(datum[4])
          if (cycleDragLimitMs == null) continue

          cycleDragLimitByGroupRef.current[groupKey] = cycleDragLimitMs
        }
      }

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

    const updateGroupFromOriginal = (
      groupKey: string,
      offsetMs: number,
      options?: { snapToStep?: boolean }
    ) => {
      const appliedOffsetMs =
        options?.snapToStep === false
          ? offsetMs
          : Math.round(offsetMs / STEP_MS) * STEP_MS
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

        updates.push({
          id,
          data: buildShiftedData(kind, original, appliedOffsetMs),
        })
      }

      if (!updates.length) return

      chart.setOption({ series: updates }, false, false)
    }

    const getOriginalOffsetSeconds = (groupKey: string) => {
      const locationAxisData =
        originalDataByIdRef.current[TIME_SPACE_LOCATION_AXIS_SERIES_ID] ?? []
      const matchingDatum = locationAxisData.find((datum) => {
        if (!Array.isArray(datum)) {
          return false
        }

        return getGroupKey(String(datum[2] ?? '')) === groupKey
      })
      const baseOffsetSeconds =
        typeof matchingDatum?.[5] === 'number'
          ? matchingDatum[5]
          : Number(matchingDatum?.[5])

      return Number.isFinite(baseOffsetSeconds) ? baseOffsetSeconds : 0
    }

    const resetGroupOffsetToZero = (groupKey: string) => {
      const targetOffsetMs = -getOriginalOffsetSeconds(groupKey) * STEP_MS
      const clampedOffsetMs = clampOffsetToCycleLength(
        targetOffsetMs,
        cycleDragLimitByGroupRef.current[groupKey]
      )

      offsetsByGroupRef.current[groupKey] = clampedOffsetMs
      updateGroupFromOriginal(groupKey, clampedOffsetMs, { snapToStep: false })
    }

    const findOffsetResetTarget = (mouseX: number, mouseY: number) => {
      const option = chart.getOption() as EChartsOption
      const gridLeft = getPrimaryGridLeft(option)
      const locationAxisSeries = getAllSeries(chart).locationAxis[0]
      const locationAxisData = Array.isArray(locationAxisSeries?.data)
        ? locationAxisSeries.data
        : []

      for (const datum of locationAxisData) {
        const value = getSeriesDatumValue(datum)
        if (!value) continue

        const offsetValue =
          typeof value[5] === 'number' ? value[5] : Number(value[5])
        if (!Number.isFinite(offsetValue) || offsetValue === 0) {
          continue
        }

        const distanceValue =
          typeof value[1] === 'number' ? value[1] : Number(value[1])
        if (!Number.isFinite(distanceValue)) {
          continue
        }

        const pixel = chart.convertToPixel(
          { xAxisIndex: 0, yAxisIndex: 0 },
          [value[0], distanceValue]
        )
        if (
          !Array.isArray(pixel) ||
          pixel.length < 2 ||
          !Number.isFinite(pixel[1])
        ) {
          continue
        }

        const badgeLayout = getTimeSpaceLocationOffsetBadgeLayout(
          gridLeft,
          pixel[1],
          formatSignedOffsetSeconds(offsetValue),
          false
        )
        const withinHighlightBounds =
          mouseX >= badgeLayout.highlightX &&
          mouseX <= badgeLayout.highlightX + badgeLayout.highlightWidth &&
          mouseY >= badgeLayout.highlightY &&
          mouseY <= badgeLayout.highlightY + badgeLayout.highlightHeight

        if (withinHighlightBounds) {
          const groupKey = getGroupKey(String(value[2] ?? ''))
          if (groupKey) {
            return groupKey
          }
        }
      }

      return null
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
      const snappedDx = Math.round(dx / STEP_MS) * STEP_MS

      const key = draggingGroupKeyRef.current
      const previousOffsetMs = offsetsByGroupRef.current[key] ?? 0
      const nextOffsetMs = Math.round(
        clampOffsetToCycleLength(
          previousOffsetMs + snappedDx,
          cycleDragLimitByGroupRef.current[key]
        ) / STEP_MS
      ) * STEP_MS

      if (nextOffsetMs !== previousOffsetMs + snappedDx) {
        lastXRef.current = Number(xData) - (previousOffsetMs + snappedDx - nextOffsetMs)
      } else {
        lastXRef.current = Number(xData)
      }

      offsetsByGroupRef.current[key] = nextOffsetMs

      updateGroupFromOriginal(key, offsetsByGroupRef.current[key])
    }

    const onMouseUp = () => {
      draggingRef.current = false
      draggingGroupKeyRef.current = null
      lastXRef.current = null
    }

    const onDoubleClick = (e: any) => {
      const targetGroupKey = findOffsetResetTarget(e.offsetX, e.offsetY)
      if (!targetGroupKey) {
        return
      }

      onMouseUp()
      resetGroupOffsetToZero(targetGroupKey)
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

    const attachZrListeners = () => {
      const zr = chart.getZr?.() ?? null

      if (!zr) {
        zrRetryRafId = window.requestAnimationFrame(attachZrListeners)
        return
      }

      zrRef = zr
      zr.on('mousedown', onMouseDown)
      zr.on('mousemove', onMouseMove)
      zr.on('mouseup', onMouseUp)
      zr.on('dblclick', onDoubleClick)
    }

    attachZrListeners()

    chart.on('restore', onRestore)
    chart.on('finished', onFinished)

    return () => {
      window.cancelAnimationFrame(zrRetryRafId)
      zrRef?.off('mousedown', onMouseDown)
      zrRef?.off('mousemove', onMouseMove)
      zrRef?.off('mouseup', onMouseUp)
      zrRef?.off('dblclick', onDoubleClick)

      chart.off('restore', onRestore)
      chart.off('finished', onFinished)
    }
  }, [chart, syncVersion])
}
