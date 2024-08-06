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

export const transformCongestionTrackerData = (
  response: CongestionTrackerResponse
) => {
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

  for (let i = 0; i < totalCells; i++) {
    const rowIndex = Math.floor(i / 7)
    const colIndex = i % 7
    const dateString =
      i >= monthStartDay && dayCounter <= days
        ? `${year}-${String(month).padStart(2, '0')}-${String(
            dayCounter
          ).padStart(2, '0')}`
        : ''

    const cellWidth = 80

    grids.push({
      show: true,
      left: `${(colIndex / 7) * cellWidth + 5}%`,
      top: `${(rowIndex / 7) * 100 + 20}%`,
      width: `${(1 / 7) * cellWidth}%`,
      height: `${(1 / 7) * 85}%`,
      borderWidth: 1,
    })

    xAxes.push({
      type: 'time',
      show: false,
      gridIndex: count,
    })

    yAxes.push({
      type: 'value',
      show: i % 7 === 0,
      gridIndex: count,
      max: ySeriesMax,
      min: 0,
      axisLine: {
        show: false,
      },
      splitLine: {
        show: false,
      },
      axisLabel: {
        fontSize: 8,
        hideOverlap: true,
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

      if (dayData.series.average) {
        series.push({
          name: `Average`,
          type: 'line',
          xAxisIndex: count,
          yAxisIndex: count,
          color: Color.Red,
          data: transformSeriesData(dayData.series.average),
          showSymbol: false,
        })
      }

      if (dayData.series.eightyFifth) {
        series.push({
          name: `85th Percentile`,
          type: 'line',
          xAxisIndex: count,
          yAxisIndex: count,
          color: Color.Blue,
          data: transformSeriesData(dayData.series.eightyFifth),
          showSymbol: false,
        })
      }

      titles.push({
        left:
          parseFloat(`${(colIndex / 7) * cellWidth + 5.3}%`) +
          parseFloat(`${(1 / 7) * 10 - 1}%`) / 2 +
          '%',
        top: `${(rowIndex / 7) * 100 + 20}%`,
        textAlign: 'center',
        text: String(dayCounter),
        textStyle: {
          fontSize: 10,
        },
      })
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
        left: `${(colIndex / 7) * cellWidth + 10}%`,
        top: `${(rowIndex / 7) * 100 + 16}%`,
        textAlign: 'center',
        text: daysOfTheWeek[i],
        textStyle: {
          fontSize: 12,
        },
      })
    }

    count++
  }

  const legend = createLegend({
    data: [
      { name: 'Speed Limit', icon: DashedLineSeriesSymbol },
      { name: 'Average', icon: SolidLineSeriesSymbol },
      { name: '85th Percentile', icon: SolidLineSeriesSymbol },
    ],
  })

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
  }

  return option
}
