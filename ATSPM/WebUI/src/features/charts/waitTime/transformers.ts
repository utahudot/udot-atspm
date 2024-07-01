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
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import { EChartsOption } from 'echarts'
import { RawWaitTimeData, RawWaitTimeResponse, WaitTimePlan } from './types'

export default function transformWaitTimeData(
  response: RawWaitTimeResponse
): TransformedChartResponse {
  const charts = response.data.map((data) => {
    const chartOptions = transformData(data)
    return {
      chart: chartOptions,
    }
  })

  return {
    type: ChartType.WaitTime,
    data: {
      charts,
    },
  }
}

function transformData(data: RawWaitTimeData) {
  const {
    plans,
    gapOuts,
    maxOuts,
    forceOffs,
    unknowns,
    average,
    volumes,
    planSplits,
  } = data

  const info = createInfoString([data.detectionTypes, ''])
  const titleHeader = `Wait Time\n${data.locationDescription} - ${data.approachDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: info,
  })

  const volumePerHourText = 'Volume Per Hour';
  const averageWaitText = 'Average Wait';
  const programmedSplitsText = 'Programmed Splits';
  const gapOutText = 'Gap Out';
  const maxOutText = 'Max Out';
  const forceOffText = 'Force Off';
  const unknownText = 'Unknown';

  const xAxis = createXAxis(data.start, data.end)
  const yAxis = createYAxis(
    true,
    { name: 'Wait Time (Seconds)' },
    { name: volumePerHourText, position: 'right', nameGap: 50 }
  )

  const grid = createGrid({
    top: 200,
    left: 80,
    right: 220,
  })

  const legend = createLegend({
    data: [
      volumes.length
        ? {
            name: volumePerHourText,
            icon: SolidLineSeriesSymbol,
          }
        : {},
      average.length
        ? { name: averageWaitText, icon: SolidLineSeriesSymbol }
        : {},
      planSplits.length
        ? { name: programmedSplitsText, icon: SolidLineSeriesSymbol }
        : {},
      gapOuts.length ? { name: gapOutText } : {},
      maxOuts.length ? { name: maxOutText } : {},
      forceOffs.length ? { name: forceOffText } : {},
      unknowns.length ? { name: unknownText } : {},
    ],
  })

  const dataZoom = createDataZoom()

  const toolbox = createToolbox(
    { title: titleHeader, dateRange },
    data.locationIdentifier,
    ChartType.WaitTime
  )

  const tooltip = createTooltip()

  const symbolSize = 7

  const series = createSeries(
    {
      name: volumePerHourText,
      data: transformSeriesData(volumes),
      type: 'line',
      color: Color.Blue,
      yAxisIndex: 1,
    },
    {
      name: gapOutText,
      data: transformSeriesData(gapOuts),
      type: 'scatter',
      color: Color.Green,
      symbolSize: symbolSize,
    },
    {
      name: maxOutText,
      data: transformSeriesData(maxOuts),
      type: 'scatter',
      color: Color.Red,
      symbolSize: symbolSize,
    },
    {
      name: forceOffText,
      data: transformSeriesData(forceOffs),
      type: 'scatter',
      color: Color.Blue,
      symbolSize: symbolSize,
    },
    {
      name: unknownText,
      data: transformSeriesData(unknowns),
      type: 'scatter',
      color: Color.Yellow,
      symbolSize: symbolSize,
    },
    {
      name: averageWaitText,
      data: transformSeriesData(average),
      type: 'line',
      color: Color.Pink,
    },
    {
      name: programmedSplitsText,
      data: transformSeriesData(planSplits),
      type: 'line',
      step: 'end',
      color: Color.Red,
    }
  )

  const planOptions: PlanOptions<WaitTimePlan> = {
    averageWaitTime: (value: number) => `Avg ${value.toFixed(1)} s`,
    maxWaitTime: (value: number) => `Max ${value.toFixed(1)} s`,
  }

  const planSeries = createPlans(plans, yAxis.length, planOptions)

  const displayProps = createDisplayProps({
    description: data.approachDescription,
  })

  const chartOptions: EChartsOption = {
    title: title,
    xAxis: xAxis,
    yAxis: yAxis,
    grid: grid,
    legend: legend,
    dataZoom: dataZoom,
    toolbox: toolbox,
    tooltip: tooltip,
    series: [...series, planSeries],
    displayProps: displayProps,
  }

  return chartOptions
}