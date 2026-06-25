// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - speedVariability.transformer.ts
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
import { SpeedVariabilityDto } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { SM_ChartType } from '@/features/speedManagementTool/api/getSMCharts'
import { DataZoomComponentOption } from 'echarts'
import {
  createDisplayProps,
  createGrid,
  createLegend,
  createSeries,
  createTooltip,
  createYAxis,
  transformSeriesData,
} from '../../common/transformers'
import { DataPoint } from '../../common/types'
import { ExtendedEChartsOption } from '../../types'
import { Color, SolidLineSeriesSymbol, formatChartDateRange } from '../../utils'
import { createSpeedManagementTitle } from '../createSpeedManagementTitle'

export default function transformSpeedVariabilityData(
  response: SpeedVariabilityDto
) {
  const chart = transformData(response)
  return {
    type: SM_ChartType.SPEED_VARIABILITY,
    charts: [chart],
  }
}

function transformData(response: SpeedVariabilityDto) {
  const dateRange = formatChartDateRange(
    response.startDate,
    response.endDate,
    'date'
  )

  const title = createSpeedManagementTitle({
    title: `Speed Variability - ${response.segmentName}`,
    dateRange,
  })
  const speedVariability = 'Speed Variability'
  const maxSpeed = 'Max Speed'
  const minSpeed = 'Min Speed'

  const xAxis = {
    type: 'time',
    name: 'Date',
    nameGap: 30,
    nameLocation: 'middle',
    splitNumber: 10,
    minorTick: {
      show: true,
      splitNumber: 2,
    },
  }

  const yAxis = createYAxis(true, { name: 'Speed (mph)' })

  const grid = createGrid({
    top: 80,
    left: 60,
    right: 170,
  })

  const legend = createLegend({
    top: grid.top,
    data: [
      { name: speedVariability, icon: SolidLineSeriesSymbol },
      { name: maxSpeed, icon: SolidLineSeriesSymbol },
      { name: minSpeed, icon: SolidLineSeriesSymbol },
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

  const tooltip = createTooltip()

  // const seriesData = transformData(response)
  const minSpeeds: DataPoint[] = response.data
    .filter((variability) => variability.minSpeed !== null)
    .map((variability) => {
      return {
        timestamp: variability.date,
        value: variability.minSpeed,
      }
    })
  const maxSpeeds = response.data
    .filter((variability) => variability.maxSpeed !== null)
    .map((variability) => {
      return {
        timestamp: variability.date,
        value: variability.maxSpeed,
      }
    })
  const variabilityData = response.data.map((variability) => {
    return {
      timestamp: variability.date,
      value: variability.speedVariability,
    }
  })

  const series = createSeries(
    {
      name: maxSpeed,
      data: transformSeriesData(maxSpeeds),
      type: 'line',
      color: Color.Green,
      clip: true,
    },
    {
      name: minSpeed,
      data: transformSeriesData(minSpeeds),
      type: 'line',
      stack: 'speed',
      color: Color.Blue,
      clip: true,
    },
    {
      name: speedVariability,
      data: transformSeriesData(variabilityData),
      type: 'bar',
      stack: 'speed',
      color: Color.Orange,
      clip: true,
      barWidth: 3,
      barMaxWidth: 5,
    }
  )

  // series.push(customBars)
  const displayProp = createDisplayProps({
    data: response,
    description: '',
  })

  const chartOptions: ExtendedEChartsOption = {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    tooltip,
    series,
    dataZoom,
    displayProp,
  }

  return chartOptions
}
