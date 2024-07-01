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
import { ChartType } from '@/features/charts/common/types'
import { TransformedChartResponse } from '@/features/charts/types'
import {
  Color,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import { EChartsOption, SeriesOption } from 'echarts'
import {
  RawTurningMovementCountsData,
  RawTurningMovementCountsResponse,
} from './types'

export default function transformTurningMovementCountsData(
  response: RawTurningMovementCountsResponse
): TransformedChartResponse {
  const charts = response.data.charts.map((data) => {
    const chartOptions = transformData(data)
    return {
      chart: chartOptions,
    }
  })

  charts.sort((a, b) => {
    const directionOrder = ['North', 'South', 'East', 'West']
    const titleA = a.chart.displayProps.description
    const titleB = b.chart.displayProps.description
    const directionA = directionOrder.find((dir) => titleA.includes(dir))
    const directionB = directionOrder.find((dir) => titleB.includes(dir))
    return (
      directionOrder.indexOf(directionA) - directionOrder.indexOf(directionB)
    )
  })

  return {
    type: ChartType.TurningMovementCounts,
    data: {
      charts,
      table: response.data.table,
      peakHourFactor: response.data.peakHourFactor,
      peakHour: response.data.peakHour,
    },
  }
}

function transformData(data: RawTurningMovementCountsData) {
  const { lanes, plans, totalHourlyVolumes } = data

  const info = createInfoString(
    ['Total Volume: ', `${data.totalVolume.toLocaleString()}`],
    ['Peak Hour: ', data.peakHour],
    ['Peak Hour Volume: ', data.peakHourVolume.toLocaleString()],
    ['Peak Hour Factor: ', data.peakHourFactor.toFixed(2)],
    ['fLU: ', data.laneUtilizationFactor.toFixed(2)]
  )

  const titleHeader = `Turning Movement Counts\n${data.locationDescription} - ${data.direction} ${data.movementType} - ${data.laneType}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: info,
  })

  const xAxis = createXAxis(data.start, data.end)
  const yAxis = createYAxis(true, { name: 'Volume Per Hour' })

  const grid = createGrid({
    top: 180,
    left: 70,
    right: 150,
  })

  const legendData = [] as { name: string; icon: string }[]

  lanes.forEach((lane) => {
    legendData.push({
      name: `Lane ${lane.laneNumber}`,
      icon: SolidLineSeriesSymbol,
    })
  })

  const legend = createLegend({
    data: [
      { name: 'Total Volume', icon: SolidLineSeriesSymbol },
      ...legendData,
    ],
  })

  const dataZoom = createDataZoom()

  const toolbox = createToolbox(
    { title: titleHeader, dateRange },
    data.locationIdentifier,
    ChartType.TurningMovementCounts
  )

  const tooltip = createTooltip()

  const colorValues = Object.values(Color)

  const series: SeriesOption[] = []

  if (lanes.length > 1) {
    series.push(
      ...createSeries({
        name: `Total Volume`,
        data: transformSeriesData(totalHourlyVolumes),
        type: 'line',
        color: Color.Red,
        tooltip: {
          valueFormatter: (val) => `${Math.round(val as number)} vph`,
        },
      })
    )
  }

  lanes.forEach((lane, i) => {
    series.push(
      ...createSeries({
        name: `Lane ${lane.laneNumber}`,
        data: transformSeriesData(lane.volume),
        type: 'line',
        color: colorValues[i % colorValues.length],
        tooltip: {
          valueFormatter: (val) => `${Math.round(val as number)} vph`,
        },
      })
    )
  })

  const plansSeries = createPlans(plans, yAxis.length)

  const displayProps = createDisplayProps({
    description: `${data.direction}${data.movementType}`,
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
    series: [...series, plansSeries],
    displayProps,
  }

  return chartOptions
}
