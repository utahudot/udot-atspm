import { CongestionTrackerResponse } from '@/features/speedManagementTool/api/getCongestionTrackingData'

export const transformCongestionTrackerData = (
  response: CongestionTrackerResponse
) => {
  console.log('response.data', response.data)
  const date = response.data[0].date

  const daysInMonth = (year, month) => new Date(year, month, 0).getDate()

  const year = new Date(date).getFullYear()
  const month = new Date(date).getMonth()
  const days = daysInMonth(year, month)

  const grids = []
  const xAxes = []
  const yAxes = []
  const series = []
  const titles = []
  let count = 0

  const monthStartDay = new Date(year, month - 1, 1).getDay()
  const totalCells = monthStartDay + days + (7 - ((monthStartDay + days) % 7))

  let dayCounter = 1

  for (let i = 0; i < totalCells; i++) {
    const rowIndex = Math.floor(i / 7)
    const colIndex = i % 7
    const date =
      i >= monthStartDay && i < monthStartDay + days
        ? `${year}-${String(month).padStart(2, '0')}-${String(
            dayCounter
          ).padStart(2, '0')}`
        : ''

    grids.push({
      show: true,
      left: `${(colIndex / 7) * 90}%`,
      top: `${(rowIndex / 7) * 90 + 10}%`,
      width: `${(1 / 7) * 90}%`,
      height: `${(1 / 7) * 90}%`,
      borderWidth: 1,
    })

    xAxes.push({
      type: 'value',
      show: false,
      min: 0,
      max: 1,
      gridIndex: count,
    })

    yAxes.push({
      type: 'value',
      show: false,
      min: -0.4,
      max: 1.4,
      gridIndex: count,
    })

    if (date) {
      series.push({
        name: `Average`,
        type: 'line',
        xAxisIndex: count,
        yAxisIndex: count,
        data: response.data[i].series.average,
        showSymbol: false,
      })

      series.push({
        name: `85th Percentile`,
        type: 'line',
        xAxisIndex: count,
        yAxisIndex: count,
        data: response.data[i].series.eightyFifth,
        showSymbol: false,
      })

      titles.push({
        left:
          parseFloat(`${(colIndex / 7) * 90 + 5}%`) +
          parseFloat(`${(1 / 7) * 90 - 1}%`) / 2 +
          '%',
        top: (rowIndex / 7) * 90 + 10 + '%',
        textAlign: 'center',
        text: count,
        textStyle: {
          fontSize: 12,
          fontWeight: 'normal',
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

    count++
  }

  const option = {
    title: titles.concat([
      {
        text: 'Calendar with Line Charts',
        top: 'top',
        left: 'left',
      },
    ]),
    grid: grids,
    legend: {
      orient: 'vertical',
      top: 'middle',
      right: 0,
    },
    xAxis: xAxes,
    yAxis: yAxes,
    series: series,
  }

  console.log('ddd', option)

  return option
}
