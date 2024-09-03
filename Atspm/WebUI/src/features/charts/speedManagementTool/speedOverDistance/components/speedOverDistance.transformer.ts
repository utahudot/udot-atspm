import {
  createDataZoom,
  createGrid,
  createLegend,
  createTitle,
  createTooltip,
  createYAxis,
  transformSeriesData,
} from '@/features/charts/common/transformers'
import { ExtendedEChartsOption } from '@/features/charts/types'
import { Color, SolidLineSeriesSymbol } from '@/features/charts/utils'
import { SpeedOverDistanceResponse } from '@/features/speedManagementTool/api/getSpeedOverDistanceChart'

export default function transformSpeedOverDistanceData(
  response: SpeedOverDistanceResponse[]
) {
  const title = createTitle({
    title: 'Speed Over Distance',
    dateRange: '',
  })

  const xAxis = {
    type: 'value',
    name: 'Mile Points',
    min: Math.min(...response.map((segment) => segment.startingMilePoint)),
    max: Math.max(...response.map((segment) => segment.endingMilePoint)),
  }

  const yAxis = createYAxis(true, { name: 'Speed (mph)' })

  const grid = createGrid({
    top: 100,
    left: 60,
    right: 210,
  })

  const legend = createLegend({
    data: [
      { name: 'Average Speed', icon: SolidLineSeriesSymbol },
      { name: '85th Percentile Speed', icon: SolidLineSeriesSymbol },
      { name: 'Speed Limit', icon: SolidLineSeriesSymbol },
    ],
  })

  const dataZoom = createDataZoom()

  const tooltip = createTooltip()

  // Call mergeSeriesData once and destructure the result
  const { averageSpeedData, eightyFifthPercentileData, speedLimitData } =
    mergeSeriesData(response)

  console.log('averageSpeedData', averageSpeedData)

  const series = [
    {
      name: 'Average Speed',
      data: transformSeriesData(averageSpeedData),
      type: 'line',
      step: 'start',
      color: Color.Blue,
    },
    {
      name: '85th Percentile Speed',
      data: transformSeriesData(eightyFifthPercentileData),
      type: 'line',
      step: 'start',
      color: Color.Red,
    },
    {
      name: 'Speed Limit',
      data: transformSeriesData(speedLimitData),
      type: 'line',
      step: 'start',
      color: Color.Green,
    },
  ]

  const chartOptions: ExtendedEChartsOption = {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    tooltip,
    series,
    dataZoom,
  }

  return chartOptions
}

function mergeSeriesData(response: SpeedOverDistanceResponse[]) {
  const averageSpeedData = []
  const eightyFifthPercentileData = []
  const speedLimitData = []

  response.forEach((segment, index) => {
    const startingMilePoint = segment.startingMilePoint
    const endingMilePoint = segment.endingMilePoint

    const averageValue = segment.data[0].series.average?.find(
      (item) => item.value > 0
    )?.value
    const eightyFifthValue = segment.data[0].series.eightyFifth?.find(
      (item) => item.value > 0
    )?.value
    const speedLimitValue = segment.speedLimit

    if (averageValue !== undefined) {
      addSegmentData(
        averageSpeedData,
        index,
        startingMilePoint,
        endingMilePoint,
        averageValue,
        response
      )
    }
    if (eightyFifthValue !== undefined) {
      addSegmentData(
        eightyFifthPercentileData,
        index,
        startingMilePoint,
        endingMilePoint,
        eightyFifthValue,
        response
      )
    }
    if (speedLimitValue !== undefined) {
      addSegmentData(
        speedLimitData,
        index,
        startingMilePoint,
        endingMilePoint,
        speedLimitValue,
        response
      )
    }
  })

  return {
    averageSpeedData,
    eightyFifthPercentileData,
    speedLimitData,
  }
}

function addSegmentData(
  dataArray: { timestamp: number; value: number }[],
  index: number,
  startingMilePoint: number,
  endingMilePoint: number,
  value: number,
  response: SpeedOverDistanceResponse[]
) {
  const previousSegment = response[index - 1]

  // If this is the first segment, just add the points
  if (index === 0) {
    dataArray.push({ timestamp: startingMilePoint, value })
    dataArray.push({ timestamp: endingMilePoint, value })
  } else {
    // Only add the starting mile point if it's different from the previous segment's end
    if (previousSegment.endingMilePoint !== startingMilePoint) {
      dataArray.push({
        timestamp: previousSegment.endingMilePoint,
        value: dataArray[dataArray.length - 1]?.value,
      })
      dataArray.push({ timestamp: startingMilePoint, value })
    }

    // Always add the ending mile point
    dataArray.push({ timestamp: endingMilePoint, value })
  }
}
