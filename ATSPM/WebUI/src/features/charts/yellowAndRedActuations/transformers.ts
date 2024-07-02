import {
  createDisplayProps,
  createGrid,
  createInfoString,
  createLegend,
  createPlans,
  createSeries,
  createTitle,
  createToolbox,
  createTooltip,
  createXAxis,
  createYAxis,
  transformSeriesData,
} from '@/features/charts/common/transformers'
import { ChartType, PlanOptions } from '@/features/charts/common/types'
import { TransformedChartResponse } from '@/features/charts/types'
import {
  Color,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import { DataZoomComponentOption, EChartsOption } from 'echarts'
import {
  RawYellowAndRedActuationsData,
  RawYellowAndRedActuationsResponse,
  YellowAndRedActuationsPlan,
} from './types'

export default function transformYellowAndRedActuationsData(
  response: RawYellowAndRedActuationsResponse
): TransformedChartResponse {
  const charts = response.data.map((data) => {
    const chartOptions = transformData(data)
    return {
      chart: chartOptions,
    }
  })

  return {
    type: ChartType.YellowAndRedActuations,
    data: {
      charts,
    },
  }
}

function transformData(data: RawYellowAndRedActuationsData) {
  const { plans, redEvents, yellowEvents, redClearanceEvents, detectorEvents } =
    data

  const info = createInfoString(
    ['Total Violations: ', data.totalViolations.toLocaleString()],
    ['Severe Violations (SV): ', data.severeViolations.toLocaleString()],
    [
      'Yellow Light Occurrences (YLO): ',
      data.yellowLightOccurences.toLocaleString(),
    ]
  )

  const titleHeader = `Yellow And Red Actuations \n${data.locationDescription} - ${data.approachDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: info,
  })

  const yAxis = createYAxis(true, {
    name: 'Yellow/Red Time (Seconds)',
    min: 0,
    axisLabel: {
      formatter(value: string) {
        return Math.round(parseInt(value)).toFixed(0)
      },
    },
  })

  const xAxis = createXAxis(data.start, data.end)

  const detectorEventsText = 'Detector Events'
  const yellowChangeText = 'Yellow Change'
  const redClearanceText = 'Red Clearance'

  const legend = createLegend({
    data: [
      { name: detectorEventsText },
      { name: yellowChangeText, icon: SolidLineSeriesSymbol },
      { name: redClearanceText, icon: SolidLineSeriesSymbol },
    ],
  })

  const dataZoom: DataZoomComponentOption[] = [
    {
      type: 'slider',
      filterMode: 'none',
      minSpan: 0.2,
    },
    {
      type: 'slider',
      orient: 'vertical',
      filterMode: 'none',
      right: 160,
      endValue: 20, // todo - should use measure default or something
      yAxisIndex: 0,
      minSpan: 0.2,
    },
    {
      type: 'inside',
      filterMode: 'none',
      minSpan: 0.2,
    },
    {
      type: 'inside',
      orient: 'vertical',
      filterMode: 'none',
      yAxisIndex: 0,
      minSpan: 0.2,
    },
  ]

  const toolbox = createToolbox(
    { title: titleHeader, dateRange },
    data.locationIdentifier,
    ChartType.YellowAndRedActuations
  )

  const tooltip = createTooltip()

  const grid = createGrid({
    top: 230,
    left: 60,
    right: 220,
    backgroundColor: '#FF000050',
  })

  const series = createSeries(
    {
      name: detectorEventsText,
      data: transformSeriesData(detectorEvents),
      type: 'scatter',
      symbolSize: 5,
      color: Color.Black,
      zlevel: 1,
      tooltip: {
        valueFormatter: (value) =>
          `${Math.round(value as number).toLocaleString()}s`,
      },
    },
    {
      name: yellowChangeText,
      data: transformSeriesData(yellowEvents),
      type: 'line',
      color: Color.Yellow,
      stack: 'locationCycle',
      tooltip: {
        show: false,
      },
      areaStyle: {},
    },
    {
      name: redClearanceText,
      data: transformSeriesData(redClearanceEvents),
      type: 'line',
      color: '#FF0000',
      stack: 'locationCycle',
      tooltip: {
        show: false,
      },
      areaStyle: {},
    }
    // {
    //   name: 'Red',
    //   data: transformSeriesData(redEvents),
    //   type: 'line',
    //   color: '#FF000050',
    //   stack: 'locationCycle',
    //   areaStyle: {},
    // },
  )

  const planOptions: PlanOptions<YellowAndRedActuationsPlan> = {
    totalViolations: (value: number) => `TV: ${value}`,
    severeViolations: (value: number) => `SV: ${value}`,
    percentSevereViolations: (value: number) => `% SV: ${Math.round(value)}%`,
    percentViolations: (value: number) => `% V: ${Math.round(value)}%`,
    averageTimeViolations: (value: number) => `Avg V: ${Math.round(value)}s`,
  }

  const planSeries = createPlans(plans, yAxis.length, planOptions)

  const displayProps = createDisplayProps({
    description: data.approachDescription,
    isPermissivePhase: data.isPermissivePhase,
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
    series: [...series, planSeries],
    displayProps,
  }

  return chartOptions
}
