import {
  createDisplayProps,
  createGrid,
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
  RawSplitMonitorData,
  RawSplitMonitorResponse,
  SplitMonitorPlan,
} from '@/features/charts/splitMonitor/types'
import { TransformedChartResponse } from '@/features/charts/types'
import {
  Color,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import { DataZoomComponentOption, EChartsOption } from 'echarts'

export default function transformSplitMonitorData(
  response: RawSplitMonitorResponse
): TransformedChartResponse {
  const charts = response.data.map((data) => {
    const chartOptions = transformData(data)
    return {
      chart: chartOptions,
    }
  })

  console.log('before')
  console.log(charts)

  return {
    type: ChartType.SplitMonitor,
    data: {
      charts,
    },
  }
}

function transformData(data: RawSplitMonitorData) {
  const {
    plans,
    programmedSplits,
    gapOuts,
    maxOuts,
    forceOffs,
    unknowns,
    peds,
    percentileSplit,
  } = data

  const titleHeader = `Split Monitor\n${data.locationDescription} - ${data.phaseDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
  })

  const xAxis = createXAxis(data.start, data.end)

  const yAxis = createYAxis(true, {
    name: 'Phase Duration (sec)',
    min: 0,
    axisLabel: {
      formatter(value: string) {
        return Math.round(parseInt(value)).toFixed(0)
      },
    },
  })

  const grid = createGrid({
    top: 200,
    left: 60,
    right: 200,
  })

  const programmedSplitsText = 'Programmed Splits'
  const gapOutsText = 'Gap Outs'
  const maxOutsText = 'Max Outs'
  const forceOffsText = 'Force Offs'
  const unknownsText = 'Unknowns'
  const pedestriansText = 'Pedestrians'

  const legend = createLegend({
    data: [
      programmedSplits.length
        ? { name: programmedSplitsText, icon: SolidLineSeriesSymbol }
        : {},
      gapOuts.length ? { name: gapOutsText } : {},
      maxOuts.length ? { name: maxOutsText } : {},
      forceOffs.length ? { name: forceOffsText } : {},
      peds.length ? { name: pedestriansText } : {},
      unknowns ? { name: unknownsText } : {},
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
      endValue: 100, // todo - should use measure default or something
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
    ChartType.SplitMonitor
  )

  const tooltip = createTooltip()

  const programmedSplitsData = formatDataPointForStepView(
    programmedSplits,
    data.end
  )

  const series = createSeries(
    {
      name: programmedSplitsText,
      data: transformSeriesData(programmedSplits),
      type: 'custom',
      color: '#EE4B2B',
      clip: true,
      renderItem(param, api) {
        if (param.dataIndex === 0) {
          const polylines = createPolyLines(programmedSplitsData, api)

          return {
            type: 'group',
            children: polylines,
          }
        }
      },
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}s`,
      },
    },
    {
      name: gapOutsText,
      data: transformSeriesData(gapOuts),
      type: 'scatter',
      symbolSize: 5,
      color: Color.Green,
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}s`,
      },
    },
    {
      name: maxOutsText,
      data: transformSeriesData(maxOuts),
      type: 'scatter',
      symbolSize: 5,
      color: Color.Red,
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}s`,
      },
    },
    {
      name: forceOffsText,
      data: transformSeriesData(forceOffs),
      type: 'scatter',
      symbolSize: 5,
      color: Color.Blue,
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}s`,
      },
    },
    {
      name: unknownsText,
      data: transformSeriesData(unknowns),
      type: 'scatter',
      symbolSize: 5,
      color: Color.Pink,
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}s`,
      },
    },
    {
      name: pedestriansText,
      data: transformSeriesData(peds),
      type: 'scatter',
      symbolSize: 5,
      color: Color.Orange,
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}`,
      },
    }
  )

  const planOptions: PlanOptions<SplitMonitorPlan> = {
    percentileSplit: (value: number) =>
      percentileSplit &&
      `${Math.round(value)} Percentile Split (${percentileSplit}th)`,
    averageSplit: (value) => `${Math.round(value)} Avg. Split`,
    percentGapOuts: (value) => `${Math.round(value)}% Gap Outs`,
    percentMaxOuts: (value) => value && `${Math.round(value)}% Max Outs`,
    percentForceOffs: (value) => value && `${Math.round(value)}% Force Offs`,
    percentSkips: (value) => `${Math.round(value)}% Skips`,
  }

  const plansSeries = createPlans(plans, yAxis.length, planOptions, 100)

  const displayProps = createDisplayProps({
    description: 'ph' + data.phaseNumber.toLocaleString(),
    plans: plans,
    phaseNumber: data.phaseNumber,
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
