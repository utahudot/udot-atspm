// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - tranformers.ts
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
  formatExportFileName,
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
import { EChartsOption } from 'echarts'

export default function transformSplitMonitorData(
  response: RawSplitMonitorResponse
): TransformedChartResponse {
  const charts = response.data.map((data) => {
    const chartOptions = transformData(data)
    return {
      chart: chartOptions,
    }
  })

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

  const dataZoom = createDataZoom([
    {
      type: 'slider',
      orient: 'vertical',
      filterMode: 'none',
      right: 160,
      endValue: 100,
      yAxisIndex: 0,
    },
  ])

  const toolbox = createToolbox(
    {
      title: formatExportFileName(titleHeader, data.start, data.end),
      dateRange,
    },
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
      percentileSplit && `${Math.round(value)}s (${percentileSplit}th %)`,
    averageSplit: (value) => `${Math.round(value)}s Avg. Split`,
    percentGapOuts: (value) => `${Math.round(value)}% Gap Outs`,
    percentMaxOuts: (value) => value && `${Math.round(value)}% Max Outs`,
    percentForceOffs: (value) => value && `${Math.round(value)}% Force Offs`,
    percentSkips: (value) => `${Math.round(value)}% Skips`,
  }

  plans.forEach((plan) => {
    if (plan.percentMaxOuts === 0) {
      plan.percentMaxOuts = null
    }
    if (plan.percentForceOffs === 0) {
      plan.percentForceOffs = null
    }
  })

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
