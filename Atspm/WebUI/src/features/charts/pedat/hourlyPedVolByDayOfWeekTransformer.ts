// /features/activeTransportation/transformers/hourlyPedVolByDayOfWeek.ts
import {
  createDataZoom,
  createGrid,
  createLegend,
  createToolbox,
  createTooltip,
} from '@/features/charts/common/transformers'
import { EChartsOption } from 'echarts'

export interface DailyPedestrianVolume {
  day: string
  averageVolume: number
}

const dayOrder = [
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
  'Sunday',
]

export default function transformHourlyPedVolByDayOfWeekTransformer(
  data: DailyPedestrianVolume[]
): EChartsOption {
  const sortedData = dayOrder.map(
    (day) => data.find((d) => d.day === day) ?? { day, averageVolume: 0 }
  )

  const title = {
    text: 'Average Hourly Pedestrian Volume by Day of Week',
    left: 'center',
  }

  const xAxis = {
    type: 'category',
    name: 'Day',
    data: sortedData.map((d) => d.day),
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
    { title: 'Hourly Pedestrian Volume by Day', dateRange: '' },
    '',
    'basic'
  )

  const tooltip = createTooltip()

  const series = [
    {
      type: 'bar',
      name: 'Average Hourly Volume',
      data: sortedData.map((d) => d.averageVolume),
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
