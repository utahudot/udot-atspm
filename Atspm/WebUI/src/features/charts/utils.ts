// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - utils.ts
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
import type { DataZoomComponentOption, ECharts, SeriesOption } from 'echarts'
import { ChartType, MarkAreaData, PlanData } from './common/types'

export enum Color {
  Blue = '#0072B2',
  Green = '#009E73',
  Orange = '#E69F00',
  Yellow = '#F0E442',
  Pink = '#CC79A7',
  LightBlue = '#56B4E9',
  Red = '#D55E00',
  PlanA = 'rgba(0, 0, 0, 0)',
  PlanB = 'rgba(125, 125, 125, 0.1)',
  Grey = '#808080 ',
  Black = '#000000',
  White = '#FFFFFF',
  BrightRed = '#d73131',
}

export const dateFormat: Intl.DateTimeFormatOptions = {
  weekday: 'short',
  year: 'numeric',
  month: 'long',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  hour12: false,
}

export const hLineSvgSymbol = 'path://M0 288H640V352H0V288Z'

export const mySvgPath =
  'path://' +
  'M0 0 C151.47 0 302.94 0 459 0 C459 56.76 459 113.52 459 172 C307.53 172 156.06 172 0 172 C0 115.24 0 58.48 0 0 Z ' +
  'M171 98 C257.13 98 343.26 98 432 98 C432 122.42 432 146.84 432 172 C321.12 172 210.24 172 96 172 ' +
  'C96 171.67 96 171.34 96 171 C96.98 170.78 97.96 170.56 98.97 170.34 ' +
  'C123.49 164.66 143.91 153.67 158 132 ' +
  'C164.15 121.39 168.59 110.03 171 98 Z ' +
  'M95 0 C206.21 0 317.42 0 432 0 C432 24.42 432 48.84 432 74 ' +
  'C345.87 74 259.74 74 171 74 ' +
  'C169.68 68.72 168.36 63.44 167 58 ' +
  'C165.87 54.95 164.8 52.19 163.38 49.31 ' +
  'C163.04 48.61 162.7 47.92 162.35 47.2 ' +
  'C151.75 26.13 133.19 12.04 111.25 4.13 ' +
  'C105.96 2.53 100.46 1.79 95 1 Z ' +
  'M0 95 C0.33 95 0.66 95 1 95 ' +
  'C1.12 95.95 1.25 96.89 1.38 97.87 ' +
  'C4.69 120.19 16.03 140.11 33.94 153.94 ' +
  'C47.09 163.26 61.11 168.71 77 171 ' +
  'C77 171.33 77 171.66 77 172 ' +
  'C51.59 172 26.18 172 0 172 Z ' +
  'M0 0 C25.74 0 51.48 0 78 0 ' +
  'C78 0.33 78 0.66 78 1 ' +
  'C76.37 1.24 76.37 1.24 74.7 1.48 ' +
  'C65.53 2.9 57.38 4.83 49 9 ' +
  'C48.07 9.45 47.13 9.91 46.17 10.38 ' +
  'C39.45 13.83 33.68 18.02 28 23 ' +
  'C26.97 23.89 26.97 23.89 25.91 24.8 ' +
  'C10.6 38.96 3.26 58.8 1 79 ' +
  'C0.67 79 0.34 79 0 79 Z'

