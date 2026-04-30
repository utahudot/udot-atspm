// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - timeSpaceHistoricTransformer.ts
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
  generateAdvanceCountEventLines,
  generateLaneByLaneCountEventLines,
  generateStopBarPresenceEventLines,
} from '@/features/charts/timeSpaceDiagram/historic/series/timeSpaceHistoricDetectionSeries'
import {
  buildCycleEventMarkersOnCyclesSeries,
  buildTspRequestAndServiceLineSeries,
  generateSrmEntityLines,
  generateTMCEvent,
} from '@/features/charts/timeSpaceDiagram/historic/series/timeSpaceHistoricMovementSeries'
import { generatePedestrianIntervalLines } from '@/features/charts/timeSpaceDiagram/historic/series/timeSpaceHistoricPedestrianSeries'
import {
  generateCycleLabels,
  generateCycles,
  generateGreenEventLines,
  getDistancesLabelOption,
  getLocationsLabelOption,
  TIME_SPACE_Y_AXIS_PADDING,
} from '@/features/charts/timeSpaceDiagram/shared/transformers/timeSpaceTransformerBase'
import {
  RawTimeSpaceDiagramResponse,
  RawTimeSpaceHistoricData,
  TimeSpaceDiagramPhaseResult,
} from '@/features/charts/timeSpaceDiagram/shared/types'
import { TransformedTimeSpaceResponse } from '@/features/charts/types'
import {
  Color,
  formatChartDateTimeRange,
  SolidLineSeriesSymbol,
  triangleSvgSymbol,
} from '@/features/charts/utils'
import { EChartsOption, SeriesOption } from 'echarts'

