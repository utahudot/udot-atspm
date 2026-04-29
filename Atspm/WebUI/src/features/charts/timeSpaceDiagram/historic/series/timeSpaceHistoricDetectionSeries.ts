import {
  getChartTimespanMs,
  getTimeLikeMs,
} from '@/features/charts/timeSpaceDiagram/core/math/timeSpaceLayout'
import {
  getCycleContinuationPatternFill,
  TIME_SPACE_CONTINUATION_NODE_NAME,
  TIME_SPACE_MOVEMENT_SERIES_Z,
} from '@/features/charts/timeSpaceDiagram/shared/transformers/timeSpaceTransformerBase'
import { RawTimeSpaceHistoricData } from '@/features/charts/timeSpaceDiagram/shared/types'
import {
  CustomSeriesRenderItemReturn,
  SeriesOption,
} from 'echarts'
import {
  getArrivalTime,
  PASSIVE_DETECTION_SERIES_PROPS,
} from './timeSpaceHistoricSeries.shared'

const opacity = 1

export function generateLaneByLaneCountEventLines(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  color: string,
  phaseType?: string,
  isPrimary?: boolean,
  distanceScale = 1
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []
  data.forEach((location, i) => {
    if (!location.laneByLaneCountDetectors) return
    const sideScope = isPrimary ? 'primary' : 'opposing'
    const series: SeriesOption = {
      name: `Lane by Lane Count ${phaseType?.length && phaseType}`,
      id: `LLC ${location.locationIdentifier} ${
        phaseType?.length ? phaseType : ''
      } row-${i} ${sideScope}`,
      type: 'line',
      symbol: 'none',
      z: TIME_SPACE_MOVEMENT_SERIES_Z,
      ...PASSIVE_DETECTION_SERIES_PROPS,
      lineStyle: {
        width: 2,
        color,
        opacity: 0.7,
      },
      data: location.laneByLaneCountDetectors.flatMap((events) => {
        const distanceToNext = isPrimary
          ? location.calculatedDistanceToNext
          : -location.calculatedDistanceToNext
        const displayDistanceToNext = distanceToNext * distanceScale
        const initialX = events.detectorOn
        const finalX = getArrivalTime(
          location.calculatedDistanceToNext,
          location.speed,
          initialX
        )
        return [
          [initialX, distanceData[i]],
          [finalX, distanceData[i] + displayDistanceToNext],
          null,
        ]
      }),
    }
    seriesOptions.push(series)
  })
  return seriesOptions
}

export function generateAdvanceCountEventLines(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  color: string,
  phaseType?: string,
  isPrimary?: boolean,
  distanceScale = 1
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []
  const directionMultiplier = isPrimary ? 1 : -1

  data.forEach((location, i) => {
    if (location.isIgnoredLocation) return
    if (!location.advanceCountDetectors?.length) return

    const currentDistance = distanceData[i]
    const previousDistance =
      currentDistance -
      directionMultiplier *
        Math.abs(location.calculatedDistanceToPrevious * distanceScale)
    const sideScope = isPrimary ? 'primary' : 'opposing'

    const series: SeriesOption = {
      name: `Advance Count ${phaseType?.length && phaseType}`,
      id: `AC ${location.locationIdentifier} ${
        phaseType?.length ? phaseType : ''
      } row-${i} ${sideScope}`,
      type: 'line',
      symbol: 'none',
      z: TIME_SPACE_MOVEMENT_SERIES_Z,
      ...PASSIVE_DETECTION_SERIES_PROPS,
      lineStyle: {
        width: 2,
        color,
        opacity,
      },
      data: location.advanceCountDetectors.flatMap((events) => {
        const finalX = getArrivalTime(
          events.distanceToStopBar,
          location.speed,
          events.detectorOn
        )

        const initialX = getArrivalTime(
          -Math.abs(location.calculatedDistanceToPrevious),
          location.speed,
          finalX
        )
        return [
          [initialX, previousDistance],
          [finalX, currentDistance],
          null,
        ]
      }),
    }
    seriesOptions.push(series)
  })
  return seriesOptions
}

