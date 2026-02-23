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
    text: 'Average Daily Pedestrian Volume by Location',
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
