// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - timeSpaceHistoricTransformer.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
import { IndianaEvent } from '@/api/data'
import {
  createDisplayProps,
  createLegend,
  createTitle,
  createXAxis,
  createYAxis,
} from '@/features/charts/common/transformers'
import { ToolType } from '@/features/charts/common/types'
import {
  generateCycleLabels,
  generateCycles,
  generateGreenEventLines,
  getDistancesLabelOption,
  getLocationsLabelOption,
} from '@/features/charts/timeSpaceDiagram/shared/transformers/timeSpaceTransformerBase'
import {
  RawTimeSpaceDiagramResponse,
  RawTimeSpaceHistoricData,
  TimeSpaceDiagramPhaseResult,
} from '@/features/charts/timeSpaceDiagram/shared/types'
import { TransformedTimeSpaceResponse } from '@/features/charts/types'
import {
  Color,
  formatChartDateTimeRange,
  SolidLineSeriesSymbol,
  triangleSvgSymbol,
} from '@/features/charts/utils'
import { dateToTimestamp } from '@/utils/dateTime'
import {
  CustomSeriesRenderItemReturn,
  DataZoomComponentOption,
  EChartsOption,
  GridComponentOption,
  SeriesOption,
} from 'echarts'
import { TSP_CODES } from '../../prioritySummary/priorityDetails.transformer'
import { PedestrianInterval } from '../../timingAndActuation/types'

const opacity = 0.4

export default function transformTimeSpaceHistoricData(
  response: RawTimeSpaceDiagramResponse
): TransformedTimeSpaceResponse & { errors?: string[] } {
  // Extract successful results and filter out errors
  const wrappedData =
    response.data as TimeSpaceDiagramPhaseResult<RawTimeSpaceHistoricData>[]

  // Collect error messages
  const errorMessages = wrappedData
    .filter((item) => !item.isSuccess && item.error)
    .map((item) => item.error as string)

  // Extract only successful results
  const successfulData = wrappedData
    .filter((item) => item.isSuccess && item.result)
    .map((item) => item.result as RawTimeSpaceHistoricData)

  if (successfulData.length === 0) {
    // Return error information instead of throwing
    return {
      type: ToolType.TimeSpaceHistoric,
      data: { chart: {} },
      errors:
        errorMessages.length > 0
          ? errorMessages
          : [
              'No valid time space diagram data available. All phases returned errors.',
            ],
    }
  }

  const result: TransformedTimeSpaceResponse & { errors?: string[] } = {
    type: ToolType.TimeSpaceHistoric,
    data: {
      chart: transformData(successfulData),
    },
  }

  // Include errors if some phases failed but we still have partial data
  if (errorMessages.length > 0) {
    result.errors = errorMessages
  }

  return result
}
const PEDESTRIAN_LINE_WIDTH = 0.8
const PEDESTRIAN_LINE_Y_OFFSET = 10
const PEDESTRIAN_ZIGZAG_AMPLITUDE = 2
const PEDESTRIAN_ZIGZAG_STEP_PX = 3
const PEDESTRIAN_CLEARANCE_DOT_PATTERN = [1, 3]
const PEDESTRIAN_BOUNDARY_TICK_HALF_HEIGHT = PEDESTRIAN_ZIGZAG_AMPLITUDE

