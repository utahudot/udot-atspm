// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - totalPedVolByLocationTransformer.ts
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

// /features/activeTransportation/transformers/totalPedVolByLocation.ts
import { EChartsOption } from 'echarts'

export interface TotalVolumeByLocation {
  locationIdentifier: string
  percentage: number
}

export function transformPieChartTransformer(
  data: TotalVolumeByLocation[]
): EChartsOption {
  return {
    title: { text: '', left: 'center' },
    tooltip: {
      trigger: 'item',
      formatter: '{b}: {d}%',
    },
    legend: {
      orient: 'vertical',
      right: 10,
      top: 'center',
    },
    series: [
      {
        type: 'pie',
        radius: '70%',
        center: ['50%', '50%'],
        colorBy: 'data',
        data: data.map((d) => ({
          id: d.locationIdentifier,
          name: d.locationIdentifier,
          value: d.percentage,
        })),
        label: { formatter: '{d}%' },
      },
    ],
  }
}

export function transformBlockChartTransformer(
  data: TotalVolumeByLocation[]
): EChartsOption {
  return {
    tooltip: {
      formatter: '{b}: {c}%',
    },
    series: [
      {
        type: 'treemap',
        sort: null,
        colorMappingBy: 'id',
        data: data.map((d) => ({
          id: d.locationIdentifier,
          name: d.locationIdentifier,
          value: d.percentage,
        })),
        label: { show: true, formatter: '{b}' },
        roam: false,
        breadcrumb: { show: false },
      },
    ],
  }
}
