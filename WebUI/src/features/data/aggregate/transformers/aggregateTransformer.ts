import {
  createDataZoom,
  createGrid,
  createLegend,
  createTitle,
  createXAxis,
} from '@/features/charts/common/transformers'
import {
  Color,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import {
  AggregateData,
  AggregateDataPointTypes,
  TransformedAggregateData,
} from '../types/aggregateData'
import { EChartsOption, SeriesOption } from 'echarts'
import { AggregateOptionsHandler } from '../handlers/aggregateDataHandler'
import {
  MetricTypeOptionsList,
  YAxisOptions,
  chartTypeOptions,
} from '../types/aggregateOptionsData'
import { GroupedListDataItems } from '../components/aggregateTypeSelect'

export default function transformAggregateData(
  handler: AggregateOptionsHandler
): TransformedAggregateData {
  const charts = handler.aggregatedData.map((data) => {
    return {
      chart: transformData(handler, data),
    }
  })
  return {
    data: {
      charts,
    },
  }
}

const getAggregationType = (
  aggregateId: string
): GroupedListDataItems | null => {
  return MetricTypeOptionsList.find((item) => item.id === aggregateId) || null
}

const getAggregateMetricType = (
  aggregateType: GroupedListDataItems,
  metricId: string
) => {
  return aggregateType.options.find((item) => item.id === metricId)
}

export const transformData = (
  handler: AggregateOptionsHandler,
  data: AggregateData
): EChartsOption => {
  const length = data.series[0].dataPoints.length
  const start = data.series[0].dataPoints[0].start
  const end = data.series[0].dataPoints[length - 1].start
  const locationIdentifier = handler.updatedLocations[0].locationIdentifier

  const metric = handler.metricType.split('-')
  const aggregationType = getAggregationType(metric[0]) as GroupedListDataItems
  const aggregateMetricType = getAggregateMetricType(aggregationType, metric[1])

  const titleHeader = `${handler.averageOrSum === 0 ? 'Sum:' : 'Average:'} ${
    aggregationType.id
  } - ${aggregateMetricType?.label}\n${locationIdentifier}`
  const dateRange = formatChartDateTimeRange(start, end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
  })

  const xAxis = createXAxis(start, end)

  const yAxis = [
    {
      name: YAxisOptions.find((option) => option.id === handler.yAxisType)
        ?.label,
      min: 0,
    },
  ]

  // const metric = handler.metricType.split('-')
  const dataName = metric[1]

  const legendData =
    data.series.length === 1
      ? [{ name: dataName, icon: SolidLineSeriesSymbol }]
      : data.series.map((arr) => {
          return { name: arr.identifier, icon: SolidLineSeriesSymbol }
        })

  const legend = createLegend({
    data: legendData,
  })

  const grid = createGrid({
    top: 200,
    left: 60,
    right: 200,
  })

  const dataZoom = createDataZoom()

  const seriesType = chartTypeOptions.find(
    (c) => c.id === handler.visualChartType
  )?.id

  const series: SeriesOption[] = []

  if (data.series.length === 1) {
    series.push({
      name: dataName,
      data: transformSeriesData(data.series[0].dataPoints),
      type: seriesType ? seriesType : 'line',
      symbolSize: 5,
      color: Color.Green,
    })
  } else {
    data.series.forEach((arr) =>
      series.push({
        name: arr.identifier,
        data: transformSeriesData(arr.dataPoints),
        type: seriesType ? seriesType : 'line',
        symbolSize: 5,
      })
    )
  }

  const chartOptions: EChartsOption = {
    title: title,
    xAxis: xAxis,
    yAxis: yAxis,
    grid: grid,
    legend: legend,
    dataZoom: dataZoom,
    series: series,
  }

  return chartOptions
}

const transformSeriesData = (
  dataPoints: AggregateDataPointTypes
): (string | number)[][] => {
  const series: (string | number)[][] = []
  for (const dataPoint of dataPoints) {
    const values: (string | number)[] = [
      dataPoint.start,
      dataPoint.value.toFixed(2),
    ]
    series.push(values)
  }
  return series
}