function transformData(data: RawTimeSpaceHistoricData[]): EChartsOption {
  const primaryPhaseData = data.filter(
    (location) => location.phaseType === 'Primary'
  )

  const opposingPhaseData = data.filter(
    (location) => location.phaseType === 'Opposing'
  )

  const dateRange = formatChartDateTimeRange(data[0].start, data[0].end)

  const chartStartMs = Date.parse(primaryPhaseData[0].start)
  const chartEndMs = Date.parse(primaryPhaseData[0].end)
  const totalSeconds = Math.floor((chartEndMs - chartStartMs) / 1000)

  const xAxisTopSeconds = {
    type: 'value',
    position: 'top',
    nameGap: 25,
    min: 0,
    max: totalSeconds,
    name: 'Time Since Start (seconds)',
    nameLocation: 'middle',
    minInterval: 60,
    maxInterval: 60,
    minorTick: { show: true, splitNumber: 4 },
    axisLabel: {
      formatter: (v: number) => String(Math.round(v / 1) * 1),
    },
  } as const

  const xAxis = [
    createXAxis(data[0].start, data[0].end),
    { min: 0, max: 1, show: false },
    xAxisTopSeconds,
  ]

  let initialDistance = 0

  const primaryDirection = primaryPhaseData[0].approachDescription
  const opposingDirection = opposingPhaseData[0].approachDescription

  const distanceData: number[] = []
  primaryPhaseData.forEach((location) => {
    distanceData.push(initialDistance)
    initialDistance += location.distanceToNextLocation
  })
  const yAxis = createYAxis(false, {
    show: false,
    data: distanceData,
    axisTick: { show: true },
    max: distanceData[distanceData.length - 1] + 350,
    min: -250,
  })

  const title = createTitle({
    title: 'Time Space Diagram • Historic',
    location: dateRange,
  })

  const grid: GridComponentOption = {
    top: 80,
    left: 280,
    right: 300,
    bottom: 100,
    show: true,
    borderWidth: 1,
  }

  const start = new Date(data[0].end)
  const end = new Date(data[0].start)
  const timeDiff = (start.getTime() - end.getTime()) / 3_600_000

  let dataZoom: DataZoomComponentOption[]

  if (timeDiff > 6) {
    dataZoom = [
      {
        type: 'slider',
        filterMode: 'filter',
        show: true,
        start: 0,
        end: 10,
        maxSpan: 10,
        minSpan: 0.2,
      },
    ]
  } else {
    dataZoom = [
      {
        type: 'slider',
        filterMode: 'none',
        show: true,
      },
    ]
  }

  const toolbox = {
    feature: {
      saveAsImage: { name: title },
      dataView: {
        readOnly: true,
      },
      restore: {},
    },
  }

  // const { requestRects, serviceRects, intersectionLines } =
  //   buildRectsAndLinesForTSD(primaryPhaseData)
  const series: SeriesOption[] = []

  series.push(
    ...generateCycles(primaryPhaseData, distanceData, primaryDirection)
  )

  series.push(
    ...generateLaneByLaneCountEventLines(
      primaryPhaseData,
      distanceData,
      'darkblue',
      primaryDirection
    )
  )

  series.push(
    ...generateAdvanceCountEventLines(
      primaryPhaseData,
      distanceData,
      'darkblue',
      primaryDirection
    )
  )

  series.push(
    ...generateStopBarPresenceEventLines(
      primaryPhaseData,
      distanceData,
      'lightblue',
      primaryDirection,
      true
    )
  )

  series.push(
    ...generateGreenEventLines(
      primaryPhaseData,
      distanceData,
      primaryDirection,
      true
    )
  )

  series.push(
    ...generatePedestrianIntervalLines(
      primaryPhaseData,
      distanceData,
      primaryDirection
    )
  )
  series.push(
    ...generateSrmEntityLines(primaryPhaseData, distanceData, primaryDirection)
  )

  series.push(
    ...generateTMCEvent(primaryPhaseData, distanceData, primaryDirection)
  )

  series.push(
    ...buildCycleEventMarkersOnCyclesSeries(primaryPhaseData, distanceData)
  )

  series.push(
    ...buildTspRequestAndServiceLineSeries(primaryPhaseData, distanceData)
  )

  const locationLabels = getLocationsLabelOption(
    primaryPhaseData,
    distanceData,
    grid
  )
  series.push(locationLabels)
  series.push(
    getDistancesLabelOption(primaryPhaseData, distanceData, grid.left as number)
  )

  let reverseDistanceData = [...distanceData].reverse()
  reverseDistanceData = reverseDistanceData.map((distance) => (distance += 300))
  series.push(
    ...generateCycles(opposingPhaseData, reverseDistanceData, opposingDirection)
  )

  series.push(
    ...generateLaneByLaneCountEventLines(
      opposingPhaseData,
      reverseDistanceData,
      'orange',
      opposingDirection
    )
  )

  series.push(
    ...generateAdvanceCountEventLines(
      opposingPhaseData,
      reverseDistanceData,
      'orange',
      opposingDirection
    )
  )

  series.push(
    ...generateStopBarPresenceEventLines(
      opposingPhaseData,
      reverseDistanceData,
      'orange',
      opposingDirection,
      false
    )
  )

  series.push(
    ...generateGreenEventLines(
      opposingPhaseData,
      reverseDistanceData,
      opposingDirection,
      false
    )
  )

  series.push(
    ...generatePedestrianIntervalLines(
      opposingPhaseData,
      reverseDistanceData,
      opposingDirection
    )
  )

  series.push(
    ...generateSrmEntityLines(
      opposingPhaseData,
      reverseDistanceData,
      opposingDirection
    )
  )

  series.push(
    ...buildCycleEventMarkersOnCyclesSeries(
      opposingPhaseData,
      reverseDistanceData
    )
  )

  series.push(
    ...buildTspRequestAndServiceLineSeries(
      opposingPhaseData,
      reverseDistanceData
    )
  )

  // for each series set the xAxisIndex to 0
  series.forEach((s) => {
    s.xAxisIndex = 0
  })

  const formatPct = (v?: number | null) =>
    v === null || v === undefined ? '—' : `${Math.round(v)}%`

  const primaryLinesByIndex = primaryPhaseData.map((p) => [
    `AOG: ${formatPct(p.percentArrivalOnGreen)}`,
  ])

  const primaryHeadersByIndex = primaryPhaseData.map((p) => p.approachDescription)

  const opposingHeadersByIndex = [...opposingPhaseData]
    .reverse()
    .map((p) => p.approachDescription)

  const opposingLinesByIndex = [...opposingPhaseData].reverse().map((p) => [
    `AOG: ${formatPct(p.percentArrivalOnGreen)}`,
  ])

  const primaryIgnoredByIndex = primaryPhaseData.map((p) =>
    Boolean(p.isIgnoredLocation)
  )

  const opposingIgnoredByIndex = [...opposingPhaseData].reverse().map((p) =>
    Boolean(p.isIgnoredLocation)
  )

  const primaryLabelSeries = generateCycleLabels(
    distanceData,
    primaryDirection,
    grid.left as number,
    primaryHeadersByIndex,
    primaryLinesByIndex,
    'left',
    primaryIgnoredByIndex
  )

  const opposingLabelSeries = generateCycleLabels(
    distanceData,
    opposingDirection,
    grid.left as number,
    opposingHeadersByIndex,
    opposingLinesByIndex,
    'right',
    opposingIgnoredByIndex
  )

  series.push(primaryLabelSeries)
  series.push(opposingLabelSeries)

  // series.push(
  //   ...getDraggableOffsetabelOption(
  //     primaryPhaseData,
  //     distanceData,
  //     primaryDirection
  //   )
  // )

  const MIN_SEGMENT = 1800

  const segments = distanceData.map((v, i) => v - (distanceData[i - 1] ?? 0))

  const positiveSegments = segments.filter((d) => Number.isFinite(d) && d > 0)
  const minSegment = positiveSegments.length ? Math.min(...positiveSegments) : 0

  const scale =
    minSegment > 0 && minSegment < MIN_SEGMENT ? MIN_SEGMENT / minSegment : 1

  const scaledCumulative = distanceData.map((d) =>
    Number.isFinite(d) ? d * scale : d
  )

  const totalDistance = scaledCumulative[scaledCumulative.length - 1] ?? 0

  const displayProps = createDisplayProps({
    description: '',
    numberOfLocations: primaryPhaseData.length,
    height: totalDistance / 19 + 220,
    locations: primaryPhaseData.map((p) => p.locationIdentifier),
  })

  const legends = createLegend({
    top: grid.top,
    data: [
      {
        name: `Cycles ${primaryDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: '#f0807f' },
      },
      {
        name: `Cycles ${opposingDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: '#f0807f' },
      },
      {
        name: `Cycle Durations ${primaryDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
      },
      {
        name: `Cycle Durations ${opposingDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
      },
      {
        name: `Green Bands ${primaryDirection}`,
        itemStyle: { color: 'green', opacity: 0.3 },
      },
      {
        name: `Green Bands ${opposingDirection}`,
        itemStyle: { color: 'green', opacity: 0.3 },
      },
      {
        name: `Lane by Lane Count ${primaryDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: 'darkblue' },
      },
      {
        name: `Lane by Lane Count ${opposingDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: 'orange' },
      },
      {
        name: `Advance Count ${primaryDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: 'darkblue' },
      },
      {
        name: `Advance Count ${opposingDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: 'orange' },
      },
      {
        name: `Stop Bar Presence ${primaryDirection}`,
        itemStyle: { color: 'lightBlue' },
      },
      {
        name: `Stop Bar Presence ${opposingDirection}`,
        itemStyle: { color: 'orange' },
      },
      {
        name: `Pedestrian Interval ${primaryDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
      },
      {
        name: `Pedestrian Interval ${opposingDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
      },
      {
        name: `SRM Entity ${primaryDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
      },
      {
        name: `SRM Entity ${opposingDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
      },
      {
        name: `Left Turn ${primaryDirection}`,
        itemStyle: { color: 'black' },
      },
      {
        name: `Right Turn ${primaryDirection}`,
        itemStyle: { color: 'black' },
      },
      {
        name: `Early Green (113)`,
        icon: triangleSvgSymbol,
        itemStyle: { color: Color.Black },
      },
      {
        name: `Extend Green (114)`,
        itemStyle: { color: 'black' },
      },
      {
        name: `TSP Request (112-115)`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Red },
      },
      {
        name: `TSP Service (118-119)`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.LightBlue },
      },
    ],
    selected: {
      [`Cycles ${primaryDirection}`]: true,
      [`Cycles ${opposingDirection}`]: true,
      [`Cycle Durations ${primaryDirection}`]: true,
      [`Cycle Durations ${opposingDirection}`]: true,
      [`Green Bands ${primaryDirection}`]: true,
      [`Green Bands ${opposingDirection}`]: true,
      [`Lane by Lane Count ${primaryDirection}`]: false,
      [`Lane by Lane Count ${opposingDirection}`]: false,
      [`Advance Count ${primaryDirection}`]: false,
      [`Advance Count ${opposingDirection}`]: false,
      [`Stop Bar Presence ${primaryDirection}`]: false,
      [`Stop Bar Presence ${opposingDirection}`]: false,
      [`Pedestrian Interval ${primaryDirection}`]: false,
      [`Pedestrian Interval ${opposingDirection}`]: false,
      [`SRM Entity ${primaryDirection}`]: true,
      [`SRM Entity ${opposingDirection}`]: true,
      [`Left Turn ${primaryDirection}`]: false,
      [`Right Turn ${primaryDirection}`]: false,
      [`Early Green (113)`]: false,
      [`Extend Green (114)`]: false,
      [`TSP Request (112-115)`]: false,
      [`TSP Service (118-119)`]: false,
    },
  })

  const chartOptions: EChartsOption = {
    title,
    xAxis: xAxis,
    yAxis,
    grid,
    dataZoom,
    legend: legends,
    toolbox,
    // animation: false,
    series,
    displayProps,
    animation: true,
    responsive: true,
    maintainAspectRatio: false,
  }

  return chartOptions
}

function generateLaneByLaneCountEventLines(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  color: string,
  phaseType?: string
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []
  data.forEach((location, i) => {
    if (!location.laneByLaneCountDetectors) return
    const series: SeriesOption = {
      name: `Lane by Lane Count ${phaseType?.length && phaseType}`,
      id: `LLC ${location.locationIdentifier} ${phaseType?.length ? phaseType : ''}`,
      type: 'line',
      symbol: 'none',
      lineStyle: {
        width: 2,
        color,
        opacity,
      },
      data: location.laneByLaneCountDetectors.flatMap((events) => {
        const initialX = events.detectorOn
        const finalX = getArrivalTime(
          location.calculatedDistanceToNext,
          location.speed,
          initialX
        )
        const values = [
          [initialX, distanceData[i]],
          [finalX, distanceData[i + 1]],
          null,
        ]
        return values
      }),
    }
    seriesOptions.push(series)
  })
  return seriesOptions
}

function getArrivalTime(
  distanceToNextLocation: number,
  speed: number,
  currentDetectorOn: Date | string
): string {
  const start = new Date(currentDetectorOn)
  const speedInFeetPerSecond = getSpeedInFeetPerSecond(speed)
  const timeToTravelSeconds = distanceToNextLocation / speedInFeetPerSecond

  const arrivalMs = start.getTime() + timeToTravelSeconds * 1000

  return dateToTimestamp(new Date(arrivalMs))
}

function getSpeedInFeetPerSecond(speed: number): number {
  return (speed * 5280) / 3600
}

function generateAdvanceCountEventLines(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  color: string,
  phaseType?: string
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []
  data.forEach((location, i) => {
    if (i === 0) return
    if (!location.advanceCountDetectors) return
    const series: SeriesOption = {
      name: `Advance Count ${phaseType?.length && phaseType}`,
      id: `AC ${i !== 0 ? data[i - 1].locationIdentifier : location.locationIdentifier} ${phaseType?.length ? phaseType : ''}`,
      type: 'line',
      symbol: 'none',
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
          -location.calculatedDistanceToPrevious,
          location.speed,
          finalX
        )
        const values = [
          [initialX, distanceData[i - 1]],
          [finalX, distanceData[i]],
          null,
        ]
        return values
      }),
    }
    seriesOptions.push(series)
  })
  return seriesOptions
}

