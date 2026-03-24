// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - avgDailyPedVolByLocation.ts
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

// /features/activeTransportation/transformers/avgDailyPedVolByLocation.ts
import {
  createDataZoom,
  createGrid,
  createLegend,
  createToolbox,
  createTooltip,
} from '@/features/charts/common/transformers'
import { EChartsOption } from 'echarts'

export interface PedestrianVolumeByLocation {
  locationId: string
  locationIdentifier: string
  averageDailyVolume: number
}

export default function transformAvgDailyPedVolByLocation(
  data: PedestrianVolumeByLocation[]
): EChartsOption {
  const title = {
    text: 'Average Daily Pedestrian Volume, by Location',
    left: 'center',
  }

  const xAxis = {
    type: 'category',
    data: data.map((d) => d.locationIdentifier),
    name: 'Location',
  }

  const yAxis = {
    type: 'value',
    name: 'Volume',
  }

  const grid = createGrid({ top: 80, left: 60, right: 190, bottom: 80 })

  const legend = createLegend({
    data: ['Average Daily Volume'],
  })

  const dataZoom = createDataZoom()

  const toolbox = createToolbox(
    { title: 'Average Daily Pedestrian Volume', dateRange: '' },
    '',
    'basic'
  )

  const tooltip = createTooltip()

  const series = [
    {
      type: 'bar',
      data: data.map((d) => d.averageDailyVolume),
      name: 'Average Daily Volume',
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
