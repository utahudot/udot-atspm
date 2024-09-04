import {
  createDataZoom,
  createGrid,
  createLegend,
  createSeries,
  createTitle,
  createTooltip,
  createXAxis,
  createYAxis,
  transformSeriesData,
} from '@/features/charts/common/transformers'
import { ExtendedEChartsOption } from '@/features/charts/types'
import { Color, SolidLineSeriesSymbol } from '@/features/charts/utils'
import { SpeedOverDistanceResponse } from '@/features/speedManagementTool/api/getSpeedOverDistanceChart'

export default function transformSpeedOverTimeData(
  response: SpeedOverDistanceResponse
) {
  const title = createTitle({
    title: `Speed Over Time\n${
      response.segmentName
    } (between MP ${response.startingMilePoint.toFixed(
      1
    )} and MP ${response.endingMilePoint.toFixed(1)})`,
    dateRange: '',
  })

  // todo add date range

  const averageSpeed = 'Average Speed (mph)'
  const eightyFifthPercentile = '85th Percentile Speed (mph)'

  const xAxis = createXAxis()

  const yAxis = createYAxis(true, { name: 'Speed (mph)' })

  const grid = createGrid({
    top: 100,
    left: 60,
    right: 210,
  })

  const legend = createLegend({
    data: [
      { name: averageSpeed, icon: SolidLineSeriesSymbol },
      { name: eightyFifthPercentile, icon: SolidLineSeriesSymbol },
    ],
  })

  const dataZoom = createDataZoom()

  const tooltip = createTooltip()

  const seriesData = transformData(response)

  const series = createSeries(
    {
      name: averageSpeed,
      data: transformSeriesData(seriesData.average),
      type: 'line',
      color: Color.Blue,
    },
    {
      name: eightyFifthPercentile,
      data: transformSeriesData(seriesData.eightyFifth),
      type: 'line',
      color: Color.Red,
    }
  )

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

function transformData(response) {
  const result = {
    average: [],
    eightyFifth: [],
  }

  response.data.forEach((entry) => {
    const { series } = entry
    const { average, eightyFifth } = series

    if (average && Array.isArray(average)) {
      average.forEach((item) => {
        result.average.push({ timestamp: item.timestamp, value: item.value })
      })
    }

    if (eightyFifth && Array.isArray(eightyFifth)) {
      eightyFifth.forEach((item) => {
        result.eightyFifth.push({
          timestamp: item.timestamp,
          value: item.value,
        })
      })
    }
  })

  return result
}
