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
      show: true,
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