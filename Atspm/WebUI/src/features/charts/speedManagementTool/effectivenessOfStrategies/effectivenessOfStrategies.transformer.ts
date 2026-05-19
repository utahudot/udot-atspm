import {
  EffectivenessOfStrategiesDto,
  ImpactSpeedAggregationCategoryFilter,
  TimeSegmentEffectiveness,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
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
  formatChartDateRange,
} from '@/features/charts/utils'
import { SM_ChartType } from '@/features/speedManagementTool/api/getSMCharts'
import {
  LegendComponentOption,
  SeriesOption,
  XAXisComponentOption,
} from 'echarts'

const AcceptedImpactSpeedAggregationCategories: ImpactSpeedAggregationCategoryFilter[] =
  [
    ImpactSpeedAggregationCategoryFilter.ChangeInAverageSpeed,
    ImpactSpeedAggregationCategoryFilter.ChangeInEightyFifthPercentileSpeed,
    ImpactSpeedAggregationCategoryFilter.ChangeInPercentViolations,
    ImpactSpeedAggregationCategoryFilter.ChangeInPercentExtremeViolations,
  ]

// Transformer for Effectiveness of Strategies chart data
export default function transformEffectivenessOfStrategiesData(
  segmentData: EffectivenessOfStrategiesDto[],
  customSpeedLimit?: string,
  impactType?: ImpactSpeedAggregationCategoryFilter
) {
  const chart = transformData(segmentData, customSpeedLimit, impactType)

  return {
    type: SM_ChartType.EFFECTIVENESS_OF_STRATEGIES,
    charts: [chart],
  }
}

