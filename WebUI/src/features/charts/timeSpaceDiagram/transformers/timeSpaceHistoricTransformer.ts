import {
  DataZoomComponentOption,
  EChartsOption,
  GridComponentOption,
  SeriesOption,
} from 'echarts'
import {
  createDisplayProps,
  createLegend,
  createTitle,
  createToolbox,
  createXAxis,
  createYAxis,
} from '../../common/transformers'
import { ToolType } from '../../common/types'
import { TransformedToolResponse } from '../../types'
import { SolidLineSeriesSymbol, formatChartDateTimeRange } from '../../utils'
import { RawTimeSpaceDiagramResponse, RawTimeSpaceHistoricData } from '../types'
import {
  generateCycles,
  generateGreenEventLines,
  generateOpposingCycleLabels,
  generatePrimaryCycleLabels,
  getDistancesLabelOption,
  getLocationsLabelOption,
} from './timeSpaceTransformerBase'

export default function transformTimeSpaceHistoricData(
  response: RawTimeSpaceDiagramResponse
): TransformedToolResponse {
  const chart = {
    chart: transformData(response.data as RawTimeSpaceHistoricData[]),
  }
  return {
    type: ToolType.TimeSpaceHistoric,
    data: {
      charts: [chart],
    },
  }
}

function transformData(data: RawTimeSpaceHistoricData[]): EChartsOption {
  const primaryPhaseData = data.filter(
    (location) => location.phaseType === 'Primary'
  )

  const opposingPhaseData = data.filter(
    (location) => location.phaseType === 'Opposing'
  )

  const titleHeader = `Time Space Diagram (Historic),\nPrimary Phase - ${primaryPhaseData[0].approachDescription}\nOpposing Phase - ${opposingPhaseData[0].approachDescription}`
  const dateRange = formatChartDateTimeRange(data[0].start, data[0].end)
  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: `Route data from ${primaryPhaseData[0].locationDescription} to ${primaryPhaseData[primaryPhaseData.length-1].locationDescription} \n`,
  })

  const xAxis = createXAxis(data[0].start, data[0].end)

  let initialDistance = 0

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

  const legends = createLegend({
    top: 60,
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
        name: `Green Bands ${primaryDirection}`,
        itemStyle: { color: 'green', opacity: 0.3 },
      },
      {
        name: `Green Bands ${opposingDirection}`,
        itemStyle: { color: 'green', opacity: 0.3 },
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
    },
  })

  const grid: GridComponentOption = {
    top: 200,
    left: 100,
    right: 210,
    show: true,
    borderWidth: 1,
  }

  const start = new Date(data[0].end)
  const end = new Date(data[0].start)
  const timeDiff = (start.getTime() - end.getTime()) / 3600000

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
      {
        type: 'inside',
        filterMode: 'filter',
        show: true,
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
      {
        type: 'inside',
        filterMode: 'none',
        show: true,
      },
    ]
  }

  const toolbox = createToolbox(
    { title: titleHeader, dateRange },
    data[0].locationIdentifier,
    ToolType.TimeSpaceHistoric
  )

  const colorMap: Map<number, string> = new Map([
    [1, 'lightgreen'],
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
    generateLaneByLaneCountEventLines(
      primaryPhaseData,
      distanceData,
      'darkblue',
      primaryDirection
    )
  )

  series.push(
    generateAdvanceCountEventLines(
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
      primaryDirection
    )
  )

  series.push(
    generateGreenEventLines(primaryPhaseData, distanceData, primaryDirection)
  )
  series.push(getLocationsLabelOption(primaryPhaseData, distanceData))
  series.push(getDistancesLabelOption(primaryPhaseData, distanceData))

  let reverseDistanceData = distanceData.reverse()
  reverseDistanceData = reverseDistanceData.map((distance) => (distance += 120))
  series.push(
    ...generateCycles(
      opposingPhaseData,
      reverseDistanceData,
      colorMap,
      opposingDirection
    )
  )

  series.push(
    generateLaneByLaneCountEventLines(
      opposingPhaseData,
      reverseDistanceData,
      'orange',
      opposingDirection
    )
  )

  series.push(
    generateAdvanceCountEventLines(
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
      opposingDirection
    )
  )

  series.push(
    generateGreenEventLines(
      opposingPhaseData,
      reverseDistanceData,
      opposingDirection
    )
  )

  series.push(generatePrimaryCycleLabels(distanceData, primaryDirection))
  series.push(
    generateOpposingCycleLabels(reverseDistanceData, opposingDirection)
  )

  const displayProps = createDisplayProps({
    description: '',
    numberOfLocations: primaryPhaseData.length,
  })

  const chartOptions: EChartsOption = {
    title: title,
    xAxis: xAxis,
    yAxis: yAxis,
    grid: grid,
    dataZoom: dataZoom,
    legend: legends,
    toolbox: toolbox,
    animation: false,
    series: series,
    displayProps,
  }

  return chartOptions
}

