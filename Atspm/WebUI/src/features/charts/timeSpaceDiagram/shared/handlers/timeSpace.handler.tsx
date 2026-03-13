import { dateToTimestamp } from '@/utils/dateTime'
import { ECharts, EChartsOption, SeriesOption } from 'echarts'
import { useEffect, useRef } from 'react'

const HIT_TOLERANCE = 10
const STEP_MS = 1000

type SeriesKind =
  | 'base'
  | 'cycle-duration'
  | 'band'
  | 'llc'
  | 'ac'
  | 'sbp'
  | 'offset'

function shiftTimeStr(t: string, offsetMs: number): string {
  const time = new Date(t).getTime()
  return dateToTimestamp(new Date(time + offsetMs))
}

function stripCategory(id: string) {
  return id
    .replace(
      /^(Cycles|Cycle Duration Labels|Green Bands|LLC|AC|SBP|Offset)\s+/,
      ''
    )
    .trim()
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
      cycleDurations: [],
      bands: [],
      llc: [],
      ac: [],
      sbp: [],
      offset: [],
    }
  }

  const series = options.series as SeriesOption[]

  return {
    base: series.filter(
      (s) => typeof s.id === 'string' && s.id.includes('Cycles')
    ),
    cycleDurations: series.filter(
      (s) =>
        typeof s.id === 'string' && s.id.includes('Cycle Duration Labels')
    ),
    bands: series.filter(
      (s) => typeof s.id === 'string' && s.id.includes('Green Bands')
    ),
    llc: series.filter((s) => typeof s.id === 'string' && s.id.includes('LLC')),
    ac: series.filter((s) => typeof s.id === 'string' && s.id.includes('AC')),
    sbp: series.filter((s) => typeof s.id === 'string' && s.id.includes('SBP')),
    offset: series.filter(
      (s) => typeof s.id === 'string' && s.id.includes('Offset')
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

function buildShiftedData(kind: SeriesKind, original: any[], offsetMs: number) {
  switch (kind) {
    case 'base':
      return original.map((d) => [shiftTimeStr(d[0], offsetMs), d[1], d[2]])
    case 'cycle-duration':
      return original.map((d) => [Number(d[0]) + offsetMs, d[1], d[2]])
    case 'band':
    case 'sbp':
      return original.map((d) => [shiftTimeStr(d[0], offsetMs), d[1]])
    case 'llc':
    case 'ac':
      return original.map((d) =>
        d === null ? null : [shiftTimeStr(d[0], offsetMs), d[1]]
      )
    case 'offset':
      return original.map((d) => [d[0], d[1], d[2], offsetMs / 1000])
    default:
      return original
  }
}

export const useTimeSpaceHandler = (chart: ECharts | null) => {
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
      const { base, cycleDurations, bands, llc, ac, sbp, offset } =
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

      put(base, 'base')
      put(cycleDurations, 'cycle-duration')
      put(bands, 'band')
      put(llc, 'llc')
      put(ac, 'ac')
      put(sbp, 'sbp')
      put(offset, 'offset')

      return { base }
    }

    const recaptureAll = () => {
      // wipe caches so we don’t shift from stale “originals”
      originalDataByIdRef.current = {}
      kindByIdRef.current = {}
      resetRefs()
      captureOriginal()
    }

    // initial capture
    const { base } = captureOriginal()
    if (!base.length) return

    const updateGroupFromOriginal = (groupKey: string, offsetMs: number) => {
      const rounded = Math.round(offsetMs / STEP_MS) * STEP_MS
      const updates: Array<{ id: string; data: any[] }> = []

      for (const [id, original] of Object.entries(
        originalDataByIdRef.current
      )) {
        if (getGroupKeyFromSeriesId(id) !== groupKey) continue

        const kind = kindByIdRef.current[id]
        if (!kind) continue

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

      const latestBase =
        (chart.getOption()?.series as SeriesOption[] | undefined)?.filter(
          (s) => typeof s.id === 'string' && s.id.includes('Cycles')
        ) ?? base

      const closest = findClosestGroup(chart, latestBase, e.offsetY)
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
  }, [chart])
}
