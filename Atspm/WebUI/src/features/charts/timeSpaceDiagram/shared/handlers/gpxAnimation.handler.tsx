import { ECharts, SeriesOption } from 'echarts'
import { useCallback, useMemo, useRef } from 'react'
import { GpxPoint } from '../gpxFileParser'
import {
  GpxUploadOptions,
  TIME_SPACE_GPX_TRACKS_LEGEND_NAME,
} from '../types'

const GPX_OVERLAY_SERIES_ID_PREFIXES = ['gpx-', 'srm-'] as const

function isSeriesOption(
  value: SeriesOption | null | undefined
): value is SeriesOption {
  return Boolean(value && typeof value === 'object')
}

function isGpxOverlaySeries(
  series: SeriesOption | null | undefined
): boolean {
  if (!isSeriesOption(series) || typeof series.id !== 'string') {
    return false
  }

  return GPX_OVERLAY_SERIES_ID_PREFIXES.some((prefix) =>
    series.id.startsWith(prefix)
  )
}

function getCurrentChartSeries(chart: ECharts): SeriesOption[] {
  const option = chart.getOption()
  const rawSeries = Array.isArray(option?.series)
    ? option.series
    : option?.series
      ? [option.series]
      : []

  return rawSeries.filter(isSeriesOption)
}

export function mergeChartSeriesWithGpxOverlays(
  currentSeries: Array<SeriesOption | null | undefined>,
  overlaySeries: SeriesOption[]
) {
  const baseSeries = currentSeries.filter(
    (series): series is SeriesOption => !isGpxOverlaySeries(series)
  )

  return [...baseSeries, ...overlaySeries]
}

function buildShiftedGpxData(
  chart: ECharts,
  upload: GpxUploadOptions,
  points: GpxPoint[]
): [string, number][] {
  const { startLocation, endLocation } = upload
  if (!points?.length) return []

  const options = chart?.getOption()
  const locations = options.displayProps.locations
  const yAxis = chart.getOption()?.yAxis

  if (!Array.isArray(yAxis) || !Array.isArray(yAxis[0]?.data)) return []

  const startIndex = locations.indexOf(startLocation)
  const endIndex = locations.indexOf(endLocation)

  if (startIndex === -1 || endIndex === -1) return []

  const startValue = yAxis[0].data[startIndex] as number
  const endValue = yAxis[0].data[endIndex] as number

  const direction = startIndex <= endIndex ? 1 : -1
  const min = Math.min(startValue, endValue)
  const max = Math.max(startValue, endValue)

  return points.map((p) => {
    const raw = startValue + direction * p.distance
    const clipped = Math.min(Math.max(raw, min), max)

    return [p.time, clipped]
  })
}

export const useGpxAnimationHandler = (
  chart: ECharts | null,
  uploads: GpxUploadOptions[]
) => {
  const playingRef = useRef(false)
  const currentTimeRef = useRef<string>('')

  const processedSeries = useMemo(() => {
    if (!chart) return []

    const series: SeriesOption[] = []

    uploads
      .filter((u) => !u.error)
      .forEach((upload) => {
        if (upload.parsedData?.length) {
          series.push({
            id: `gpx-${upload.id}`,
            name: TIME_SPACE_GPX_TRACKS_LEGEND_NAME,
            type: 'line',
            data: buildShiftedGpxData(chart, upload, upload.parsedData),
            color: 'black',
            symbol: 'none',
            silent: true,
            lineStyle: {
              width: 3,
              color: 'black',
            },
            clip: true,
          })
        }

        const entityTracks = upload.parsedEntityData ?? []
        entityTracks.forEach((track, index) => {
          series.push({
            id: `srm-${upload.id}-${track.entityId}-${index}`,
            name: `SRM ${track.entityId}`,
            type: 'line',
            data: buildShiftedGpxData(chart, upload, track.points),
            color: 'black',
            symbol: 'none',
            silent: true,
            lineStyle: {
              width: 2,
              color: 'black',
            },
            clip: true,
          })
        })
      })

    return series
  }, [chart, uploads])

  const maxTime = useMemo(
    () => processedSeries.flatMap((s) => s.data).at(-1)?.[0] ?? '',
    [processedSeries]
  )

  const play = useCallback(() => {
    if (!chart) return

    playingRef.current = processedSeries.length > 0

    const nextSeries = mergeChartSeriesWithGpxOverlays(
      getCurrentChartSeries(chart),
      processedSeries
    )

    chart.setOption(
      {
        series: nextSeries,
      },
      { replaceMerge: ['series'] }
    )
  }, [chart, processedSeries])

  return {
    play,
    currentTimeRef,
    maxTime,
  }
}