function generateStopBarPresenceEventLines(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  color: string,
  phaseType?: string,
  isPrimary?: boolean
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []

  for (let i = 0; i < data.length; i++) {
    const location = data[i]
    if (!location.stopBarPresenceDetectors) continue
    const dataPoints = getStopBarPresenceDataPoints(location, distanceData[i])

    const seriesOption: SeriesOption = {
      name: `Stop Bar Presence ${phaseType?.length && phaseType}`,
      id: `SBP ${data[i].locationIdentifier} ${phaseType?.length ? phaseType : ''}`,
      type: 'custom',
      data: dataPoints,
      clip: true,
      selectedMode: false,
      renderItem: function (params, api) {
        const i = params.dataIndex
        if (!dataPoints || i >= dataPoints.length - 1 || i % 2 !== 0) {
          return
        }
        const nextIndex = i + 1
        const distanceToNext = isPrimary
          ? location.calculatedDistanceToNext
          : -location.calculatedDistanceToNext
        const [x1, y1] = [api.value(0), api.value(1)]

        const [x2, y2] = [api.value(0, nextIndex), api.value(1, nextIndex)]
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
        const points = [
          api.coord([x1, y1]),
          api.coord([x2, y2]),
          api.coord([nextPointFinalX, (y2 as number) + distanceToNext]),
          api.coord([currPointFinalX, (y1 as number) + distanceToNext]),
        ]
        return {
          type: 'polygon',
          transition: ['shape'],
          shape: {
            points: points,
          },
          style: {
            opacity: 1,
            fill: color,
            fillOpacity: opacity,
            lineWidth: 3,
          },
        }
      },
    }
    seriesOptions.push(seriesOption)
  }
  return seriesOptions
}

