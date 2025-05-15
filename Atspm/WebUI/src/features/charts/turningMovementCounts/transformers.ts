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
    right: 180,
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

  const dataZoom = createDataZoom([
    {
      type: 'slider',
      orient: 'vertical',
      right: 140,
      yAxisIndex: 0,
    },
  ])

  const toolbox = createToolbox(
    {
      title: formatExportFileName(titleHeader, data.start, data.end),
      dateRange,
    },
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
