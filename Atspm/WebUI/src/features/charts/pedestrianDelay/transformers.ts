// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - transformers.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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
  formatExportFileName,
  transformSeriesData,
} from '@/features/charts/common/transformers'
import { ChartType, PlanOptions } from '@/features/charts/common/types'
import {
  RawPedestrianDelayData,
  RawPedestrianDelayResponse,
  pedestrianDelayPlan,
} from '@/features/charts/pedestrianDelay/types'
import { TransformedChartResponse } from '@/features/charts/types'
import {
  Color,
  DashedLineSeriesSymbol,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import { EChartsOption } from 'echarts'

export default function transformPedestrianDelayData(
  response: RawPedestrianDelayResponse
): TransformedChartResponse {
  const charts = response.data.map((data) => {
    const chartOptions = transformData(data)
    return {
      chart: chartOptions,
    }
  })

  return {
    type: ChartType.PedestrianDelay,
    data: {
      charts,
    },
  }
}

function transformData(data: RawPedestrianDelayData) {
  const {
    plans,
    cycleLengths,
    pedestrianDelay,
    startOfWalk,
    percentDelayByCycleLength,
  } = data

  const info = createInfoString(
    ['Ped Presses (PP): ', data.pedPresses.toLocaleString()],
    [
      'Cycles with Ped Requests (CPR): ',
      data.cyclesWithPedRequests.toLocaleString(),
    ],
    [
      `Time-Buffered ${data.timeBuffered}s Presses (TBP): `,
      data.uniquePedestrianDetections.toLocaleString(),
    ],
    ['Avg Delay (AD): ', `${Math.round(data.averageDelay)}s`],
    ['Min Delay: ', `${Math.round(data.minDelay)}s`],
    ['Max Delay: ', `${Math.round(data.maxDelay)}s`]
  )

  const titleHeader = `Pedestrian Delay\n${data.locationDescription} - ${data.phaseDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: info,
  })

  const xAxis = createXAxis(data.start, data.end)
  const yAxis = createYAxis(
    true,
    { name: 'Delay per Request (seconds)', min: 0 },
    {
      name: 'Delay by Cycle Length',
      position: 'right',
      axisLabel: { formatter: (val) => `${Math.round(val)}%` },
      nameGap: 50,
      min: 0,
    }
  )

  const grid = createGrid({
    top: 230,
    left: 65,
    right: 270,
  })

  const pedestrianDelayText = 'Pedestrian Delay'
  const cycleLengthText = 'Cycle Length'
  const startOfWalkText = 'Start of Walk'
  const percentDelayText = 'Percent Delay\nby Cycle Length'

  const legend = createLegend({
    data: [
      { name: pedestrianDelayText },
      { name: cycleLengthText, icon: SolidLineSeriesSymbol },
      {
        name: percentDelayText,
        icon: DashedLineSeriesSymbol,
      },
      { name: startOfWalkText },
    ],
  })

  const dataZoom = createDataZoom([
    {
      type: 'slider',
      orient: 'vertical',
      filterMode: 'none',
      right: 160,
      minSpan: 0.2,
      yAxisIndex: [0, 1],
    },
  ])

  const toolbox = createToolbox(
    {
      title: formatExportFileName(titleHeader, data.start, data.end),
      dateRange,
    },
    data.locationIdentifier,
    ChartType.PedestrianDelay
  )

  const tooltip = createTooltip()

  const series = createSeries(
    {
      name: pedestrianDelayText,
      data: transformSeriesData(pedestrianDelay),
      type: 'bar',
      barWidth: 2,
      color: Color.Blue,
      tooltip: {
        valueFormatter: (value) =>
          `${Math.round(value as number).toLocaleString()}s`,
      },
    },
    {
      name: cycleLengthText,
      data: transformSeriesData(cycleLengths),
      type: 'line',
      color: Color.Red,
      tooltip: {
        valueFormatter: (value) =>
          `${Math.round(value as number).toLocaleString()}s`,
      },
    },
    {
      name: startOfWalkText,
      data: transformSeriesData(startOfWalk),
      type: 'scatter',
      color: Color.Orange,
      symbolSize: 5,
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}`,
      },
    },
    {
      name: percentDelayText,
      data: transformSeriesData(percentDelayByCycleLength),
      yAxisIndex: 1,
      type: 'line',
      step: 'end',
      color: Color.Pink,
      lineStyle: {
        type: 'dotted',
      },
      tooltip: {
        valueFormatter: (value) => `${Math.round(value as number)}%`,
      },
    }
  )

  function round(x: number) {
    return Math.ceil(x / 5) * 5
  }
  const planOptions: PlanOptions<pedestrianDelayPlan> = {
    pedRecallMessage: (value: string) => value,
    cyclesWithPedRequests: (value: number) => `${value} CPR`,
    uniquePedDetections: (value: number) => `${value} TBP`,
    averageDelaySeconds: (value: number) => `${Math.round(value)} AD`,
    averageCycleLengthSeconds: (value: number) =>
      `Avg CL: ${round(Number.parseInt(value.toFixed(2)))}s`,
    pedPresses: (value: number) => `${value} PP`,
  }

  const plansSeries = createPlans(plans, yAxis.length, planOptions, 125)

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
    series: [...series, plansSeries],
    displayProps,
  }

  return chartOptions
}