function getStopBarPresenceDataPoints(
  location: RawTimeSpaceHistoricData,
  currDistance: number
) {
  if (location.stopBarPresenceDetectors.length) {
    return location.stopBarPresenceDetectors.flatMap((events) => {
      return [
        [events.detectorOn, currDistance],
        [events.detectorOff, currDistance],
      ]
    })
  }
}

function generatePedestrianIntervalLines(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  phaseType?: string
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []

  data.forEach((location, i) => {
    if (!location.pedestrianIntervals?.length) return

    const pedData = generatePedData(
      location.pedestrianIntervals,
      distanceData[i]
    )

    const series: SeriesOption = {
      name: `Pedestrian Interval ${phaseType?.length ? phaseType : ''}`,
      id: `PI ${location.locationIdentifier} ${phaseType ?? ''}`,
      type: 'custom',
      clip: true,
      data: pedData,
      encode: {
        x: [1, 2],
        y: [3],
      },
      z: 6,
      renderItem: (_param, api): CustomSeriesRenderItemReturn => {
        const x1 = api.value(0)
        const x2 = api.value(1)
        const interval = api.value(2)
        const distance = api.value(3)
        const p1 = api.coord([x1, distance])
        const p2 = api.coord([x2, distance])
        const y = p1[1] + PEDESTRIAN_LINE_Y_OFFSET

        return createPedestrianIntervalShape(
          interval as number,
          p1[0],
          p2[0],
          y
        )
      },
    }

    seriesOptions.push(series)
  })

  return seriesOptions
}

