import {
  createLegend,
  createTitle,
  createTooltip,
  transformSeriesData,
} from '@/features/charts/common/transformers'
import {
  Color,
  DashedLineSeriesSymbol,
  SolidLineSeriesSymbol,
} from '@/features/charts/utils'
import { CongestionTrackerResponse } from '@/features/speedManagementTool/api/getCongestionTrackingData'
import { addHours, startOfDay } from 'date-fns'

// Utility function to darken a color
const lightenColor = (color: string, percent: number) => {
  const num = parseInt(color.slice(1), 16),
    amt = Math.round(2.55 * percent),
    R = (num >> 16) + amt,
    G = ((num >> 8) & 0x00ff) + amt,
    B = (num & 0x0000ff) + amt

  return `#${(
    0x1000000 +
    (R < 255 ? (R < 1 ? 0 : R) : 255) * 0x10000 +
    (G < 255 ? (G < 1 ? 0 : G) : 255) * 0x100 +
    (B < 255 ? (B < 1 ? 0 : B) : 255)
  )
    .toString(16)
    .slice(1)
    .toUpperCase()}`
}

// Calculate the week number based on the day of the month
const getWeekNumber = (dayOfMonth: number, monthStartDay: number): number => {
  return Math.ceil((dayOfMonth + monthStartDay) / 7)
}

// Calculate the date range for each week in the format "2/1 - 2/3"
const getWeekDateRange = (
  weekNumber: number,
  monthStartDay: number,
  year: number,
  month: number
) => {
  const startDayOfWeek = (weekNumber - 1) * 7 + 1 - monthStartDay
  const endDayOfWeek = Math.min(
    startDayOfWeek + 6,
    new Date(year, month, 0).getDate()
  )

  const startDate = new Date(year, month - 1, startDayOfWeek)
  const endDate = new Date(year, month - 1, endDayOfWeek)

  const startString = `${startDate.getMonth() + 1}/${startDate.getDate()}`
  const endString = `${endDate.getMonth() + 1}/${endDate.getDate()}`

  return `${startString}-${endString}`
}

