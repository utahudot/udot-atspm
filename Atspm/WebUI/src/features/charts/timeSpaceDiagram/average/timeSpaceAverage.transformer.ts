// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - timeSpaceAverageTransformer.ts
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
  createLegend,
  createTitle,
  createToolbox,
  createTooltip,
  createXAxis,
  createYAxis,
  formatExportFileName,
} from '@/features/charts/common/transformers'
import { ToolType } from '@/features/charts/common/types'
import {
  generateCycleLabels,
  generateCycles,
  generateGreenEventLines,
  getDisplayDistanceScale,
  getDistancesLabelOption,
  getLocationsLabelOption,
  getOffsetAndProgramSplitLabel,
  getTimeSpaceChartHeight,
  getTimeSpacePhaseRowDistances,
  TIME_SPACE_Y_AXIS_PADDING,
} from '@/features/charts/timeSpaceDiagram/shared/transformers/timeSpaceTransformerBase'
import {
  RawTimeSpaceAverageData,
  RawTimeSpaceDiagramResponse,
  TimeSpaceDiagramPhaseResult,
} from '@/features/charts/timeSpaceDiagram/shared/types'
import { TransformedTimeSpaceResponse } from '@/features/charts/types'
import {
  formatChartDateTimeRange,
  SolidLineSeriesSymbol,
} from '@/features/charts/utils'
import { format } from 'date-fns'
import {
  DataZoomComponentOption,
  EChartsOption,
  GridComponentOption,
  SeriesOption,
} from 'echarts'

export default function transformTimeSpaceAverageData(
  response: RawTimeSpaceDiagramResponse
): TransformedTimeSpaceResponse & { errors?: string[] } {
  // Extract successful results and filter out errors
  const wrappedData =
    response.data as TimeSpaceDiagramPhaseResult<RawTimeSpaceAverageData>[]

  // Collect error messages
  const errorMessages = wrappedData
    .filter((item) => !item.isSuccess && item.error)
    .map((item) => item.error as string)

  // Extract only successful results
  const successfulData = wrappedData
    .filter((item) => item.isSuccess && item.result)
    .map((item) => item.result as RawTimeSpaceAverageData)

  if (successfulData.length === 0) {
    // Return error information instead of throwing
    return {
      type: ToolType.TimeSpaceAverage,
      data: { chart: {} },
      errors:
        errorMessages.length > 0
          ? errorMessages
          : [
              'No valid time space diagram data available. All phases returned errors.',
            ],
    }
  }

  const result: TransformedTimeSpaceResponse & { errors?: string[] } = {
    type: ToolType.TimeSpaceAverage,
    data: {
      chart: transformData(successfulData),
    },
  }

  // Include errors if some phases failed but we still have partial data
  if (errorMessages.length > 0) {
    result.errors = errorMessages
  }

  return result
}

function transformData(data: RawTimeSpaceAverageData[]): EChartsOption {
  const byOrder = (a: RawTimeSpaceAverageData, b: RawTimeSpaceAverageData) =>
    (a.order ?? Number.MAX_SAFE_INTEGER) - (b.order ?? Number.MAX_SAFE_INTEGER)

  const primaryPhaseData = data
    .filter((location) => location.phaseType === 'Primary')
    .sort(byOrder)

  const opposingPhaseData = data
    .filter((location) => location.phaseType === 'Opposing')
    .sort(byOrder)
  const titleHeader = `Time Space Diagram (50th Percentile),\nPrimary Phase - ${primaryPhaseData[0].approachDescription},\nOpposing Phase - ${opposingPhaseData[0].approachDescription},\nCoordinated Phases - ${data[0].coordinatedPhases}`
  const dateRange = formatChartDateTimeRange(data[0].start, data[0].end)
  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: `Route data from ${data[0].locationDescription} to ${
      data[data.length - 1].locationDescription
    } \n`,
  })

  const startDate = new Date(data[0].start)
  const endDate = new Date(data[0].end)

  startDate.setHours(
    endDate.getHours(),
    endDate.getMinutes(),
    endDate.getSeconds()
  )

  const endDateFormat = format(startDate, "yyyy-MM-dd'T'HH:mm:ss")

  const xAxis = createXAxis(data[0].start, endDateFormat)

  const primaryDirection = primaryPhaseData[0].approachDescription.split(' ')[0]
  const opposingDirection =
    opposingPhaseData[0].approachDescription.split(' ')[0]

  let initialDistance = 0
  const rawDistanceData: number[] = []
  primaryPhaseData.forEach((location) => {
    rawDistanceData.push(initialDistance)
    initialDistance += location.distanceToNextLocation
  })
  const distanceScale = getDisplayDistanceScale(rawDistanceData)
  const locationCenterDistanceData = rawDistanceData.map(
    (distance) => distance * distanceScale
  )
  const { primaryDistanceData, opposingDistanceData } =
    getTimeSpacePhaseRowDistances(locationCenterDistanceData)
  const minDisplayDistance = Math.min(
    ...primaryDistanceData,
    ...opposingDistanceData
  )
  const maxDisplayDistance = Math.max(
    ...primaryDistanceData,
    ...opposingDistanceData
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

  const chartHeight = getTimeSpaceChartHeight(
    minDisplayDistance,
    maxDisplayDistance,
    primaryPhaseData.length
  )

  const grid: GridComponentOption = {
    top: 200,
    left: 220,
    right: 325,
    bottom: 80,
    show: true,
    borderWidth: 1,
    // borderColor: Color.Black,
  }

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
  })

  const cycleEvents = data[0].cycleAllEvents ?? []
  const startEvent = cycleEvents[cycleEvents.length - 1]
  const endEvent = cycleEvents[0]
  const timeDiff =
    startEvent != null && endEvent != null
      ? (new Date(startEvent.start).getTime() -
          new Date(endEvent.start).getTime()) /
        3600000
      : 0

  let dataZoom: DataZoomComponentOption[]

  if (timeDiff > 6) {
    dataZoom = [
      {
        type: 'slider',
        filterMode: 'filter',
        show: true,
        start: 0,
        end: 10,
        maxSpan: 10,
        minSpan: 0.2,
      },
      {
        type: 'inside',
        filterMode: 'none',
        show: true,
        minSpan: 0.2,
      },
    ]
  } else {
    dataZoom = createDataZoom()
  }

  const toolbox = createToolbox(
    {
      title: formatExportFileName(titleHeader, data[0].start, data[0].end),
      dateRange,
    },
    data[0].locationIdentifier,
    ToolType.TimeSpaceHistoric
  )

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
    getOffsetAndProgramSplitLabel(
      primaryPhaseData,
      opposingPhaseData,
      locationCenterDistanceData,
      primaryDirection,
      opposingDirection,
      endDateFormat
    )
  )
  series.push(
    generateCycleLabels(
      locationCenterDistanceData,
      primaryDirection,
      undefined,
      primaryPhaseData.map((p) => p.approachDescription),
      undefined,
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
      undefined,
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

  const tooltip = createTooltip()

  const chartOptions: EChartsOption = {
    title: title,
    xAxis: xAxis,
    yAxis: yAxis,
    grid: grid,
    dataZoom: dataZoom,
    legend: legends,
    toolbox: toolbox,
    animation: false,
    series: series,
    tooltip,
    displayProps,
  }

  return chartOptions
}
