// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - watchDogDetectionType.transformer.ts
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
import { EChartsOption } from 'echarts'

interface HardwareData {
  name: string
  counts: number
}

interface DetectionTypeData {
  detectionType: number
  hardware: HardwareData[]
  name: string
}

const transformDetectionTypeData = (data: DetectionTypeData[]): EChartsOption => {
  const xAxisData = data.map(item => `${item.name} (${item.detectionType})`)
  const legendData = Array.from(new Set(data.flatMap(item => item.hardware.map(hw => hw.name))))

  const series = legendData.map(hardwareName => ({
    name: hardwareName,
    type: 'bar',

    label: {
      show: true,
      position: 'insideBottom',
      distance: 15,
      align: 'left',
      verticalAlign: 'middle',
      rotate: 90,
      formatter: '{c}  {name|{a}}',
      fontSize: 8,
      color: '#000000',
      rich: {
        name: {}
      }
    },
    emphasis: {
      focus: 'series'
    },
    data: data.map(item => {
      const hardwareItem = item.hardware.find(hw => hw.name === hardwareName)
      return hardwareItem ? hardwareItem.counts : 0
    })
  }))

  return {
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'shadow'
      }
    },
    legend: {
        orient: 'vertical',
        right:70,
        top: 'center',
        data: legendData,
        itemWidth: 26,  
        itemHeight: 15, 
        textStyle: {
          fontSize: 16  
        }
      },
      grid: {
        left: '3%',
        right: '25%',
        bottom: '5%',
        containLabel: false
      },
    toolbox: {
      show: false,
      orient: 'vertical',
      left: 'right',
      top: 'center',
    },
    xAxis: [
      {
        type: 'category',
        axisTick: { show: false },
        data: xAxisData
      }
    ],
    yAxis: [
      {
        type: 'value'
      }
    ],
    series: series
  }
}

export default transformDetectionTypeData