function generatePedData(
  pedestrianIntervals: PedestrianInterval[],
  distance: number
) {
  return pedestrianIntervals.map((interval, index, array) => {
    const startCoord = interval.start
    const endCoord =
      index < array.length - 1 ? array[index + 1].start : startCoord

    return [startCoord, endCoord, interval.value, distance]
  })
}

function createPedestrianIntervalShape(
  intervalValue: number,
  startX: number,
  endX: number,
  y: number
): CustomSeriesRenderItemReturn {
  const baseStyle = {
    stroke: Color.Black,
    lineWidth: PEDESTRIAN_LINE_WIDTH,
    fill: 'none',
    lineCap: 'round',
    lineJoin: 'round',
  }
  const boundaryTick = {
    type: 'line',
    shape: {
      x1: startX,
      y1: y - PEDESTRIAN_BOUNDARY_TICK_HALF_HEIGHT,
      x2: startX,
      y2: y + PEDESTRIAN_BOUNDARY_TICK_HALF_HEIGHT,
    },
    style: baseStyle,
  }
  let intervalShape

  if (intervalValue === 22 || intervalValue === 68) {
    intervalShape = {
      type: 'line',
      shape: { x1: startX, y1: y, x2: endX, y2: y },
      style: { ...baseStyle, lineDash: PEDESTRIAN_CLEARANCE_DOT_PATTERN },
    }
  } else if (intervalValue === 23 || intervalValue === 69) {
    intervalShape = {
      type: 'polyline',
      shape: {
        points: buildPedestrianZigZagPoints(startX, endX, y),
      },
      style: baseStyle,
    }
  } else {
    intervalShape = {
      type: 'line',
      shape: { x1: startX, y1: y, x2: endX, y2: y },
      style: baseStyle,
    }
  }

  return {
    type: 'group',
    children: [intervalShape, boundaryTick],
  }
}

