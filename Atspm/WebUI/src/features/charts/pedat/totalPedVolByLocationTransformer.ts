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
