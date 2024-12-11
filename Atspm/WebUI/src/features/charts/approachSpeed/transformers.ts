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
import { TransformedChartResponse } from '@/features/charts/types'
import {
  Color,
  DashedLineSeriesSymbol,
  formatChartDateTimeRange,
  SolidLineSeriesSymbol,
} from '@/features/charts/utils'
import { EChartsOption, TooltipComponentOption } from 'echarts'
import {
  ApproachSpeedPlan,
  RawApproachSpeedData,
  RawApproachSpeedReponse,
} from './types'

export default function transformApproachDelayData(
  response: RawApproachSpeedReponse
): TransformedChartResponse {
  const charts = response.data.map((data) => {
    const chartOptions = transformData(data)
    return {
      chart: chartOptions,
    }
  })

  return {
    type: ChartType.ApproachSpeed,
    data: {
      charts,
    },
  }
}

function transformData(data: RawApproachSpeedData) {
  const { averageSpeeds, eightyFifthSpeeds, fifteenthSpeeds, plans } = data

  const infoA = createInfoString(
    ['Detection Type: ', data.detectionType],
    ['Speed Accuracy: ', '± 2 mph\n']
  )

  const infoB = createInfoString(
    ['Detector Distance from Stop Bar: ', `${data.distanceFromStopBar} ft`],
    [
      'Includes records over 5 mph that occur between 15s after start of green to start of yellow',
      '',
    ]
  )

  const titleHeader = `Approach Speed\n${data.locationDescription} - ${data.phaseDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: `${infoA}\n${infoB}`,
  })

  const xAxis = createXAxis(data.start, data.end)

  const yAxis = createYAxis(true, { name: 'MPH' })

  const grid = createGrid({
    top: 250,
    left: 60,
    right: 200,
  })

  const averageMPHText = 'Average MPH'
  const eightyFivePercentText = '85th Percentile Speed'
  const fifteenPercentText = '15th Percentile Speed'
  const postedSpeed = 'Posted Speed'

  const legend = createLegend({
    data: [
      { name: averageMPHText, icon: SolidLineSeriesSymbol },
      { name: eightyFivePercentText, icon: SolidLineSeriesSymbol },
      { name: fifteenPercentText, icon: SolidLineSeriesSymbol },
      { name: postedSpeed, icon: DashedLineSeriesSymbol },
    ],
  })

  const dataZoom = createDataZoom()

  const toolbox = createToolbox(
    {
      title: formatExportFileName(titleHeader, data.start, data.end),
      dateRange,
    },
    data.locationIdentifier,
    ChartType.ApproachSpeed
  )

  const tooltip = createTooltip()

  const seriesTooltip = {
    valueFormatter: (value: string) => `${parseInt(value).toFixed(0)} mph`,
  }

  const series = createSeries(
    {
      name: averageMPHText,
      data: transformSeriesData(averageSpeeds),
      type: 'line',
      color: Color.Red,
      tooltip: seriesTooltip as TooltipComponentOption,
    },
    {
      name: eightyFivePercentText,
      data: transformSeriesData(eightyFifthSpeeds),
      type: 'line',
      color: Color.Blue,
      tooltip: seriesTooltip as TooltipComponentOption,
    },
    {
      name: fifteenPercentText,
      data: transformSeriesData(fifteenthSpeeds),
      type: 'line',
      color: Color.Yellow,
      tooltip: seriesTooltip as TooltipComponentOption,
    },
    {
      name: postedSpeed,
      data: [
        [data.start, data.postedSpeed],
        [data.end, data.postedSpeed],
      ],
      type: 'line',
      color: Color.Green,
      lineStyle: {
        type: 'dashed',
      },
    }
  )

  const planOptions: PlanOptions<ApproachSpeedPlan> = {
    averageSpeed: (value: number) => `Avg Speed: ${value} mph`,
    eightyFifthPercentile: (value: number) => `85%: ${value} mph`,
    fifteenthPercentile: (value: number) => `15%: ${value} mph`,
    standardDeviation: (value: number) => `Std Dev: ${value}`,
  }

  const planSeries = createPlans(plans, yAxis.length, planOptions, 165)

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
