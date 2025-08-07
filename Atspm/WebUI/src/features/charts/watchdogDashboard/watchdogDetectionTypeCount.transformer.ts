// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - watchdogDetectionTypeCount.transformer.ts
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
import { DetectionTypeCount } from '@/features/watchdog/types'
import { EChartsOption } from 'echarts'

const transformDetectionTypeCountData = (
  data: DetectionTypeCount[]
): EChartsOption => {
  const total = data.reduce((sum, item) => sum + item.count, 0)
  const seriesData = data.map((item) => ({
    value: item.count,
    name: item.id,
  }))

  return {
    title: {
      text: 'Detection Type Count',
      left: 'center',
      top: 5,
    },
    tooltip: {
      trigger: 'item',
      formatter: '{a} <br/>{b}: {c} ({d}%)',
    },
    legend: {
      type: 'scroll',
      orient: 'vertical',
      top: '22%',
      right: 20,
      itemWidth: 26,
      itemHeight: 15,
      itemGap: 20,
      textStyle: {
        fontSize: 13,
      },
    },
    series: [
      {
        name: 'Detection Type',
        type: 'pie',
        radius: '80%',
        center: ['35%', '60%'],
        data: seriesData,
        emphasis: {
          itemStyle: {
            shadowBlur: 10,
            shadowOffsetX: 0,
            shadowColor: 'rgba(0, 0, 0, 0.5)',
          },
        },
        label: {
          show: true,
          formatter: (params: any) => {
            const percent = ((params.value / total) * 100).toFixed(1)
            return `${params.name}\n${params.value} (${percent}%)`
          },
          position: 'inside',
        },
      },
    ],
    media: [
      {
        query: { maxWidth: 600 },
        option: {
          legend: {
            orient: 'horizontal',
            top: 'bottom',
            left: 'center',
            right: 'auto',
          },
          series: [
            {
              radius: '60%',
              center: ['50%', '50%'],
            },
          ],
        },
      },
    ],
  }
}

export default transformDetectionTypeCountData
