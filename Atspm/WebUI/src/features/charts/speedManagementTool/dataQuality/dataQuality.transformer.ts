import {
  createDataZoom,
  createGrid,
  createLegend,
  createTitle,
  createTooltip,
  createYAxis,
} from '@/features/charts/common/transformers'
import { ExtendedEChartsOption } from '@/features/charts/types'
import {
  Color,
  DashedLineSeriesSymbol,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import { round } from '@/utils/math'

export default function transformDataQualityData(response: any[]) {
  console.log('response', response)
  return null
  const sortedResponse = response
    .map((segment) => {
      if (segment.startingMilePoint > segment.endingMilePoint) {
        return {
          ...segment,
          startingMilePoint: segment.endingMilePoint,
          endingMilePoint: segment.startingMilePoint,
        }
      }
      return segment
    })
    .sort((a, b) => a.startingMilePoint - b.startingMilePoint)

  const dateRange = formatChartDateTimeRange(
    response[0].startDate,
    response[0].endDate
  )

  const title = createTitle({
    title: 'Speed Over Distance',
    dateRange: dateRange,
  })

  const xAxis = {
    type: 'value',
    name: 'Mile Points',
    min: round(
      Math.min(...sortedResponse.map((segment) => segment.startingMilePoint)),
      2
    ),
    max: round(
      Math.max(...sortedResponse.map((segment) => segment.endingMilePoint)),
      2
    ),
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
      { name: 'Speed Limit', icon: DashedLineSeriesSymbol },
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
      showSymbol: false,
      color: Color.Blue,
    },
    {
      name: '85th Percentile Speed',
      data: eightyFifthPercentileData,
      type: 'line',
      step: 'start',
      showSymbol: false,
      color: Color.Red,
    },
    {
      name: 'Speed Limit',
      data: speedLimitData,
      type: 'line',
      step: 'start',
      showSymbol: false,
      color: Color.Black,
      lineStyle: {
        type: 'dashed',
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

  console.log('chartOptions', chartOptions)

  return chartOptions
}

function mergeSeriesData(response: DataQualityResponse[]) {
  const averageSpeedData: [number, number | null][] = []
  const eightyFifthPercentileData: [number, number | null][] = []
  const speedLimitData: [number, number | null][] = []

  response.forEach((segment, index) => {
    const {
      startingMilePoint,
      endingMilePoint,
      average,
      eightyFifth,
      speedLimit,
    } = segment
    const previousSegment = response[index - 1]

    // Ensure the startingMilePoint of the first segment is added
    if (index === 0) {
      if (average !== undefined) {
        averageSpeedData.push([startingMilePoint, average])
      }
      if (eightyFifth !== undefined) {
        eightyFifthPercentileData.push([startingMilePoint, eightyFifth])
      }
      if (speedLimit !== undefined) {
        speedLimitData.push([startingMilePoint, speedLimit])
      }
    }

    // Handle the break between non-continuous segments by adding [null, null]
    if (index > 0 && previousSegment.endingMilePoint !== startingMilePoint) {
      // Add a break for non-continuous segments
      averageSpeedData.push([null, null])
      eightyFifthPercentileData.push([null, null])
      speedLimitData.push([null, null])

      // Add the startingMilePoint of the current segment
      if (average !== undefined) {
        averageSpeedData.push([startingMilePoint, average])
      }
      if (eightyFifth !== undefined) {
        eightyFifthPercentileData.push([startingMilePoint, eightyFifth])
      }
      if (speedLimit !== undefined) {
        speedLimitData.push([startingMilePoint, speedLimit])
      }
    }

    // Always add the endingMilePoint for each segment
    if (average !== undefined) {
      averageSpeedData.push([endingMilePoint, average])
    }
    if (eightyFifth !== undefined) {
      eightyFifthPercentileData.push([endingMilePoint, eightyFifth])
    }
    if (speedLimit !== undefined) {
      speedLimitData.push([endingMilePoint, speedLimit])
    }
  })

  return {
    averageSpeedData,
    eightyFifthPercentileData,
    speedLimitData,
  }
}
