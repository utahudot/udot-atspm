import {
  DailyHistoricalRouteData,
  MonthlyHistoricalRouteData,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { createTooltip } from '@/features/charts/common/transformers'
import { SolidLineSeriesSymbol } from '@/features/charts/utils'
import { getDataSourceColor } from '@/features/speedManagementTool/components/SM_Modal/SM_Popup'
import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import { compareAsc, eachDayOfInterval, format } from 'date-fns'
import { EChartsOption, SeriesOption, TitleComponentOption } from 'echarts'

const sourceMapping = {
  1: 'ATSPM',
  2: 'PeMS',
  3: 'Clear Guide',
}

type Granularity = 'daily' | 'monthly'

export function createAverageChart(
  data: DailyHistoricalRouteData[] | MonthlyHistoricalRouteData[],
  startDate: string,
  endDate: string,
  routeRenderOption: RouteRenderOption,
  granularity: Granularity
) {
  const { valueKey, percentileTitle } = getValueKeyAndTitle(routeRenderOption)
  const xAxisLabels = generateXAxisLabels(startDate, endDate, granularity, data)

  const seriesData = data
    .map((source) => {
      const sourceData = xAxisLabels
        .map((label) => {
          const matchedEntry = findMatchedEntry(source, label, granularity)

          if (matchedEntry) {
            const value = matchedEntry[valueKey as keyof typeof matchedEntry]
            if (value) {
              return [label, parseFloat(value.toFixed(2))]
            }
          }
          // If no matched entry, return null to maintain alignment with xAxisLabels
          return [label, null]
        })
        .filter((point) => point !== undefined)

      return {
        name: sourceMapping[source.sourceId as keyof typeof sourceMapping],
        type: 'line',
        data: sourceData,
        symbol: 'none',
        color: getDataSourceColor(source.sourceId),
      } as SeriesOption
    })
    .filter((series) => series.data.some((point) => point[1] !== null))

  if (seriesData.length === 0) {
    return {
      title: {
        text: 'No Data',
        left: 'center',
        textStyle: {
          fontSize: 16,
          fontWeight: 'bold',
          align: 'center',
        },
      },
    } as EChartsOption
  }

  const legend = generateLegend(seriesData)

  const tooltip = createTooltip()

  const title: TitleComponentOption = {
    text: `{title|${capitalizeFirstLetter(granularity)} ${percentileTitle}}`,
    left: 'center',
    textStyle: {
      rich: {
        title: {
          fontSize: 16,
          fontWeight: 'bold',
          align: 'center',
        },
      },
    },
  }

  const option: EChartsOption = {
    title,
    tooltip,
    legend,
    xAxis: {
      type: 'category',
      data: xAxisLabels,
    },
    yAxis: {
      type: 'value',
      interval: 20,
      max: 100,
    },
    series: seriesData,
  }

  return option
}

function getValueKeyAndTitle(routeRenderOption: RouteRenderOption) {
  let valueKey = 'average'
  let percentileTitle = 'Average'

  if (routeRenderOption === RouteRenderOption.Percentile_85th) {
    valueKey = 'eightyFifthSpeed'
    percentileTitle = '85th Percentile'
  } else if (routeRenderOption === RouteRenderOption.Percentile_95th) {
    valueKey = 'ninetyFifthSpeed'
    percentileTitle = '95th Percentile'
  } else if (routeRenderOption === RouteRenderOption.Percentile_99th) {
    valueKey = 'ninetyNinthSpeed'
    percentileTitle = '99th Percentile'
  }

  return { valueKey, percentileTitle }
}

function generateLegend(seriesData: SeriesOption[]) {
  return {
    y: 30,
    data: seriesData.map((series) => {
      return {
        name: series.name,
        icon: SolidLineSeriesSymbol,
        color: series.color,
      }
    }),
  }
}

function generateXAxisLabels(startDate, endDate, granularity, data) {
  const start = new Date(startDate)
  const end = new Date(endDate)

  if (granularity === 'monthly') {
    // Extract all months from the data
    const monthsSet = new Set()

    data.forEach((item) => {
      if (item.monthlyAverages) {
        item.monthlyAverages.forEach((monthData) => {
          if (monthData.month) {
            monthsSet.add(monthData.month)
          }
        })
      }
    })

    // Convert to array and sort dates
    const sortedMonths = Array.from(monthsSet)
      .map((month) => new Date(month))
      .sort((a, b) => compareAsc(a, b))

    // Format dates as MM/yyyy
    return sortedMonths.map((date) => format(date, 'MM/yyyy'))
  } else {
    const days = eachDayOfInterval({ start, end }).map((date) => ({
      formatted: format(date, 'M/d/yyyy'),
      date: date,
    }))

    return days
      .sort((a, b) => compareAsc(a.date, b.date))
      .map(({ formatted }) => formatted)
  }
}

function findMatchedEntry(
  source: DailyHistoricalRouteData | MonthlyHistoricalRouteData,
  label: string,
  granularity: Granularity
) {
  if (granularity === 'monthly' && 'monthlyAverages' in source) {
    if (!source.monthlyAverages) return null
    return source.monthlyAverages.find((entry) => {
      if (!entry.month) return null
      const entryDate = new Date(entry.month)
      const formattedDate = format(entryDate, 'MM/yyyy')
      return formattedDate === label
    })
  } else if (granularity === 'daily' && 'dailyAverages' in source) {
    if (!source.dailyAverages) return null
    return source.dailyAverages.find((entry) => {
      if (!entry.date) return null
      const entryDate = new Date(entry.date)
      const formattedDate = format(entryDate, 'M/d/yyyy')
      return formattedDate === label
    })
  }
}

function capitalizeFirstLetter(string: string) {
  return string.charAt(0).toUpperCase() + string.slice(1)
}