function generateLaneByLaneCountEventLines(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  color: string,
  phaseType?: string
): SeriesOption {
  return {
    name: `Lane by Lane Count ${phaseType?.length && phaseType}`,
    type: 'line',
    symbol: 'none',
    lineStyle: {
      width: 2,
      color,
    },
    data: data.reduce((result, location, i) => {
      if (location.laneByLaneCountDetectors) {
        const points: any[] = location.laneByLaneCountDetectors.flatMap(
          (events) => {
            const values = [
              [events.initialX, distanceData[i]],
              [events.finalX, distanceData[i + 1]],
              null,
            ]
            return values
          }
        )
        points.push(null)
        result.push(...points)
      }
      return result
    }, [] as any[]),
  }
}

function generateAdvanceCountEventLines(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  color: string,
  phaseType?: string
): SeriesOption {
  return {
    name: `Advance Count ${phaseType?.length && phaseType}`,
    type: 'line',
    symbol: 'none',
    lineStyle: {
      width: 2,
      color,
    },
    data: data.reduce((result, location, i) => {
      if (location.advanceCountDetectors) {
        const points: any[] = location.advanceCountDetectors.flatMap(
          (events) => {
            const values = [
              [events.initialX, distanceData[i - 1]],
              [events.finalX, distanceData[i]],
              null,
            ]
            return values
          }
        )
        // points.push(null)
        result.push(...points)
      }
      return result
    }, [] as any[]),
  }
}

function generateStopBarPresenceEventLines(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  color: string,
  phaseType?: string
): SeriesOption[] {
  const dataPoints = getStopBarPresenceDataPoints(data, distanceData)
  const chunkSize = 1000
  const options: SeriesOption[] = []

  for (let i = 0, j = 0; i < dataPoints.length; i += chunkSize, j++) {
    const startIndex = j * chunkSize
    const endIndex = Math.min((j + 1) * chunkSize, dataPoints.length)
    const chunk = dataPoints.slice(startIndex, endIndex)

    options.push({
      name: `Stop Bar Presence ${phaseType?.length && phaseType}`,
      type: 'custom',
      data: chunk,
      clip: true,
      selectedMode: false,
      renderItem: function (params, api) {
        if (params.context.rendered) {
          return
        }
        params.context.rendered = true
        let points = []
        const polygons: any[] = []
        for (let j = 0; j < chunk.length; j++) {
          if (chunk[j] === null) {
            polygons.push({
              type: 'polygon',
              transition: ['shape'],
              shape: {
                points: points,
              },
              style: {
                opacity: 1,
                fill: color,
                lineWidth: 3,
              },
            })
            points = []
          } else {
            points.push(api.coord(chunk[j]))
          }
        }
        return {
          type: 'group',
          children: polygons,
        }
      },
    })
  }

  return options
}

function getStopBarPresenceDataPoints(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[]
) {
  return data.reduce((result, location, index) => {
    if (location.stopBarPresenceDetectors) {
      const stopBarEvents = location.stopBarPresenceDetectors
      for (let i = 0; i < stopBarEvents.length; ) {
        const currPoint = stopBarEvents[i]
        const nextPoint = stopBarEvents[i + 1]
        if (i === 0 && currPoint.isDetectorOn === false) {
          result.push(
            [location.start, distanceData[index]],
            [currPoint.initialX, distanceData[index]],
            [currPoint.finalX, distanceData[index + 1]],
            [location.start, distanceData[index + 1]],
            null
          )
          i++
        } else if (
          i === stopBarEvents.length - 1 &&
          currPoint.isDetectorOn === true
        ) {
          result.push(
            [currPoint.initialX, distanceData[index]],
            [location.end, distanceData[index]],
            [location.end, distanceData[index + 1]],
            [currPoint.finalX, distanceData[index + 1]],
            null
          )
          i++
        } else {
          result.push(
            ...[
              [currPoint.initialX, distanceData[index]],
              [nextPoint.initialX, distanceData[index]],
              [nextPoint.finalX, distanceData[index + 1]],
              [currPoint.finalX, distanceData[index + 1]],
              null,
            ]
          )
          i += 2
        }
      }
    }
    return result
  }, [] as any)
}
