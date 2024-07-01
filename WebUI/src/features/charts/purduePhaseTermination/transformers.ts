import {
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
} from '@/features/charts/common/transformers'
import { ChartType } from '@/features/charts/common/types'
import {
  Phase,
  RawPurduePhaseTerminationData,
  RawPurduePhaseTerminationResponse,
} from '@/features/charts/purduePhaseTermination/types'
import {
  ExtendedEChartsOption,
  TransformedChartResponse,
} from '@/features/charts/types'
import {
  Color,
  formatChartDateTimeRange,
  triangleSvgSymbol,
} from '@/features/charts/utils'
import {
  DataZoomComponentOption,
  SeriesOption,
  TooltipComponentOption,
} from 'echarts'

export default function transformPurduePhaseTerminationData(
  response: RawPurduePhaseTerminationResponse
): TransformedChartResponse {
  const chart = transformData(response.data)

  return {
    type: ChartType.PurduePhaseTermination,
    data: {
      charts: [chart],
    },
  }
}
function transformData(data: RawPurduePhaseTerminationData) {
  const { phases, plans } = data

  const info = createInfoString([
    `Currently showing Force-Offs, Max-Outs and Gap-Outs with a consecutive occurence of ${data.consecutiveCount} or more. Pedestrian events are never filtered.`,
    '',
  ])

  const titleHeader = `Phase Termination\n${data.locationDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: info,
  })

  const xAxis = createXAxis(data.start, data.end)
  const yAxis = createYAxis(true, {
    name: 'Phase Number',
    type: 'category',
    boundaryGap: true,
    splitLine: { show: true },
    data: phases.map((phase) => phase.phaseNumber),
  })

  const grid = createGrid({
    top: 190,
    left: 60,
    right: 250,
  })

  const gapOuts = 'Gap Outs'
  const forceOffs = 'Force Offs'
  const maxOuts = 'Max Outs'
  const pedWalkBeings = 'Ped Walk Begins'
  const unknownTerminations = 'Unknown Terminations'

  const legend = createLegend({
    data: [
      { name: gapOuts },
      { name: forceOffs },
      { name: maxOuts },
      { name: pedWalkBeings, icon: triangleSvgSymbol },
      { name: unknownTerminations },
    ],
  })

  const tooltip = createTooltip()

  const dataZoom: DataZoomComponentOption[] = [
    {
      type: 'slider',
      minSpan: 0.2,
    },
    {
      type: 'slider',
      orient: 'vertical',
      right: 190,
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
      minSpan: 0.2,
    },
  ]

  const toolbox = createToolbox(
    { title: titleHeader, dateRange },
    data.locationIdentifier,
    ChartType.PurduePhaseTermination
  )

  const combinedGapOuts = combineArrays(data, 'gapOuts')
  const combinedForceOffs = combineArrays(data, 'forceOffs')
  const combinedMaxOuts = combineArrays(data, 'maxOuts')
  const combinedPedWalkBegins = combineArrays(data, 'pedWalkBegins')
  const combinedUnknownTerminations = combineArrays(data, 'unknownTerminations')

  const symbolSize = 4

  const seriesTooltip = {
    trigger: 'item',
    valueFormatter: (value: string[]) =>
      new Date(value[0]).toLocaleString(undefined, { hour12: false }),
  }

  const series = createSeries(
    {
      name: gapOuts,
      data: combinedGapOuts,
      type: 'scatter',
      symbolSize: symbolSize,
      color: Color.Green,
      symbolOffset: [0, '-200%'],
      tooltip: seriesTooltip as TooltipComponentOption,
    },
    {
      name: forceOffs,
      data: combinedForceOffs,
      type: 'scatter',
      symbolSize: symbolSize,
      color: Color.Blue,
      symbolOffset: [0, '-100%'],
      tooltip: seriesTooltip as TooltipComponentOption,
    },
    {
      name: maxOuts,
      data: combinedMaxOuts,
      type: 'scatter',
      symbolSize: symbolSize,
      color: Color.Pink,
      tooltip: seriesTooltip as TooltipComponentOption,
    },
    {
      name: pedWalkBeings,
      data: combinedPedWalkBegins,
      type: 'scatter',
      symbolSize: symbolSize,
      color: Color.Red,
      symbol: triangleSvgSymbol,
      symbolOffset: [0, '100%'],
      tooltip: seriesTooltip as TooltipComponentOption,
    },
    {
      name: unknownTerminations,
      data: combinedUnknownTerminations,
      type: 'scatter',
      symbolSize: symbolSize,
      color: Color.Yellow,
      symbolOffset: [0, '200%'],
      tooltip: seriesTooltip as TooltipComponentOption,
    }
  )

  const planSeries: SeriesOption = {
    ...createPlans(plans, yAxis.length, undefined, 155),
    tooltip: { trigger: 'none' },
  }

  const chartOptions: ExtendedEChartsOption = {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    dataZoom,
    toolbox,
    animation: false,
    series: [...series, planSeries],
    tooltip,
  }

  return { chart: chartOptions }
}

function combineArrays<T extends keyof Phase>(
  locationData: { phases: Phase[] },
  prop: T
): [string, number][] {
  const combinedItems: [string, number][] = []

  for (let i = locationData.phases.length - 1; i >= 0; i--) {
    const phase = locationData.phases[i]
    const items = phase[prop] as string[]

    for (const item of items) {
      combinedItems.push([item, i])
    }
  }

  return combinedItems
}
