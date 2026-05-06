import { TSP_CODES } from '@/features/charts/prioritySummary/priorityDetails.transformer'
import { TIME_SPACE_MOVEMENT_SERIES_Z } from '@/features/charts/timeSpaceDiagram/shared/transformers/timeSpaceTransformerBase'
import { RawTimeSpaceHistoricData } from '@/features/charts/timeSpaceDiagram/shared/types'
import { Color, triangleSvgSymbol } from '@/features/charts/utils'
import { dateToTimestamp } from '@/utils/dateTime'
import { CustomSeriesRenderItemReturn, SeriesOption } from 'echarts'
import {
  getArrivalTime,
  PASSIVE_DETECTION_SERIES_PROPS,
} from './timeSpaceHistoricSeries.shared'

export const SRM_CONTINUOUS_LEGEND_PREFIX = 'SRM Collection'
export const SRM_GAP_LEGEND_PREFIX = 'SRM Estimated Trajectory'

const TSP_OVERLAY_Z = 7
const TSP_REQUEST_BAND_HEIGHT_PX = 2
const TSP_SERVICE_BAND_HEIGHT_PX = 5
const TSP_SERVICE_OVERLAY_Z = TSP_OVERLAY_Z + 1
const TSP_SERVICE_OFFSET_PX = 20
const TSP_REQUEST_OFFSET_PX = 20
const TSP_MARKER_OFFSET_PX = 20
const TSP_MARKER_Z = TSP_SERVICE_OVERLAY_Z + 1

type TspHistoricEvent = {
  code: number
  timestamp: string
  timestampMs: number
}

type SrmTrackPoint = NonNullable<
  RawTimeSpaceHistoricData['srmEntityTracks']
>[number]['points'][number]

type SrmSeriesPoint = [string, number]
type SrmSeriesDataPoint = SrmSeriesPoint | null

export function generateTMCEvent(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  phaseType?: string,
  distanceScale = 1,
  idScope = 'default'
) {
  const seriesOptions: SeriesOption[] = []

  data.forEach((location, i) => {
    if (!location.tmcForPhase) return

    const leftTurnEvents = location.tmcForPhase.leftTurnEvents.flatMap(
      (leftTurnEvent) => {
        const initialX = leftTurnEvent.start
        const finalX = getArrivalTime(
          location.calculatedDistanceToNext,
          location.speed,
          initialX
        )

        return [
          [initialX, distanceData[i]],
          [
            finalX,
            distanceData[i] + location.calculatedDistanceToNext * distanceScale,
          ],
          null,
        ]
      }
    )

    if (leftTurnEvents.length) {
      seriesOptions.push({
        name: `Left Turn ${phaseType}`,
        id: `Left Turn ${location.locationIdentifier} ${phaseType ?? ''} row-${i} ${idScope}`,
        type: 'line',
        ...PASSIVE_DETECTION_SERIES_PROPS,
        data: leftTurnEvents,
        symbol: 'none',
        z: TIME_SPACE_MOVEMENT_SERIES_Z,
        color: 'black',
      })
    }

    const rightTurnEvents = location.tmcForPhase.rightTurnEvents.flatMap(
      (rightTurnEvent) => {
        const initialX = rightTurnEvent.start
        const finalX = getArrivalTime(
          location.calculatedDistanceToNext,
          location.speed,
          initialX
        )

        return [
          [initialX, distanceData[i]],
          [
            finalX,
            distanceData[i] + location.calculatedDistanceToNext * distanceScale,
          ],
          null,
        ]
      }
    )

    if (rightTurnEvents.length) {
      seriesOptions.push({
        name: `Right Turn ${phaseType}`,
        id: `Right Turn ${location.locationIdentifier} ${phaseType ?? ''} row-${i} ${idScope}`,
        type: 'line',
        ...PASSIVE_DETECTION_SERIES_PROPS,
        data: rightTurnEvents,
        symbol: 'none',
        z: TIME_SPACE_MOVEMENT_SERIES_Z,
        color: 'black',
      })
    }
  })

  return seriesOptions
}

