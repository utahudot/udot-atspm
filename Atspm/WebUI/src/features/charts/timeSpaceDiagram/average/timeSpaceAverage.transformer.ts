// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceAverage.transformer.ts
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
  createDisplayProps,
  createLegend,
  createTitle,
  createYAxis,
} from '@/features/charts/common/transformers'
import { ToolType } from '@/features/charts/common/types'
import {
  buildTimeSpaceChartScaffold,
  buildTimeSpaceDataZoom,
  buildTimeSpaceGrid,
  buildTimeSpaceToolbox,
  buildTimeSpaceXAxis,
} from '@/features/charts/timeSpaceDiagram/core/transformers/timeSpaceChartScaffold'
import { buildTimeSpacePhaseLayout } from '@/features/charts/timeSpaceDiagram/core/transformers/timeSpacePhaseLayout'
import {
  buildEmptyTimeSpaceTransformResult,
  buildTimeSpaceTransformResult,
  unwrapTimeSpaceTransformResults,
} from '@/features/charts/timeSpaceDiagram/core/transformers/timeSpaceTransformResult'
import {
  generateCycleLabels,
  generateCycles,
  generateGreenEventLines,
  getDistancesLabelOption,
  getLocationsLabelOption,
  TIME_SPACE_Y_AXIS_PADDING,
} from '@/features/charts/timeSpaceDiagram/shared/transformers/timeSpaceTransformerBase'
import {
  RawTimeSpaceAverageData,
  RawTimeSpaceDiagramResponse,
  TimeSpaceDiagramPhaseResult,
} from '@/features/charts/timeSpaceDiagram/shared/types'
import { TransformedTimeSpaceResponse } from '@/features/charts/types'
import { SolidLineSeriesSymbol } from '@/features/charts/utils'
import { format, isSameDay } from 'date-fns'
import { EChartsOption, SeriesOption } from 'echarts'

export default function transformTimeSpaceAverageData(
  response: RawTimeSpaceDiagramResponse
): TransformedTimeSpaceResponse & { errors?: string[] } {
  const wrappedData =
    response.data as TimeSpaceDiagramPhaseResult<RawTimeSpaceAverageData>[]
  const { errorMessages, successfulData } =
    unwrapTimeSpaceTransformResults(wrappedData)

  if (successfulData.length === 0) {
    return buildEmptyTimeSpaceTransformResult(
      ToolType.TimeSpaceAverage,
      errorMessages
    )
  }

  return buildTimeSpaceTransformResult(
    ToolType.TimeSpaceAverage,
    transformData(successfulData),
    errorMessages
  )
}

