// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - aggregateTransformer.ts
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
import { dateToTimestamp } from '@/utils/dateTime'
import { EChartsOption, SeriesOption, XAXisComponentOption } from 'echarts'
import { GroupedListDataItems } from '../components/aggregateTypeSelect'
import {
  AggregateChartSettings,
  AggregateResult,
} from '../handlers/aggregateDataHandler'
import {
  AggregateData,
  AggregateDataPointTypes,
  TransformedAggregateData,
} from '../types/aggregateData'
import {
  MetricTypeOptionsList,
  YAxisOptions,
  categoryXAxisTypes,
  chartTypeOptions,
  xAxisOptions,
} from '../types/aggregateOptionsData'

export default function transformAggregateData(
  result: AggregateResult
): TransformedAggregateData {
  const charts = result.charts.map((data) => {
    return {
      chart: transformData(result.settings, data),
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

// Names up to three locations and counts the rest, e.g. "7115, 7116, 7117 and
// 17 more" for a route of 20.
export const formatLocations = (locationIdentifiers: string[]) => {
  const named = locationIdentifiers.slice(0, 3).join(', ')
  const others = locationIdentifiers.length - 3
  return others > 0 ? `${named} and ${others} more` : named
}

export const transformData = (
  settings: AggregateChartSettings,
  data: AggregateData
): EChartsOption => {
  const isCategoryXAxis = categoryXAxisTypes.includes(settings.xAxisType)
  const points =
    data.series.find((series) => series.dataPoints.length)?.dataPoints ?? []
  const start = points[0]?.start
  const end = points[points.length - 1]?.start

  const metric = settings.metricType.split('-')
  const aggregationType = getAggregationType(metric[0]) as GroupedListDataItems
  const aggregateMetricType = getAggregateMetricType(aggregationType, metric[1])

  const locations = formatLocations(data.locationIdentifiers)
  const titleHeader = `${settings.averageOrSum === 0 ? 'Sum:' : 'Average:'} ${
    aggregationType.id
  } - ${aggregateMetricType?.label}${locations ? `\n${locations}` : ''}`
  // Every title shows the Start and End the chart was run with: Time Of Day
  // points all fall on the first day and the other X-Axis choices carry no
  // dates.
  const dateRange = formatChartDateTimeRange(
    dateToTimestamp(settings.startDateTime),
    dateToTimestamp(settings.endDateTime)
  )

  const title = createTitle({
    title: titleHeader,
    dateRange,
  })

  const categories = isCategoryXAxis
    ? [
        ...new Set(
          data.series.flatMap((series) =>
            series.dataPoints.map((point) => point.identifier)
          )
        ),
      ]
    : []

  const xAxis: XAXisComponentOption = isCategoryXAxis
    ? {
        type: 'category',
        data: categories,
        name: xAxisOptions.find((option) => option.id === settings.xAxisType)
          ?.label,
        nameGap: 30,
        nameLocation: 'middle',
      }
    : createXAxis(start, end)

  const toSeriesData = (
    dataPoints: AggregateDataPointTypes
  ): (string | null | (string | number)[])[] =>
    isCategoryXAxis
      ? transformCategorySeriesData(dataPoints, categories)
      : transformSeriesData(dataPoints)

  const yAxis = [
    {
      name: YAxisOptions.find((option) => option.id === settings.yAxisType)
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
    right: 430,
  })

  const dataZoom = createDataZoom()

  const seriesType = chartTypeOptions.find(
    (c) => c.id === settings.visualChartType
  )?.id

  const series: SeriesOption[] = []

  if (data.series.length === 1) {
    series.push({
      name: dataName,
      data: toSeriesData(data.series[0].dataPoints),
      type: seriesType ? seriesType : 'line',
      symbolSize: 5,
      color: Color.Green,
    })
  } else {
    data.series.forEach((arr) =>
      series.push({
        name: arr.identifier,
        data: toSeriesData(arr.dataPoints),
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

// One value per category, in the X-Axis order, blank where a series has no
// point for that category.
const transformCategorySeriesData = (
  dataPoints: AggregateDataPointTypes,
  categories: string[]
): (string | null)[] =>
  categories.map(
    (category) =>
      dataPoints
        .find((dataPoint) => dataPoint.identifier === category)
        ?.value.toFixed(2) ?? null
  )
