// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - transformers.ts
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
import {
  createDataZoom,
  createDisplayProps,
  createTitle,
  createTooltip,
  createXAxis,
  createYAxis,
} from '@/features/charts/common/transformers'
import { ChartType } from '@/features/charts/common/types'
import { TransformedChartResponse } from '@/features/charts/types'
import {
  Color,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import {
  EChartsOption,
  GridComponentOption,
  SeriesOption,
  ToolboxComponentOption,
} from 'echarts'
import {
  AdvancedDetectors,
  BasicDetectors,
  Cycle,
  RawTimingAndActuationData,
  RawTimingAndActuationResponse,
} from './types'

export default function transformTimingAndActuationData(
  response: RawTimingAndActuationResponse
): TransformedChartResponse {
  const charts = response.data.map((data, i) => {
    const chartOptions = transformData(data, i)
    return {
      chart: chartOptions,
    }
  })

  const legendItems = getAllEventDetails()

  const title = createTitleOnlyChart(response.data[0])

  const legendChartOptions = createLegendOnlyChart(legendItems)

  return {
    type: ChartType.TimingAndActuation,
    data: {
      title,
      charts,
      legends: legendChartOptions,
    },
  }
}

function transformData(data: RawTimingAndActuationData) {
  const {
    pedestrianIntervals,
    pedestrianEvents,
    cycleAllEvents,
    advanceCountDetectors,
    advancePresenceDetectors,
    stopBarDetectors,
    laneByLanesDetectors,
    end,
  } = data

  const title = { text: data.approachDescription }

  const xAxis = createXAxis(data.start, data.end)

  const yAxis = createYAxis(false, {
    type: 'category',
    boundaryGap: true,
    z: 2,
    axisPointer: {
      show: true,
      type: 'shadow',
      triggerTooltip: false,
    },
    splitLine: {
      show: true,
      lineStyle: {
        color: 'black',
      },
    },
    data: extractDetectorsNames(data),
  })

  const grid: GridComponentOption = {
    top: 30,
    bottom: 90,
    right: 60,
    left: 210,
    show: true,
    borderColor: Color.Black,
  }

  const cycleSeries = generateCycles(cycleAllEvents, end)

  const dataZoom = createDataZoom([
    {
      type: 'slider',
      height: 25,
    },
  ])

  const toolbox: ToolboxComponentOption = {
    feature: {
      mySaveAsImage: {
        name: `Timing_and_Actuation_${data.locationDescription}`,
        show: true,
        icon: 'M4.7,22.9L29.3,45.5L54.7,23.4M4.6,43.6L4.6,58L53.8,58L53.8,43.6M29.2,45.1L29.2,0',
        onclick: () => {
          // Dispatch a custom event with some text data
          const event = new CustomEvent('saveChartImage', {
            detail: {
              text: title.text,
            },
          })
          window.dispatchEvent(event)
        },
      },
      dataView: {
        readOnly: true,
      },
    },
  }

  const series: SeriesOption[] = []

  const legendData = [
    { name: 'Detector Events' },
    ...cycleSeries.map((s) => ({
      name: s.name,
      icon: 'roundRect',
      itemStyle: { color: s.markArea.itemStyle.color },
    })),
    { name: 'Pedestrian Intervals', icon: 'roundRect' },
  ]

  if (pedestrianIntervals.length > 0) {
    const pedestrianIntervalSeries =
      generatePedestrianIntervalLines(pedestrianIntervals)
    series.push(...pedestrianIntervalSeries)

    legendData.push(
      ...pedestrianIntervalSeries.map((s) => ({
        name: s.name,
        icon: 'roundRect',
        itemStyle: { color: s.markLine.lineStyle.color },
      }))
    )
  }

  const detectorSeries = generateDetectorSeriesData([
    ...pedestrianEvents,
    ...stopBarDetectors,
    ...advanceCountDetectors,
    ...laneByLanesDetectors,
    ...advancePresenceDetectors,
  ])

  legendData.push(
    {
      name: 'Detector On (82)',
      icon: 'circle',
      itemStyle: { color: 'black' },
    },
    {
      name: 'Detector Off (81)',
      icon: 'square',
      itemStyle: { color: 'darkgrey' },
    },
    {
      name: 'Line Connecting Detector On to Detector Off',
      icon: SolidLineSeriesSymbol,
      itemStyle: { color: 'black' },
    }
  )

  series.push(...detectorSeries)

  const legend = createLegend(legendData)

  if (cycleAllEvents && cycleAllEvents.length > 0) {
    const cycleSeries = generateCycles(cycleAllEvents, end)
    series.push(...cycleSeries)
  }

  let amountOfChannels =
    stopBarDetectors.length +
    laneByLanesDetectors.length +
    advanceCountDetectors.length +
    advancePresenceDetectors.length +
    0.75

  if (pedestrianEvents.length > 0) {
    amountOfChannels += pedestrianEvents.length
  }

  const displayProps = createDisplayProps({
    description: data.approachDescription,
    approachDescription: data.approachDescription,
    amountOfChannels: amountOfChannels,
    detectorEvents: ['Detector Events'],
    phaseType: data.phaseType,
    numberOfLocations: 0,
  })

  const tooltip = createTooltip({ trigger: 'item', confine: true })

  const chartOptions: EChartsOption = {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    dataZoom,
    toolbox,
    tooltip,
    animation: false,
    series,
    displayProps,
  }

  return chartOptions
}

export function generateCycles(
  cycleAllEvents: Cycle[] | null,
  end: string
): SeriesOption[] {
  const seriesMap = new Map<string, SeriesOption>()

  if (!cycleAllEvents) return []

  for (let i = 0; i < cycleAllEvents.length; i++) {
    const event = cycleAllEvents[i]
    const { color, name } = getEventDetails(event.value)
    const startTime = new Date(event.start).toISOString()
    const endTime =
      i < cycleAllEvents.length - 1
        ? new Date(cycleAllEvents[i + 1].start).toISOString()
        : new Date(end).toISOString()

    if (!seriesMap.has(color)) {
      // If the color doesn't exist, create a new series
      seriesMap.set(color, {
        name,
        type: 'scatter',
        cursor: 'default',
        silent: true,

        markArea: {
          silent: true,
          itemStyle: {
            color,
          },
          data: [],
        },
      })
    }

    const series = seriesMap.get(color)
    series!.markArea.data.push([{ xAxis: startTime }, { xAxis: endTime }])
  }

  return Array.from(seriesMap.values())
}

function extractDetectorsNames(data: RawTimingAndActuationData): string[] {
  const names: string[] = []

  data.advanceCountDetectors.forEach((item: AdvancedDetectors) => {
    names.push(item.name)
  })

  data.advancePresenceDetectors.forEach((item: AdvancedDetectors) => {
    names.push(item.name)
  })

  data.laneByLanesDetectors.forEach((item: BasicDetectors) => {
    names.push(item.name)
  })

  data.stopBarDetectors.forEach((item: BasicDetectors) => {
    names.push(item.name)
  })

  data.pedestrianEvents.forEach((item: BasicDetectors) => {
    names.push(item.name)
  })

  if (data.pedestrianIntervals.length > 0) {
    names.push('Pedestrian Intervals')
  }

  return names
}

enum CycleColor {
  Default = Color.PlanB,
  Green = Color.Green,
  LightGreen = '#8ef08d',
  Yellow = Color.Yellow,
  DarkRed = '#FF0000',
  LightRed = '#f0807f',
  LightBlue = Color.LightBlue,
  Blue = Color.Blue,
  DarkGrey = Color.Grey,
  Black = Color.Black,
  Grey = Color.Grey,
}

function generateDetectorSeriesData(
  detectors: BasicDetectors[]
): SeriesOption[] {
  const onSeriesData = []
  const offSeriesData = []
  const lineSeriesData = []

  detectors.forEach((detector) => {
    detector.events.forEach((event, i) => {
      const onPoint = [event.detectorOn, detector.name]
      onSeriesData.push(onPoint)

      if (!detector.name.includes('Stop')) return

      const offPoint = [event.detectorOff, detector.name]
      offSeriesData.push(offPoint)

      if (event.detectorOn && event.detectorOff) {
        lineSeriesData.push([
          {
            coord: [event.detectorOn, detector.name],
          },
          {
            coord: [event.detectorOff, detector.name],
          },
        ])
      }
    })
  })

  return [
    {
      name: 'Detector On (82)',
      type: 'scatter',
      tooltip: {
        confine: true,
        valueFormatter: (value) => (value as string[])[0],
      },
      large: true,
      symbol: 'circle',
      symbolSize: 5,
      itemStyle: {
        color: 'black',
      },
      data: onSeriesData,
    },
    {
      name: 'Detector Off (81)',
      type: 'scatter',
      large: true,
      tooltip: {
        confine: true,
        valueFormatter: (value) => (value as string[])[0],
      },
      symbol: 'square',
      symbolSize: 5,
      itemStyle: {
        color: 'darkgrey',
      },
      data: offSeriesData,
    },
    {
      name: 'Line Connecting Detector On to Detector Off',
      type: 'lines',
      large: true,
      coordinateSystem: 'cartesian2d',
      lineStyle: {
        color: 'black',
        width: 2,
      },
      data: lineSeriesData,
    },
  ]
}

function generatePedestrianIntervalLines(
  pedestrianIntervals: { start: string; value: number }[]
): SeriesOption[] {
  const seriesMap = new Map<string, SeriesOption>()

  pedestrianIntervals.forEach((interval, index, array) => {
    const { color, name } = getEventDetails(interval.value)
    const startCoord = new Date(interval.start).toISOString()
    const endCoord =
      index < array.length - 1
        ? new Date(array[index + 1].start).toISOString()
        : 'max'

    if (!seriesMap.has(color)) {
      // If the color doesn't exist, create a new series
      seriesMap.set(color, {
        name,
        type: 'line',
        markLine: {
          silent: true,
          symbol: ['none', 'none'],
          lineStyle: {
            type: 'solid',
            color: color,
            width: 20,
          },
          data: [],
        },
      })
    }

    const series = seriesMap.get(color)
    series!.markLine.data.push([
      {
        coord: [startCoord, 'Pedestrian Intervals'],
      },
      {
        coord: [endCoord, 'Pedestrian Intervals'],
      },
    ])
  })

  return Array.from(seriesMap.values())
}

function createLegend(
  data: Array<{
    name?: string | number
    icon?: string
    itemStyle?: { color: string }
  }>
) {
  const uniqueNames = new Set<string | number>()

  const uniqueData = data.reduce(
    (acc, item) => {
      if (item.name && !uniqueNames.has(item.name)) {
        uniqueNames.add(item.name)
        acc.push({
          name: item.name as string,
          icon: item.icon || ('roundRect' as const),
          itemStyle: {
            color: item.itemStyle?.color,
          },
        })
      }
      return acc
    },
    [] as Array<{
      name: string
      icon: string
      itemStyle: { color: string | undefined }
    }>
  )

  return {
    orient: 'vertical' as const,
    left: 'right' as const,
    show: false,
    data: uniqueData,
  }
}

function getEventDetails(eventValue: number) {
  for (const group of EVENT_GROUPS) {
    if (group.codes.includes(eventValue)) {
      return { name: group.name, color: group.color }
    }
  }
  return { name: 'Unknown Event', color: CycleColor.Default }
}

const EVENT_GROUPS = [
  {
    codes: [1, 61],
    name: 'Phase Begin Green (1)\nOverlap Begin Green (61)',
    color: CycleColor.Green,
  },
  {
    codes: [3, 62],
    name: 'Phase Min Complete (3)\nOverlap Begin Trailing Green (Extension) (62)',
    color: CycleColor.LightGreen,
  },
  {
    codes: [8, 63],
    name: 'Phase Begin Yellow Clearance (8)\nBegin Overlap Yellow (63)',
    color: CycleColor.Yellow,
  },
  {
    codes: [9, 64],
    name: 'Phase End Yellow Clearance (9)\nOverlap Begin Red Clearance (64)',
    color: CycleColor.DarkRed,
  },
  {
    codes: [11, 65],
    name: 'Phase End Red Clearance (11)\nOverlap Off (Inactive with Red Indication) (65)',
    color: CycleColor.LightRed,
  },
  {
    codes: [21, 67],
    name: 'Pedestrian Begin Walk (21, 67)',
    color: CycleColor.LightBlue,
  },
  {
    codes: [22, 68],
    name: 'Pedestrian Begin Clearance (22, 68)',
    color: CycleColor.Blue,
  },
  {
    codes: [23, 69],
    name: "Pedestrian Begin Solid Don't Walk (23, 69)",
    color: CycleColor.DarkGrey,
  },
  {
    codes: [82],
    name: 'Detector On (82)',
    color: CycleColor.Black,
    icon: 'circle',
  },
  {
    codes: [81],
    name: 'Detector Off (81)',
    color: CycleColor.Grey,
    icon: 'square',
  },
]

function getAllEventDetails() {
  return Object.values(EVENT_GROUPS).filter(
    (detail) => detail.name !== 'Unknown Event'
  )
}

function createTitleOnlyChart(data: RawTimingAndActuationData) {
  const titleHeader = `Timing and Actuation\n${data.locationDescription} - ${data.approachDescription}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  return {
    title: createTitle({
      title: titleHeader,
      dateRange,
    }),
  }
}

function createLegendOnlyChart() {
  const legendItems = getAllEventDetails()
  legendItems.push({
    name: 'Line Connecting Detector On to Detector Off',
    color: 'black',
    icon: SolidLineSeriesSymbol,
  })

  const chartConfigurations = []
  const groupSizes = [5, 3, 3]

  let startIndex = 0

  groupSizes.forEach((size) => {
    const group = legendItems.slice(startIndex, startIndex + size)
    const chartConfig = {
      legend: {
        data: group.map((item) => ({
          name: item.name,
          icon: item.icon || 'roundRect',
          itemStyle: {
            color: item.color,
          },
        })),
        textStyle: {
          lineHeight: 15,
        },
        orient: 'vertical',
        left: 'left',
        top: 'top',
      },
      series: group.map((item) => ({
        name: item.name,
        type: 'line',
      })),
      xAxis: {
        show: false,
      },
      yAxis: {
        show: false,
      },
    }
    chartConfigurations.push(chartConfig)
    startIndex += size
  })

  return chartConfigurations
}
