import {
  createDataZoom,
  createGrid,
  createLegend,
  createSeries,
  createTitle,
  createToolbox,
  createTooltip,
  createYAxis,
} from '@/features/charts/common/transformers'
import { ExtendedEChartsOption } from '@/features/charts/types'
import { Color, formatChartDateTimeRange } from '@/features/charts/utils'
import { SM_ChartType } from '@/features/speedManagementTool/api/getSMCharts'

type DailySpeedViolationDto = {
  date: string
  dailyFlow: number
  dailyViolationsCount: number
  dailyExtremeViolationsCount: number
  dailyPercentViolations: number
  dailyPercentExtremeViolations: number
}

type SpeedViolationSegment = {
  segmentId: string
  segmentName: string
  totalFlow: number
  totalViolationsCount: number
  totalExtremeViolationsCount: number
  percentViolations: number
  percentExtremeViolations: number
  speedLimit: number
  dailySpeedViolationsDto: DailySpeedViolationDto[]
}

export type SpeedViolationsResponse = SpeedViolationSegment[]

export default function transformSpeedViolationsData(
  response: SpeedViolationsResponse
) {
  const charts = response
    .map((segment) => {
      const chartOptions = transformSegmentData(segment)
      return chartOptions ? { chart: chartOptions } : null
    })
    .filter(Boolean)

  return {
    type: SM_ChartType.SPEED_VIOLATIONS,
    charts,
  }
}

function transformSegmentData(
  segment: SpeedViolationSegment
): ExtendedEChartsOption | null {
  const dataPoints = segment.dailySpeedViolationsDto

  if (!dataPoints || dataPoints.length === 0) return null

  const dateRange = formatChartDateTimeRange(
    dataPoints[0].date,
    dataPoints[dataPoints.length - 1].date
  )

  const title = createTitle({
    title: `Speed Violations - ${segment.segmentName}`,
    dateRange,
  })

  const dateList = dataPoints.map((dp) => dp.date.split('T')[0])

  const xAxis = {
    type: 'category' as const,
    data: dateList,
    axisTick: {
      alignWithLabel: true,
    },
  }

  const yAxis = createYAxis(
    false,
    {
      name: '% Violations',
      axisLabel: {
        formatter: '{value}%',
      },
      max: 100,
    },
    {
      name: 'Total Flow',
      position: 'right',
      nameGap: 60,
      axisLabel: {
        formatter: '{value}',
      },
    }
  )

  const grid = createGrid({
    top: 90,
    left: 80,
    right: 250,
  })

  const legend = createLegend()

  const dataZoom = createDataZoom()

  const toolbox = createToolbox()
  const tooltip = createTooltip({
    trigger: 'axis',
    axisPointer: {
      type: 'cross',
    },
  })

  const series = createSeries(
    {
      name: 'Total Flow',
      type: 'line',
      lineStyle: {
        type: 'dashed',
      },
      data: dataPoints.map((dp) => [dp.date.split('T')[0], dp.dailyFlow]),
      yAxisIndex: 1,
      color: 'black',
    },
    {
      name: '% Violations',
      type: 'line',
      areaStyle: { opacity: 1 },
      color: Color.Blue,
      data: dataPoints.map((dp) => [
        dp.date.split('T')[0],
        dp.dailyPercentViolations * 100,
      ]),
      yAxisIndex: 0,
      tooltip: {
        valueFormatter: (value) => `${(value as number).toFixed(2)}%`,
      },
    },
    {
      name: '% Extreme Violations',
      type: 'line',
      areaStyle: { opacity: 1 },
      data: dataPoints.map((dp) => [
        dp.date.split('T')[0],
        dp.dailyPercentExtremeViolations * 100,
      ]),
      color: Color.Red,
      yAxisIndex: 0,
      tooltip: {
        valueFormatter: (value) => `${(value as number).toFixed(2)}%`,
      },
    }
  )

  const chartOptions: ExtendedEChartsOption = {
    title,
    tooltip,
    toolbox,
    legend,
    grid,
    xAxis,
    yAxis,
    dataZoom,
    series,
    response: segment,
  }

  return chartOptions
}
