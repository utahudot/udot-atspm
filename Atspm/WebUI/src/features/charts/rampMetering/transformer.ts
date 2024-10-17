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
  createSeries,
  createTitle,
  createToolbox,
  createTooltip,
  createXAxis,
  createYAxis,
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
  DescriptionWithDetectorEvents,
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
    lanesQueueOnEvents,
    lanesQueueOffEvents,
  } = data

  const titleHeader1 = `Ramp Rate vs. Mainline Occupancy (Active Rate vs Mainline Avg Occupancy)\n${data.locationIdentifier}`
  const titleHeader2 = `Queue Override\n${data.locationIdentifier}`
  const titleHeader3 = `Ramp Rate vs. Mainline Volume - (Active Rate vs Mainline Avg Flow)\n${data.locationIdentifier}`
  const titleHeader4 = `Ramp Rate vs. Mainline Speed  (Active Rate vs Mainline Avg Speed)\n${data.locationIdentifier}`
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
      name: 'Meter Active Indication',
      min: 0,
      max: 6,
      interval: 1,
    }
  )

  const yAxisThree = createYAxis(
    false,
    {
      name: 'Mainline Flow',
      nameGap: 50,
    },
    { name: 'Meter Rate (vph)' }
  )

  const yAxisFour = createYAxis(
    false,
    {
      name: 'Mainline Speed',
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
    return {
      name: `Active Rate: ${lane.description}`,
      icon: SolidLineSeriesSymbol,
    }
  })

  const lanesBaseLegends = lanesBaseRate.map((lane) => {
    return {
      name: `Base Rate: ${lane.description}`,
      icon: SolidLineSeriesSymbol,
    }
  })

  const lanesQueueOnLegends = lanesQueueOnEvents.map((lane) => {
    return {
      name: `Queue On: ${lane.description}`,
      icon: triangleSvgSymbol,
      itemStyle: { color: '#00C5FF' },
    }
  })

  const lanesQueueOffLegends = lanesQueueOffEvents.map((lane) => {
    return {
      name: `Queue Off: ${lane.description}`,
      icon: 'square',
      itemStyle: { color: '#00C5FF' },
    }
  })

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
      ...lanesQueueOnLegends,
      ...lanesQueueOffLegends,
      {
        name: 'Start Up Warning',
      },
      {
        name: 'Shutdown Warning',
      },
      {
        name: 'Queue Rate Activated',
      },
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

  const dataZoom = createDataZoom()

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
    seriesOne.push(
      ...createSeries({
        name: `Active Rate: ${lane.description}`,
        data: transformSeriesData(lane.value),
        type: 'line',
        yAxisIndex: 1,
        color: chartColors[index + 1],
      })
    )
  })

  const seriesTwo: SeriesOption[] = []

  // const seriesTwoQueueData = generateQueueSeriesData(lanesQueueEvents)

  // seriesTwo.push(...seriesTwoQueueData)

  lanesQueueOnEvents.forEach((lane) => {
    seriesTwo.push(
      ...createSeries({
        name: `Queue On: ${lane.description}`,
        data: transformSeriesData(lane.value),
        type: 'scatter',
        // tooltip: {
        //   confine: true,
        //   valueFormatter: (value) => (value as string[])[0],
        // },
        yAxisIndex: 1,
        symbol: 'triangle',
        symbolSize: 5,
        itemStyle: {
          color: '#00C5FF',
        },
      })
    )
  })

  lanesQueueOffEvents.forEach((lane) => {
    seriesTwo.push(
      ...createSeries({
        name: `Queue Off: ${lane.description}`,
        data: transformSeriesData(lane.value),
        type: 'scatter',
        yAxisIndex: 1,
        // tooltip: {
        //   confine: true,
        //   valueFormatter: (value) => (value as string[])[0],
        // },
        symbol: 'square',
        symbolSize: 5,
        itemStyle: {
          color: '#00C5FF',
        },
      })
    )
  })

  lanesActiveRate.forEach((lane, index) => {
    seriesTwo.push(
      ...createSeries({
        name: `Active Rate: ${lane.description}`,
        data: transformSeriesData(lane.value),
        type: 'line',
        color: chartColors[index + 1],
      })
    )
  })

  lanesBaseRate.forEach((lane, index) => {
    seriesTwo.push(
      ...createSeries({
        name: `Base Rate: ${lane.description}`,
        data: transformSeriesData(lane.value),
        type: 'line',
        color: chartColors[index + 2],
      })
    )
  })

  seriesTwo.push(createStartupArea(data.startUpWarning))

  // seriesTwo.push(
  //   ...createSeries({
  //     name: 'Start Up Warning',
  //     data: transformSeriesData(data.startUpWarning),
  //     type: 'line',
  //     yAxisIndex: 1,
  //     color: Color.Green,
  //     lineStyle: {
  //       width: 5,
  //     },
  //   })
  // )

  // seriesTwo.push(
  //   ...createSeries({
  //     name: 'Shutdown Warning',
  //     data: transformSeriesData(data.shutdownWarning),
  //     type: 'line',
  //     yAxisIndex: 1,
  //     color: Color.Red,
  //     lineStyle: {
  //       width: 5,
  //     },
  //   })
  // )

  const seriesThree: SeriesOption[] = []

  lanesActiveRate.forEach((lane, index) => {
    seriesThree.push(
      ...createSeries({
        name: `Active Rate: ${lane.description}`,
        data: transformSeriesData(lane.value),
        type: 'line',
        yAxisIndex: 1,
        color: chartColors[index + 1],
      })
    )
  })

  lanesBaseRate.forEach((lane, index) => {
    seriesThree.push(
      ...createSeries({
        name: `Base Rate: ${lane.description}`,
        data: transformSeriesData(lane.value),
        type: 'line',
        yAxisIndex: 1,
        color: chartColors[index + 2],
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

  const seriesFour: SeriesOption[] = []

  lanesActiveRate.forEach((lane, index) => {
    seriesFour.push(
      ...createSeries({
        name: `Active Rate: ${lane.description}`,
        data: transformSeriesData(lane.value),
        type: 'line',
        yAxisIndex: 1,
        color: chartColors[index + 1],
      })
    )
  })

  lanesBaseRate.forEach((lane, index) => {
    seriesFour.push(
      ...createSeries({
        name: `Base Rate: ${lane.description}`,
        data: transformSeriesData(lane.value),
        type: 'line',
        yAxisIndex: 1,
        color: chartColors[index + 2],
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

  // const queueActives = {
  //   markArea: {
  //     data: [
  //       [
  //         {
  //           xAxis: '2023-08-24T07:00:00',
  //           itemStyle: {
  //             color: 'rgba(125, 125, 125, 0.1)',
  //           },
  //         },
  //         {
  //           xAxis: '2023-08-24T07:22:22',
  //         },
  //       ],
  //       [
  //         {
  //           xAxis: '2023-08-24T07:30:43',
  //           itemStyle: {
  //             color: 'rgba(125, 125, 125, 0.1)',
  //           },
  //         },
  //         {
  //           xAxis: '2023-08-24T07:50:00',
  //         },
  //       ],
  //     ],
  //   },

  //   type: 'line',
  //   yAxisIndex: 1,
  //   color: '#00C5FF',
  //   lineStyle: {
  //     width: 5,
  //   },
  //   symbol: 'line',
  //   symbolSize: 0,
  // }

  // const queueActivesTwo = {
  //   name: 'Queue Rate Activated',
  //   data: [
  //     ['2023-08-24T07:25:49', '0.00'],
  //     ['2023-08-24T07:26:49', '0.00'],
  //     null,
  //     ['2023-08-24T07:28:29', '0.00'],
  //     ['2023-08-24T07:29:49', '0.00'],
  //   ],
  //   type: 'line',
  //   yAxisIndex: 1,
  //   color: '#00C5FF',
  //   lineStyle: {
  //     width: 5,
  //   },
  //   symbol: 'line',
  //   symbolSize: 0,
  // }

  // seriesTwo.push(queueActives as any)
  // seriesTwo.push(queueActivesTwo as any)

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
  laneQueueEvents: DescriptionWithDetectorEvents[]
): SeriesOption[] {
  const onSeriesData: any = []
  const offSeriesData: any = []
  const lineSeriesData: any = []

  laneQueueEvents.forEach((lane) => {
    lane.value.forEach((event) => {
      const onPoint = {
        value: [event.detectorOn, 6, lane.description],
        symbol: 'triangle',
        symbolSize: 5,
        itemStyle: {
          color: 'black',
        },
      }
      const offPoint = {
        value: [event.detectorOff, 6, lane.description],
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
            coord: [event.detectorOn, 6, lane.description],
          },
          {
            coord: [event.detectorOff, 6, lane.description],
          },
        ])
      }
    })
  })

  return [
    {
      name: 'Queue On',
      type: 'scatter',
      tooltip: {
        confine: true,
        valueFormatter: (value) => (value as string[])[0],
      },
      data: onSeriesData,
    },
    {
      name: 'Queue Off',
      type: 'scatter',
      tooltip: {
        confine: true,
        valueFormatter: (value) => (value as string[])[0],
      },
      data: offSeriesData,
    },
    {
      name: 'Line Connecting Queue On to Off',
      type: 'lines',
      coordinateSystem: 'cartesian2d',
      lineStyle: {
        color: 'black',
        width: 2,
      },
      data: lineSeriesData,
    },
  ]
}
function createStartupArea(events: TimeSpaceDetectorEvent[]): SeriesOption {
  const markAreaData = events.map((event) => [
    {
      xAxis: event.initialX,
      itemStyle: Color.PlanB,
    },
    {
      xAxis: event.finalX,
    },
  ])

  return {
    type: 'line',
    markArea: {
      data: markAreaData as MarkAreaData[],
    },
  }
}
