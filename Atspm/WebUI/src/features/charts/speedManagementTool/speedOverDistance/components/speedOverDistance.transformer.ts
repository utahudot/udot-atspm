import {
  createDataZoom,
  createGrid,
  createLegend,
  createTitle,
  createTooltip,
  createYAxis,
} from '@/features/charts/common/transformers'
import { ExtendedEChartsOption } from '@/features/charts/types'
import { Color, SolidLineSeriesSymbol } from '@/features/charts/utils'
import { SpeedOverDistanceResponse } from '@/features/speedManagementTool/api/getSpeedOverDistanceChart'

export default function transformSpeedOverDistanceData(
  response: SpeedOverDistanceResponse[]
) {
  // Sort the response by startingMilePoint before processing
  const sortedResponse = response.sort(
    (a, b) => a.startingMilePoint - b.startingMilePoint
  )

  const title = createTitle({
    title: 'Speed Over Distance',
    dateRange: '',
  })

  const xAxis = {
    type: 'value',
    name: 'Mile Points',
    min: Math.min(
      ...sortedResponse.map((segment) => segment.startingMilePoint)
    ),
    max: Math.max(...sortedResponse.map((segment) => segment.endingMilePoint)),
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

  // Destructure the series data from mergeSeriesData
  const { averageSpeedData, eightyFifthPercentileData, speedLimitData } =
    mergeSeriesData(sortedResponse)

  const series = [
    {
      name: 'Average Speed',
      data: averageSpeedData,
      type: 'line',
      step: 'start',
      color: Color.Blue,
    },
    {
      name: '85th Percentile Speed',
      data: eightyFifthPercentileData,
      type: 'line',
      step: 'start',
      color: Color.Red,
    },
    {
      name: 'Speed Limit',
      data: speedLimitData,
      type: 'line',
      step: 'start',
      lineStyle: {
        type: 'dashed',
        color: '#000', // Black dashed line for the speed limit
      },
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
  const averageSpeedData: [number, number | null][] = []
  const eightyFifthPercentileData: [number, number | null][] = []
  const speedLimitData: [number, number | null][] = []

  response.forEach((segment, index) => {
    const startingMilePoint = segment.startingMilePoint
    const endingMilePoint = segment.endingMilePoint

    // Extract average, eightyFifth, and speedLimit directly
    const averageValue = segment.average
    const eightyFifthValue = segment.eightyFifth
    const speedLimitValue = segment.speedLimit

    // Only add data if values are present
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
  dataArray: [number, number | null][], // Array of [milepoint, value] pairs
  index: number,
  startingMilePoint: number,
  endingMilePoint: number,
  value: number,
  response: SpeedOverDistanceResponse[]
) {
  const previousSegment = response[index - 1]

  // If this is the first segment, just add the points
  if (index === 0) {
    dataArray.push([startingMilePoint, value])
    dataArray.push([endingMilePoint, value])
  } else {
    // Add a break if segments are not continuous
    if (previousSegment.endingMilePoint !== startingMilePoint) {
      // Insert a break (null) between non-continuous segments
      dataArray.push([previousSegment.endingMilePoint, null]) // Break in the data
      dataArray.push([startingMilePoint, value])
    }

    // Always add the ending mile point
    dataArray.push([endingMilePoint, value])
  }
}
