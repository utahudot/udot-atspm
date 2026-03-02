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