// Allow user to select a week or month view
export const transformCongestionTrackerData = (
  response: CongestionTrackerResponse,
  view: 'week' | 'month' = 'month'
) => {
  // if (!response.data) return null

  const date = response.data[0].date

  const daysInMonth = (year: number, month: number) =>
    new Date(year, month, 0).getDate()

  const year = new Date(date).getFullYear()
  const month = new Date(date).getMonth() + 1 // JavaScript months are 0-11
  const days = daysInMonth(year, month)

  const grids = []
  const xAxes = []
  const yAxes = []
  const series = []
  const titles = []
  let count = 0

  const monthStartDay = new Date(year, month - 1, 1).getDay()
  const totalCells = monthStartDay + days + (7 - ((monthStartDay + days) % 7))
  const calendarDayBuffer = 40
  const ySeriesMax =
    Math.max(
      response.speedLimit || 0,
      ...response.data.flatMap((d) => [
        ...(d.series.average
          ? d.series.average.map((point) => point.value)
          : []),
        ...(d.series.eightyFifth
          ? d.series.eightyFifth.map((point) => point.value)
          : []),
      ])
    ) + calendarDayBuffer

  let dayCounter = 1
  const daysOfTheWeek = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']

  const cellWidth = 74
  const gapBetweenCells = 110
  const paddingLeft = 5
  const paddingTop = 20
  const cellHeightPercent = view === 'month' ? 110 : 75

  for (let i = 0; i < totalCells; i++) {
    const rowIndex = Math.floor(i / 7)
    const colIndex = i % 7
    const dateString =
      i >= monthStartDay && dayCounter <= days
        ? `${year}-${String(month).padStart(2, '0')}-${String(
            dayCounter
          ).padStart(2, '0')}`
        : ''

    grids.push({
      show: true,
      left: `${(colIndex / 7) * cellWidth + paddingLeft}%`,
      top:
        view === 'month'
          ? `${(rowIndex / 7) * gapBetweenCells + paddingTop}%`
          : `${paddingTop}%`,
      width: `${(1 / 7) * cellWidth}%`,
      height:
        view === 'month'
          ? `${(1 / 7) * cellHeightPercent}%`
          : `${cellHeightPercent}%`,
      borderWidth: 1,
    })

    xAxes.push({
      type: 'time',
      show: false,
      gridIndex: count,
    })

    yAxes.push({
      type: 'value',
      gridIndex: count,
      max: Math.round(ySeriesMax),
      min: 0,
      axisLine: {
        show: false,
      },
      splitLine: {
        show: view === 'week',
        lineStyle: {
          type: 'dashed',
        },
      },
      axisLabel: {
        fontSize: view === 'month' ? 8 : 10,
        hideOverlap: true,
        show: i % 7 === 0,
        formatter: (value: number) => {
          return view === 'month' && value === 0 ? '' : value
        },
      },
    })

    const dayData = response?.data?.find((d) => d?.date?.startsWith(dateString))
    if (dateString && dayData) {
      if (response.speedLimit) {
        series.push({
          name: `Speed Limit`,
          type: 'line',
          lineStyle: {
            type: 'dashed',
            width: 1,
          },
          xAxisIndex: count,
          yAxisIndex: count,
          color: Color.Black,
          data: Array.from({ length: 24 }, (_, i) => [
            addHours(startOfDay(new Date(dayData.date)), i),
            response.speedLimit,
          ]),
          showSymbol: false,
        })
      }

      const weekNumber = getWeekNumber(dayCounter, monthStartDay)
      const weekRange = getWeekDateRange(weekNumber, monthStartDay, year, month)

      if (dayData.series.average) {
        series.push({
          name: view === 'month' ? 'Average' : `${weekRange} Average`,
          type: 'line',
          xAxisIndex: count,
          yAxisIndex: count,
          color: lightenColor(Color.Blue, weekNumber * 8),
          data: transformSeriesData(dayData.series.average),
          showSymbol: false,
        })
      }

      if (dayData.series.eightyFifth) {
        series.push({
          name:
            view === 'month'
              ? '85th Percentile'
              : `${weekRange} 85th Percentile`,
          type: 'line',
          xAxisIndex: count,
          yAxisIndex: count,
          color: lightenColor(Color.Red, weekNumber * 8),
          data: transformSeriesData(dayData.series.eightyFifth),
          showSymbol: false,
        })
      }

      if (view === 'month') {
        titles.push({
          left:
            parseFloat(`${(colIndex / 7) * cellWidth + 5.3}%`) +
            parseFloat(`${(1 / 7) * 10 - 1}%`) / 2 +
            '%',
          top: `${(rowIndex / 7) * gapBetweenCells + paddingTop}%`,
          textAlign: 'center',
          text: String(dayCounter),
          textStyle: {
            fontSize: 10,
          },
        })
      }

      dayCounter++
    } else {
      series.push({
        type: 'line',
        xAxisIndex: count,
        yAxisIndex: count,
        data: [],
        showSymbol: false,
      })

      titles.push({})
    }

    if (i < 7) {
      titles.push({
        left: `${(colIndex / 7) * cellWidth + paddingLeft + 5}%`,
        top: '16%',
        textAlign: 'center',
        text: daysOfTheWeek[i],
        textStyle: {
          fontSize: 12,
        },
      })
    }

    count++
  }

  let legend = ''
  if (view === 'week') {
    // Calculate number of weeks
    const numOfWeeks = Math.ceil((days + monthStartDay) / 7)

    // Dynamically generate the legend entries
    const legendData = []

    // Add all averages first
    for (let week = 1; week <= numOfWeeks; week++) {
      const weekRange = getWeekDateRange(week, monthStartDay, year, month)
      legendData.push({
        name: `${weekRange} Average`,
        icon: SolidLineSeriesSymbol,
        textStyle: {
          fontSize: 10,
        },
      })
    }

    // Add all 85th percentiles
    for (let week = 1; week <= numOfWeeks; week++) {
      const weekRange = getWeekDateRange(week, monthStartDay, year, month)
      legendData.push({
        name: `${weekRange} 85th Percentile`,
        icon: SolidLineSeriesSymbol,
        textStyle: {
          fontSize: 10,
        },
      })
    }

    legend = createLegend({
      data: legendData,
    })
  } else {
    legend = createLegend({
      data: [
        { name: 'Speed Limit', icon: DashedLineSeriesSymbol },
        { name: 'Average', icon: SolidLineSeriesSymbol },
        { name: '85th Percentile', icon: SolidLineSeriesSymbol },
      ],
    })
  }

  const monthAndYear = new Date(date).toLocaleString('default', {
    month: 'long',
    year: 'numeric',
  })

  const tooltip = createTooltip()

  const title = createTitle({
    title: `Congestion Tracker\n${
      response.segmentName
    } (between MP ${response.startingMilePoint.toFixed(
      1
    )} and MP ${response.endingMilePoint.toFixed(1)})`,
    dateRange: monthAndYear,
  })

  const option = {
    title: titles.concat([title]),
    grid: grids,
    legend: legend,
    tooltip: tooltip,
    xAxis: xAxes,
    yAxis: yAxes,
    series: series,
    response: response,
    currentView: view,
  }

  return option
}
