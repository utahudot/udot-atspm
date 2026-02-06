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
  getDistancesLabelOption,
  getDraggableOffsetabelOption,
  getLocationsLabelOption,
} from '@/features/charts/timeSpaceDiagram/shared/transformers/timeSpaceTransformerBase'
import {
  RawTimeSpaceDiagramResponse,
  RawTimeSpaceHistoricData,
} from '@/features/charts/timeSpaceDiagram/shared/types'
import { TransformedTimeSpaceResponse } from '@/features/charts/types'
import {
  formatChartDateTimeRange,
  SolidLineSeriesSymbol,
} from '@/features/charts/utils'
import { dateToTimestamp } from '@/utils/dateTime'
import {
  CustomSeriesRenderItemReturn,
  DataZoomComponentOption,
  EChartsOption,
  GridComponentOption,
  SeriesOption,
} from 'echarts'
import {
  CycleColor,
  EVENT_GROUPS,
} from '../../timingAndActuation/timingAndActuation.transformer'
import { PedestrianInterval } from '../../timingAndActuation/types'

export default function transformTimeSpaceHistoricData(
  response: RawTimeSpaceDiagramResponse
): TransformedTimeSpaceResponse {
  const data = {
    chart: transformData(response.data as RawTimeSpaceHistoricData[]),
  }
  return {
    type: ToolType.TimeSpaceHistoric,
    data,
  }
}

const opacity = 0.4

function transformData(data: RawTimeSpaceHistoricData[]): EChartsOption {
  const primaryPhaseData = data.filter(
    (location) => location.phaseType === 'Primary'
  )

  const opposingPhaseData = data.filter(
    (location) => location.phaseType === 'Opposing'
  )

  const dateRange = formatChartDateTimeRange(data[0].start, data[0].end)

  const xAxis = createXAxis(data[0].start, data[0].end)

  let initialDistance = 250

  const primaryDirection = primaryPhaseData[0].approachDescription.split(' ')[0]
  const opposingDirection =
    opposingPhaseData[0].approachDescription.split(' ')[0]

  const distanceData: number[] = []
  primaryPhaseData.forEach((location) => {
    distanceData.push(initialDistance)
    initialDistance += location.distanceToNextLocation
  })
  const yAxis = createYAxis(false, {
    show: false,
    data: distanceData,
    axisLabel: {
      show: false,
    },
  })

  const primaryPOG = primaryPhaseData.map((phase, index) => {
    return `  ${phase.locationIdentifier} => ${phase.percentArrivalOnGreen}s`
  })

  const opposingPOG = opposingPhaseData.map((phase, index) => {
    return `  ${phase.locationIdentifier} => ${phase.percentArrivalOnGreen}s`
  })

  const title = createTitle({
    title: 'Time Space Diagram • Historic',
    location: `Primary: ${primaryPhaseData[0].approachDescription} • Opposing: ${opposingPhaseData[0].approachDescription}`,
    dateRange,
    info: `Route data from ${primaryPhaseData[0].locationDescription} to ${
      primaryPhaseData[primaryPhaseData.length - 1].locationDescription
    } \n 
    POG for ${primaryDirection}: ${primaryPOG} \n
    POG for ${opposingDirection}: ${opposingPOG}`,
  })

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

  const colorMap: Map<number, string> = new Map([
    [1, 'lightgreen'],
    [3, 'green'],
    [8, 'yellow'],
    [9, 'red'],
  ])

  const series: SeriesOption[] = []

  series.push(
    ...generateCycles(
      primaryPhaseData,
      distanceData,
      colorMap,
      primaryDirection
    )
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
    ...generateTMCEvent(primaryPhaseData, distanceData, primaryDirection)
  )

  const locationLabels = getLocationsLabelOption(primaryPhaseData, distanceData)
  series.push(locationLabels)
  series.push(
    getDistancesLabelOption(
      primaryPhaseData,
      distanceData,
      locationLabels.gridLeft
    )
  )

  let reverseDistanceData = [...distanceData].reverse()
  reverseDistanceData = reverseDistanceData.map((distance) => (distance += 300))
  series.push(
    ...generateCycles(
      opposingPhaseData,
      reverseDistanceData,
      colorMap,
      opposingDirection
    )
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
    generateCycleLabels(distanceData, primaryDirection, locationLabels.gridLeft)
  )
  series.push(
    generateCycleLabels(
      reverseDistanceData,
      opposingDirection,
      locationLabels.gridLeft
    )
  )
  series.push(
    ...getDraggableOffsetabelOption(
      primaryPhaseData,
      distanceData,
      primaryDirection
    )
  )

  const displayProps = createDisplayProps({
    description: '',
    numberOfLocations: primaryPhaseData.length,
    locations: primaryPhaseData.map((p) => p.locationIdentifier),
  })

  const chartStartMs = Date.parse(primaryPhaseData[0].start)
  const chartEndMs = Date.parse(primaryPhaseData[0].end)
  const totalSeconds = Math.floor((chartEndMs - chartStartMs) / 1000)

  const xAxisTopSeconds = {
    type: 'value',
    position: 'top',
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

  const grid: GridComponentOption = {
    top: 210,
    left: locationLabels.gridLeft + 80,
    right: 300,
    bottom: 100,
    show: true,
    borderWidth: 1,
  }

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
        itemStyle: { color: 'grey' },
      },
      {
        name: `Pedestrian Interval ${opposingDirection}`,
        itemStyle: { color: 'grey' },
      },
      {
        name: `Left Turn ${primaryDirection}`,
        itemStyle: { color: 'black' },
      },
      {
        name: `Right Turn ${primaryDirection}`,
        itemStyle: { color: 'black' },
      },
    ],
    selected: {
      [`Cycles ${primaryDirection}`]: true,
      [`Cycles ${opposingDirection}`]: true,
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
      [`Left Turn ${primaryDirection}`]: false,
      [`Right Turn ${primaryDirection}`]: false,
    },
  })

  const chartOptions: EChartsOption = {
    title,
    xAxis: [xAxis, xAxisTopSeconds],
    yAxis,
    grid,
    dataZoom,
    legend: legends,
    toolbox,
    // animation: false,
    series,
    displayProps,
    animation: true,
  }
  console.log(chartOptions)

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
        console.log('this is stopbar', points[0], points[1])
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
      renderItem: (param, api): CustomSeriesRenderItemReturn => {
        const x1 = api.value(0)
        const x2 = api.value(1)
        const interval = api.value(2)
        const distance = api.value(3)
        const { color } = getEventDetails(interval as number)
        const p1 = api.coord([x1, distance])
        const p2 = api.coord([x2, distance])
        return {
          type: 'rect',
          shape: {
            x: p1[0],
            y: p1[1] + 2.5,
            width: p2[0] - p1[0],
            height: 5,
          },
          style: {
            fill: color,
          },
        }
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

function getEventDetails(eventValue: number) {
  for (const group of EVENT_GROUPS) {
    if (group.codes.includes(eventValue)) {
      return { name: group.name, color: group.color }
    }
  }
  return { name: 'Unknown Event', color: CycleColor.Default }
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
