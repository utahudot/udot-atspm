// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - rampMetering.transformer.ts
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
  createDisplayProps,
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
  formatExportFileName,
  transformSeriesData,
} from '../common/transformers'
import { ChartType, MarkAreaData } from '../common/types'
import { TimeSpaceDetectorEvent } from '../timeSpaceDiagram/types'
import { TransformedChartResponse } from '../types'
import {
  Color,
  DashedLineSeriesSymbol,
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
      chart,
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

  const chartName1 = 'Ramp Rate vs. Mainline Occupancy'
  const chartName2 = 'Queue Override'
  const chartName3 = 'Ramp Rate vs. Mainline Volume'
  const chartName4 = 'Ramp Rate vs. Mainline Speed'
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title1 = createTitle({
    title: `${chartName1} (Active Rate vs. Mainline Avg Occupancy)`,
    location: data.locationDescription,
    dateRange,
  })

  const title2 = createTitle({
    title: chartName2,
    location: data.locationDescription,
    dateRange,
  })

  const title3 = createTitle({
    title: `${chartName3} (Active Rate vs. Mainline Avg Flow)`,
    location: data.locationDescription,
    dateRange,
  })

  const title4 = createTitle({
    title: `${chartName4} (Active Rate vs. Mainline Avg Speed)`,
    location: data.locationDescription,
    dateRange,
  })

  // ECharts pushes the right axis name clear of its tick labels; a small gap and
  // margin keep it tucked against them, away from the vertical zoom slider.
  const rightAxisName = { nameGap: 20, nameTextStyle: { textMargin: 3 } }

  const yAxisOne = createYAxis(
    false,
    {
      name: 'Mainline Occupancy',
    },
    { name: 'Meter Rate (vph)', ...rightAxisName }
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
    { name: 'Meter Rate (vph)', ...rightAxisName }
  )

  const yAxisFour = createYAxis(
    false,
    {
      name: 'Mainline Speed (mph)',
      nameGap: 50,
    },
    { name: 'Meter Rate (vph)', ...rightAxisName }
  )

  const xAxis = createXAxis(data.start, data.end)
  const grid = createGrid({
    top: 120,
    left: 65,
    right: 280,
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
      icon: DashedLineSeriesSymbol,
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
    top: grid.top,
    data: [
      ...lanesActiveLegends,
      {
        name: 'Mainline Avg Occ',
        icon: SolidLineSeriesSymbol,
      },
    ],
  })

  const legendTwo = createLegend({
    top: grid.top,
    data: [
      ...lanesActiveLegends,
      ...lanesBaseLegends,
      queueOnLegend,
      queueOffLegend,
    ],
  })

  const legendThree = createLegend({
    top: grid.top,
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
    top: grid.top,
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
      right: grid.right - 90,
      yAxisIndex: 0,
      minSpan: 0.2,
    },
  ])

  const createChartToolbox = (chartName: string) =>
    createToolbox(
      {
        title: formatExportFileName(
          `${chartName} ${data.locationDescription}`,
          data.start,
          data.end
        ),
        dateRange,
      },
      data.locationIdentifier,
      ChartType.RampMetering
    )

  const tooltip = createTooltip()

  // Each lane keeps one color: solid for its active rate, dashed for its base rate.
  // Yellow fades into the background and light blue reads as the blue mainline.
  const laneColors = [Color.Green, Color.Orange, Color.Pink, Color.Red]
  const laneDescriptions = [
    ...new Set(
      [...lanesActiveRate, ...lanesBaseRate].map((lane) => lane.description)
    ),
  ]
  const laneColor = (description: string) =>
    laneColors[laneDescriptions.indexOf(description) % laneColors.length]

  const seriesOne = createSeries({
    name: 'Mainline Avg Occ',
    data: transformSeriesData(mainlineAvgOcc),
    type: 'line',
    color: Color.Blue,
  })

  lanesActiveRate.forEach((lane) => {
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
        color: laneColor(lane.description),
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

  lanesActiveRate.forEach((lane) => {
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
        color: laneColor(lane.description),
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

  lanesBaseRate.forEach((lane) => {
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
        color: laneColor(lane.description),
        clip: true,
        renderItem(param, api) {
          if (param.dataIndex === 0) {
            const polylines = createPolyLines(baseRateData, api, 'dashed')

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

  lanesActiveRate.forEach((lane) => {
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
        color: laneColor(lane.description),
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

  lanesBaseRate.forEach((lane) => {
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
        color: laneColor(lane.description),
        clip: true,
        renderItem(param, api) {
          if (param.dataIndex === 0) {
            const polylines = createPolyLines(baseRateData, api, 'dashed')

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

  lanesActiveRate.forEach((lane) => {
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
        color: laneColor(lane.description),
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

  lanesBaseRate.forEach((lane) => {
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
        color: laneColor(lane.description),
        clip: true,
        renderItem(param, api) {
          if (param.dataIndex === 0) {
            const polylines = createPolyLines(baseRateData, api, 'dashed')

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
      toolbox: createChartToolbox(chartName1),
      tooltip,
      series: seriesOne,
      displayProps: createDisplayProps({
        description: chartName1,
      }),
    },
    {
      title: title2,
      xAxis,
      yAxis: yAxisTwo,
      grid,
      legend: legendTwo,
      dataZoom,
      toolbox: createChartToolbox(chartName2),
      tooltip,
      series: seriesTwo,
      displayProps: createDisplayProps({ description: chartName2 }),
    },
    {
      title: title3,
      xAxis,
      yAxis: yAxisThree,
      grid,
      legend: legendThree,
      dataZoom,
      toolbox: createChartToolbox(chartName3),
      tooltip,
      series: seriesThree,
      displayProps: createDisplayProps({
        description: chartName3,
      }),
    },
    {
      title: title4,
      xAxis,
      yAxis: yAxisFour,
      grid,
      legend: legendFour,
      dataZoom,
      toolbox: createChartToolbox(chartName4),
      tooltip,
      series: seriesFour,
      displayProps: createDisplayProps({
        description: chartName4,
      }),
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
