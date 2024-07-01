import {
  createDisplayProps,
  createGrid,
  createLegend,
  createPlans,
  createPolyLines,
  createSeries,
  createTitle,
  createYAxis,
  formatDataPointForStepView,
  transformSeriesData,
} from '@/features/charts/common/transformers'
import { ChartType, DataPoint } from '@/features/charts/common/types'
import {
  Bin,
  RawGreenTimeUtilizationData,
  RawGreenTimeUtilizationResponse,
} from '@/features/charts/greenTimeUtilization/types'
import { TransformedChartResponse } from '@/features/charts/types'
import {
  Color,
  DashedLineSeriesSymbol,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import {
  DataZoomComponentOption,
  EChartsOption,
  ToolboxComponentOption,
  VisualMapComponentOption,
  XAXisComponentOption,
} from 'echarts'

export default function transformGreenTimeUtilizationData(
  response: RawGreenTimeUtilizationResponse
): TransformedChartResponse {
  console.log(response)
  const charts = response.data.map((data) => {
    const chartOptions = transformData(data)
    return {
      chart: chartOptions,
    }
  })

  return {
    type: ChartType.GreenTimeUtilization,
    data: {
      charts,
    },
  }
}

function transformData(data: RawGreenTimeUtilizationData) {
  const {
    bins,
    averageSplits,
    xAxisBinSize,
    yAxisBinSize,
    programmedSplits,
    start,
    end,
    plans,
  } = data

  // const xAxisBinSize = 15 // TODO: These should be passed by api
  // const yAxisBinSize = 3
  const titleHeader = `Green Time Utilization\n${data.locationDescription} - ${data.approachDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
  })

  const grid = createGrid({
    top: 160,
    left: 90,
    right: 230,
    bottom: 95,
  })

  const legend = createLegend({
    top: 200,
    data: [
      { name: 'Average Split', icon: SolidLineSeriesSymbol },
      { name: 'Programmed Splits', icon: DashedLineSeriesSymbol },
      { name: 'Amount of\nVehicles Through' },
    ],
  })

  const tooltip = {}

  const toolbox: ToolboxComponentOption = {
    feature: {
      saveAsImage: {},
      dataView: { readOnly: true },
    },
  }

  const dataZoom: DataZoomComponentOption[] = [
    {
      type: 'slider',
      filterMode: 'none',
      minSpan: 0.2,
    },
    {
      type: 'slider',
      orient: 'vertical',
      right: 190,
      filterMode: 'none',
      minSpan: 0.2,
    },
    {
      type: 'inside',
      filterMode: 'none',
      minSpan: 0.2,
    },
    {
      type: 'inside',
      orient: 'vertical',
      filterMode: 'none',
      minSpan: 0.2,
    },
  ]

  const maxStackHeight = Math.max(...bins.map((bin) => bin.y))
  const amountOfVehiclesThrough = transformBins(bins)
  const xAxisValues = createTimeAxis(start, end, xAxisBinSize)

  const programmedSplitsData = formatDataPointForStepView(
    programmedSplits,
    data.end
  )

  const averageSplitsData = formatDataPointForStepView(averageSplits, data.end)

  // Todo: We should be able to extend the line by adding something like this
  // const transformedData = transformSeriesData(averageSplits)
  // const lastDataPoint = transformedData[transformedData.length - 1]

  // markLine: {
  //   animation: false,
  //   silent: true,
  //   symbol: 'none',
  //   lineStyle: {
  //     type: 'solid',
  //     color: 'black',
  //     width: 2,
  //   },
  //   data: [
  //     [
  //       {
  //         coord: lastDataPoint,
  //         symbol: 'none',
  //       },
  //       {
  //         xAxis: end,
  //         yAxis: lastDataPoint[1],
  //         symbol: 'none',
  //       },
  //     ],
  //   ],
  // },

  const series = createSeries(
    {
      name: 'Average Split',
      data: transformSeriesData(averageSplits),
      type: 'custom',
      xAxisIndex: 1,
      clip: true,
      color: Color.Black,
      z: 10,
      renderItem(param, api) {
        if (param.dataIndex === 0) {
          const polylines = createPolyLines(averageSplitsData, api)

          return {
            type: 'group',
            children: polylines,
          }
        }
      },
    },
    {
      name: 'Programmed Splits',
      data: transformSeriesData(programmedSplits),
      type: 'custom',
      color: Color.Black,
      xAxisIndex: 1,
      clip: true,
      z: 10,
      renderItem(param, api) {
        if (param.dataIndex === 0) {
          const polylines = createPolyLines(programmedSplitsData, api, 'dashed')

          return {
            type: 'group',
            children: polylines,
            lineStyle: {
              width: 1.5,
            },
          }
        }
      },
    },
    {
      name: 'Amount of\nVehicles Through',
      type: 'heatmap',
      yAxisIndex: 1,
      data: amountOfVehiclesThrough,
      color: Color.Blue,
      label: {
        show: true,
        formatter: (params) => {
          const value = (params.value as string[])[2]
          if (xAxisValues.length < 25) {
            return value
          } else if (xAxisValues.length < 50) {
            return parseFloat(value).toFixed(1)
          } else {
            return ' '
          }
        },
      },
    }
  )

  const lineSeriesMax = Math.max(
    ...averageSplits.map((s: DataPoint) => s.value),
    ...programmedSplits.map((s: DataPoint) => s.value)
  )

  const lineSeriesMaxRounded =
    Math.ceil(lineSeriesMax / yAxisBinSize) * yAxisBinSize

  const stackMax = maxStackHeight * yAxisBinSize

  const yAxisMax = Math.max(lineSeriesMaxRounded, stackMax) + yAxisBinSize
  let currentHeight = 0
  const yAxisValues = []

  while (currentHeight < yAxisMax) {
    yAxisValues.push(currentHeight)
    currentHeight += yAxisBinSize
  }

  const yAxis = createYAxis(
    true,
    {
      type: 'value',
      interval: yAxisBinSize,
      max: yAxisMax,
      alignTicks: true,
      show: false,
      position: 'right',
    },
    {
      name: 'Time Since Start of Green',
      nameLocation: 'middle',
      nameGap: 40,
      type: 'category',
      splitLine: {
        show: true,
      },
      position: 'left',
      max: yAxisMax / yAxisBinSize - 1,
      data: yAxisValues,
    }
  )

  const xAxis: XAXisComponentOption[] = [
    {
      type: 'category',
      name: 'Time',
      nameLocation: 'middle',
      nameGap: 30,
      min: data.start,
      max: data.end,
      data: xAxisValues,
    },
    {
      type: 'time',
      name: 'DateTime',
      min: data.start,
      max: data.end,
      show: false,
    },
  ]

  const visualMap: VisualMapComponentOption = {
    seriesIndex: 2,
    calculable: true,
    max: 2,
    orient: 'vertical',
    bottom: 110,
    right: '60',
    itemHeight: 200,
    precision: 2,
    handleStyle: {
      borderColor: Color.Black,
      borderCap: 'square',
    },
    inRange: {
      color: [Color.White, Color.Blue],
    },
  }

  const planSeries = createPlans(plans, yAxis.length, undefined, 120, 1)

  const displayProps = createDisplayProps({
    description: data.approachDescription,
    numberOfLocations: 0,
  })

  const chartOptions: EChartsOption = {
    title,
    xAxis,
    yAxis,
    dataZoom,
    grid,
    legend,
    tooltip,
    toolbox,
    visualMap,
    series: [...series, planSeries],
    displayProps,
  }

  return chartOptions
}

function transformBins(bins: Bin[]) {
  const amountOfVehiclesThrough: (number | string)[][] = []

  bins.forEach((bin) => {
    amountOfVehiclesThrough.push([bin.x, bin.y, bin.value])
  })

  return amountOfVehiclesThrough
}

function createTimeAxis(start: string, end: string, interval: number) {
  // Parse the start and end times
  const startTime = new Date(start)
  const endTime = new Date(end)
  const xAxisValues = []

  // Loop over the time range, incrementing by the interval
  for (
    let time = startTime;
    time < endTime;
    time = addMinutes(time, interval)
  ) {
    xAxisValues.push(formatTime(time))
  }

  return xAxisValues
}

function addMinutes(date: Date, interval: number) {
  return new Date(date.getTime() + interval * 60_000) // 60000 ms in 1 minute
}

function formatTime(date: Date) {
  const hours = date.getHours().toString().padStart(2, '0')
  const minutes = date.getMinutes().toString().padStart(2, '0')
  return `${hours}:${minutes}`
}
