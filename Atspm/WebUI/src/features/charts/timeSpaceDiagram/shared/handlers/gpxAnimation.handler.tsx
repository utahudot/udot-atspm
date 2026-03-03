import { ECharts, SeriesOption } from 'echarts'
import { useMemo, useRef } from 'react'
import { GpxPoint } from '../gpxFileParser'
import { GpxUploadOptions } from '../types'

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
  const STEP = 10

  const play = () => {
    if (!chart || !processedSeries.length) return

    playingRef.current = true

    chart.setOption({
      series: processedSeries,
    })
  }

  return {
    play,
    currentTimeRef,
    maxTime,
  }
}
