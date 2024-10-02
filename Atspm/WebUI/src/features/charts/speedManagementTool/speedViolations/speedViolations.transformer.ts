import {
  createLegend,
  createTitle,
  createToolbox,
  createTooltip,
} from '@/features/charts/common/transformers'
import { ExtendedEChartsOption } from '@/features/charts/types'
import { formatChartDateTimeRange } from '@/features/charts/utils'
import { SM_ChartType } from '@/features/speedManagementTool/api/getSMCharts'
import { SeriesOption } from 'echarts'

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
    dataPoints[dataPoints.length - 1].date,
    'date'
  )

  const title = createTitle({
    title: `Speed Violations - ${segment.segmentName}`,
    dateRange,
  })

  const dateList = dataPoints.map((dp) => dp.date.split('T')[0])

  const xAxis = [
    {
      type: 'category',
      data: dateList,
      gridIndex: 0,
      axisTick: {
        alignWithLabel: true,
      },
    },
    {
      type: 'category',
      data: dateList,
      gridIndex: 1,
      axisTick: {
        alignWithLabel: true,
      },
    },
  ]

  const yAxis = [
    // First Chart Y-Axes
    {
      type: 'value',
      name: 'Violations Count',
      position: 'left',
      nameTextStyle: { align: 'right' },
      axisLine: {
        show: true,
      },
      axisLabel: {
        formatter: '{value}',
      },
      gridIndex: 0,
    },
    {
      type: 'value',
      name: 'Total Flow',
      position: 'right',
      axisLine: {
        show: true,
      },
      axisLabel: {
        formatter: '{value}',
      },
      gridIndex: 0,
    },
    // Second Chart Y-Axes
    {
      type: 'value',
      name: '% Violations',
      position: 'left',
      nameTextStyle: { align: 'right' },
      axisLine: {
        show: true,
      },
      axisLabel: {
        formatter: '{value}%',
      },
      max: 100,
      gridIndex: 1,
    },
    {
      type: 'value',
      name: 'Total Flow',
      position: 'right',
      axisLine: {
        show: true,
      },
      axisLabel: {
        formatter: '{value}',
      },
      gridIndex: 1,
    },
  ]

  const grid = [
    {
      top: 90,
      left: 80,
      right: 320,
      height: '30%',
    },
    {
      left: 80,
      right: 320,
      top: '57%',
      height: '30%',
      bottom: 200, // Added bottom buffer for data zoom
    },
  ]

  const legend = [
    createLegend({
      data: ['Total Flow', 'Violations Count', 'Violations Extreme Count'],
      top: 40,
    }),
    createLegend({
      data: ['Total Flow', '% Violations', '% Extreme Violations'],
      top: '60%',
    }),
  ]

  const dataZoom = [
    {
      type: 'slider',
      xAxisIndex: [0, 1],
      start: 0,
      end: 100,
    },
  ]

  const toolbox = createToolbox()
  const tooltip = createTooltip({
    trigger: 'axis',
    axisPointer: {
      type: 'cross',
    },
  })

  const series: SeriesOption[] = [
    // First Chart Series
    {
      name: 'Total Flow',
      type: 'line',
      lineStyle: {
        type: 'dashed',
      },
      symbolSize: 0,
      data: dataPoints.map((dp) => dp.dailyFlow),
      xAxisIndex: 0,
      yAxisIndex: 1,
      color: 'black',
    },
    {
      name: 'Violations Count',
      type: 'line',
      areaStyle: { opacity: 0.8 },
      color: '#57b9ff',
      symbolSize: 0,
      data: dataPoints.map((dp) => dp.dailyViolationsCount),
      xAxisIndex: 0,
      yAxisIndex: 0,
    },
    {
      name: 'Violations Extreme Count',
      type: 'line',
      areaStyle: { opacity: 0.8 },
      color: '#ff9248', // Ensured consistent color
      symbolSize: 0,
      data: dataPoints.map((dp) => dp.dailyExtremeViolationsCount),
      xAxisIndex: 0,
      yAxisIndex: 0,
    },
    // Second Chart Series
    {
      name: 'Total Flow',
      type: 'line',
      lineStyle: {
        type: 'dashed',
      },
      symbolSize: 0,
      data: dataPoints.map((dp) => dp.dailyFlow),
      xAxisIndex: 1,
      yAxisIndex: 3,
      color: 'black',
    },
    {
      name: '% Violations',
      type: 'line',
      areaStyle: { opacity: 0.8 },
      symbolSize: 0,
      color: '#57b9ff',
      data: dataPoints.map((dp) => dp.dailyPercentViolations * 100),
      xAxisIndex: 1,
      yAxisIndex: 2,
      tooltip: {
        valueFormatter: (value) => `${(value as number).toFixed(2)}%`,
      },
    },
    {
      name: '% Extreme Violations',
      type: 'line',
      areaStyle: { opacity: 0.8 },
      symbolSize: 0,
      data: dataPoints.map((dp) => dp.dailyPercentExtremeViolations * 100),
      color: '#ff9248', // Ensured consistent color
      xAxisIndex: 1,
      yAxisIndex: 2,
      tooltip: {
        valueFormatter: (value) => `${(value as number).toFixed(2)}%`,
      },
    },
  ]

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
  }

  return chartOptions
}