function transformData(
  segmentData: EffectivenessOfStrategiesDto[],
  customSpeedLimit?: string,
  impactType?: ImpactSpeedAggregationCategoryFilter
): ExtendedEChartsOption {
  let categoryFilter: ImpactSpeedAggregationCategoryFilter | null = null
  if (impactType && impactType.length > 0) {
    categoryFilter = AcceptedImpactSpeedAggregationCategories.includes(
      impactType
    )
      ? impactType
      : null
  }

  const dateRange = formatChartDateRange(
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
    {
      name: 'Speed (MPH)',
      nameGap: 35,
    },
    { name: 'Percent Violations (%)' }
  )

  const xAxis: XAXisComponentOption = {
    type: 'category',
    data: (
      segmentData[0].weeklyEffectiveness as TimeSegmentEffectiveness[]
    ).map((week) => `${formatDate(week.startDate as string)}`),
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

  const legend = createLegends(categoryFilter)
  const dataZoom = createDataZoom()
  const tooltip = createTooltip()

  // const {
  //   averageSpeedData,
  //   eightyFifthPercentileData,
  //   speedLimitData,
  //   percentViolationsData,
  //   percentExtremeViolationsData,
  // } = mergeEffectivenessSeriesData(segmentData, customSpeedLimit)

  const series = createSeries(segmentData, categoryFilter, customSpeedLimit)

  // const series = [
  //   {
  //     name: 'Average Speed',
  //     data: averageSpeedData,
  //     type: 'line',
  //     showSymbol: false,
  //     color: Color.Blue,
  //     yAxisIndex: 0,
  //   },
  //   {
  //     name: '85th Percentile Speed',
  //     data: eightyFifthPercentileData,
  //     type: 'line',
  //     showSymbol: false,
  //     color: Color.Red,
  //     yAxisIndex: 0,
  //   },
  //   {
  //     name: 'Speed Limit',
  //     data: speedLimitData,
  //     type: 'line',
  //     showSymbol: false,
  //     color: Color.Black,
  //     lineStyle: {
  //       type: 'dashed',
  //     },
  //     yAxisIndex: 0,
  //   },
  //   {
  //     name: 'Percent Violations',
  //     data: percentViolationsData,
  //     type: 'line',
  //     showSymbol: false,
  //     color: Color.Yellow,
  //     yAxisIndex: 1,
  //     lineStyle: {
  //       type: 'dashed',
  //     },
  //   },
  //   {
  //     name: 'Percent Extreme Violations',
  //     data: percentExtremeViolationsData,
  //     type: 'line',
  //     showSymbol: false,
  //     color: Color.Green,
  //     yAxisIndex: 1,
  //     lineStyle: {
  //       type: 'dashed',
  //     },
  //   },
  // ]

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

function createSeries(
  eoSData: EffectivenessOfStrategiesDto[],
  categoryFilter: ImpactSpeedAggregationCategoryFilter | null,
  customSpeedLimit?: string
): SeriesOption[] {
  let series: SeriesOption[] = []

  if (categoryFilter && categoryFilter.length > 0) {
    series = createCustomSeries(eoSData, categoryFilter)
  } else {
    series = createDefaultSeries(eoSData, customSpeedLimit)
  }

  return series
}

function createDefaultSeries(
  eoSData: EffectivenessOfStrategiesDto[],
  customSpeedLimit?: string
): SeriesOption[] {
  return eoSData
    .map((item) => {
      return [
        {
          name: `Average Speed`,
          data: (item.weeklyEffectiveness as TimeSegmentEffectiveness[]).map(
            (weeklyData) => {
              const weekLabel = `${formatDate(weeklyData.startDate as string)}`
              return [weekLabel, weeklyData.averageSpeed]
            }
          ),
          type: 'line',
          showSymbol: false,
          color: Color.Blue,
          yAxisIndex: 0,
          endLabel: eoSData.length > 1 && {
            show: true,
            formatter: () => {
              return item.segmentName
            },
          },
          tooltip: {
            valueFormatter: (value: number) => `${Math.round(value)}`,
          },
        },
        {
          name: `85th Percentile Speed`,
          data: (item.weeklyEffectiveness as TimeSegmentEffectiveness[]).map(
            (weeklyData) => {
              const weekLabel = `${formatDate(weeklyData.startDate as string)}`
              return [weekLabel, weeklyData.averageEightyFifthSpeed]
            }
          ),
          type: 'line',
          showSymbol: false,
          color: Color.Red,
          yAxisIndex: 0,
          tooltip: {
            valueFormatter: (value: number) => `${Math.round(value)}`,
          },
        },
        {
          name: `Speed Limit`,
          data: (item.weeklyEffectiveness as TimeSegmentEffectiveness[]).map(
            (weeklyData) => {
              const weekLabel = `${formatDate(weeklyData.startDate as string)}`
              return [
                weekLabel,
                customSpeedLimit ? customSpeedLimit : item.speedLimit,
              ]
            }
          ),
          type: 'line',
          showSymbol: false,
          color: Color.Black,
          lineStyle: {
            type: 'dashed',
          },
        },
        {
          name: `Percent Violations`,
          data: (item.weeklyEffectiveness as TimeSegmentEffectiveness[]).map(
            (weeklyData) => {
              const weekLabel = `${formatDate(weeklyData.startDate as string)}`
              return [weekLabel, weeklyData.percentViolations]
            }
          ),
          type: 'line',
          showSymbol: false,
          color: Color.Yellow,
          yAxisIndex: 1,
          lineStyle: {
            type: 'dashed',
          },
        },
        {
          name: `Percent Extreme Violations`,
          data: (item.weeklyEffectiveness as TimeSegmentEffectiveness[]).map(
            (weeklyData) => {
              const weekLabel = `${formatDate(weeklyData.startDate as string)}`
              return [weekLabel, weeklyData.percentExtremeViolations]
            }
          ),
          type: 'line',
          showSymbol: false,
          color: Color.Green,
          yAxisIndex: 1,
          lineStyle: {
            type: 'dashed',
          },
        },
      ]
    })
    .flat() as SeriesOption[]
}

function createCustomSeries(
  eoSData: EffectivenessOfStrategiesDto[],
  categoryFilter: ImpactSpeedAggregationCategoryFilter
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = eoSData.map((segment) => {
    const data = (
      segment.weeklyEffectiveness as TimeSegmentEffectiveness[]
    ).map((week) => {
      const weekLabel = `${formatDate(week.startDate as string)}`
      switch (categoryFilter) {
        case ImpactSpeedAggregationCategoryFilter.ChangeInAverageSpeed:
          return [weekLabel, week.averageSpeed]
        case ImpactSpeedAggregationCategoryFilter.ChangeInEightyFifthPercentileSpeed:
          return [weekLabel, week.averageEightyFifthSpeed]
        case ImpactSpeedAggregationCategoryFilter.ChangeInPercentViolations:
          return [weekLabel, week.percentViolations]
        case ImpactSpeedAggregationCategoryFilter.ChangeInPercentExtremeViolations:
          return [weekLabel, week.percentExtremeViolations]
        default:
          return [weekLabel, null]
      }
    })

    // Customize style and configuration based on the `item`
    const config: {
      name: string
      color: string
      lineStyle?: object
      yAxisIndex?: number
    } = (() => {
      switch (categoryFilter) {
        case ImpactSpeedAggregationCategoryFilter.ChangeInAverageSpeed:
          return { name: 'Average Speed', color: Color.Blue, yAxisIndex: 0 }
        case ImpactSpeedAggregationCategoryFilter.ChangeInEightyFifthPercentileSpeed:
          return {
            name: '85th Percentile Speed',
            color: Color.Red,
            yAxisIndex: 0,
          }
        case ImpactSpeedAggregationCategoryFilter.ChangeInPercentViolations:
          return {
            name: 'Percent Violations',
            color: Color.Yellow,
            lineStyle: { type: 'dashed' },
            yAxisIndex: 1,
          }
        case ImpactSpeedAggregationCategoryFilter.ChangeInPercentExtremeViolations:
          return {
            name: 'Percent Extreme Violations',
            color: Color.Green,
            lineStyle: { type: 'dashed' },
            yAxisIndex: 1,
          }
        default:
          return {
            name: 'Speed Limit',
            color: Color.Black,
            lineStyle: { type: 'dashed' },
            yAxisIndex: 0,
          }
      }
    })()

    return {
      type: 'line',
      showSymbol: false,
      data,
      endLabel: {
        show: true,
        formatter: () => {
          return segment.segmentName as string
        },
      },
      ...config,
    }
  })
  return seriesOptions
}

// Merges series data for the Effectiveness of Strategies chart
// function mergeEffectivenessSeriesData(segmentData, customSpeedLimit?: string) {
//   const averageSpeedData: [string, number | null][] = []
//   const eightyFifthPercentileData: [string, number | null][] = []
//   const speedLimitData: [string, number | null][] = []
//   const percentViolationsData: [string, number | null][] = []
//   const percentExtremeViolationsData: [string, number | null][] = []

//   segmentData.forEach((segment) => {
//     segment.weeklyEffectiveness.forEach((week) => {
//       const {
//         startDate,
//         endDate,
//         averageSpeed,
//         averageEightyFifthSpeed,
//         speedLimit,
//         percentViolations,
//         percentExtremeViolations,
//       } = week

//       const weekLabel = `${formatDate(startDate)}`
//       averageSpeedData.push([
//         weekLabel,
//         averageSpeed !== undefined ? averageSpeed : null,
//       ])
//       eightyFifthPercentileData.push([
//         weekLabel,
//         averageEightyFifthSpeed !== undefined ? averageEightyFifthSpeed : null,
//       ])
//       speedLimitData.push([
//         weekLabel,
//         customSpeedLimit ? customSpeedLimit : segmentData[0].speedLimit,
//       ])
//       percentViolationsData.push([
//         weekLabel,
//         percentViolations !== undefined ? percentViolations : null,
//       ])
//       percentExtremeViolationsData.push([
//         weekLabel,
//         percentExtremeViolations !== undefined
//           ? percentExtremeViolations
//           : null,
//       ])
//     })
//   })

//   return {
//     averageSpeedData,
//     eightyFifthPercentileData,
//     speedLimitData,
//     percentViolationsData,
//     percentExtremeViolationsData,
//   }
// }

// Format date helper function
function formatDate(dateString: string): string {
  const date = new Date(dateString)
  const month = (date.getMonth() + 1).toString().padStart(2, '0')
  const day = date.getDate().toString().padStart(2, '0')
  const year = date.getFullYear()

  return `${month}/${day}/${year}\n`
}

function createLegends(
  categoryFilter: ImpactSpeedAggregationCategoryFilter | null
): Partial<LegendComponentOption> {
  let legend: Partial<LegendComponentOption> = {}
  if (categoryFilter && categoryFilter.length > 0) {
    const legendData: { name: string; icon: string }[] = []
    switch (categoryFilter) {
      case ImpactSpeedAggregationCategoryFilter.ChangeInAverageSpeed:
        legendData.push({ name: 'Average Speed', icon: SolidLineSeriesSymbol })
      case ImpactSpeedAggregationCategoryFilter.ChangeInEightyFifthPercentileSpeed:
        legendData.push({
          name: '85th Percentile Speed',
          icon: SolidLineSeriesSymbol,
        })
      case ImpactSpeedAggregationCategoryFilter.ChangeInPercentViolations:
        legendData.push({
          name: 'Percent Violations',
          icon: SolidLineSeriesSymbol,
        })
      case ImpactSpeedAggregationCategoryFilter.ChangeInPercentExtremeViolations:
        legendData.push({
          name: 'Percent Extreme Violations',
          icon: SolidLineSeriesSymbol,
        })
    }
    legendData.push({ name: 'Speed Limit', icon: DashedLineSeriesSymbol })
    if (legendData.length > 0) {
      legend = createLegend({
        data: legendData as { name: string; icon: string }[],
      })
    }
  } else {
    legend = createLegend({
      data: [
        { name: 'Speed Limit', icon: DashedLineSeriesSymbol },
        { name: 'Average Speed', icon: SolidLineSeriesSymbol },
        { name: '85th Percentile Speed', icon: SolidLineSeriesSymbol },
        { name: 'Percent Violations', icon: SolidLineSeriesSymbol },
        { name: 'Percent Extreme Violations', icon: SolidLineSeriesSymbol },
        { name: 'Speed Limit', icon: DashedLineSeriesSymbol },
      ],
    })
  }

  return legend
}
