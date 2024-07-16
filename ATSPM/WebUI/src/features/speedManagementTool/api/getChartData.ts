import { createTooltip } from '@/features/charts/common/transformers'
import { RouteRenderOption } from '@/features/speedManagementTool/speedManagementStore'

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
  const seriesData = data.map((source) => ({
    name: sourceMapping[source.sourceId],
    type: 'line',
    data: source.monthlyAverages
      .map(({ year, month, averageSpeed }) => ({
        value: averageSpeed,
        name: `${month}/${year}`,
      }))
      .reverse(),
  }))

  const xAxisLabels = generateXAxisLabels(startDate, endDate)

  const tooltip = createTooltip({
    formatters: [
      {
        name: sourceMapping[1],
        format: (val) => `${parseInt(val).toFixed(0)} mph`,
      },
      {
        name: sourceMapping[2],
        format: (val) => `${parseInt(val).toFixed(0)} mph`,
      },
      {
        name: sourceMapping[3],
        format: (val) => `${parseInt(val).toFixed(0)} mph`,
      },
    ],
  })

  let percentile = 'Average'

  if (routeRenderOption === RouteRenderOption.Percentile_85th) {
    percentile = '85th Percentile'
  } else if (routeRenderOption === RouteRenderOption.Percentile_95th) {
    percentile = '95th Percentile'
  } else if (routeRenderOption === RouteRenderOption.Percentile_99th) {
    percentile = '99th Percentile'
  }

  const option = {
    title: { text: `Monthly ${percentile}` },
    tooltip: tooltip,
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
  const seriesData = data.map((source) => ({
    name: sourceMapping[source.sourceId],
    type: 'line',
    data: source.dailyAverages
      .map(({ date, averageSpeed }) => ({
        value: averageSpeed,
        name: `${date.split('T')[0]}`,
      }))
      .reverse(),
  }))

  const xAxisLabels = generateDailyXAxisLabels(startDate, endDate)

  const tooltip = createTooltip({
    formatters: [
      {
        name: sourceMapping[1],
        format: (val) => `${parseInt(val).toFixed(0)} mph`,
      },
      {
        name: sourceMapping[2],
        format: (val) => `${parseInt(val).toFixed(0)} mph`,
      },
      {
        name: sourceMapping[3],
        format: (val) => `${parseInt(val).toFixed(0)} mph`,
      },
    ],
  })

  let percentile = 'Average'

  if (routeRenderOption === RouteRenderOption.Percentile_85th) {
    percentile = '85th Percentile'
  } else if (routeRenderOption === RouteRenderOption.Percentile_95th) {
    percentile = '95th Percentile'
  } else if (routeRenderOption === RouteRenderOption.Percentile_99th) {
    percentile = '99th Percentile'
  }

  const option = {
    title: { text: `Daily ${percentile}` },
    tooltip: tooltip,
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

function generateXAxisLabels(startDate: string, endDate: string): string[] {
  const start = new Date(startDate)
  const end = new Date(endDate)
  const labels = []

  for (let dt = new Date(start); dt <= end; dt.setMonth(dt.getMonth() + 1)) {
    labels.push(`${dt.getMonth() + 1}/${dt.getFullYear().toString().slice(-2)}`)
  }

  return labels
}

function generateDailyXAxisLabels(startDate, endDate) {
  let labels = []
  let currentDate = new Date(startDate)
  const end = new Date(endDate)

  while (currentDate <= end) {
    // Construct the date string as 'month/day'
    let month = currentDate.getMonth() + 1 // getMonth() is zero-based
    let day = currentDate.getDate()
    let formattedDate = `${month}/${day}`

    labels.push(formattedDate) // Add the formatted date string to labels
    currentDate.setDate(currentDate.getDate() + 1) // Increment the date by 1 day
  }

  return labels
}