export const squiggleSvgSymbol =
  'path://M84.582,59.425c-2.673,0-3.359-3.843-4.311-9.162c-0.328-1.833-0.83-4.646-1.356-6.051  c-0.525,1.405-1.028,4.218-1.355,6.051c-0.95,5.318-1.637,9.161-4.309,9.161c-2.671,0-3.356-3.843-4.306-9.161  c-0.327-1.832-0.828-4.641-1.353-6.048c-0.525,1.407-1.027,4.216-1.354,6.048c-0.95,5.318-1.637,9.161-4.308,9.161  s-3.357-3.843-4.307-9.161c-0.327-1.832-0.829-4.641-1.354-6.048c-0.524,1.407-1.026,4.216-1.354,6.047  c-0.949,5.319-1.636,9.162-4.307,9.162c-2.671,0-3.357-3.843-4.307-9.161c-0.327-1.831-0.829-4.64-1.353-6.047  c-0.524,1.407-1.025,4.215-1.353,6.046c-0.949,5.319-1.635,9.162-4.306,9.162c-2.67,0-3.355-3.843-4.304-9.161  c-0.327-1.831-0.828-4.638-1.352-6.045c-0.524,1.407-1.025,4.215-1.352,6.045c-0.949,5.318-1.635,9.161-4.306,9.161  c-2.671,0-3.357-3.843-4.306-9.161c-0.327-1.832-0.829-4.64-1.353-6.047c-0.524,1.407-1.026,4.216-1.353,6.047  c-0.949,5.318-1.635,9.161-4.306,9.161s-3.357-3.843-4.307-9.161c-0.396-2.215-1.047-5.861-1.685-6.725  C10,43.388,9.5,42.79,9.5,42.075c0-0.829,0.671-1.5,1.5-1.5c2.671,0,3.357,3.843,4.307,9.161c0.327,1.832,0.829,4.641,1.354,6.048  c0.524-1.407,1.026-4.216,1.353-6.048c0.949-5.318,1.635-9.161,4.306-9.161s3.357,3.843,4.306,9.161  c0.327,1.831,0.829,4.64,1.353,6.047c0.524-1.407,1.025-4.216,1.353-6.047c0.949-5.318,1.635-9.161,4.306-9.161  c2.67,0,3.355,3.843,4.304,9.161c0.327,1.831,0.828,4.638,1.352,6.046c0.524-1.408,1.025-4.216,1.352-6.046  c0.949-5.318,1.635-9.161,4.305-9.161c2.671,0,3.357,3.843,4.307,9.161c0.327,1.832,0.829,4.642,1.354,6.048  c0.525-1.406,1.027-4.216,1.354-6.048c0.949-5.318,1.636-9.161,4.307-9.161s3.357,3.843,4.307,9.161  c0.327,1.832,0.829,4.642,1.354,6.049c0.525-1.406,1.027-4.217,1.354-6.049c0.95-5.318,1.637-9.161,4.308-9.161  s3.356,3.843,4.306,9.161c0.327,1.832,0.829,4.642,1.354,6.049c0.525-1.406,1.027-4.217,1.354-6.049  c0.95-5.318,1.637-9.161,4.309-9.161s3.358,3.842,4.31,9.16c0.328,1.834,0.831,4.65,1.357,6.054  c0.526-1.404,1.029-4.219,1.357-6.053c0.951-5.318,1.638-9.161,4.311-9.161c0.828,0,1.5,0.671,1.5,1.5c0,0.715-0.5,1.313-1.17,1.463  c-0.64,0.863-1.291,4.51-1.688,6.726C87.941,55.582,87.255,59.425,84.582,59.425z'

export const xSvgSymbol =
  'path://M640 320L512 192 320 384 128 192 0 320l192 192L0 704l128 128 192-192 192 192 128-128L448 512 640 320z'

export const crossSvgSymbol =
  'path://M256 0H384V256H640V384H384V640H256V384H0V256H256V0Z'

export const diamondSvgSymbol = 'path://M320 0L640 320L320 640L0 320Z'

export const triangleSvgSymbol =
  'path://M7.93189 1.24806C7.84228 1.09446 7.67783 1 7.5 1C7.32217 1 7.15772 1.09446 7.06811 1.24806L0.0681106 13.2481C-0.0220988 13.4027 -0.0227402 13.5938 0.0664289 13.749C0.155598 13.9043 0.320967 14 0.5 14H14.5C14.679 14 14.8444 13.9043 14.9336 13.749C15.0227 13.5938 15.0221 13.4027 14.9319 13.2481L7.93189 1.24806Z'

export const DashedLineSeriesSymbol =
  'path://M180 1000 l0 -80 200 0 200 0 0 80 0 80 -200 0 -200 0 0 -80z, M810 1000 l0 -80 200 0 200 0 0 80 0 80 -200 0 -200 0 0 -80zm, M1440 1000 l0 -80 200 0 200 0 0 80 0 80 -200 0 -200 0 0 -80z'

export const DottedLineSeriesSymbol =
  'path://M335 1316 c-63 -28 -125 -122 -125 -191 0 -71 62 -164 127 -192 18 -7 58 -13 90 -13 72 0 125 28 168 88 27 39 30 52 30 117 0 65 -3 78 -30 117 -43 61 -95 88 -170 87 -33 0 -73 -6 -90 -13z, M1035 1313 c-76 -40 -115 -103 -115 -188 0 -121 85 -205 205 -205 121 0 205 84 205 205 0 84 -39 148 -112 186 -46 24 -140 25 -183 2z, M1714 1298 c-61 -42 -94 -102 -94 -173 0 -71 33 -131 94 -172 41 -28 57 -33 107 -33 76 0 115 16 161 68 76 84 76 190 0 274 -46 52 -85 68 -161 68 -50 0 -66 -5 -107 -32z'

export const SolidLineSeriesSymbol =
  'path://M180 1000 l0 -20 200 0 200 0 0 20 0 20 -200 0 -200 0 0 -20z'

export const dateTimeFormat: Intl.DateTimeFormatOptions = {
  weekday: 'short',
  year: 'numeric',
  month: 'long',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  second: '2-digit',
  hour12: false,
}

export function formatChartDateTimeRange(startDate: string, endDate: string) {
  const start = new Date(startDate)
  const end = new Date(endDate)

  if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) {
    return `${startDate} - ${endDate}`
  }

  const sameDay =
    start.getFullYear() === end.getFullYear() &&
    start.getMonth() === end.getMonth() &&
    start.getDate() === end.getDate()

  const dayLabel = start.toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })

  const timeFmt: Intl.DateTimeFormatOptions = {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  }

  const startTime = start.toLocaleTimeString('en-US', timeFmt)
  const endTime = end.toLocaleTimeString('en-US', timeFmt)

  if (sameDay) {
    return `${dayLabel} • ${startTime}–${endTime}`
  }

  const startFull = start.toLocaleString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    ...timeFmt,
  })
  const endFull = end.toLocaleString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    ...timeFmt,
  })

  return `${startFull} - ${endFull}`
}

