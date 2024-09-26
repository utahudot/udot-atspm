import { createTooltip } from '@/features/charts/common/transformers'
import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import {
  compareAsc,
  eachDayOfInterval,
  eachMonthOfInterval,
  format,
} from 'date-fns'

interface MonthlyAverage {
  month: number
  year: number
  averageSpeed: number
}

interface MonthlyHistoricalData {
  sourceId: number
  monthlyAverages: MonthlyAverage[]
}

interface DailyAverage {
  date: string
  averageSpeed: number
}

interface DailyHistoricalRouteData {
  sourceId: number
  dailyAverages: DailyAverage[]
}

const sourceMapping = {
  1: 'ATSPM',
  2: 'PeMS',
  3: 'Clear Guide',
}

export function createMonthlyAverageChart(
  data: MonthlyHistoricalData[],
  startDate: string,
  endDate: string,
  routeRenderOption?: RouteRenderOption
) {
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

  const xAxisLabels = generateXAxisLabels(startDate, endDate)

  const seriesData = data.map((source) => {
    const sourceData = xAxisLabels
      .map((label) => {
        const matchedEntry = source.monthlyAverages.find((entry) => {
          const entryDate = new Date(entry.month)
          const formattedDate = format(entryDate, 'MM/yyyy')
          return formattedDate === label
        })

        if (matchedEntry) {
          return [label, parseFloat(matchedEntry[valueKey].toFixed(2))]
        }
      })
      .filter(Boolean)

    return {
      name: sourceMapping[source.sourceId],
      type: 'line',
      data: sourceData,
    }
  })

  const tooltip = createTooltip({
    formatters: [
      {
        name: sourceMapping[1],
        format: (val) => `${parseFloat(val).toFixed(2)} mph`,
      },
      {
        name: sourceMapping[2],
        format: (val) => `${parseFloat(val).toFixed(2)} mph`,
      },
      {
        name: sourceMapping[3],
        format: (val) => `${parseFloat(val).toFixed(2)} mph`,
      },
    ],
  })

  const option = {
    title: { text: `Monthly ${percentileTitle}` },
    tooltip,
    legend: {
      data: seriesData.map((s) => s.name),
      y: 30,
    },
    xAxis: {
      type: 'category',
      data: xAxisLabels,
    },
    yAxis: {
      type: 'value',
    },
    dataZoom: [
      {
        type: 'inside',
      },
      {
        type: 'slider',
        height: 25,
      },
    ],
    series: seriesData,
  }

  return option
}

export function createDailyAverageChart(
  data: DailyHistoricalRouteData[],
  startDate: string,
  endDate: string,
  routeRenderOption?: RouteRenderOption
) {
  let valueKey = 'average' // Default to averageSpeed
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

  const xAxisLabels = generateDailyXAxisLabels(startDate, endDate)

  const seriesData = data.map((source) => {
    const sourceData = xAxisLabels
      .map((label) => {
        const matchedEntry = source.dailyAverages.find((entry) => {
          const formattedDate = format(new Date(entry.date), 'M/d/yyyy') // Format date to match xAxisLabels
          return formattedDate === label
        })

        if (matchedEntry) {
          return [label, parseFloat(matchedEntry[valueKey].toFixed(2))] // Use valueKey to dynamically access the correct value and round it to 2 decimal places
        }
      })
      .filter(Boolean) // Remove undefined entries where no match is found

    return {
      name: sourceMapping[source.sourceId],
      type: 'line',
      data: sourceData,
    }
  })

  const tooltip = createTooltip({
    formatters: [
      {
        name: sourceMapping[1],
        format: (val) => `${parseFloat(val).toFixed(2)} mph`,
      },
      {
        name: sourceMapping[2],
        format: (val) => `${parseFloat(val).toFixed(2)} mph`,
      },
      {
        name: sourceMapping[3],
        format: (val) => `${parseFloat(val).toFixed(2)} mph`,
      },
    ],
  })

  const option = {
    title: { text: `Daily ${percentileTitle}` },
    tooltip,
    legend: {
      data: seriesData.map((s) => s.name),
      y: 30,
    },
    xAxis: {
      type: 'category',
      data: xAxisLabels,
    },
    yAxis: {
      type: 'value',
    },
    dataZoom: [
      {
        type: 'inside',
      },
      {
        type: 'slider',
        height: 25,
      },
    ],
    series: seriesData,
  }

  return option
}

// Adjusted to handle 'MM/yyyy' and ensure proper sorting
function generateXAxisLabels(startDate: string, endDate: string): string[] {
  const start = new Date(startDate)
  const end = new Date(endDate)

  const months = eachMonthOfInterval({ start, end }).map((date) => ({
    formatted: format(date, 'MM/yyyy'),
    date: date, // Storing actual date object for sorting
  }))

  // Sort by actual date object
  return months
    .sort((a, b) => compareAsc(a.date, b.date))
    .map(({ formatted }) => formatted)
}

function generateDailyXAxisLabels(
  startDate: string,
  endDate: string
): string[] {
  const days = eachDayOfInterval({
    start: new Date(startDate),
    end: new Date(endDate),
  }).map((date) => ({
    formatted: format(date, 'M/d/yyyy'),
    date: date,
  }))

  return days
    .sort((a, b) => compareAsc(a.date, b.date))
    .map(({ formatted }) => formatted)
}