export function generateSrmEntityLines(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  phaseType?: string,
  distanceScale = 1,
  idScope = 'default'
): SeriesOption[] {
  const isOpposing = (phaseType ?? '').toLowerCase().includes('opposing')
  const directionMultiplier = isOpposing ? -1 : 1
  const seriesOptions: SeriesOption[] = []

  data.forEach((location, i) => {
    const tracks = location.srmEntityTracks ?? []
    if (!tracks.length) return

    tracks.forEach((track, trackIndex) => {
      if (!track?.points?.length) return
      const baseDistance = distanceData[i] ?? 0
      const points = track.points.map((point) => [
        point.time,
        baseDistance + directionMultiplier * point.distance * distanceScale,
      ]) as SrmSeriesPoint[]

      const segments = splitSrmTrackByIntersection(track.points, points)

      const collectionData = joinSrmSegments(segments)
      if (collectionData.length) {
        seriesOptions.push({
          name: `${SRM_CONTINUOUS_LEGEND_PREFIX} ${
            phaseType?.length ? phaseType : ''
          }`,
          id: `SRM ${location.locationIdentifier} ${
            track.entityId
          } ${trackIndex} collection ${phaseType ?? ''} row-${i} ${idScope}`,
          type: 'line',
          symbol: 'none',
          z: TIME_SPACE_MOVEMENT_SERIES_Z,
          lineStyle: {
            width: 2,
            color: Color.Black,
            opacity: 0.85,
          },
          data: collectionData,
        })
      }

      segments.slice(1).forEach((segment, gapIndex) => {
        const previousSegment = segments[gapIndex]
        const previousPoint = previousSegment[previousSegment.length - 1]
        const nextPoint = segment[0]
        if (!previousPoint || !nextPoint) return

        seriesOptions.push({
          name: `${SRM_GAP_LEGEND_PREFIX} ${
            phaseType?.length ? phaseType : ''
          }`,
          id: `SRM ${location.locationIdentifier} ${
            track.entityId
          } ${trackIndex} gap-${gapIndex} ${phaseType ?? ''} row-${i} ${idScope}`,
          type: 'line',
          symbol: 'none',
          z: TIME_SPACE_MOVEMENT_SERIES_Z,
          lineStyle: {
            width: 2,
            color: Color.Black,
            opacity: 0.85,
            type: 'dotted',
          },
          data: [previousPoint, nextPoint],
        })
      })
    })
  })

  return seriesOptions
}

function splitSrmTrackByIntersection(
  rawPoints: SrmTrackPoint[],
  points: SrmSeriesPoint[]
): SrmSeriesPoint[][] {
  const hasIntersectionIds = rawPoints.some((point) =>
    Boolean(point.intersectionId?.trim())
  )

  if (!hasIntersectionIds) {
    return [points]
  }

  return rawPoints.reduce<SrmSeriesPoint[][]>((segments, point, index) => {
    const currentPoint = points[index]
    if (!currentPoint) return segments

    const currentIntersection = point.intersectionId?.trim() ?? ''
    const previousIntersection = rawPoints[index - 1]?.intersectionId?.trim()

    if (
      segments.length === 0 ||
      (index > 0 && currentIntersection !== previousIntersection)
    ) {
      segments.push([currentPoint])
    } else {
      segments[segments.length - 1].push(currentPoint)
    }

    return segments
  }, [])
}

function joinSrmSegments(segments: SrmSeriesPoint[][]): SrmSeriesDataPoint[] {
  return segments.reduce<SrmSeriesDataPoint[]>((points, segment, index) => {
    if (!segment.length) return points
    if (index > 0) points.push(null)
    points.push(...segment)
    return points
  }, [])
}