export default function transformTimeSpaceHistoricData(
  response: RawTimeSpaceDiagramResponse
): TransformedTimeSpaceResponse & { errors?: string[] } {
  const wrappedData =
    response.data as TimeSpaceDiagramPhaseResult<RawTimeSpaceHistoricData>[]
  const { errorMessages, successfulData } =
    unwrapTimeSpaceTransformResults(wrappedData)

  if (successfulData.length === 0) {
    return buildEmptyTimeSpaceTransformResult(
      ToolType.TimeSpaceHistoric,
      errorMessages
    )
  }

  return buildTimeSpaceTransformResult(
    ToolType.TimeSpaceHistoric,
    transformData(successfulData),
    errorMessages
  )
}
function transformData(data: RawTimeSpaceHistoricData[]): EChartsOption {
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
  } = buildTimeSpacePhaseLayout(data)

  const dateRange = formatChartDateTimeRange(data[0].start, data[0].end)

  const chartStartMs = Date.parse(primaryPhaseData[0].start)
  const chartEndMs = Date.parse(primaryPhaseData[0].end)
  const xAxis = buildTimeSpaceXAxis(
    data[0].start,
    data[0].end,
    chartStartMs,
    chartEndMs
  )
  const yAxis = createYAxis(false, {
    show: false,
    data: locationCenterDistanceData,
    axisTick: { show: true },
    max: maxDisplayDistance + TIME_SPACE_Y_AXIS_PADDING,
    min: minDisplayDistance - TIME_SPACE_Y_AXIS_PADDING,
  })

  const title = createTitle({
    title: 'Time Space Diagram • Historic',
    location: dateRange,
  })

  const grid = buildTimeSpaceGrid()
  const dataZoom = buildTimeSpaceDataZoom(chartStartMs, chartEndMs)
  const toolbox = buildTimeSpaceToolbox('Time Space Diagram Historic')

  // const { requestRects, serviceRects, intersectionLines } =
  //   buildRectsAndLinesForTSD(primaryPhaseData)
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
    ...generateLaneByLaneCountEventLines(
      opposingPhaseData,
      opposingDistanceData,
      'orange',
      opposingDirection,
      false,
      distanceScale
    )
  )

  series.push(
    ...generateAdvanceCountEventLines(
      opposingPhaseData,
      opposingDistanceData,
      'orange',
      opposingDirection,
      false,
      distanceScale
    )
  )

  series.push(
    ...generateStopBarPresenceEventLines(
      opposingPhaseData,
      opposingDistanceData,
      'orange',
      opposingDirection,
      false,
      distanceScale
    )
  )

  series.push(
    ...generateLaneByLaneCountEventLines(
      primaryPhaseData,
      primaryDistanceData,
      'darkblue',
      primaryDirection,
      true,
      distanceScale
    )
  )

  series.push(
    ...generateAdvanceCountEventLines(
      primaryPhaseData,
      primaryDistanceData,
      'darkblue',
      primaryDirection,
      true,
      distanceScale
    )
  )

  series.push(
    ...generateStopBarPresenceEventLines(
      primaryPhaseData,
      primaryDistanceData,
      'darkblue',
      primaryDirection,
      true,
      distanceScale
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
    ...generatePedestrianIntervalLines(
      primaryPhaseData,
      primaryDistanceData,
      primaryDirection,
      'primary'
    )
  )
  series.push(
    ...generateSrmEntityLines(
      primaryPhaseData,
      primaryDistanceData,
      primaryDirection,
      distanceScale,
      'primary'
    )
  )

  series.push(
    ...generateTMCEvent(
      primaryPhaseData,
      primaryDistanceData,
      primaryDirection,
      distanceScale,
      'primary'
    )
  )

  series.push(
    ...buildCycleEventMarkersOnCyclesSeries(
      primaryPhaseData,
      primaryDistanceData,
      'primary'
    )
  )

  series.push(
    ...buildTspRequestAndServiceLineSeries(
      primaryPhaseData,
      primaryDistanceData,
      'primary'
    )
  )

  const locationLabels = getLocationsLabelOption(
    primaryPhaseData,
    locationCenterDistanceData,
    grid
  )
  series.push(locationLabels)
  series.push(
    getDistancesLabelOption(
      primaryPhaseData,
      locationCenterDistanceData,
      grid.left as number,
      distanceScale
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
    ...generatePedestrianIntervalLines(
      opposingPhaseData,
      opposingDistanceData,
      opposingDirection,
      'opposing'
    )
  )

  series.push(
    ...generateSrmEntityLines(
      opposingPhaseData,
      opposingDistanceData,
      opposingDirection,
      distanceScale,
      'opposing'
    )
  )

  series.push(
    ...buildCycleEventMarkersOnCyclesSeries(
      opposingPhaseData,
      opposingDistanceData,
      'opposing'
    )
  )

  series.push(
    ...buildTspRequestAndServiceLineSeries(
      opposingPhaseData,
      opposingDistanceData,
      'opposing'
    )
  )

  // for each series set the xAxisIndex to 0
  series.forEach((s) => {
    s.xAxisIndex = 0
  })

  const formatPct = (v?: number | null) =>
    v === null || v === undefined ? '—' : `${Math.round(v)}%`

  const primaryLinesByIndex = primaryPhaseData.map((p) => [
    `AOG: ${formatPct(p.percentArrivalOnGreen)}`,
  ])

  const primaryHeadersByIndex = primaryPhaseData.map(
    (p) => p.approachDescription
  )

  const opposingHeadersByIndex = [...opposingPhaseData]
    .reverse()
    .map((p) => p.approachDescription)

  const opposingLinesByIndex = [...opposingPhaseData]
    .reverse()
    .map((p) => [`AOG: ${formatPct(p.percentArrivalOnGreen)}`])

  const primaryIgnoredByIndex = primaryPhaseData.map((p) =>
    Boolean(p.isIgnoredLocation)
  )

  const opposingIgnoredByIndex = [...opposingPhaseData]
    .reverse()
    .map((p) => Boolean(p.isIgnoredLocation))

  const primaryLabelSeries = generateCycleLabels(
    locationCenterDistanceData,
    primaryDirection,
    grid.left as number,
    primaryHeadersByIndex,
    primaryLinesByIndex,
    'left',
    primaryIgnoredByIndex
  )

  const opposingLabelSeries = generateCycleLabels(
    locationCenterDistanceData,
    opposingDirection,
    grid.left as number,
    opposingHeadersByIndex,
    opposingLinesByIndex,
    'right',
    opposingIgnoredByIndex
  )

  series.push(primaryLabelSeries)
  series.push(opposingLabelSeries)

  // series.push(
  //   ...getDraggableOffsetabelOption(
  //     primaryPhaseData,
  //     distanceData,
  //     primaryDirection
  //   )
  // )

  const displayProps = createDisplayProps({
    description: '',
    numberOfLocations: primaryPhaseData.length,
    height: chartHeight,
    locations: primaryPhaseData.map((p) => p.locationIdentifier),
  })

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
        itemStyle: { color: Color.Black },
      },
      {
        name: `Cycle Durations ${opposingDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
      },
      {
        name: `Green Bands ${primaryDirection}`,
        itemStyle: { color: 'green', opacity: 0.3 },
      },
      {
        name: `Green Bands ${opposingDirection}`,
        itemStyle: { color: 'green', opacity: 0.3 },
      },
      {
        name: `Lane by Lane Count ${primaryDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: 'darkblue' },
      },
      {
        name: `Lane by Lane Count ${opposingDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: 'orange' },
      },
      {
        name: `Advance Count ${primaryDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: 'darkblue' },
      },
      {
        name: `Advance Count ${opposingDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: 'orange' },
      },
      {
        name: `Stop Bar Presence ${primaryDirection}`,
        itemStyle: { color: 'lightBlue' },
      },
      {
        name: `Stop Bar Presence ${opposingDirection}`,
        itemStyle: { color: 'orange' },
      },
      {
        name: `Pedestrian Interval ${primaryDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
      },
      {
        name: `Pedestrian Interval ${opposingDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
      },
      {
        name: `SRM Entity ${primaryDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
      },
      {
        name: `SRM Entity ${opposingDirection}`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
      },
      {
        name: `Left Turn ${primaryDirection}`,
        itemStyle: { color: 'black' },
      },
      {
        name: `Right Turn ${primaryDirection}`,
        itemStyle: { color: 'black' },
      },
      {
        name: `Early Green (113)`,
        icon: 'circle',
        itemStyle: {
          color: Color.White,
          borderColor: Color.Black,
          borderWidth: 1.5,
        },
      },
      {
        name: `Extend Green (114)`,
        icon: triangleSvgSymbol,
        itemStyle: {
          color: Color.White,
          borderColor: Color.Black,
          borderWidth: 1.5,
        },
      },
      {
        name: `TSP Request (112-115)`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
      },
      {
        name: `TSP Service (118-119)`,
        icon: SolidLineSeriesSymbol,
        itemStyle: { color: Color.Black },
      },
    ],
    selected: {
      [`Cycles ${primaryDirection}`]: true,
      [`Cycles ${opposingDirection}`]: true,
      [`Cycle Durations ${primaryDirection}`]: true,
      [`Cycle Durations ${opposingDirection}`]: true,
      [`Green Bands ${primaryDirection}`]: true,
      [`Green Bands ${opposingDirection}`]: true,
      [`Lane by Lane Count ${primaryDirection}`]: false,
      [`Lane by Lane Count ${opposingDirection}`]: false,
      [`Advance Count ${primaryDirection}`]: false,
      [`Advance Count ${opposingDirection}`]: false,
      [`Stop Bar Presence ${primaryDirection}`]: false,
      [`Stop Bar Presence ${opposingDirection}`]: false,
      [`Pedestrian Interval ${primaryDirection}`]: false,
      [`Pedestrian Interval ${opposingDirection}`]: false,
      [`SRM Entity ${primaryDirection}`]: true,
      [`SRM Entity ${opposingDirection}`]: true,
      [`Left Turn ${primaryDirection}`]: false,
      [`Right Turn ${primaryDirection}`]: false,
      [`Early Green (113)`]: false,
      [`Extend Green (114)`]: false,
      [`TSP Request (112-115)`]: false,
      [`TSP Service (118-119)`]: false,
    },
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
