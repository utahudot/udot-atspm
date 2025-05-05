// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - transformer.ts
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
import { EChartsOption, SeriesOption } from 'echarts'
import {
  createDataZoom,
  createGrid,
  createLegend,
  createPolyLines,
  createSeries,
  createTitle,
  createToolbox,
  createTooltip,
  createXAxis,
  createYAxis,
  formatDataPointForStepView,
  transformSeriesData,
} from '../common/transformers'
import { ChartType, MarkAreaData } from '../common/types'
import { TimeSpaceDetectorEvent } from '../timeSpaceDiagram/types'
import { TransformedChartResponse } from '../types'
import {
  Color,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
  triangleSvgSymbol,
} from '../utils'
import {
  QueueDetectorEvent,
  RampMeteringData,
  RawRampMeteringResponse,
} from './types'

export default function transformRampMeteringData(
  response: RawRampMeteringResponse
): TransformedChartResponse {
  const chartOptions = transformData(response.data)
  const transformedData = chartOptions.map((chart) => {
    return {
      chart: chart,
    }
  })

  return {
    type: ChartType.RampMetering,
    data: {
      charts: transformedData,
    },
  }
}

function transformData(data: RampMeteringData): EChartsOption[] {
  const {
    mainlineAvgFlow,
    mainlineAvgOcc,
    mainlineAvgSpeed,
    lanesActiveRate,
    lanesBaseRate,
    lanesQueueOnAndOffEvents,
  } = data

  const titleHeader1 = `Ramp Rate vs. Mainline Occupancy (Active Rate vs Mainline Avg Occupancy)\n${data.locationDescription}`
  const titleHeader2 = `Queue Override\n${data.locationDescription}`
  const titleHeader3 = `Ramp Rate vs. Mainline Volume - (Active Rate vs Mainline Avg Flow)\n${data.locationDescription}`
  const titleHeader4 = `Ramp Rate vs. Mainline Speed  (Active Rate vs Mainline Avg Speed)\n${data.locationDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title1 = createTitle({
    title: titleHeader1,
    dateRange,
  })

  const title2 = createTitle({
    title: titleHeader2,
    dateRange,
  })

  const title3 = createTitle({
    title: titleHeader3,
    dateRange,
  })

  const title4 = createTitle({
    title: titleHeader4,
    dateRange,
  })

  const yAxisOne = createYAxis(
    false,
    {
      name: 'Mainline Occupancy',
    },
    { name: 'Meter Rate (vph)' }
  )

  const yAxisTwo = createYAxis(
    false,
    {
      name: 'Metering Rate (vph)',
      nameGap: 50,
    },
    {
      name: 'Lane Number',
      min: 0,
      max: 6,
      interval: 1,
    }
  )

  const yAxisThree = createYAxis(
    false,
    {
      name: 'Mainline Flow (vph)',
      nameGap: 50,
    },
    { name: 'Meter Rate (vph)' }
  )

  const yAxisFour = createYAxis(
    false,
    {
      name: 'Mainline Speed (mph)',
      nameGap: 50,
    },
    { name: 'Meter Rate (vph)' }
  )

  const xAxis = createXAxis(data.start, data.end)
  const grid = createGrid({
    top: 150,
    left: 65,
    right: 250,
  })

  const lanesActiveLegends = lanesActiveRate.map((lane) => {
    const description =
      lanesActiveRate.length === 1
        ? 'Active Rate'
        : `Active Rate: ${lane.description}`
    return {
      name: description,
      icon: SolidLineSeriesSymbol,
    }
  })

  const lanesBaseLegends = lanesBaseRate.map((lane) => {
    const description =
      lanesBaseRate.length === 1
        ? 'Base Rate'
        : `Base Rate: ${lane.description}`
    return {
      name: description,
      icon: SolidLineSeriesSymbol,
    }
  })

  const queueOnLegend = {
    name: 'Queue On',
    icon: triangleSvgSymbol,
    itemStyle: { color: 'black' },
  }

  const queueOffLegend = {
    name: 'Queue Off',
    icon: 'square',
    itemStyle: { color: 'darkgrey' },
  }

  const legendOne = createLegend({
    data: [
      ...lanesActiveLegends,
      {
        name: 'Mainline Avg Occ',
        icon: SolidLineSeriesSymbol,
      },
    ],
  })

  const legendTwo = createLegend({
    data: [
      ...lanesActiveLegends,
      ...lanesBaseLegends,
      queueOnLegend,
      queueOffLegend,
    ],
  })

  const legendThree = createLegend({
    data: [
      ...lanesActiveLegends,
      ...lanesBaseLegends,
      {
        name: 'Mainline Avg Flow',
        icon: SolidLineSeriesSymbol,
      },
    ],
  })

  const legendFour = createLegend({
    data: [
      ...lanesActiveLegends,
      ...lanesBaseLegends,
      {
        name: 'Mainline Avg Speed',
        icon: SolidLineSeriesSymbol,
      },
    ],
  })

  const dataZoom = createDataZoom([
    {
      type: 'slider',
      orient: 'vertical',
      filterMode: 'none',
      right: 160,
      yAxisIndex: 0,
      minSpan: 0.2,
    },
  ])

  const toolbox = createToolbox(
    { title: titleHeader1, dateRange },
    data.locationIdentifier,
    ChartType.ApproachVolume
  )

  const tooltip = createTooltip()

  const chartColors = Object.values(Color)

  const seriesOne = createSeries({
    name: 'Mainline Avg Occ',
    data: transformSeriesData(mainlineAvgOcc),
    type: 'line',
    color: Color.Blue,
  })

  lanesActiveRate.forEach((lane, index) => {
    const activeRateData = formatDataPointForStepView(lane.value, data.end)
    const description =
      lanesActiveRate.length === 1
        ? 'Active Rate'
        : `Active Rate: ${lane.description}`
    seriesOne.push(
      ...createSeries({
        name: description,
        data: transformSeriesData(lane.value),
        type: 'custom',
        yAxisIndex: 1,
        color: chartColors[index + 1],
        clip: true,
        renderItem(param, api) {
          if (param.dataIndex === 0) {
            const polylines = createPolyLines(activeRateData, api)

            return {
              type: 'group',
              children: polylines,
            }
          }
        },
      })
    )
  })

  seriesOne.push(createStartupArea(data.startUpWarning, data.start, data.end))

  const seriesTwo: SeriesOption[] = []

  seriesTwo.push(...generateQueueSeriesData(lanesQueueOnAndOffEvents))

  lanesActiveRate.forEach((lane, index) => {
    const activeRateData = formatDataPointForStepView(lane.value, data.end)
    const description =
      lanesActiveRate.length === 1
        ? 'Active Rate'
        : `Active Rate: ${lane.description}`
    seriesTwo.push(
      ...createSeries({
        name: description,
        data: transformSeriesData(lane.value),
        type: 'custom',
        yAxisIndex: 0,
        color: chartColors[index + 1],
        clip: true,
        renderItem(param, api) {
          if (param.dataIndex === 0) {
            const polylines = createPolyLines(activeRateData, api)

            return {
              type: 'group',
              children: polylines,
            }
          }
        },
      })
    )
  })

  lanesBaseRate.forEach((lane, index) => {
    const baseRateData = formatDataPointForStepView(lane.value, data.end)
    const description =
      lanesBaseRate.length === 1
        ? 'Base Rate'
        : `Base Rate: ${lane.description}`
    seriesTwo.push(
      ...createSeries({
        name: description,
        data: transformSeriesData(lane.value),
        type: 'custom',
        yAxisIndex: 0,
        color: chartColors[index + 2],
        clip: true,
        renderItem(param, api) {
          if (param.dataIndex === 0) {
            const polylines = createPolyLines(baseRateData, api)

            return {
              type: 'group',
              children: polylines,
            }
          }
        },
      })
    )
  })

  seriesTwo.push(createStartupArea(data.startUpWarning, data.start, data.end))

  const seriesThree: SeriesOption[] = []

  lanesActiveRate.forEach((lane, index) => {
    const activeRateData = formatDataPointForStepView(lane.value, data.end)
    const description =
      lanesActiveRate.length === 1
        ? 'Active Rate'
        : `Active Rate: ${lane.description}`
    seriesThree.push(
      ...createSeries({
        name: description,
        data: transformSeriesData(lane.value),
        type: 'custom',
        yAxisIndex: 1,
        color: chartColors[index + 1],
        clip: true,
        renderItem(param, api) {
          if (param.dataIndex === 0) {
            const polylines = createPolyLines(activeRateData, api)

            return {
              type: 'group',
              children: polylines,
            }
          }
        },
      })
    )
  })

  lanesBaseRate.forEach((lane, index) => {
    const baseRateData = formatDataPointForStepView(lane.value, data.end)
    const description =
      lanesBaseRate.length === 1
        ? 'Base Rate'
        : `Base Rate: ${lane.description}`
    seriesThree.push(
      ...createSeries({
        name: description,
        data: transformSeriesData(lane.value),
        type: 'custom',
        yAxisIndex: 1,
        color: chartColors[index + 2],
        clip: true,
        renderItem(param, api) {
          if (param.dataIndex === 0) {
            const polylines = createPolyLines(baseRateData, api)

            return {
              type: 'group',
              children: polylines,
            }
          }
        },
      })
    )
  })

  seriesThree.push(
    ...createSeries({
      name: 'Mainline Avg Flow',
      data: transformSeriesData(mainlineAvgFlow),
      type: 'line',
      color: Color.Blue,
    })
  )

  seriesThree.push(createStartupArea(data.startUpWarning, data.start, data.end))

  const seriesFour: SeriesOption[] = []

  lanesActiveRate.forEach((lane, index) => {
    const activeRateData = formatDataPointForStepView(lane.value, data.end)
    const description =
      lanesActiveRate.length === 1
        ? 'Active Rate'
        : `Active Rate: ${lane.description}`
    seriesFour.push(
      ...createSeries({
        name: description,
        data: transformSeriesData(lane.value),
        type: 'custom',
        yAxisIndex: 1,
        color: chartColors[index + 1],
        clip: true,
        renderItem(param, api) {
          if (param.dataIndex === 0) {
            const polylines = createPolyLines(activeRateData, api)

            return {
              type: 'group',
              children: polylines,
            }
          }
        },
      })
    )
  })

  lanesBaseRate.forEach((lane, index) => {
    const baseRateData = formatDataPointForStepView(lane.value, data.end)
    const description =
      lanesBaseRate.length === 1
        ? 'Base Rate'
        : `Base Rate: ${lane.description}`
    seriesFour.push(
      ...createSeries({
        name: description,
        data: transformSeriesData(lane.value),
        type: 'custom',
        yAxisIndex: 1,
        color: chartColors[index + 2],
        clip: true,
        renderItem(param, api) {
          if (param.dataIndex === 0) {
            const polylines = createPolyLines(baseRateData, api)

            return {
              type: 'group',
              children: polylines,
            }
          }
        },
      })
    )
  })

  seriesFour.push(
    ...createSeries({
      name: 'Mainline Avg Speed',
      data: transformSeriesData(mainlineAvgSpeed),
      type: 'line',
      color: Color.Blue,
    })
  )

  seriesFour.push(createStartupArea(data.startUpWarning, data.start, data.end))

  const chartOptions: EChartsOption[] = [
    {
      title: title1,
      xAxis,
      yAxis: yAxisOne,
      grid,
      legend: legendOne,
      dataZoom,
      toolbox,
      tooltip,
      series: seriesOne,
    },
    {
      title: title2,
      xAxis,
      yAxis: yAxisTwo,
      grid,
      legend: legendTwo,
      dataZoom,
      toolbox,
      tooltip,
      series: seriesTwo,
    },
    {
      title: title3,
      xAxis,
      yAxis: yAxisThree,
      grid,
      legend: legendThree,
      dataZoom,
      toolbox,
      tooltip,
      series: seriesThree,
    },
    {
      title: title4,
      xAxis,
      yAxis: yAxisFour,
      grid,
      legend: legendFour,
      dataZoom,
      toolbox,
      tooltip,
      series: seriesFour,
    },
  ]

  return chartOptions
}

function generateQueueSeriesData(
  laneQueueEvents: QueueDetectorEvent[]
): SeriesOption[] {
  const onSeriesData: any[] = []
  const offSeriesData: any[] = []
  const lineSeriesData: any[] = []

  laneQueueEvents.forEach((event) => {
    const laneDescription = `Lane ${event.value}`

    const onPoint = {
      value: [event.detectorOn, event.value, laneDescription],
      symbol: 'triangle',
      symbolSize: 5,
      itemStyle: {
        color: 'black',
      },
    }
    const offPoint = {
      value: [event.detectorOff, event.value, laneDescription],
      symbol: 'square',
      symbolSize: 5,
      itemStyle: {
        color: 'darkgrey',
      },
    }

    onSeriesData.push(onPoint)
    offSeriesData.push(offPoint)

    if (event.detectorOn && event.detectorOff) {
      lineSeriesData.push([
        {
          coord: [event.detectorOn, event.value, laneDescription],
        },
        {
          coord: [event.detectorOff, event.value, laneDescription],
        },
      ])
    }
  })

  return [
    {
      name: 'Queue On',
      type: 'scatter',
      yAxisIndex: 1,
      tooltip: {
        confine: true,
        valueFormatter: (value) => (value as string[])[2],
      },
      data: onSeriesData,
    },
    {
      name: 'Queue Off',
      type: 'scatter',
      yAxisIndex: 1,
      tooltip: {
        confine: true,
        valueFormatter: (value) => (value as string[])[2],
      },
      data: offSeriesData,
    },
    {
      name: 'Queue On',
      type: 'lines',
      yAxisIndex: 1,
      coordinateSystem: 'cartesian2d',
      lineStyle: {
        color: 'black',
        width: 2,
      },
      data: lineSeriesData,
    },
  ]
}

function createStartupArea(
  events: TimeSpaceDetectorEvent[],
  startDate: string,
  endDate: string
): SeriesOption {
  // Convert startDate and endDate to Date objects
  const start = new Date(startDate)
  const end = new Date(endDate)

  // Parse and sort events by initialX
  events.sort(
    (a, b) => new Date(a.initialX).getTime() - new Date(b.initialX).getTime()
  )

  const markAreaData: MarkAreaData[] = []

  // Add inverse area from startDate to the initialX of the first event
  if (events.length > 0 && start < new Date(events[0].initialX)) {
    markAreaData.push([
      { xAxis: startDate, itemStyle: { color: Color.PlanB } },
      { xAxis: events[0].initialX },
    ])
  }

  // Add inverse areas between consecutive events
  for (let i = 0; i < events.length - 1; i++) {
    const currentEventEnd = new Date(events[i].finalX)
    const nextEventStart = new Date(events[i + 1].initialX)

    if (currentEventEnd < nextEventStart) {
      markAreaData.push([
        { xAxis: events[i].finalX, itemStyle: { color: Color.PlanB } },
        { xAxis: events[i + 1].initialX },
      ])
    }
  }

  // Add inverse area from the finalX of the last event to endDate
  if (events.length > 0 && new Date(events[events.length - 1].finalX) < end) {
    markAreaData.push([
      {
        xAxis: events[events.length - 1].finalX,
        itemStyle: { color: Color.PlanB },
      },
      { xAxis: endDate },
    ])
  }

  return {
    type: 'line',
    markArea: {
      data: markAreaData,
    },
  }
}
