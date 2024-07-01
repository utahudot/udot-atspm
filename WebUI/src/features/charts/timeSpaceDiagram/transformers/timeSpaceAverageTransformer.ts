import { format } from 'date-fns'
import {
  DataZoomComponentOption,
  EChartsOption,
  GridComponentOption,
  SeriesOption,
} from 'echarts'
import {
  createDataZoom,
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
import { RawTimeSpaceAverageData, RawTimeSpaceDiagramResponse } from '../types'
import {
  generateCycles,
  generateGreenEventLines,
  generateOpposingCycleLabels, 
  generatePrimaryCycleLabels,
  getDistancesLabelOption,
  getLocationsLabelOption,
  getOffsetAndProgramSplitLabel,
} from './timeSpaceTransformerBase'

export default function transformTimeSpaceAverageData(
  response: RawTimeSpaceDiagramResponse
): TransformedToolResponse {
  const chart = {
    chart: transformData(response.data as RawTimeSpaceAverageData[]),
  }
  return {
    type: ToolType.TimeSpaceHistoric,
    data: {
      charts: [chart],
    },
  }
}

function transformData(data: RawTimeSpaceAverageData[]): EChartsOption {
  const primaryPhaseData = data.filter(
    (location) => location.phaseType === 'Primary'
  )

  const opposingPhaseData = data.filter(
    (location) => location.phaseType === 'Opposing'
  )
  const titleHeader = `Time Space Diagram (50th Percentile),\nPrimary Phase - ${primaryPhaseData[0].approachDescription},\nOpposing Phase - ${opposingPhaseData[0].approachDescription},\nCoordinated Phases - ${data[0].coordinatedPhases}`
  const dateRange = formatChartDateTimeRange(data[0].start, data[0].end)
  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: `Route data from ${data[0].locationDescription} to ${
      data[data.length - 1].locationDescription
    } \n`,
  })

  const startDate = new Date(data[0].start)
  const endDate = new Date(data[0].end)

  startDate.setHours(
    endDate.getHours(),
    endDate.getMinutes(),
    endDate.getSeconds()
  )

  const endDateFormat = format(startDate, "yyyy-MM-dd'T'HH:mm:ss")

  const xAxis = createXAxis(data[0].start, endDateFormat)

  const primaryDirection = primaryPhaseData[0].approachDescription.split(' ')[0]
  const opposingDirection =
    opposingPhaseData[0].approachDescription.split(' ')[0]

  let initialDistance = 0
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
    top: 75,
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
    ],
  })

  const grid: GridComponentOption = {
    top: 200,
    left: 100,
    right: 210,
    show: true,
    borderWidth: 1,
    // borderColor: Color.Black,
  }

  const start = new Date(
    data[0].cycleAllEvents[data[0].cycleAllEvents?.length - 1].start
  )
  const end = new Date(data[0].cycleAllEvents[0].start)
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
        filterMode: 'none',
        show: true,
        minSpan: 0.2,
      },
    ]
  } else {
    dataZoom = createDataZoom()
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
    generateGreenEventLines(primaryPhaseData, distanceData, primaryDirection)
  )

  series.push(getLocationsLabelOption(primaryPhaseData, distanceData))
  series.push(getDistancesLabelOption(primaryPhaseData, distanceData))
  series.push(
    getOffsetAndProgramSplitLabel(
      primaryPhaseData,
      opposingPhaseData,
      distanceData,
      primaryDirection,
      opposingDirection,
      endDateFormat
    )
  )
  series.push(generatePrimaryCycleLabels(distanceData, primaryDirection))

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
    generateGreenEventLines(
      opposingPhaseData,
      reverseDistanceData,
      opposingDirection
    )
  )

  series.push(
    generateOpposingCycleLabels(reverseDistanceData, opposingDirection)
  )

  const displayProps = createDisplayProps({
    description: '',
    numberOfLocations: data.length,
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