export function generateStopBarPresenceEventLines(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  color: string,
  phaseType?: string,
  isPrimary?: boolean,
  distanceScale = 1
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []

  for (let i = 0; i < data.length; i++) {
    const location = data[i]
    if (!location.stopBarPresenceDetectors) continue
    const chartTimespanMs = getChartTimespanMs(location.start, location.end)
    const dataPoints = getStopBarPresenceDataPoints(location, distanceData[i])
    const sideScope = isPrimary ? 'primary' : 'opposing'

    const seriesOption: SeriesOption = {
      name: `Stop Bar Presence ${phaseType?.length && phaseType}`,
      id: `SBP ${data[i].locationIdentifier} ${
        phaseType?.length ? phaseType : ''
      } row-${i} ${sideScope}`,
      type: 'custom',
      data: dataPoints,
      clip: true,
      selectedMode: false,
      z: TIME_SPACE_MOVEMENT_SERIES_Z,
      ...PASSIVE_DETECTION_SERIES_PROPS,
      renderItem: function (params, api) {
        const dataIndex = params.dataIndex
        if (
          !dataPoints ||
          dataIndex >= dataPoints.length - 1 ||
          dataIndex % 2 !== 0
        ) {
          return
        }
        const nextIndex = dataIndex + 1
        const distanceToNext = isPrimary
          ? location.calculatedDistanceToNext
          : -location.calculatedDistanceToNext
        const displayDistanceToNext = distanceToNext * distanceScale
        const [x1, y1] = [api.value(0), api.value(1)]
        const [x2, y2] = [api.value(0, nextIndex), api.value(1, nextIndex)]
        const x1Ms = getTimeLikeMs(x1)
        const x2Ms = getTimeLikeMs(x2)
        const currPointFinalX = getArrivalTime(
          location.calculatedDistanceToNext,
          location.speed,
          x1 as string
        )
        const nextPointFinalX = getArrivalTime(
          location.calculatedDistanceToNext,
          location.speed,
          x2 as string
        )
        const currPointFinalMs = getTimeLikeMs(currPointFinalX)
        const nextPointFinalMs = getTimeLikeMs(nextPointFinalX)

        if (
          x1Ms == null ||
          x2Ms == null ||
          currPointFinalMs == null ||
          nextPointFinalMs == null
        ) {
          return
        }

        const buildPoints = (shiftMs = 0) => [
          api.coord([x1Ms + shiftMs, y1]),
          api.coord([x2Ms + shiftMs, y2]),
          api.coord([
            nextPointFinalMs + shiftMs,
            (y2 as number) + displayDistanceToNext,
          ]),
          api.coord([
            currPointFinalMs + shiftMs,
            (y1 as number) + displayDistanceToNext,
          ]),
        ]

        const children = []

        if (chartTimespanMs != null && chartTimespanMs > 0) {
          children.push(
            buildStopBarPresencePolygon(
              buildPoints(-chartTimespanMs),
              true,
              color
            )
          )
        }

        children.push(buildStopBarPresencePolygon(buildPoints(), false, color))

        if (chartTimespanMs != null && chartTimespanMs > 0) {
          children.push(
            buildStopBarPresencePolygon(
              buildPoints(chartTimespanMs),
              true,
              color
            )
          )
        }

        if (children.length === 1) {
          return children[0]
        }

        return {
          type: 'group',
          emphasisDisabled: true,
          children,
        }
      },
    }
    seriesOptions.push(seriesOption)
  }
  return seriesOptions
}

function buildStopBarPresencePolygon(
  points: number[][],
  isContinuation: boolean,
  color: string
): CustomSeriesRenderItemReturn {
  return {
    type: 'polygon',
    ...(isContinuation ? { name: TIME_SPACE_CONTINUATION_NODE_NAME } : null),
    z2: isContinuation ? 0 : 1,
    transition: ['shape'],
    emphasisDisabled: true,
    shape: {
      points,
    },
    style: isContinuation
      ? {
          fill: getCycleContinuationPatternFill(),
        }
      : {
          opacity: 1,
          fill: color,
          fillOpacity: opacity,
          lineWidth: 2,
        },
  }
}

function getStopBarPresenceDataPoints(
  location: RawTimeSpaceHistoricData,
  currDistance: number
) {
  if (location.stopBarPresenceDetectors.length) {
    return location.stopBarPresenceDetectors.flatMap((events) => [
      [events.detectorOn, currDistance],
      [events.detectorOff, currDistance],
    ])
  }

  return []
}
