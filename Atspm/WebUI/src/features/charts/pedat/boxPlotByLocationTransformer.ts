// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - boxPlotByLocationTransformer.ts
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
import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import {
  createGrid,
  createLegend,
  createToolbox,
  createTooltip,
} from '@/features/charts/common/transformers'
import { EChartsOption } from 'echarts'

export default function transformBoxPlotByLocationTransformer(
  data: PedatChartsContainerProps['data'],
  timeUnit: string
): EChartsOption {
  const title = {
    text: `Box Plot of Pedestrian Volume, by ${timeUnit}, by Location`,
    left: 'center',
  }

  const xAxis = {
    type: 'category',
    name: 'Location',
    data: data?.map((d) => d.locationIdentifier),
  }

  const yAxis = {
    type: 'value',
    name: 'Pedestrian Volume',
  }

  const grid = createGrid({ top: 80, left: 60, right: 190, bottom: 60 })

  const legend = createLegend({
    data: ['Volume Distribution'],
  })

  const toolbox = createToolbox(
    { title: 'Box Plot by Location', dateRange: '' },
    '',
    'basic'
  )

  const tooltip = createTooltip({
    trigger: 'item',
    // formatter: (params: any) => {
    //   const [min, q1, median, q3, max] = params.data.slice(1)
    //   return `
    //     <strong>${params.name}</strong><br/>
    //     Min: ${min}<br/>
    //     Q1: ${q1}<br/>
    //     Median: ${median}<br/>
    //     Q3: ${q3}<br/>
    //     Max: ${max}
    //   `
    // },
  })

  const series = [
    {
      name: 'Volume Distribution',
      type: 'boxplot',
      data: data?.map((d) => ({
        name: d.locationIdentifier,
        value: [
          d.statisticData?.min,
          d.statisticData?.twentyFifthPercentile,
          d.statisticData?.fiftiethPercentile,
          d.statisticData?.seventyFifthPercentile,
          d.statisticData?.max,
        ],
      })),
    },
  ]

  const chartOptions: EChartsOption = {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    toolbox,
    tooltip,
    series,
  }

  return chartOptions
}