export function adjustPlanPositions(chart: ECharts) {
  const options = chart.getOption()

  if (!Array.isArray(options.dataZoom) || !Array.isArray(options.series)) return

  const viewRange = extractViewRange(options.dataZoom[0])

  const planSeries = options.series[options.series.length - 1]

  if (
    !planSeries.markArea ||
    !planSeries.markArea.data.length ||
    !planSeries.data
  )
    return

  const planData: PlanData[] = planSeries.data

  planData.forEach((_, i) => {
    const markData = planSeries.markArea.data[i]
    adjustMarkDataForViewRange(markData, viewRange, planData[i])
  })

  chart.setOption(options)
}

function extractViewRange(dataZoom: DataZoomComponentOption): {
  start: number
  end: number
} {
  return {
    start: new Date(dataZoom.startValue as string).getTime(),
    end: new Date(dataZoom.endValue as string).getTime(),
  }
}

// const determineDecimals = (binWidth: number) => {
//   if (binWidth > 40) return 2
//   if (binWidth > 25) return 1
//   return null
// }

function adjustMarkDataForViewRange(
  markData: MarkAreaData,
  viewRange: { start: number; end: number },
  planData: PlanData
) {
  const planStart = new Date(markData[0].xAxis).getTime()
  const planEnd = new Date(markData[1].xAxis).getTime()

  const adjustedStart = Math.max(planStart, viewRange.start)

  const adjustedEnd = Math.min(planEnd, viewRange.end)

  planData[0] = new Date((adjustedStart + adjustedEnd) / 2).toISOString()
}

export function handleGreenTimeUtilizationDataZoom(chart: ECharts) {
  const options = chart.getOption()
  const startValue = options.dataZoom[0].startValue
  const pixelPositionStart = chart.convertToPixel({ xAxisIndex: 0 }, startValue)
  const pixelPositionNext = chart.convertToPixel(
    { xAxisIndex: 0 },
    startValue + 1
  )
  const binPixelWidth = pixelPositionNext - pixelPositionStart
  const heatmapIndex = (options.series as SeriesOption[]).findIndex(
    (s) => s.type === 'heatmap'
  )
  const newMaxValue = determineYAxisMax(options, heatmapIndex, 3)
  const labelFormatter = computeLabelFormatter(binPixelWidth)
  const seriesUpdateTemplate = (options.series as SeriesOption[]).map(
    () => ({})
  )
  seriesUpdateTemplate[heatmapIndex] = {
    label: {
      formatter: labelFormatter,
    },
  }
  chart.setOption({
    yAxis: [{ max: newMaxValue }],
    series: seriesUpdateTemplate,
  })
}

function determineYAxisMax(options, heatmapIndex, binSize: number) {
  const heatmapData = (options.series[heatmapIndex] as any).data
  const maxHeatmapValue = Math.max(...heatmapData.map((item: any) => item[2]))

  // Get max index from heatmap Y-values (which correspond to categories)
  const maxHeatmapIndex = Math.max(...heatmapData.map((item: any) => item[1]))
  const maxCategoryValue = (maxHeatmapIndex + 1) * binSize

  const currentYAxisMax = options.yAxis[0]?.max ?? 0

  const calculatedMax = Math.max(maxHeatmapValue, maxCategoryValue)

  return calculatedMax > currentYAxisMax ? calculatedMax : currentYAxisMax
}
function computeLabelFormatter(binPixelWidth) {
  const determineDecimals = (binWidth: number) => {
    if (binWidth > 40) return 2
    if (binWidth > 25) return 1
    return null
  }

  const decimals = determineDecimals(binPixelWidth)

  return (params) => {
    const value = params.value[2]
    if (decimals !== null) {
      return parseFloat(value).toFixed(decimals)
    } else {
      return ' '
    }
  }
}

export const getDisplayNameFromChartType = (chartType: ChartType) => {
  // Split the chartType string into words, assuming PascalCase or camelCase formatting
  const words = chartType.replace(/([A-Z])/g, ' $1').trim()

  // Capitalize the first letter of each word and join them with spaces
  const displayName = words
    .split(' ')
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ')

  return displayName
}

export const lightenColor = (color: string, percent: number) => {
  const num = parseInt(color.slice(1), 16),
    amt = Math.round(2.55 * percent),
    R = (num >> 16) + amt,
    G = ((num >> 8) & 0x00ff) + amt,
    B = (num & 0x0000ff) + amt

  return `#${(
    0x1000000 +
    (R < 255 ? (R < 1 ? 0 : R) : 255) * 0x10000 +
    (G < 255 ? (G < 1 ? 0 : G) : 255) * 0x100 +
    (B < 255 ? (B < 1 ? 0 : B) : 255)
  )
    .toString(16)
    .slice(1)
    .toUpperCase()}`
}
