import { SpeedVariabilityDto } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { DataZoomComponentOption } from 'echarts'
import {
  createDisplayProps,
  createGrid,
  createLegend,
  createSeries,
  createTitle,
  createTooltip,
  createYAxis,
  transformSeriesData,
} from '../../common/transformers'
import { DataPoint } from '../../common/types'
import { ExtendedEChartsOption } from '../../types'
import { Color, SolidLineSeriesSymbol } from '../../utils'

export default function transformSpeedVariabilityData(
  response: SpeedVariabilityDto
) {
  const title = createTitle({
    title: `Speed Variability\n${
      response.segmentName
    } (between MP ${response.startingMilePoint.toFixed(
      1
    )} and MP ${response.startingMilePoint.toFixed(1)})`,
    dateRange: '',
  })
  const speedVariability = 'Speed Variability'
  const maxSpeed = 'Max Speed'
  const minSpeed = 'Min Speed'

  const xAxis = {
    type: 'time',
    name: 'Time',
    nameGap: 30,
    nameLocation: 'middle',
    splitNumber: 10,
    minorTick: {
      show: true,
      splitNumber: 2,
    },
    boundaryGap: true,
  }

  const yAxis = createYAxis(true, { name: 'Speed (mph)' })

  const grid = createGrid({
    top: 100,
    left: 60,
    right: 210,
  })

  const legend = createLegend({
    data: [
      { name: speedVariability, icon: SolidLineSeriesSymbol },
      { name: maxSpeed, icon: SolidLineSeriesSymbol },
      { name: minSpeed, icon: SolidLineSeriesSymbol },
    ],
  })

  const dataZoom: DataZoomComponentOption[] = [
    {
      type: 'slider',
      filterMode: 'weakFilter',
      show: true,
      minSpan: 0.2,
    },
    {
      type: 'inside',
      filterMode: 'weakFilter',
      show: true,
      minSpan: 0.2,
    },
  ]

  const tooltip = createTooltip()

  // const seriesData = transformData(response)
  const minSpeeds: DataPoint[] = response.data.map((variability) => {
    return {
      timestamp: variability.date,
      value: variability.minSpeed,
    }
  })
  const maxSpeeds = response.data.map((variability) => {
    return {
      timestamp: variability.date,
      value: variability.maxSpeed,
    }
  })
  const variabilityData = response.data.map((variability) => {
    return {
      timestamp: variability.date,
      value: variability.speedVariability,
    }
  })

  const series = createSeries(
    {
      name: maxSpeed,
      data: transformSeriesData(maxSpeeds),
      type: 'line',
      color: Color.Blue,
      clip: true,
    },
    {
      name: minSpeed,
      data: transformSeriesData(minSpeeds),
      type: 'line',
      stack: 'speed',
      color: Color.Blue,
      clip: true,
    },
    {
      name: speedVariability,
      data: transformSeriesData(variabilityData),
      type: 'bar',
      stack: 'speed',
      color: Color.Orange,
      clip: true,
      barWidth: 3,
      barMaxWidth: 5,
    }
  )

  // series.push(customBars)
  const displayProp = createDisplayProps({
    data: response,
    description: '',
  })

  const chartOptions: ExtendedEChartsOption = {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    tooltip,
    series,
    dataZoom,
    displayProp,
  }

  return chartOptions
}
