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
    stack:'total',

    label: {
      show: false,
      position: 'insideBottom',
      distance: 15,
      align: 'left',
      verticalAlign: 'middle',
      rotate: 0,
      formatter: '{c}  {name|{a}}',
      fontSize: 16,
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
      data: legendData
    },
    toolbox: {
      show: true,
      orient: 'vertical',
      left: 'right',
      top: 'center',
      feature: {
        mark: { show: true },
        dataView: { show: true, readOnly: false },
        magicType: { show: true, type: ['line', 'bar', 'stack'] },
        restore: { show: true },
        saveAsImage: { show: true }
      }
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