function buildPedestrianZigZagPoints(
  startX: number,
  endX: number,
  baseY: number
): Array<[number, number]> {
  const width = endX - startX
  if (Math.abs(width) <= PEDESTRIAN_ZIGZAG_STEP_PX) {
    return [
      [startX, baseY],
      [endX, baseY],
    ]
  }

  const segmentCount = Math.max(
    3,
    Math.ceil(Math.abs(width) / PEDESTRIAN_ZIGZAG_STEP_PX)
  )
  const step = width / segmentCount
  const points: Array<[number, number]> = [[startX, baseY]]

  for (let i = 1; i < segmentCount; i++) {
    const x = startX + step * i
    const y =
      baseY +
      (i % 2 === 0 ? PEDESTRIAN_ZIGZAG_AMPLITUDE : -PEDESTRIAN_ZIGZAG_AMPLITUDE)
    points.push([x, y])
  }

  points.push([endX, baseY])

  return points
}

function generateTMCEvent(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  phaseType?: string
) {
  const seriesOptions: SeriesOption[] = []

  const leftTurnEvents: ((string | number)[] | null)[] = []
  const rightTurnEvents: ((string | number)[] | null)[] = []

  data.forEach((location, i) => {
    if (!location.tmcForPhase) return
    const leftTurns = location.tmcForPhase.leftTurnEvents.flatMap((lEvent) => {
      const initialX = lEvent.start
      const finalX = getArrivalTime(
        location.calculatedDistanceToNext,
        location.speed,
        initialX
      )
      const values = [
        [initialX, distanceData[i]],
        [finalX, distanceData[i + 1]],
        null,
      ]
      return values
    })
    leftTurnEvents.push(...leftTurns)

    const rightTurns = location.tmcForPhase.rightTurnEvents.flatMap(
      (rEvent) => {
        const initialX = rEvent.start
        const finalX = getArrivalTime(
          location.calculatedDistanceToNext,
          location.speed,
          initialX
        )
        const values = [
          [initialX, distanceData[i]],
          [finalX, distanceData[i + 1]],
          null,
        ]
        return values
      }
    )
    rightTurnEvents.push(...rightTurns)
  })

  seriesOptions.push(
    {
      name: `Left Turn ${phaseType}`,
      type: 'line',
      data: leftTurnEvents,
      symbol: 'none',
      z: 10,
      color: 'black',
    },
    {
      name: `Right Turn ${phaseType}`,
      type: 'line',
      data: rightTurnEvents,
      symbol: 'none',
      z: 10,
      color: 'black',
    }
  )

  return seriesOptions
}

