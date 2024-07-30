import {
  ArrivalsOnRedPlan,
  RawArrivalsOnRedData,
  RawArrivalsOnRedResponse,
} from '@/features/charts/arrivalsOnRed/types'
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
import { TransformedChartResponse } from '@/features/charts/types'
import {
  Color,
  DashedLineSeriesSymbol,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import { EChartsOption } from 'echarts'

export default function transformArrivalsOnRedData(
  response: RawArrivalsOnRedResponse
): TransformedChartResponse {
  const charts = response.data.map((data) => {
    const chartOptions = transformData(data)
    return {
      chart: chartOptions,
    }
  })

  return {
    type: ChartType.ArrivalsOnRed,
    data: {
      charts,
    },
  }
}

function transformData(data: RawArrivalsOnRedData) {
  const { percentArrivalsOnRed, totalVehicles, arrivalsOnRed, plans } = data

  const info = createInfoString(
    ['Total Detector Hits: ', data.totalDetectorHits.toLocaleString()],
    [`% Arrivals on Red (AoR): `, `${data.percentArrivalOnRed.toFixed(2)}%`],
    ['Total Arrivals on Red: ', data.totalArrivalOnRed.toLocaleString()]
  )

  const titleHeader = `Arrivals on Red\n${data.locationDescription} - ${data.phaseDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: info,
  })

  const arrivalsOnRedText = 'Arrivals on Red'
  const percentArrivalsOnRedText = '% Arrivals on Red'
  const totalVehiclesText = 'Total Vehicles'

  const xAxis = createXAxis(data.start, data.end)
  const yAxis = createYAxis(
    true,
    { name: 'Volume (Vehicles per Hour)', nameGap: 60 },
    {
      name: percentArrivalsOnRedText,
      nameGap: 40,
      max: 100,
      position: 'right',
      axisLine: { show: false },
    }
  )

  const grid = createGrid({
    top: 210,
    left: 90,
    right: 200,
  })

  const legend = createLegend({
    data: [
      { name: arrivalsOnRedText, icon: SolidLineSeriesSymbol },
      { name: percentArrivalsOnRedText, icon: DashedLineSeriesSymbol },
      { name: totalVehiclesText, icon: SolidLineSeriesSymbol },
    ],
  })

  const dataZoom = createDataZoom()

  const toolbox = createToolbox(
    { title: titleHeader, dateRange },
    data.locationIdentifier,
    ChartType.ArrivalsOnRed
  )

  const tooltip = createTooltip()

  const series = createSeries(
    {
      name: percentArrivalsOnRedText,
      data: transformSeriesData(percentArrivalsOnRed),
      type: 'line',
      yAxisIndex: 1,
      color: Color.Red,
      lineStyle: {
        type: 'dashed',
      },
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}%`,
      },
    },
    {
      name: totalVehiclesText,
      data: transformSeriesData(totalVehicles),
      type: 'line',
      color: Color.Blue,
      tooltip: {
        valueFormatter: (value) =>
          `${Math.round(value as number).toLocaleString()} vph`,
      },
    },
    {
      name: arrivalsOnRedText,
      data: transformSeriesData(arrivalsOnRed),
      type: 'line',
      color: Color.Red,
      tooltip: {
        valueFormatter: (value) =>
          `${Math.round(value as number).toLocaleString()} vph`,
      },
    }
  )

  const planOptions: PlanOptions<ArrivalsOnRedPlan> = {
    percentArrivalOnRed: (value: number) => `AoR: ${value}%`,
    percentRedTime: (value: number) => `Red Time: ${value}%`,
  }

  const planSeries = createPlans(plans, yAxis.length, planOptions)

  const displayProps = createDisplayProps({
    description: data.phaseDescription,
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
