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
  getTimeSpacePhaseRowDistances,
  getDistancesLabelOption,
  getLocationsLabelOption,
  TIME_SPACE_MOVEMENT_SERIES_Z,
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

const opacity = 1
const MIN_SEGMENT = 2000
const DISPLAY_DISTANCE_UNITS_PER_PIXEL = 18
const Y_AXIS_EDGE_BUFFER_PX = 25
const Y_AXIS_PADDING =
  Y_AXIS_EDGE_BUFFER_PX * DISPLAY_DISTANCE_UNITS_PER_PIXEL
const MIN_ROW_HEIGHT_PX = 100
const DISPLAY_HEIGHT_BASE = 220
const PASSIVE_DETECTION_SERIES_PROPS = {
  silent: true,
  tooltip: { show: false },
} as const

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
const PEDESTRIAN_LINE_OFFSET_PX = 13
const PEDESTRIAN_ZIGZAG_AMPLITUDE = 2
const PEDESTRIAN_ZIGZAG_STEP_PX = 3
const PEDESTRIAN_CLEARANCE_DOT_PATTERN = [1, 3]
const PEDESTRIAN_BOUNDARY_TICK_HALF_HEIGHT = PEDESTRIAN_ZIGZAG_AMPLITUDE

function getDisplayDistanceScale(distanceData: number[]): number {
  const segments = distanceData.map((value, index) => {
    if (index === 0) {
      return 0
    }

    return value - distanceData[index - 1]
  })

  const positiveSegments = segments.filter((segment) => segment > 0)
  const minSegment = positiveSegments.length ? Math.min(...positiveSegments) : 0

  return minSegment > 0 && minSegment < MIN_SEGMENT
    ? MIN_SEGMENT / minSegment
    : 1
}

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
    {
      ...createXAxis(data[0].start, data[0].end),
      axisLine: {
        onZero: false,
      },
    },
    { min: 0, max: 1, show: false },
    xAxisTopSeconds,
  ]

  let initialDistance = 0

  const primaryDirection = primaryPhaseData[0].approachDescription
  const opposingDirection = opposingPhaseData[0].approachDescription

  const rawDistanceData: number[] = []
  primaryPhaseData.forEach((location) => {
    rawDistanceData.push(initialDistance)
    initialDistance += location.distanceToNextLocation
  })
  const distanceScale = getDisplayDistanceScale(rawDistanceData)
  const locationCenterDistanceData = rawDistanceData.map(
    (distance) => distance * distanceScale
  )
  const {
    primaryDistanceData,
    opposingDistanceData,
  } = getTimeSpacePhaseRowDistances(locationCenterDistanceData)
  const minDisplayDistance = Math.min(
    ...primaryDistanceData,
    ...opposingDistanceData
  )
  const maxDisplayDistance = Math.max(
    ...primaryDistanceData,
    ...opposingDistanceData
  )
  const yAxis = createYAxis(false, {
    show: false,
    data: locationCenterDistanceData,
    axisTick: { show: true },
    max: maxDisplayDistance + Y_AXIS_PADDING,
    min: minDisplayDistance - Y_AXIS_PADDING,
  })

  const title = createTitle({
    title: 'Time Space Diagram • Historic',
    location: dateRange,
  })

  const grid: GridComponentOption = {
    top: 30,
    left: 220,
    right: 195,
    bottom: 80,
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
    ...generateCycles(
      primaryPhaseData,
      primaryDistanceData,
      primaryDirection,
      'primary'
    )
  )

  series.push(
    ...generateLaneByLaneCountEventLines(
      opposingPhaseData,
      opposingDistanceData,
      'orange',
      opposingDirection,
      false,
      distanceScale
    )
  )

  series.push(
    ...generateAdvanceCountEventLines(
      opposingPhaseData,
      opposingDistanceData,
      'orange',
      opposingDirection,
      false,
      distanceScale
    )
  )

  series.push(
    ...generateStopBarPresenceEventLines(
      opposingPhaseData,
      opposingDistanceData,
      'orange',
      opposingDirection,
      false,
      distanceScale
    )
  )

  series.push(
    ...generateLaneByLaneCountEventLines(
      primaryPhaseData,
      primaryDistanceData,
      'darkblue',
      primaryDirection,
      true,
      distanceScale
    )
  )

  series.push(
    ...generateAdvanceCountEventLines(
      primaryPhaseData,
      primaryDistanceData,
      'darkblue',
      primaryDirection,
      true,
      distanceScale
    )
  )

  series.push(
    ...generateStopBarPresenceEventLines(
      primaryPhaseData,
      primaryDistanceData,
      'darkblue',
      primaryDirection,
      true,
      distanceScale
    )
  )

  series.push(
    ...generateGreenEventLines(
      primaryPhaseData,
      primaryDistanceData,
      primaryDirection,
      true,
      distanceScale,
      'primary'
    )
  )

  series.push(
    ...generatePedestrianIntervalLines(
      primaryPhaseData,
      primaryDistanceData,
      primaryDirection,
      'primary'
    )
  )
  series.push(
    ...generateSrmEntityLines(
      primaryPhaseData,
      primaryDistanceData,
      primaryDirection,
      distanceScale,
      'primary'
    )
  )

  series.push(
    ...generateTMCEvent(
      primaryPhaseData,
      primaryDistanceData,
      primaryDirection,
      distanceScale,
      'primary'
    )
  )

  series.push(
    ...buildCycleEventMarkersOnCyclesSeries(
      primaryPhaseData,
      primaryDistanceData,
      'primary'
    )
  )

  series.push(
    ...buildTspRequestAndServiceLineSeries(
      primaryPhaseData,
      primaryDistanceData,
      'primary'
    )
  )

  const locationLabels = getLocationsLabelOption(
    primaryPhaseData,
    locationCenterDistanceData,
    grid
  )
  series.push(locationLabels)
  series.push(
    getDistancesLabelOption(
      primaryPhaseData,
      locationCenterDistanceData,
      grid.left as number,
      distanceScale
    )
  )
  series.push(
    ...generateCycles(
      opposingPhaseData,
      opposingDistanceData,
      opposingDirection,
      'opposing'
    )
  )

  series.push(
    ...generateGreenEventLines(
      opposingPhaseData,
      opposingDistanceData,
      opposingDirection,
      false,
      distanceScale,
      'opposing'
    )
  )

  series.push(
    ...generatePedestrianIntervalLines(
      opposingPhaseData,
      opposingDistanceData,
      opposingDirection,
      'opposing'
    )
  )

  series.push(
    ...generateSrmEntityLines(
      opposingPhaseData,
      opposingDistanceData,
      opposingDirection,
      distanceScale,
      'opposing'
    )
  )

  series.push(
    ...buildCycleEventMarkersOnCyclesSeries(
      opposingPhaseData,
      opposingDistanceData,
      'opposing'
    )
  )

  series.push(
    ...buildTspRequestAndServiceLineSeries(
      opposingPhaseData,
      opposingDistanceData,
      'opposing'
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

  const primaryHeadersByIndex = primaryPhaseData.map(
    (p) => p.approachDescription
  )

  const opposingHeadersByIndex = [...opposingPhaseData]
    .reverse()
    .map((p) => p.approachDescription)

  const opposingLinesByIndex = [...opposingPhaseData]
    .reverse()
    .map((p) => [`AOG: ${formatPct(p.percentArrivalOnGreen)}`])

  const primaryIgnoredByIndex = primaryPhaseData.map((p) =>
    Boolean(p.isIgnoredLocation)
  )

  const opposingIgnoredByIndex = [...opposingPhaseData]
    .reverse()
    .map((p) => Boolean(p.isIgnoredLocation))

  const primaryLabelSeries = generateCycleLabels(
    locationCenterDistanceData,
    primaryDirection,
    grid.left as number,
    primaryHeadersByIndex,
    primaryLinesByIndex,
    'left',
    primaryIgnoredByIndex
  )

  const opposingLabelSeries = generateCycleLabels(
    locationCenterDistanceData,
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

  const minHeightFromRows = primaryPhaseData.length * MIN_ROW_HEIGHT_PX
  const heightFromDistance =
    Math.ceil(
      (maxDisplayDistance - minDisplayDistance + Y_AXIS_PADDING * 2) /
        DISPLAY_DISTANCE_UNITS_PER_PIXEL
    ) + DISPLAY_HEIGHT_BASE
  const chartHeight = Math.max(heightFromDistance, minHeightFromRows + DISPLAY_HEIGHT_BASE)

  const displayProps = createDisplayProps({
    description: '',
    numberOfLocations: primaryPhaseData.length,
    height: chartHeight,
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
        itemStyle: {
          color: Color.White,
          borderColor: Color.Black,
          borderWidth: 1.5,
        },
      },
      {
        name: `Extend Green (114)`,
        itemStyle: {
          color: Color.White,
          borderColor: Color.Black,
          borderWidth: 1.5,
        },
      },
      {
        name: `TSP Request (112-115)`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Red },
      },
      {
        name: `TSP Service (118-119)`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
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

  // console.log('Chart Options:', chartOptions)

  return chartOptions
}

function generateLaneByLaneCountEventLines(
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
        const values = [
          [initialX, distanceData[i]],
          [finalX, distanceData[i] + displayDistanceToNext],
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
        const values = [
          [initialX, previousDistance],
          [finalX, currentDistance],
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
  isPrimary?: boolean,
  distanceScale = 1
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []

  for (let i = 0; i < data.length; i++) {
    const location = data[i]
    if (!location.stopBarPresenceDetectors) continue
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
        const i = params.dataIndex
        if (!dataPoints || i >= dataPoints.length - 1 || i % 2 !== 0) {
          return
        }
        const nextIndex = i + 1
        const distanceToNext = isPrimary
          ? location.calculatedDistanceToNext
          : -location.calculatedDistanceToNext
        const displayDistanceToNext = distanceToNext * distanceScale
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
          api.coord([nextPointFinalX, (y2 as number) + displayDistanceToNext]),
          api.coord([currPointFinalX, (y1 as number) + displayDistanceToNext]),
        ]
        return {
          type: 'polygon',
          transition: ['shape'],
          emphasisDisabled: true,
          shape: {
            points: points,
          },
          style: {
            opacity: 1,
            fill: color,
            fillOpacity: opacity,
            lineWidth: 2,
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
  phaseType?: string,
  idScope = 'default'
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []
  const directionMultiplier = idScope === 'opposing' ? -1 : 1

  data.forEach((location, i) => {
    if (!location.pedestrianIntervals?.length) return

    const pedData = generatePedData(
      location.pedestrianIntervals,
      distanceData[i]
    )

    const series: SeriesOption = {
      name: `Pedestrian Interval ${phaseType?.length ? phaseType : ''}`,
      id: `PI ${location.locationIdentifier} ${phaseType ?? ''} row-${i} ${idScope}`,
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
        const y = p1[1] + directionMultiplier * PEDESTRIAN_LINE_OFFSET_PX

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
  phaseType?: string,
  distanceScale = 1,
  idScope = 'default'
) {
  const seriesOptions: SeriesOption[] = []

  data.forEach((location, i) => {
    if (!location.tmcForPhase) return

    const leftTurnEvents = location.tmcForPhase.leftTurnEvents.flatMap((lEvent) => {
      const initialX = lEvent.start
      const finalX = getArrivalTime(
        location.calculatedDistanceToNext,
        location.speed,
        initialX
      )

      return [
        [initialX, distanceData[i]],
        [finalX, distanceData[i] + location.calculatedDistanceToNext * distanceScale],
        null,
      ]
    })

    if (leftTurnEvents.length) {
      seriesOptions.push({
        name: `Left Turn ${phaseType}`,
        id: `Left Turn ${location.locationIdentifier} ${phaseType ?? ''} row-${i} ${idScope}`,
        type: 'line',
        data: leftTurnEvents,
        symbol: 'none',
        z: TIME_SPACE_MOVEMENT_SERIES_Z,
        color: 'black',
      })
    }

    const rightTurnEvents = location.tmcForPhase.rightTurnEvents.flatMap(
      (rEvent) => {
        const initialX = rEvent.start
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
        data: rightTurnEvents,
        symbol: 'none',
        z: TIME_SPACE_MOVEMENT_SERIES_Z,
        color: 'black',
      })
    }
  })

  return seriesOptions
}

function generateSrmEntityLines(
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
      ])

      seriesOptions.push({
        name: `SRM Entity ${phaseType?.length ? phaseType : ''}`,
        id: `SRM ${location.locationIdentifier} ${track.entityId} ${trackIndex} ${
          phaseType ?? ''
        } row-${i} ${idScope}`,
        type: 'line',
        symbol: 'none',
        z: TIME_SPACE_MOVEMENT_SERIES_Z,
        lineStyle: {
          width: 2,
          color: Color.Black,
          opacity: 0.85,
        },
        data: points,
      })
    })
  })

  return seriesOptions
}

function buildCycleEventMarkersOnCyclesSeries(
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
      const timestamp = dateToTimestamp(e.timestamp as string)
      if (!timestamp) return

      const key = `${e.eventCode}|${i}|${e.timestamp}`
      if (seen.has(key)) return
      seen.add(key)

      const point: [string, number] = [timestamp, yValue]

      if (e.eventCode === TSP_CODES.EarlyGreen) {
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
        formatter: (p: any) => `${p.seriesName} ${p?.value?.[0] ?? ''}`,
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

type TspHistoricEvent = {
  code: number
  timestamp: string
  timestampMs: number
}

const TSP_OVERLAY_Z = 7
const TSP_REQUEST_BAND_HEIGHT_PX = 2
const TSP_SERVICE_BAND_HEIGHT_PX = 5
const TSP_SERVICE_OVERLAY_Z = TSP_OVERLAY_Z + 1
const TSP_SERVICE_OFFSET_PX = 20
const TSP_REQUEST_OFFSET_PX = 20
const TSP_MARKER_OFFSET_PX = 20
const TSP_MARKER_Z = TSP_SERVICE_OVERLAY_Z + 1

function buildTspRequestAndServiceLineSeries(
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
            typeof endValue === 'number' ? endValue : Date.parse(String(endValue))
          if (
            !Number.isFinite(startMs) ||
            !Number.isFinite(endMs) ||
            !Number.isFinite(rowY)
          ) {
            return
          }

          const startPoint = api.coord([startMs, rowY])
          const endPoint = api.coord([endMs, rowY])
          const centerY = startPoint[1] + directionMultiplier * TSP_REQUEST_OFFSET_PX

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
              fill: Color.Red,
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
            typeof endValue === 'number' ? endValue : Date.parse(String(endValue))
          if (
            !Number.isFinite(startMs) ||
            !Number.isFinite(endMs) ||
            !Number.isFinite(rowY)
          ) {
            return
          }

          const startPoint = api.coord([startMs, rowY])
          const endPoint = api.coord([endMs, rowY])
          const centerY = startPoint[1] + directionMultiplier * TSP_SERVICE_OFFSET_PX
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
              fill: Color.Black,
              opacity: 0.95,
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