function generateSrmEntityLines(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  phaseType?: string
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
        baseDistance + directionMultiplier * point.distance,
      ])

      seriesOptions.push({
        name: `SRM Entity ${phaseType?.length ? phaseType : ''}`,
        id: `SRM ${location.locationIdentifier} ${track.entityId} ${trackIndex} ${
          phaseType ?? ''
        }`,
        type: 'line',
        symbol: 'none',
        lineStyle: {
          width: 2,
          color: Color.Black,
          opacity: 0.85,
        },
        data: points,
        z: 9,
      })
    })
  })

  return seriesOptions
}

function buildCycleEventMarkersOnCyclesSeries(
  rows: RawTimeSpaceHistoricData[] = [],
  distanceData: number[] = []
): SeriesOption[] {
  const earlyGreens: Array<[string, number]> = []
  const extendGreens: Array<[string, number]> = []
  const seen = new Set<string>()

  rows.forEach((row, i) => {
    const yValue = distanceData[i]
    if (yValue == null) return

    const rowStart = Date.parse(row.start)
    const rowEnd = Date.parse(row.end)
    if (!Number.isFinite(rowStart) || !Number.isFinite(rowEnd)) return

    const tspEvents = (row.tspEvents ?? []) as IndianaEvent[]
    if (!tspEvents.length) return

    tspEvents.forEach((e) => {
      if (
        e.eventCode !== TSP_CODES.EarlyGreen &&
        e.eventCode !== TSP_CODES.ExtendGreen
      ) {
        return
      }

      const tMs = Date.parse(e.timestamp as string)
      if (!Number.isFinite(tMs)) return
      if (tMs < rowStart || tMs > rowEnd) return

      const key = `${e.eventCode}|${i}|${e.timestamp}`
      if (seen.has(key)) return
      seen.add(key)

      const point: [string, number] = [e.timestamp, yValue - 100]

      if (e.eventCode === TSP_CODES.EarlyGreen) {
        earlyGreens.push(point)
      } else {
        extendGreens.push(point)
      }
    })
  })

  const createSeries = (
    name: string,
    symbol: string,
    data: Array<[string, number]>
  ): SeriesOption => ({
    type: 'scatter',
    name,
    symbol,
    symbolSize: 9,
    itemStyle: { color: Color.Black },
    z: 6,
    tooltip: {
      show: true,
      formatter: (p: any) => `${p.seriesName} ${p?.value?.[0] ?? ''}`,
    },
    data,
  })

  const result: SeriesOption[] = []

  if (earlyGreens.length) {
    result.push(createSeries('Early Green (113)', 'circle', earlyGreens))
  }

  if (extendGreens.length) {
    result.push(
      createSeries('Extend Green (114)', triangleSvgSymbol, extendGreens)
    )
  }

  return result
}

