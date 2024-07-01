import {
  createDataZoom,
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
import {
  ExtendedEChartsOption,
  TransformedChartResponse,
} from '@/features/charts/types'
import {
  Color,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import {
  ApproachDelayPlan,
  RawApproachDelayData,
  RawApproachDelayReponse,
} from './types'

export default function transformApproachDelayData(
  response: RawApproachDelayReponse
): TransformedChartResponse {
  const charts = response.data.map((data) => {
    const chartOptions = transformData(data)
    return {
      chart: chartOptions,
    }
  })

  return {
    type: ChartType.ApproachDelay,
    data: {
      charts,
    },
  }
}

function transformData(data: RawApproachDelayData) {
  const { approachDelayPerVehicleDataPoints, approachDelayDataPoints, plans } =
    data

  const info = createInfoString(
    [
      'Average Delay Per Vehicle (AD): ',
      `${Math.round(data.averageDelayPerVehicle)} seconds`,
    ],
    [`Total Delay (TD): `, `${Math.round(data.totalDelay / 3600)} hours`]
  )

  const titleHeader = `Approach Delay\n${data.locationDescription} - ${data.phaseDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)
  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: info,
  })

  const approachDelayHour = 'Approach Delay\n(per hour)'
  const approachDelaySecond = 'Approach Delay\nPer Vehicle\n(per second)'

  const xAxis = createXAxis(data.start, data.end)
  const yAxis = createYAxis(
    true,
    { name: 'Delay per Vehicle (seconds)' },
    { name: 'Delay per Hour (hours)' }
  )

  const grid = createGrid({
    top: 210,
    left: 60,
    right: 200,
  })

  const legend = createLegend({
    data: [
      { name: approachDelaySecond, icon: SolidLineSeriesSymbol },
      { name: approachDelayHour, icon: SolidLineSeriesSymbol },
    ],
  })

  const dataZoom = createDataZoom()

  const toolbox = createToolbox(
    { title: titleHeader, dateRange },
    data.locationIdentifier,
    ChartType.ApproachDelay
  )

  const tooltip = createTooltip()

  const series = createSeries(
    {
      name: approachDelaySecond,
      data: transformSeriesData(approachDelayPerVehicleDataPoints),
      type: 'line',
      color: Color.Blue,
      tooltip: {
        valueFormatter: (value) =>
          `${Math.round(value as number).toLocaleString()}s`,
      },
    },
    {
      name: approachDelayHour,
      data: transformSeriesData(approachDelayDataPoints),
      type: 'line',
      color: Color.Red,
      yAxisIndex: 1,
      tooltip: {
        valueFormatter: (value) =>
          `${Math.round(value as number).toLocaleString()}h`,
      },
    }
  )

  const planOptions: PlanOptions<ApproachDelayPlan> = {
    averageDelay: (value: number) => `AD: ${value}s`,
    totalDelay: (value: number) => {
      const num = Number((value / 3600).toFixed(1))
      return `TD: ${Number.isInteger(num) ? num.toFixed(0) : num}h`
    },
  }

  const planSeries = createPlans(plans, yAxis.length, planOptions)

  const displayProps = createDisplayProps({
    description: data.phaseDescription,
    numberOfLocations: 0,
  })

  const chartOptions: ExtendedEChartsOption = {
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