export function buildCycleEventMarkersOnCyclesSeries(
  rows: RawTimeSpaceHistoricData[] = [],
  distanceData: number[] = [],
  idScope = 'default'
): SeriesOption[] {
  const result: SeriesOption[] = []
  const directionMultiplier = idScope === 'opposing' ? -1 : 1

  rows.forEach((row, i) => {
    const yValue = distanceData[i]
    if (yValue == null) return

    const rowStart = Date.parse(row.start)
    const rowEnd = Date.parse(row.end)
    if (!Number.isFinite(rowStart) || !Number.isFinite(rowEnd)) return

    const tspEvents = row.tspEvents ?? []
    if (!tspEvents.length) return
    const earlyGreens: Array<[string, number]> = []
    const extendGreens: Array<[string, number]> = []
    const seen = new Set<string>()

    tspEvents.forEach((event) => {
      if (
        event.eventCode !== TSP_CODES.EarlyGreen &&
        event.eventCode !== TSP_CODES.ExtendGreen
      ) {
        return
      }

      const tMs = Date.parse(event.timestamp as string)
      if (!Number.isFinite(tMs)) return
      if (tMs < rowStart || tMs > rowEnd) return
      const timestamp = dateToTimestamp(event.timestamp as string)
      if (!timestamp) return

      const key = `${event.eventCode}|${i}|${event.timestamp}`
      if (seen.has(key)) return
      seen.add(key)

      const point: [string, number] = [timestamp, yValue]

      if (event.eventCode === TSP_CODES.EarlyGreen) {
        earlyGreens.push(point)
      } else {
        extendGreens.push(point)
      }
    })

    const createSeries = (
      name: string,
      id: string,
      symbol: string | undefined,
      data: Array<[string, number]>
    ): SeriesOption => ({
      type: 'scatter',
      name,
      id,
      symbol: symbol ?? 'circle',
      symbolSize: 7,
      symbolOffset: [0, directionMultiplier * TSP_MARKER_OFFSET_PX],
      itemStyle: {
        color: Color.White,
        borderColor: Color.Black,
        borderWidth: 1.5,
      },
      z: TSP_MARKER_Z,
      tooltip: {
        show: true,
        formatter: (params) => {
          const point = Array.isArray(params) ? params[0] : params
          const value = Array.isArray(point?.value) ? point.value : []

          return `${point?.seriesName ?? ''} ${value[0] ?? ''}`
        },
      },
      data,
    })

    if (earlyGreens.length) {
      result.push(
        createSeries(
          'Early Green (113)',
          `Early Green ${row.locationIdentifier} row-${i} ${idScope}`,
          'circle',
          earlyGreens
        )
      )
    }

    if (extendGreens.length) {
      result.push(
        createSeries(
          'Extend Green (114)',
          `Extend Green ${row.locationIdentifier} row-${i} ${idScope}`,
          triangleSvgSymbol,
          extendGreens
        )
      )
    }
  })

  return result
}

