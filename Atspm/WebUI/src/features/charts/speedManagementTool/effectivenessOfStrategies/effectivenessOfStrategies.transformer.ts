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

// Transformer for Effectiveness of Strategies chart data
export default function transformEffectivenessOfStrategiesData(
  segmentData,
  customSpeedLimit?: string
) {
  const dateRange = formatChartDateTimeRange(
    segmentData[0].weeklyEffectiveness[0].startDate,
    segmentData[0].weeklyEffectiveness[
      segmentData[0].weeklyEffectiveness.length - 1
    ].endDate
  )

  const title = createTitle({
    title: 'Effectiveness of Strategies',
    dateRange: dateRange,
  })

  const yAxis = createYAxis(
    false,
    { name: 'Speed (MPH)', nameGap: 35 },
    { name: 'Percent Violations (%)' }
  )

  const xAxis = {
    type: 'category',
    data: segmentData[0].weeklyEffectiveness.map(
      (week) => `${formatDate(week.startDate)}`
    ),
    name: '',
    axisLabel: {
      rotate: 45,
      formatter: (value: string) => value,
    },
  }

  const grid = createGrid({
    top: 100,
    left: 60,
    right: 270,
    bottom: 120,
  })

  const legend = createLegend({
    data: [
      { name: 'Average Speed', icon: SolidLineSeriesSymbol },
      { name: '85th Percentile Speed', icon: SolidLineSeriesSymbol },
      { name: 'Speed Limit', icon: DashedLineSeriesSymbol },
      { name: 'Percent Violations', icon: SolidLineSeriesSymbol },
      { name: 'Percent Extreme Violations', icon: SolidLineSeriesSymbol },
    ],
  })

  const dataZoom = createDataZoom()
  const tooltip = createTooltip()

  const {
    averageSpeedData,
    eightyFifthPercentileData,
    speedLimitData,
    percentViolationsData,
    percentExtremeViolationsData,
  } = mergeEffectivenessSeriesData(segmentData, customSpeedLimit)

  const series = [
    {
      name: 'Average Speed',
      data: averageSpeedData,
      type: 'line',
      showSymbol: false,
      color: Color.Blue,
      yAxisIndex: 0,
    },
    {
      name: '85th Percentile Speed',
      data: eightyFifthPercentileData,
      type: 'line',
      showSymbol: false,
      color: Color.Red,
      yAxisIndex: 0,
    },
    {
      name: 'Speed Limit',
      data: speedLimitData,
      type: 'line',
      showSymbol: false,
      color: Color.Black,
      lineStyle: {
        type: 'dashed',
      },
      yAxisIndex: 0,
    },
    {
      name: 'Percent Violations',
      data: percentViolationsData,
      type: 'line',
      showSymbol: false,
      color: Color.Green,
      yAxisIndex: 1,
    },
    {
      name: 'Percent Extreme Violations',
      data: percentExtremeViolationsData,
      type: 'line',
      showSymbol: false,
      color: Color.Purple,
      yAxisIndex: 1,
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
    response: segmentData,
  }

  return chartOptions
}

// Merges series data for the Effectiveness of Strategies chart
function mergeEffectivenessSeriesData(segmentData, customSpeedLimit?:string) {
  const averageSpeedData: [string, number | null][] = []
  const eightyFifthPercentileData: [string, number | null][] = []
  const speedLimitData: [string, number | null][] = []
  const percentViolationsData: [string, number | null][] = []
  const percentExtremeViolationsData: [string, number | null][] = []

  segmentData.forEach((segment) => {
    segment.weeklyEffectiveness.forEach((week) => {
      const {
        startDate,
        endDate,
        averageSpeed,
        averageEightyFifthSpeed,
        speedLimit,
        percentViolations,
        percentExtremeViolations,
      } = week

      const weekLabel = `${formatDate(startDate)}`
      averageSpeedData.push([
        weekLabel,
        averageSpeed !== undefined ? averageSpeed : null,
      ])
      eightyFifthPercentileData.push([
        weekLabel,
        averageEightyFifthSpeed !== undefined ? averageEightyFifthSpeed : null,
      ])
      speedLimitData.push([weekLabel, customSpeedLimit ? customSpeedLimit : segmentData[0].speedLimit])
      percentViolationsData.push([
        weekLabel,
        percentViolations !== undefined ? percentViolations : null,
      ])
      percentExtremeViolationsData.push([
        weekLabel,
        percentExtremeViolations !== undefined
          ? percentExtremeViolations
          : null,
      ])
    })
  })

  return {
    averageSpeedData,
    eightyFifthPercentileData,
    speedLimitData,
    percentViolationsData,
    percentExtremeViolationsData,
  }
}

// Format date helper function
function formatDate(dateString: string): string {
  const date = new Date(dateString)
  const month = (date.getMonth() + 1).toString().padStart(2, '0')
  const day = date.getDate().toString().padStart(2, '0')
  const year = date.getFullYear()

  return `${month}/${day}/${year}\n`
}
