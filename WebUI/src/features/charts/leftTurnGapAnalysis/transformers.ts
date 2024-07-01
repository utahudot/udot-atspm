import {
  createDisplayProps,
  createGrid,
  createInfoString,
  createLegend,
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
import {
  DataZoomComponentOption,
  EChartsOption,
  TooltipComponentOption,
} from 'echarts'
import { RawLeftTurnGapAnalysisResponse, RawLeftTurnGapData } from './types'

export default function transformLeftTurnGapAnalysisData(
  response: RawLeftTurnGapAnalysisResponse
): TransformedChartResponse {
  const charts = response.data.map((data) => {
    const chartOptions = transformData(data)
    return {
      chart: chartOptions,
    }
  })

  return {
    type: ChartType.LeftTurnGapAnalysis,
    data: {
      charts,
    },
  }
}

function transformData(data: RawLeftTurnGapData) {
  const {
    gap1Count,
    gap2Count,
    gap3Count,
    gap4Count,
    gap1Min,
    gap1Max,
    gap2Min,
    gap2Max,
    gap3Min,
    gap3Max,
    gap4Min,
    percentTurnableSeries,
    trendLineGapThreshold,
  } = data

  const info = createInfoString([
    `Detector Type: `,
    data.detectionTypeDescription,
  ])

  const titleHeader = `Left Turn Gap Analysis\n${data.locationDescription} - ${data.phaseDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
    info,
  })

  const xAxis = createXAxis(data.start, data.end)

  const yAxis = createYAxis(
    false,
    { name: 'Gaps', nameGap: 40 },
    {
      name: `% of Green Time Where Gaps ≥ ${trendLineGapThreshold}s`,
      max: 100,
      nameGap: 40,
      position: 'right',
      axisLine: { show: false },
    }
  )

  const grid = createGrid({
    top: 175,
    left: 65,
    right: 220,
  })

  const gapNames = [
    `${gap1Min}-${gap1Max}s`,
    `${gap2Min}-${gap2Max}s`,
    `${gap3Min}-${gap3Max}s`,
    `${gap4Min}s+`,
  ]

  const percentofGreenText = '% of Green Time\nWhere Gaps ≥'

  const legend = createLegend({
    data: [
      { name: gapNames[0] },
      { name: gapNames[1] },
      { name: gapNames[2] },
      { name: gapNames[3] },
      {
        name: `${percentofGreenText} ${gap4Min}s`,
        icon: SolidLineSeriesSymbol,
      },
    ],
  })

  const dataZoom: DataZoomComponentOption[] = [
    {
      type: 'slider',
      filterMode: 'weakFilter',
      show: true,
      minSpan: 0.2,
    },
    {
      type: 'inside',
      filterMode: 'weakFilter',
      show: true,
      minSpan: 0.2,
    },
  ]

  const toolbox = createToolbox(
    { title: titleHeader, dateRange },
    data.locationIdentifier,
    ChartType.LeftTurnGapAnalysis
  )

  const tooltip = createTooltip()

  const formattedTooltip = {
    trigger: 'item',
    valueFormatter: (value: number) =>
      `${Math.round(value).toLocaleString()} gaps`,
  }

  const series = createSeries(
    {
      name: gapNames[0],
      data: transformSeriesData(gap1Count),
      type: 'bar',
      color: Color.Red,
      stack: 'gaps',
      tooltip: formattedTooltip as TooltipComponentOption,
    },
    {
      name: gapNames[1],
      data: transformSeriesData(gap2Count),
      type: 'bar',
      color: Color.Yellow,
      stack: 'gaps',
      tooltip: formattedTooltip as TooltipComponentOption,
    },
    {
      name: gapNames[2],
      data: transformSeriesData(gap3Count),
      type: 'bar',
      color: Color.Blue,
      stack: 'gaps',
      tooltip: formattedTooltip as TooltipComponentOption,
    },
    {
      name: gapNames[3],
      data: transformSeriesData(gap4Count),
      type: 'bar',
      color: Color.LightBlue,
      stack: 'gaps',
      tooltip: formattedTooltip as TooltipComponentOption,
    },
    {
      name: `${percentofGreenText} ${gap4Min}s`,
      data: transformSeriesData(percentTurnableSeries),
      yAxisIndex: 1,
      type: 'line',
      color: Color.Blue,
      tooltip: {
        valueFormatter: (value) =>
          `${Math.round(value as number).toLocaleString()}%`,
      },
    }
  )

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
    series,
    displayProps,
  }

  return chartOptions
}
