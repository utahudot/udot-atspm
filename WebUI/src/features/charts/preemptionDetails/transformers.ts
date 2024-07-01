import {
  createDataZoom,
  createDisplayProps,
  createGrid,
  createLegend,
  createSeries,
  createTitle,
  createToolbox,
  createTooltip,
  createXAxis,
  createYAxis,
} from '@/features/charts/common/transformers'
import { ChartType } from '@/features/charts/common/types'
import { TransformedPreemptDetailsResponse } from '@/features/charts/types'
import {
  Color,
  formatChartDateTimeRange,
  triangleSvgSymbol,
  xSvgSymbol,
} from '@/features/charts/utils'
import { EChartsOption, XAXisComponentOption } from 'echarts'
import {
  Cycle,
  LocationDetail,
  PreemptServiceSummary,
  RawPreemptionDetailsResponse,
} from './types'

export default function transformPreemptionDetailsData(
  response: RawPreemptionDetailsResponse
): TransformedPreemptDetailsResponse {
  const summary = transformSummaryData(response.data.summary)

  const charts = response.data.details.map((data) => {
    const chartOptions = transformDetailsData(data)

    return {
      chart: chartOptions,
    }
  })
  charts.unshift({ chart: summary })
  return {
    type: ChartType.PreemptionDetails,
    data: {
      charts,
    },
  }
}

function transformDetailsData(data: LocationDetail) {
  const { cycles } = data

  const titleHeader = `Preemption Details\n${data.locationDescription} - Preempt Number ${data.preemptionNumber}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
  })

  const xAxis = createXAxis(data.start, data.end)

  const yAxis = createYAxis(true, { name: 'Seconds Since Request' })

  const grid = createGrid({
    top: 140,
    left: 65,
    right: 200,
  })

  const legend = createLegend()

  const dataZoom = createDataZoom()

  const toolbox = createToolbox(
    { title: titleHeader, dateRange },
    data.locationIdentifer,
    ChartType.PreemptionDetails
  )

  const tooltip = createTooltip()

  const inputOff = getSeries('inputOff', cycles)
  const gateDown = getSeries('gateDown', cycles)
  const callMaxOut = getSeries('callMaxOut', cycles)
  const delay = getSeries('delay', cycles)
  const timeToService = getSeries('timeToService', cycles)
  const dwellTime = getSeries('dwellTime', cycles)
  const trackClear = getSeries('trackClear', cycles)
  const series = createSeries()

  const barWidth = 5
  console.log(data)
  if (gateDown !== null) {
    series.push({
      name: 'Gate Down',
      type: 'scatter',
      data: gateDown,
      color: Color.Black,
      symbol: triangleSvgSymbol,
    })
  }

  if (inputOff.length > 0) {
    console.log('inputOff')
    console.log(inputOff)

    series.push({
      name: 'Input Off',
      type: 'scatter',
      data: inputOff,
      color: Color.Black,
    })
  }
  console.log('GateDown')
  console.log(gateDown)

  if (callMaxOut.length > 0) {
    series.push({
      name: 'Call Max Out',
      type: 'scatter',
      data: callMaxOut,
      color: Color.Black,
      symbol: xSvgSymbol,
    })
  }

  if (delay.length > 0) {
    series.push({
      name: 'Delay',
      type: 'bar',
      data: delay,
      color: Color.Red,
      stack: 'cycle',
      barWidth: barWidth,
    })
  }

  if (timeToService.length > 0) {
    series.push({
      name: 'Time To Service',
      type: 'bar',
      data: timeToService,
      color: Color.Blue,
      stack: 'cycle',
      barWidth: barWidth,
    })
  }

  if (trackClear.length > 0) {
    series.push({
      name: 'Track Clear',
      type: 'bar',
      data: trackClear,
      color: Color.Orange,
      stack: 'cycle',
      barWidth: barWidth,
    })
  }

  if (dwellTime.length > 0) {
    series.push({
      name: 'Dwell Time',
      type: 'bar',
      data: dwellTime,
      color: Color.Green,
      stack: 'cycle',
      barWidth: barWidth,
    })
  }

  const displayProps = createDisplayProps({
    description: `Preempt # ${data.preemptionNumber}`,
    height: '550px',
  })

  const chartOptions: EChartsOption = {
    title: title,
    xAxis: xAxis,
    yAxis: yAxis,
    grid: grid,
    legend: legend,
    dataZoom: dataZoom,
    toolbox: toolbox,
    tooltip: tooltip,
    series: series,
    displayProps: displayProps,
  }
  return chartOptions
}

interface ChartDataEntry {
  0: string
  1: number | null
}

type PremptStuf = ChartDataEntry[]

function getSeries(key: keyof Cycle, cycles: Cycle[]): PremptStuf {
  return cycles.map((cycle) => {
    let value: number;

    if (cycle[key] == null) return 

    if (typeof cycle[key] === 'string' && Date.parse(cycle[key] as string)) {
      value =
        (Date.parse(cycle[key] as string) - Date.parse(cycle.inputOn)) / 1000
    } else if (typeof cycle[key] === 'number') {
      value = cycle[key] as number
    } else {
      value = 0
    }

    return [cycle.inputOn, value]
  })
}

function transformSummaryData(data: PreemptServiceSummary) {
  const { requestAndServices } = data

  const titleHeader = `Preemption Service and Request\n${data.locationDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
  })

  const xAxis: XAXisComponentOption = {
    type: 'time',
    min: data.start,
    nameLocation: 'middle',
    name: 'Time',
    nameGap: 30,
    max: data.end,
    splitNumber: 10,
    minorTick: {
      show: true,
      splitNumber: 2,
    },
  }

  const yAxis = createYAxis(true, {
    name: 'Preemption Number',
    type: 'category',
    data: ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10'],
    splitLine: {
      show: true,
    },
  })

  const grid = createGrid({
    top: 120,
    left: 65,
    right: 130,
  })

  const legend = createLegend()

  const dataZoom = createDataZoom()

  const toolbox = createToolbox(
    { title: titleHeader, dateRange },
    data.locationIdentifer,
    ChartType.PreemptionDetails
  )
  const tooltip = createTooltip()

  const series = createSeries()

  const requestArr: PremptStuf = []
  const servicetArr: PremptStuf = []
  requestAndServices.forEach((entry) => {
    const preemptionNumber = entry.preemptionNumber.toString()
    entry.requests.forEach((request) =>
      requestArr.push([request, preemptionNumber])
    )
    entry.services.forEach((service) =>
      servicetArr.push([service, preemptionNumber])
    )
  })

  series.push({
    name: 'Services',
    type: 'scatter',
    symbolOffset: [0, '-50%'],
    symbolSize: 8,
    data: servicetArr,
    color: Color.Blue,
  })

  series.push({
    name: 'Requests',
    type: 'scatter',
    symbolOffset: [0, '50%'],
    symbolSize: 8,
    data: requestArr,
    color: Color.Red,
  })

  const displayProps = createDisplayProps({
    description: 'Summary',
    height: '440px',
  })

  const chartOptions: EChartsOption = {
    title: title,
    xAxis: xAxis,
    yAxis: yAxis,
    grid: grid,
    legend: legend,
    dataZoom: dataZoom,
    toolbox: toolbox,
    tooltip: tooltip,
    series: series,
    displayProps: displayProps,
  }

  return chartOptions
}
