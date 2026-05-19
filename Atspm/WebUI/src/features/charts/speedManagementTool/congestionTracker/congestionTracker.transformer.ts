import {
  CongestionTrackingDto,
  SpeedDataDto,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import {
  createLegend,
  createTitle,
  createTooltip,
  transformSeriesData,
} from '@/features/charts/common/transformers'
import {
  Color,
  DashedLineSeriesSymbol,
  lightenColor,
  SolidLineSeriesSymbol,
} from '@/features/charts/utils'
import { SM_ChartType } from '@/features/speedManagementTool/api/getSMCharts'
import {
  addDays,
  addHours,
  differenceInCalendarDays,
  startOfDay,
} from 'date-fns'
import {
  GridComponentOption,
  LegendComponentOption,
  SeriesOption,
  TitleComponentOption,
  XAXisComponentOption,
  YAXisComponentOption,
} from 'echarts'
import { DataPoint } from '../../common/types'

/**
 * Returns the “week number” (1-based) in the overall chart range
 * given an index offset (dayIndex) from the start of the date range.
 */
const getWeekNumberInRange = (dayIndex: number): number => {
  // For example, dayIndex = 0..6 => week 1, dayIndex = 7..13 => week 2, etc.
  return Math.floor(dayIndex / 7) + 1
}

/**
 * Builds a string like "1/1-1/7" for a given week’s start and end.
 * We derive these from the overall earliest date + (weekNumber-1)*7, etc.
 */
const getWeekDateRange = (startDate: Date, weekNumber: number): string => {
  const weekStart = addDays(startDate, (weekNumber - 1) * 7)
  const weekEnd = addDays(weekStart, 6)
  // Format month/day
  const startString = `${weekStart.getMonth() + 1}/${weekStart.getDate()}`
  const endString = `${weekEnd.getMonth() + 1}/${weekEnd.getDate()}`
  return `${startString}-${endString}`
}

export const transformCongestionTrackerData = (
  response: CongestionTrackingDto,
  view: 'week' | 'month' = 'month'
) => {
  const chart = transformData(response, view)
  return {
    type: SM_ChartType.CONGESTION_TRACKING,
    charts: [chart],
  }
}

export const transformData = (
  response: CongestionTrackingDto,
  view: 'week' | 'month' = 'month'
) => {
  if (!response.data || response.data.length === 0) return null

  // 1) Determine overall date range from the data:
  const dates = response.data.map((d) => new Date(d.date as string))
  const earliestDate = new Date(Math.min(...dates.map((d) => d.valueOf())))
  const latestDate = new Date(Math.max(...dates.map((d) => d.valueOf())))

  // 2) Build an array of all days within that [earliest, latest] range:
  const totalDaysInRange =
    differenceInCalendarDays(latestDate, earliestDate) + 1
  const dayArray: Date[] = []
  for (let i = 0; i < totalDaysInRange; i++) {
    dayArray.push(addDays(earliestDate, i))
  }

  // 3) We need to offset so that the earliestDate lines up on its correct weekday column:
  const startDayOfWeek = earliestDate.getDay() // 0 (Sun) .. 6 (Sat)
  // 4) Compute total number of “cells” to fill complete rows of weeks (Sunday-Saturday):
  const leftover = (7 - ((startDayOfWeek + totalDaysInRange) % 7)) % 7
  const totalCells = startDayOfWeek + totalDaysInRange + leftover

  // Prepare chart options:
  const grids: GridComponentOption[] = []
  const xAxes: XAXisComponentOption[] = []
  const yAxes: YAXisComponentOption[] = []
  const series: SeriesOption[] = []
  const titles: TitleComponentOption[] = []

  // For vertical spacing, etc. (in “month” view, we do a grid of small multiples)
  const calendarDayBuffer = 40
  const daysOfTheWeek = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']
  const cellWidth = 74
  const paddingLeft = 5
  const paddingTop = 90
  const cellHeight = 100
  const rowCount = Math.ceil(totalCells / 7)

  // Determine max speed to set a suitable y-axis limit:
  const ySeriesMax =
    Math.max(
      response.speedLimit || 0,
      ...response.data.flatMap((d: SpeedDataDto) => {
        let avg: number[] = []
        let e85: number[] = []
        if (d.series) {
          if (d.series.average) {
            avg = d.series.average.map((pt) => pt.value as number)
          }
          if (d.series.eightyFifth) {
            e85 = d.series.eightyFifth.map((pt) => pt.value as number)
          }
        }
        return [...avg, ...e85]
      })
    ) + calendarDayBuffer

  // Map stringified date => SpeedDataDto for quick lookup:
  const dataByDate: Record<string, SpeedDataDto> = {}
  response.data.forEach((item) => {
    const key = item.date.split('T')[0] // “YYYY-MM-DD”
    dataByDate[key] = item
  })

  let count = 0 // Grid index counter

  // 5) Iterate over every “cell” we must display, building small multiples for MONTH view,
  //    or all-overlapping one big grid for WEEK view (unchanged).
  for (let i = 0; i < totalCells; i++) {
    // row & col for “month” layout
    const rowIndex = Math.floor(i / 7)
    const colIndex = i % 7

    // The index into dayArray (some might be before 0 or beyond dayArray length if i<startDayOfWeek or i>=startDayOfWeek+totalDaysInRange)
    const dayIndex = i - startDayOfWeek
    const isValidDay = dayIndex >= 0 && dayIndex < dayArray.length

    let dayObj: Date | null = null
    let dayData: SpeedDataDto | undefined
    let dayKey = ''
    if (isValidDay) {
      dayObj = dayArray[dayIndex]
      dayKey = dayObj.toISOString().split('T')[0] // “YYYY-MM-DD”
      dayData = dataByDate[dayKey]
    }

    // Build each grid cell. In “month” view we do separate small multiples,
    // but in “week” view they’ll all overlap with the same top/left.
    grids.push({
      show: true,
      left: `${(colIndex / 7) * cellWidth + paddingLeft}%`,
      top:
        view === 'month'
          ? `${rowIndex * cellHeight + paddingTop}px`
          : `${paddingTop}px`, // everything stacked for “week” view
      width: `${(1 / 7) * cellWidth}%`,
      height:
        view === 'month' ? `${cellHeight}px` : `${cellHeight * rowCount}px`,
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
      axisLine: { show: false },
      splitLine: {
        show: view === 'week',
        lineStyle: { type: 'dashed' },
      },
      axisLabel: {
        fontSize: view === 'month' ? 8 : 10,
        hideOverlap: true,
        show: i % 7 === 0, // Only show on first column if month view
        formatter: (val) => (view === 'month' && val === 0 ? '' : String(val)),
      },
    })

    if (dayObj && dayData) {
      // Speed limit
      if (response.speedLimit) {
        series.push({
          name: 'Speed Limit',
          type: 'line',
          lineStyle: {
            type: 'dashed',
            width: 1,
          },
          xAxisIndex: count,
          yAxisIndex: count,
          color: Color.Black,
          data: Array.from({ length: 24 }, (_, hr) => [
            addHours(startOfDay(dayObj), hr),
            response.speedLimit,
          ]),
          showSymbol: false,
        })
      }

      // Figure out which “week number” (in entire range) we’re in:
      const weekNumber = getWeekNumberInRange(dayIndex)
      // Build string like “1/1-1/7” if we're in “week” view:
      const weekRange = getWeekDateRange(earliestDate, weekNumber)

      // Average
      if (dayData.series && dayData.series.average) {
        series.push({
          name: view === 'month' ? 'Average' : `${weekRange} Average`,
          type: 'line',
          xAxisIndex: count,
          yAxisIndex: count,
          color: lightenColor(Color.Blue, weekNumber * 4),
          data: transformSeriesData(dayData.series.average as DataPoint[]),
          showSymbol: false,
        })
      }

      // 85th Percentile
      if (dayData.series && dayData.series.eightyFifth) {
        series.push({
          name:
            view === 'month'
              ? '85th Percentile'
              : `${weekRange} 85th Percentile`,
          type: 'line',
          xAxisIndex: count,
          yAxisIndex: count,
          color: lightenColor(Color.Red, weekNumber * 4),
          data: transformSeriesData(dayData.series.eightyFifth as DataPoint[]),
          showSymbol: false,
        })
      }

      // Label the day number in the corner (month view).  Also check if it's day 1 => show the month name
      if (view === 'month') {
        const dayNumber = dayObj.getDate()
        const monthName = dayObj.toLocaleString('default', { month: 'short' })

        let isNewMonth = false
        if (dayIndex === 0) {
          // This is the very earliest day in the entire dataset, so definitely "new month."
          isNewMonth = true
        } else {
          const prevIndex = dayIndex - 1
          if (prevIndex >= 0) {
            const prevDayObj = dayArray[prevIndex]
            if (prevDayObj && prevDayObj.getMonth() !== dayObj.getMonth()) {
              isNewMonth = true
            }
          }
        }

        titles.push({
          left:
            parseFloat(`${(colIndex / 7) * cellWidth + 5.3}%`) +
            parseFloat(`${(1 / 7) * 10 - 1}%`) / 2 +
            (isNewMonth ? 1 : 0) +
            '%',
          top: `${rowIndex * cellHeight + paddingTop}px`,
          textAlign: 'center',
          // E.g. “1 (Feb)” if the day is the 1st of the month
          text: isNewMonth ? `${dayNumber} (${monthName})` : String(dayNumber),
          textStyle: {
            fontSize: 10,
          },
        })
      } else {
        // week view => no day label in each small cell
        titles.push({})
      }
    } else {
      // Invalid day for this cell => push empty series so ECharts doesn’t break
      series.push({
        type: 'line',
        xAxisIndex: count,
        yAxisIndex: count,
        data: [],
        showSymbol: false,
      })
      titles.push({})
    }

    // Days-of-week label at top row for month view:
    if (rowIndex === 0) {
      titles.push({
        left: `${(colIndex / 7) * cellWidth + paddingLeft + 5}%`,
        top: paddingTop - 20,
        textAlign: 'center',
        text: daysOfTheWeek[colIndex],
        textStyle: {
          fontSize: 12,
        },
      })
    }

    count++
  }

  // Build legend:
  let legend: Partial<LegendComponentOption>
  if (view === 'week') {
    // Single chart with lines labeled by week range:
    const numOfWeeks = Math.ceil(totalDaysInRange / 7)
    const legendData = []
    // Averages first:
    for (let w = 1; w <= numOfWeeks; w++) {
      const rangeStr = getWeekDateRange(earliestDate, w)
      legendData.push({
        name: `${rangeStr} Average`,
        icon: SolidLineSeriesSymbol,
        textStyle: { fontSize: 10 },
      })
    }
    // 85th percentiles:
    for (let w = 1; w <= numOfWeeks; w++) {
      const rangeStr = getWeekDateRange(earliestDate, w)
      legendData.push({
        name: `${rangeStr} 85th Percentile`,
        icon: SolidLineSeriesSymbol,
        textStyle: { fontSize: 10 },
      })
    }
    legend = createLegend({ data: legendData })
  } else {
    // Month/multi-month grid: simple legend
    legend = createLegend({
      data: [
        { name: 'Speed Limit', icon: DashedLineSeriesSymbol },
        { name: 'Average', icon: SolidLineSeriesSymbol },
        { name: '85th Percentile', icon: SolidLineSeriesSymbol },
      ],
    })
  }

  const tooltip = createTooltip()

  const dateOptions = {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  } as const

  // Build the overall title.  Show the raw date range or something user-friendly:
  const dateRangeLabel = `${earliestDate.toLocaleDateString(
    'en-US',
    dateOptions
  )} - ${latestDate.toLocaleDateString('en-US', dateOptions)}`
  const title = createTitle({
    title: `Congestion Tracker - ${response.segmentName}`,
    dateRange: dateRangeLabel,
  })

  return {
    title: [...titles, title],
    grid: grids,
    legend,
    tooltip,
    xAxis: xAxes,
    yAxis: yAxes,
    series,
    response,
    currentView: view,
    numOfRows: Math.ceil(totalCells / 7),
  }
}