export function buildTspRequestAndServiceLineSeries(
  rows: RawTimeSpaceHistoricData[] = [],
  distanceData: number[] = [],
  idScope = 'default'
): SeriesOption[] {
  const series: SeriesOption[] = []
  const directionMultiplier = idScope === 'opposing' ? -1 : 1

  rows.forEach((row, i) => {
    const yValue = distanceData[i]
    if (yValue == null) return

    const rowStartMs = Date.parse(row.start)
    const rowEndMs = Date.parse(row.end)
    if (!Number.isFinite(rowStartMs) || !Number.isFinite(rowEndMs)) return

    const tspEvents = row.tspEvents ?? []
    if (!tspEvents.length) return

    const relevantEvents = tspEvents
      .reduce<TspHistoricEvent[]>((events, event) => {
        const code = event.eventCode
        const timestampRaw = event.timestamp
        const timestampMs = Date.parse(timestampRaw as string)

        if (
          !Number.isFinite(code) ||
          !timestampRaw ||
          !Number.isFinite(timestampMs)
        ) {
          return events
        }

        if (
          code !== TSP_CODES.CheckIn &&
          code !== TSP_CODES.CheckOut &&
          code !== TSP_CODES.ServiceStart &&
          code !== TSP_CODES.ServiceEnd
        ) {
          return events
        }

        if (timestampMs < rowStartMs || timestampMs > rowEndMs) {
          return events
        }

        const timestamp = dateToTimestamp(timestampRaw)
        if (!timestamp) return events

        events.push({ code, timestamp, timestampMs })
        return events
      }, [])
      .sort((a, b) => {
        const dt = a.timestampMs - b.timestampMs
        if (dt !== 0) return dt

        const sameTimeOrder: Record<number, number> = {
          [TSP_CODES.CheckIn]: 0,
          [TSP_CODES.ServiceStart]: 1,
          [TSP_CODES.ServiceEnd]: 2,
          [TSP_CODES.CheckOut]: 3,
        }

        const aOrder = sameTimeOrder[a.code] ?? 99
        const bOrder = sameTimeOrder[b.code] ?? 99
        if (aOrder !== bOrder) return aOrder - bOrder
        return a.code - b.code
      })

    let requestStart: TspHistoricEvent | null = null
    let serviceStart: TspHistoricEvent | null = null
    const tspRequestData: [string, string, number][] = []
    const tspServiceData: [string, string, number][] = []

    relevantEvents.forEach((event) => {
      if (event.code === TSP_CODES.CheckIn) {
        requestStart = event
        return
      }

      if (event.code === TSP_CODES.CheckOut) {
        if (requestStart && event.timestampMs > requestStart.timestampMs) {
          tspRequestData.push([requestStart.timestamp, event.timestamp, yValue])
        }
        requestStart = null
        return
      }

      if (event.code === TSP_CODES.ServiceStart) {
        serviceStart = event
        return
      }

      if (event.code === TSP_CODES.ServiceEnd) {
        if (serviceStart && event.timestampMs > serviceStart.timestampMs) {
          tspServiceData.push([serviceStart.timestamp, event.timestamp, yValue])
        }
        serviceStart = null
      }
    })

    if (tspRequestData.length) {
      series.push({
        name: 'TSP Request (112-115)',
        id: `TSP Request ${row.locationIdentifier} row-${i} ${idScope}`,
        type: 'custom',
        clip: true,
        encode: { x: [0, 1], y: 2 },
        data: tspRequestData,
        z: TSP_OVERLAY_Z,
        renderItem: (_param, api): CustomSeriesRenderItemReturn => {
          const startValue = api.value(0)
          const endValue = api.value(1)
          const rowY = Number(api.value(2))
          const startMs =
            typeof startValue === 'number'
              ? startValue
              : Date.parse(String(startValue))
          const endMs =
            typeof endValue === 'number'
              ? endValue
              : Date.parse(String(endValue))
          if (
            !Number.isFinite(startMs) ||
            !Number.isFinite(endMs) ||
            !Number.isFinite(rowY)
          ) {
            return
          }

          const startPoint = api.coord([startMs, rowY])
          const endPoint = api.coord([endMs, rowY])
          const centerY =
            startPoint[1] + directionMultiplier * TSP_REQUEST_OFFSET_PX

          return {
            type: 'rect',
            z2: TSP_OVERLAY_Z,
            shape: {
              x: Math.min(startPoint[0], endPoint[0]),
              y: centerY - TSP_REQUEST_BAND_HEIGHT_PX / 2,
              width: Math.max(1, Math.abs(endPoint[0] - startPoint[0])),
              height: TSP_REQUEST_BAND_HEIGHT_PX,
            },
            style: {
              fill: '#808080',
              opacity: 0.95,
            },
            emphasisDisabled: true,
          }
        },
        tooltip: { show: false },
      })
    }

    if (tspServiceData.length) {
      series.push({
        name: 'TSP Service (118-119)',
        id: `TSP Service ${row.locationIdentifier} row-${i} ${idScope}`,
        type: 'custom',
        clip: true,
        encode: { x: [0, 1], y: 2 },
        data: tspServiceData,
        z: TSP_SERVICE_OVERLAY_Z,
        renderItem: (_param, api): CustomSeriesRenderItemReturn => {
          const startValue = api.value(0)
          const endValue = api.value(1)
          const rowY = Number(api.value(2))
          const startMs =
            typeof startValue === 'number'
              ? startValue
              : Date.parse(String(startValue))
          const endMs =
            typeof endValue === 'number'
              ? endValue
              : Date.parse(String(endValue))
          if (
            !Number.isFinite(startMs) ||
            !Number.isFinite(endMs) ||
            !Number.isFinite(rowY)
          ) {
            return
          }

          const startPoint = api.coord([startMs, rowY])
          const endPoint = api.coord([endMs, rowY])
          const centerY =
            startPoint[1] + directionMultiplier * TSP_SERVICE_OFFSET_PX
          const x = Math.min(startPoint[0], endPoint[0])
          const width = Math.max(1, Math.abs(endPoint[0] - startPoint[0]))
          const y = centerY - TSP_SERVICE_BAND_HEIGHT_PX / 2

          return {
            type: 'rect',
            z2: TSP_SERVICE_OVERLAY_Z,
            shape: {
              x,
              y,
              width,
              height: TSP_SERVICE_BAND_HEIGHT_PX,
            },
            style: {
              fill: 'transparent',
              stroke: Color.Black,
              lineWidth: 1.5,
              strokeOpacity: 0.95,
            },
            emphasisDisabled: true,
          }
        },
        tooltip: { show: false },
      })
    }
  })

  return series
}
