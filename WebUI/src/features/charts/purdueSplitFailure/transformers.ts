import {
  createDataZoom,
  createDisplayProps,
  createGrid,
  createInfoString,
  createLegend,
  createPlans,
  createPolyLines,
  createSeries,
  createTitle,
  createToolbox,
  createTooltip,
  createXAxis,
  createYAxis,
  formatDataPointForStepView,
  transformSeriesData,
} from '@/features/charts/common/transformers'
import { ChartType, PlanOptions } from '@/features/charts/common/types'
import {
  PurdueSplitFailurePlan,
  RawPurdueSplitFailureData,
  RawPurdueSplitFailureResponse,
} from '@/features/charts/purdueSplitFailure/types'
import { TransformedChartResponse } from '@/features/charts/types'
import {
  Color,
  DashedLineSeriesSymbol,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
  triangleSvgSymbol,
} from '@/features/charts/utils'
import { EChartsOption } from 'echarts'

export default function transformPurdueSplitFailureData(
  response: RawPurdueSplitFailureResponse
): TransformedChartResponse {
  const charts = response.data.map((data) => {
    const chartOptions = transformData(data)
    return {
      chart: chartOptions,
    }
  })

  return {
    type: ChartType.PurdueSplitFailure,
    data: {
      charts,
    },
  }
}

function transformData(data: RawPurdueSplitFailureData) {
  const {
    plans,
    failLines,
    gapOutGreenOccupancies,
    gapOutRedOccupancies,
    forceOffGreenOccupancies,
    forceOffRedOccupancies,
    averageGor,
    averageRor,
    percentFails,
  } = data

  const info = createInfoString([
    'Total Split Failures: ',
    data.totalSplitFails.toLocaleString(),
  ])

  const titleHeader = `Purdue Split Failure\n${data.locationDescription} - ${data.approachDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: info,
  })

  const xAxis = createXAxis(data.start, data.end)
  const yAxis = createYAxis(true, {
    name: 'Occupancy Ratio',
    nameGap: 50,
    axisLabel: { formatter: (value: number) => `${value}%` },
  })

  const grid = createGrid({
    top: 210,
    left: 65,
    right: 165,
  })

  const gapOutGreenText = 'Gap Out Green\nOccupancies'
  const gapOutRedText = 'Gap Out Red\nOccupancies'
  const forceOffGreenText = 'Force Off Green\nOccupancies'
  const forceOffRedText = 'Force Off Red\nOccupancies'
  const averageRedText = 'Average Red\nOccupancies'
  const averageGreenText = 'Average Green\nOccupancies'
  const failLinesText = 'Fail Lines'
  const percentFailsText = 'Percent Fails'

  const legend = createLegend({
    data: [
      { name: gapOutGreenText },
      { name: gapOutRedText },
      { name: forceOffGreenText },
      { name: forceOffRedText },
      { name: averageRedText, icon: SolidLineSeriesSymbol },
      { name: averageGreenText, icon: SolidLineSeriesSymbol },
      { name: failLinesText, icon: SolidLineSeriesSymbol },
      { name: percentFailsText, icon: DashedLineSeriesSymbol },
    ],
  })

  const dataZoom = createDataZoom()

  const toolbox = createToolbox(
    { title: titleHeader, dateRange },
    data.locationIdentifier,
    ChartType.PurdueSplitFailure
  )

  const tooltip = createTooltip()

  const averageOnRedViewData = formatDataPointForStepView(averageRor, data.end)
  const averageOnGreenViewData = formatDataPointForStepView(
    averageGor,
    data.end
  )
  const percentFailsViewData = formatDataPointForStepView(
    percentFails,
    data.end
  )

  const series = createSeries(
    {
      name: gapOutGreenText,
      data: transformSeriesData(gapOutGreenOccupancies),
      type: 'scatter',
      color: Color.Green,
      symbolSize: 5,
      symbol: triangleSvgSymbol,
      zlevel: 1,
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}%`,
      },
    },
    {
      name: gapOutRedText,
      data: transformSeriesData(gapOutRedOccupancies),
      type: 'scatter',
      color: Color.Red,
      symbolSize: 5,
      symbol: triangleSvgSymbol,
      zlevel: 1,
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}%`,
      },
    },
    {
      name: forceOffGreenText,
      data: transformSeriesData(forceOffGreenOccupancies),
      type: 'scatter',
      color: Color.Green,
      symbolSize: 5,
      zlevel: 1,
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}%`,
      },
    },
    {
      name: forceOffRedText,
      data: transformSeriesData(forceOffRedOccupancies),
      type: 'scatter',
      color: Color.Red,
      symbolSize: 5,
      zlevel: 1,
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}%`,
      },
    },
    {
      name: averageRedText,
      data: transformSeriesData(averageRor),
      type: 'custom',
      color: Color.Red,
      zlevel: 1,
      clip: true,
      renderItem(param, api) {
        if (param.dataIndex === 0) {
          const polyLines = createPolyLines(averageOnRedViewData, api)

          return {
            type: 'group',
            children: polyLines,
          }
        }
      },
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}%`,
      },
    },
    {
      name: averageGreenText,
      data: transformSeriesData(averageGor),
      type: 'custom',
      color: Color.Green,
      zlevel: 1,
      clip: true,
      renderItem(param, api) {
        if (param.dataIndex === 0) {
          const polyLines = createPolyLines(averageOnGreenViewData, api)

          return {
            type: 'group',
            children: polyLines,
          }
        }
      },
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}%`,
      },
    },
    {
      name: percentFailsText,
      data: transformSeriesData(percentFails),
      type: 'custom',
      color: Color.LightBlue,
      zlevel: 1,
      clip: true,
      renderItem(param, api) {
        if (param.dataIndex === 0) {
          const polyLines = createPolyLines(percentFailsViewData, api, 'dashed')

          return {
            type: 'group',
            children: polyLines,
          }
        }
      },
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}%`,
      },
    },
    {
      name: failLinesText,
      symbol: 'none',
      type: 'line',
      color: Color.Yellow,
      markLine: {
        symbol: ['none', 'none'],
        lineStyle: {
          type: 'solid',
          width: 2,
        },
        label: {
          show: false,
        },
        data: failLines.map((failLine) => ({
          xAxis: failLine.timestamp,
        })),
      },
      tooltip: {
        valueFormatter: (value) => `${value}%`,
      },
    }
  )

  const planOptions: PlanOptions<PurdueSplitFailurePlan> = {
    failsInPlan: (value: number) => `${value} SF`,
    percentFails: (value: number) => `${Math.round(value)}% SF`,
    totalCycles: (value: number) => `${value} Cycles`,
  }
  const plansSeries = createPlans(plans, yAxis.length, planOptions)

  const displayProps = createDisplayProps({
    description: data.approachDescription,
  })

  const chartOptions: EChartsOption = {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    dataZoom,
    toolbox,
    tooltip,
    animation: false,
    series: [...series, plansSeries],
    displayProps,
  }

  return chartOptions
}
