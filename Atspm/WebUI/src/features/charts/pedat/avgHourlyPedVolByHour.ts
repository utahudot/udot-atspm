// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - avgHourlyPedVolByHour.ts
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

// /features/activeTransportation/transformers/hourlyPedVolByHourOfDay.ts
import {
  createDataZoom,
  createGrid,
  createLegend,
  createToolbox,
  createTooltip,
} from '@/features/charts/common/transformers'
import { EChartsOption } from 'echarts'

export interface HourlyPedestrianVolume {
  hour: number
  averageVolume: number
}

export default function transformHourlyPedVolByHourOfDay(
  data: HourlyPedestrianVolume[]
): EChartsOption {
  const title = {
    text: 'Average Hourly Pedestrian Volume, by Hour of Day',
    left: 'center',
  }

  const xAxis = {
    type: 'category',
    name: 'Hour',
    data: data.map((d) => `${d.hour}`),
  }

  const yAxis = {
    type: 'value',
    name: 'Volume',
  }

  const grid = createGrid({ top: 80, left: 60, right: 190, bottom: 80 })

  const legend = createLegend({
    data: ['Average Hourly Volume'],
  })

  const dataZoom = createDataZoom()

  const toolbox = createToolbox(
    { title: 'Hourly Pedestrian Volume', dateRange: '' },
    '',
    'basic'
  )

  const tooltip = createTooltip({
    trigger: 'axis',
    formatter: (params: any) => {
      const p = params[0]
      return `Hour: ${p.name}:00<br/>Volume: ${p.value}`
    },
  })

  const series = [
    {
      type: 'bar',
      name: 'Average Hourly Volume',
      data: data.map((d) => d.averageVolume),
    },
  ]

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
  }

  return chartOptions
}
