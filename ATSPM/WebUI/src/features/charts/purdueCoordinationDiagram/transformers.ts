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
  RawPurdueCoordinationDiagramData,
  RawPurdueCoordinationDiagramResponse,
  purdueCoordinationDiagramPlan,
} from '@/features/charts/purdueCoordinationDiagram/types'
import { TransformedChartResponse } from '@/features/charts/types'
import {
  Color,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import { EChartsOption } from 'echarts'

export default function transformPurdueCoordinationDiagramData(
  response: RawPurdueCoordinationDiagramResponse
): TransformedChartResponse {
  const charts = response.data.map((data) => {
    const chartOptions = transformPcdData(data)
    return {
      chart: chartOptions,
    }
  })

  return {
    type: ChartType.PurdueCoordinationDiagram,
    data: {
      charts,
    },
  }
}

export function transformPcdData(
  data: RawPurdueCoordinationDiagramData,
  planYLength?: number
) {
  const {
    plans,
    volumePerHour,
    redSeries,
    yellowSeries,
    greenSeries,
    detectorEvents,
  } = data

  const info = createInfoString([
    'Arrivals on Green: ',
    `${data.percentArrivalOnGreen}%`,
  ])

  const titleHeader = `Purdue Coordination Diagram\n${data.locationDescription} - ${data.phaseDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: info,
  })

  const volumePerHourText = 'Volume Per Hour'
  const redSeriesText = 'Red Series'
  const yellowSeriesText = 'Yellow Series'
  const greenSeriesText = 'Green Series'
  const detectorEventsText = 'Detector Events'

  const xAxis = createXAxis(data.start, data.end)
  const yAxis = createYAxis(
    true,
    { name: 'Cycle Time (Seconds)', min: 0 },
    { name: 'Volume per Hour', position: 'right', nameGap: 50, min: 0 }
  )

  const grid = createGrid({
    top: 220,
    left: 90,
    right: 220,
  })

  const legend = createLegend({
    data: [
      { name: volumePerHourText, icon: SolidLineSeriesSymbol },
      { name: redSeriesText, icon: SolidLineSeriesSymbol },
      { name: yellowSeriesText, icon: SolidLineSeriesSymbol },
      { name: greenSeriesText, icon: SolidLineSeriesSymbol },
      { name: detectorEventsText },
    ],
  })

  const dataZoom = createDataZoom()

  const toolbox = createToolbox(
    { title: titleHeader, dateRange },
    data.locationIdentifier,
    ChartType.PurdueCoordinationDiagram
  )

  const tooltip = createTooltip()

  const series = createSeries(
    {
      name: volumePerHourText,
      data: transformSeriesData(volumePerHour),
      type: 'line',
      color: Color.Pink,
      yAxisIndex: 1,
      tooltip: {
        valueFormatter: (value) =>
          `${Math.round(value as number).toLocaleString()} vph`,
      },
    },
    {
      name: redSeriesText,
      data: transformSeriesData(redSeries),
      type: 'line',
      color: Color.Red,
      symbolSize: 0,
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}s`,
      },
    },
    {
      name: yellowSeriesText,
      data: transformSeriesData(yellowSeries),
      type: 'line',
      color: Color.Yellow,
      symbolSize: 0,
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}s`,
      },
    },
    {
      name: greenSeriesText,
      data: transformSeriesData(greenSeries),
      type: 'line',
      color: Color.Green,
      symbolSize: 0,
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}s`,
      },
    },
    {
      name: detectorEventsText,
      data: transformSeriesData(detectorEvents),
      type: 'scatter',
      color: Color.Blue,
      symbolSize: 1.2,
      large: true,
      tooltip: {
        trigger: 'item',
        valueFormatter: (value) =>
          `${Math.round(value as number)} seconds past cycle start`,
      },
    }
  )

  const planOptions: PlanOptions<purdueCoordinationDiagramPlan> = {
    percentGreenTime: (value: number) => `GT: ${value}%`,
    percentArrivalOnGreen: (value: number) => `AoG: ${value}%`,
    platoonRatio: (value: number) => `PR: ${value.toFixed(2)}`,
  }

  const plansSeries = createPlans(plans, yAxis.length, planOptions, planYLength)

  const displayProps = createDisplayProps({
    description: data.phaseDescription,
    numberOfLocations: 0,
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