type TspHistoricEvent = {
  code: number
  timestamp: string
  timestampMs: number
}

const CYCLE_BAND_HEIGHT = 10
const TSP_OVERLAY_Z = 7

function buildTspRequestAndServiceLineSeries(
  rows: RawTimeSpaceHistoricData[] = [],
  distanceData: number[] = []
): SeriesOption[] {
  const tspRequestData: ((string | number)[] | null)[] = []
  const tspServiceData: ((string | number)[] | null)[] = []

  rows.forEach((row, i) => {
    const yValue = distanceData[i]
    if (yValue == null) return

    const rowStartMs = Date.parse(row.start)
    const rowEndMs = Date.parse(row.end)
    if (!Number.isFinite(rowStartMs) || !Number.isFinite(rowEndMs)) return

    const tspEvents = (row.tspEvents ?? []) as IndianaEvent[]
    if (!tspEvents.length) return

    const relevantEvents: TspHistoricEvent[] = tspEvents
      .map((event) => {
        const code = event.eventCode
        const timestampRaw = event.timestamp
        const timestampMs = Date.parse(timestampRaw as string)

        if (
          !Number.isFinite(code) ||
          !timestampRaw ||
          !Number.isFinite(timestampMs)
        ) {
          return null
        }

        if (
          code !== TSP_CODES.CheckIn &&
          code !== TSP_CODES.CheckOut &&
          code !== TSP_CODES.ServiceStart &&
          code !== TSP_CODES.ServiceEnd
        ) {
          return null
        }

        if (timestampMs < rowStartMs || timestampMs > rowEndMs) return null

        const timestamp = dateToTimestamp(timestampRaw)
        if (!timestamp) return null

        return { code, timestamp, timestampMs }
      })
      .filter((event): event is TspHistoricEvent => event !== null)
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

    relevantEvents.forEach((event) => {
      if (event.code === TSP_CODES.CheckIn) {
        requestStart = event
        return
      }

      if (event.code === TSP_CODES.CheckOut) {
        if (requestStart && event.timestampMs > requestStart.timestampMs) {
          tspRequestData.push(
            [requestStart.timestamp, yValue - 60],
            [event.timestamp, yValue - 60],
            null
          )
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
          tspServiceData.push(
            [serviceStart.timestamp, yValue],
            [event.timestamp, yValue],
            null
          )
        }
        serviceStart = null
      }
    })
  })

  const series: SeriesOption[] = []

  if (tspRequestData.length) {
    series.push({
      name: 'TSP Request (112-115)',
      type: 'line',
      data: tspRequestData,
      symbol: 'none',
      lineStyle: {
        width: CYCLE_BAND_HEIGHT,
        color: Color.Red,
        opacity: 0.95,
      },
      z: TSP_OVERLAY_Z,
      tooltip: { show: false },
    })
  }

  if (tspServiceData.length) {
    series.push({
      name: 'TSP Service (118-119)',
      type: 'line',
      data: tspServiceData,
      symbol: 'none',
      lineStyle: {
        width: CYCLE_BAND_HEIGHT,
        color: Color.LightBlue,
        opacity: 0.95,
      },
      z: TSP_OVERLAY_Z,
      tooltip: { show: false },
    })
  }

  return series
}