function transformData(data: RawTimeSpaceAverageData[]): EChartsOption {
  const {
    chartHeight,
    distanceScale,
    locationCenterDistanceData,
    maxDisplayDistance,
    minDisplayDistance,
    opposingDirection,
    opposingDistanceData,
    opposingPhaseData,
    primaryDirection,
    primaryDistanceData,
    primaryPhaseData,
  } = buildTimeSpacePhaseLayout(data, {
    sortByOrder: true,
  })
  const chartStart = new Date(data[0].start)
  const rawEnd = new Date(data[0].end)
  const chartEnd = new Date(chartStart)

  chartEnd.setHours(
    rawEnd.getHours(),
    rawEnd.getMinutes(),
    rawEnd.getSeconds(),
    rawEnd.getMilliseconds()
  )

  if (chartEnd <= chartStart) {
    chartEnd.setDate(chartEnd.getDate() + 1)
  }

  const chartStartIso = chartStart.toISOString()
  const chartEndIso = chartEnd.toISOString()
  const title = createTitle({
    title: 'Time Space Diagram - 50th Percentile',
    location: buildAverageHeaderRange(chartStart, rawEnd, chartEnd),
  })

  const chartStartMs = chartStart.getTime()
  const chartEndMs = chartEnd.getTime()
  const xAxis = buildTimeSpaceXAxis(
    chartStartIso,
    chartEndIso,
    chartStartMs,
    chartEndMs
  )
  const yAxis = createYAxis(false, {
    show: false,
    data: locationCenterDistanceData,
    min: minDisplayDistance - TIME_SPACE_Y_AXIS_PADDING,
    max: maxDisplayDistance + TIME_SPACE_Y_AXIS_PADDING,
    axisLabel: {
      show: false,
    },
  })
  const grid = buildTimeSpaceGrid()

  const legends = createLegend({
    top: grid.top,
    data: [
      {
        name: `Cycles ${primaryDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: '#f0807f' },
      },
      {
        name: `Cycles ${opposingDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: '#f0807f' },
      },
      {
        name: `Cycle Durations ${primaryDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: 'black' },
      },
      {
        name: `Cycle Durations ${opposingDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: 'black' },
      },
      {
        name: `Green Bands ${primaryDirection}`,
        itemStyle: { color: 'green', opacity: 0.3 },
      },
      {
        name: `Green Bands ${opposingDirection}`,
        itemStyle: { color: 'green', opacity: 0.3 },
      },
    ],
    selected: {
      [`Cycles ${primaryDirection}`]: true,
      [`Cycles ${opposingDirection}`]: true,
      [`Cycle Durations ${primaryDirection}`]: true,
      [`Cycle Durations ${opposingDirection}`]: true,
      [`Green Bands ${primaryDirection}`]: true,
      [`Green Bands ${opposingDirection}`]: true,
    },
  })

  const dataZoom = buildTimeSpaceDataZoom(chartStartMs, chartEndMs)
  const toolbox = buildTimeSpaceToolbox('Time Space Diagram - 50th Percentile')

  const series: SeriesOption[] = []

  series.push(
    ...generateCycles(
      primaryPhaseData,
      primaryDistanceData,
      primaryDirection,
      'primary'
    )
  )
  series.push(
    ...generateGreenEventLines(
      primaryPhaseData,
      primaryDistanceData,
      primaryDirection,
      true,
      distanceScale,
      'primary'
    )
  )

  series.push(
    getLocationsLabelOption(primaryPhaseData, locationCenterDistanceData, grid)
  )
  series.push(
    getDistancesLabelOption(
      primaryPhaseData,
      locationCenterDistanceData,
      grid.left as number,
      distanceScale
    )
  )
  series.push(
    generateCycleLabels(
      locationCenterDistanceData,
      primaryDirection,
      undefined,
      primaryPhaseData.map((p) => p.approachDescription),
      primaryPhaseData.map((p) => [
        `Split: ${formatSecondsDetail(p.programmedSplit)}`,
      ]),
      'left',
      primaryPhaseData.map((p) => Boolean(p.isIgnoredLocation))
    )
  )

  series.push(
    ...generateCycles(
      opposingPhaseData,
      opposingDistanceData,
      opposingDirection,
      'opposing'
    )
  )

  series.push(
    ...generateGreenEventLines(
      opposingPhaseData,
      opposingDistanceData,
      opposingDirection,
      false,
      distanceScale,
      'opposing'
    )
  )

  series.push(
    generateCycleLabels(
      locationCenterDistanceData,
      opposingDirection,
      undefined,
      [...opposingPhaseData].reverse().map((p) => p.approachDescription),
      [...opposingPhaseData]
        .reverse()
        .map((p) => [`Split: ${formatSecondsDetail(p.programmedSplit)}`]),
      'right',
      [...opposingPhaseData].reverse().map((p) => Boolean(p.isIgnoredLocation))
    )
  )

  const displayProps = createDisplayProps({
    description: '',
    numberOfLocations: primaryPhaseData.length,
    height: chartHeight,
    locations: primaryPhaseData.map((p) => p.locationIdentifier),
  })

  return buildTimeSpaceChartScaffold({
    title,
    xAxis,
    yAxis,
    grid,
    dataZoom,
    legend: legends,
    toolbox,
    series,
    displayProps,
    animation: true,
  })
}

function buildAverageHeaderRange(rawStart: Date, rawEnd: Date, chartEnd: Date) {
  const dateRange = isSameDay(rawStart, rawEnd)
    ? format(rawStart, 'EEE, MMM d, yyyy')
    : `${format(rawStart, 'EEE, MMM d, yyyy')} - ${format(
        rawEnd,
        'EEE, MMM d, yyyy'
      )}`

  return `${dateRange} - ${format(rawStart, 'HH:mm')}-${format(chartEnd, 'HH:mm')}`
}

function formatSecondsDetail(value: number | null | undefined) {
  if (typeof value !== 'number' || !Number.isFinite(value)) {
    return 'unknown'
  }

  return `${value}s`
}
