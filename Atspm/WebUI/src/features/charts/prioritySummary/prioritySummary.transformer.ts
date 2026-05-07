// #region license
// Copyright 2025 Utah Departement of Transportation
// for WebUI - prioritySummary.transformer.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion

import { PrioritySummaryResult } from '@/api/reports'
import {
  createDataZoom,
  createDisplayProps,
  createGrid,
  createInfoString,
  createLegend,
  createSeries,
  createTitle,
  createToolbox,
  createTooltip,
  createXAxis,
  createYAxis,
  formatExportFileName,
} from '@/features/charts/common/transformers'
import { ChartType } from '@/features/charts/common/types'
import {
  RawPrioritySummaryResponse,
  TransformedPrioritySummaryResponse,
  toIconPoints,
} from '@/features/charts/prioritySummary/types'
import {
  Color,
  crossSvgSymbol,
  formatChartDateTimeRange,
  triangleSvgSymbol,
} from '@/features/charts/utils'
import { EChartsOption } from 'echarts'

const SYMBOL_SIZE = 8
const UNASSOCIATED_SYMBOL_SIZE = 8
const UNASSOCIATED_MARKER_STYLE = {
  color: Color.White,
  borderColor: Color.Black,
  borderWidth: 1,
  opacity: 1,
}

export default function transformPrioritySummaryData(
  response: RawPrioritySummaryResponse | PrioritySummaryResult
): TransformedPrioritySummaryResponse {
  const data = isRawPrioritySummaryResponse(response) ? response.data : response

  const charts = transformLocation(data)

  return {
    type: ChartType.PrioritySummary,
    data: { charts: charts.map((chart) => ({ chart })) },
  }
}

function isRawPrioritySummaryResponse(
  response: RawPrioritySummaryResponse | PrioritySummaryResult
): response is RawPrioritySummaryResponse {
  return 'data' in response
}

function durationToSeconds(duration: string): string {
  const [h, m, s] = duration.split(':')
  const seconds = Number(h) * 3600 + Number(m) * 60 + Number(s)

  return seconds.toFixed(1)
}

function transformLocation(data: PrioritySummaryResult) {
  const cycles = data.cycles || []
  const tspNumbers = Array.from(
    new Set(
      cycles
        .map((c) => c.tspNumber)
        .filter((tspNumber): tspNumber is number => tspNumber != null)
    )
  ).sort((a, b) => a - b)

  const combinedChart = transformTspChart(data, cycles)

  if (!tspNumbers.length) return [combinedChart]

  return [
    combinedChart,
    ...tspNumbers.map((tspNumber) =>
      transformTspChart(
        data,
        cycles.filter((c) => c.tspNumber === tspNumber),
        tspNumber
      )
    ),
  ]
}

function transformTspChart(
  data: PrioritySummaryResult,
  cycles: NonNullable<PrioritySummaryResult['cycles']>,
  tspNumber?: number
) {
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const info = createInfoString(
    ['Average duration:', `${durationToSeconds(data.averageDuration)}s`],
    ['Total check-ins:', `${data.numberCheckins}`],
    ['Total check-outs:', `${data.numberCheckouts}`],
    ['Total early greens:', `${data.numberEarlyGreens}`],
    ['Total extended greens:', `${data.numberExtendedGreens}`]
  )

  const title = createTitle({
    title:
      tspNumber == null
        ? 'Priority Summary'
        : `Priority Summary - TSP ${tspNumber}`,
    location: data.locationDescription,
    dateRange,
    info,
  })

  const xAxis = createXAxis(data.start, data.end)
  const yAxis = createYAxis(true, { name: 'Seconds Since Check-In' })

  const grid = createGrid({ top: 140, left: 70, right: 260 })

  const legend = createLegend({
    top: grid.top,
  })

  const dataZoom = createDataZoom()

  const toolbox = createToolbox(
    {
      title: formatExportFileName(
        `Priority Summary\n${data.locationDescription}`,
        data.start,
        data.end
      ),
      dateRange,
    },
    data.locationIdentifier,
    ChartType.PrioritySummary
  )

  const tooltip = createTooltip({ trigger: 'item' })

  const series = createSeries()

  const barWidthRequest = 5
  const barWidthService = 3

  const requestBar = cycles
    .filter((c) => c.requestEndOffsetSec != null && c.requestEndOffsetSec >= 0)
    .map((c) => {
      const inMs = Date.parse(c.checkIn)
      const outMs = Date.parse(c.checkOut)

      const windowStart = new Date(inMs - 125_000).toISOString()
      const windowEnd = new Date(outMs + 125_000).toISOString()

      return [
        c.checkIn,
        c.requestEndOffsetSec as number,
        c.tspNumber,
        c.checkOut,
        data.locationIdentifier,
        windowStart,
        windowEnd,
      ]
    })

  if (requestBar.length > 0) {
    series.push({
      name: 'TSP Request (112→115)',
      type: 'bar',
      data: requestBar,
      color: Color.Red,
      barWidth: barWidthRequest,
      barGap: '30%',
      encode: { x: 0, y: 1 },
      z: 2,
    })
  }

  const serviceCycles = cycles.filter(
    (c) =>
      c.serviceStartOffsetSec != null &&
      c.serviceEndOffsetSec != null &&
      c.serviceEndOffsetSec > c.serviceStartOffsetSec
  )

  const serviceOffsetData = serviceCycles.map((c) => [
    c.checkIn,
    c.serviceStartOffsetSec,
  ])

  const serviceDurationData = serviceCycles.map((c) => {
    const inMs = Date.parse(c.checkIn)
    const outMs = Number.isFinite(Date.parse(c.checkOut))
      ? Date.parse(c.checkOut)
      : inMs + 30_000

    const windowStart = new Date(inMs - 120_000).toISOString()
    const windowEnd = new Date(outMs + 120_000).toISOString()

    return [
      c.checkIn,
      c.serviceEndOffsetSec - c.serviceStartOffsetSec,
      c.tspNumber,
      c.serviceStart,
      c.serviceEnd,
      c.serviceStartOffsetSec,
      c.checkOut,
      data.locationIdentifier,
      windowStart,
      windowEnd,
    ]
  })

  if (serviceOffsetData.length > 0) {
    series.push({
      type: 'bar',
      data: serviceOffsetData,
      stack: 'service',
      barWidth: barWidthService,
      barGap: '30%',
      itemStyle: { color: 'transparent' },
      emphasis: { itemStyle: { color: 'transparent' } },
      tooltip: { show: false },
      encode: { x: 0, y: 1 },
      z: 2,
    })
  }

  if (serviceDurationData.length > 0) {
    series.push({
      name: 'TSP Service (118→119)',
      type: 'bar',
      data: serviceDurationData,
      stack: 'service',
      color: Color.LightBlue,
      barWidth: barWidthService,
      barGap: '30%',
      encode: { x: 0, y: 1 },
      z: 3,
    })
  }

  const earlyGreenPts = toIconPoints(cycles, 113)
  const extendGreenPts = toIconPoints(cycles, 114)
  const preemptForceOffPts = toIconPoints(cycles, 116)
  const tspEarlyForceOffPts = toIconPoints(cycles, 117)
  const unassignedEarlyGreenPts = toUnassignedPoints(
    data.unassigned?.earlyGreen
  )
  const unassignedExtendGreenPts = toUnassignedPoints(
    data.unassigned?.extendGreen
  )

  if (earlyGreenPts.length > 0) {
    series.push({
      name: 'Early Green (113)',
      type: 'scatter',
      data: earlyGreenPts,
      color: Color.Black,
      symbolSize: SYMBOL_SIZE,
      symbol: 'circle',
      z: 4,
    })
  }

  if (extendGreenPts.length > 0) {
    series.push({
      name: 'Extend Green (114)',
      type: 'scatter',
      data: extendGreenPts,
      color: Color.Black,
      symbolSize: SYMBOL_SIZE,
      symbol: triangleSvgSymbol,
      z: 4,
    })
  }

  if (preemptForceOffPts.length > 0) {
    series.push({
      name: 'Preempt Force Off (116)',
      type: 'scatter',
      data: preemptForceOffPts,
      color: Color.Red,
      symbolSize: SYMBOL_SIZE,
      symbol: crossSvgSymbol,
      z: 4,
    })
  }

  if (tspEarlyForceOffPts.length > 0) {
    series.push({
      name: 'TSP Early Force Off (117)',
      type: 'scatter',
      data: tspEarlyForceOffPts,
      color: Color.Red,
      symbolSize: SYMBOL_SIZE,
      symbol: crossSvgSymbol,
      z: 4,
    })
  }

  if (unassignedEarlyGreenPts.length > 0) {
    series.push({
      name: 'Unassociated Early Green (113)',
      type: 'scatter',
      data: unassignedEarlyGreenPts,
      symbolSize: UNASSOCIATED_SYMBOL_SIZE,
      symbol: 'circle',
      itemStyle: UNASSOCIATED_MARKER_STYLE,
      clip: false,
      z: 100000,
      zlevel: 1,
    })
  }

  if (unassignedExtendGreenPts.length > 0) {
    series.push({
      name: 'Unassociated Extend Green (114)',
      type: 'scatter',
      data: unassignedExtendGreenPts,
      symbolSize: UNASSOCIATED_SYMBOL_SIZE,
      symbol: triangleSvgSymbol,
      itemStyle: UNASSOCIATED_MARKER_STYLE,
      clip: false,
      z: 100000,
      zlevel: 1,
    })
  }

  const hintGraphic = buildLegendWidthHintGraphic(
    'Click a cycle to see more details'
  )

  const displayProps = createDisplayProps({
    description: 'Summary',
  })

  const chartOptions: EChartsOption = {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    dataZoom,
    toolbox,
    graphic: [hintGraphic],
    tooltip,
    series,
    displayProps,
  }

  return chartOptions
}

function toUnassignedPoints(
  timestamps?: string[] | null
): Array<[string, number]> {
  return (timestamps ?? [])
    .filter((timestamp) => Number.isFinite(Date.parse(timestamp)))
    .map((timestamp) => [timestamp, 0])
}

function buildLegendWidthHintGraphic(text: string) {
  const right = 0
  const bottom = 110
  const padding = [10, 12]

  const bg = '#d8e2f9'
  const radius = 6
  const width = 190

  const borderBlue = '#9ca3ba'

  const fontSize = 13
  const lineHeight = 16

  // --- left icon + divider layout ---
  const iconRadius = 10
  const iconCx = padding[1] + iconRadius
  const iconCy = padding[0] + iconRadius + 12

  const dividerX = iconCx + iconRadius + 10
  const textX = dividerX + 12

  const maxTextWidth = Math.max(0, width - textX - padding[1])

  const lines = 3
  const height = padding[0] * 2 + lines * lineHeight

  return {
    type: 'group',
    right,
    bottom,
    silent: true,
    z: 100,
    children: [
      {
        type: 'rect',
        shape: { x: 0, y: 0, width, height, r: radius },
        style: { fill: bg, lineWidth: 1 },
      },

      // circle
      {
        type: 'circle',
        shape: { cx: iconCx, cy: iconCy, r: iconRadius },
        style: { fill: 'transparent', stroke: '#3c3c41', lineWidth: 2 },
      },

      // exclamation inside circle
      {
        type: 'text',
        style: {
          x: iconCx,
          y: iconCy + 1,
          text: '!',
          fill: '#3c3c41',
          fontSize: 16,
          fontWeight: 800,
          align: 'center',
          verticalAlign: 'middle',
        },
      },

      // vertical divider line
      {
        type: 'line',
        shape: {
          x1: dividerX,
          y1: padding[0],
          x2: dividerX,
          y2: height - padding[0],
        },
        style: { stroke: borderBlue, lineWidth: 1 },
      },

      // message text
      {
        type: 'text',
        style: {
          x: textX,
          y: padding[0] + 8,
          text,
          fontSize,
          lineHeight,
          fill: '#3c3c41',
          width: maxTextWidth,
          overflow: 'break',
          textVerticalAlign: 'top',
        },
      },
    ],
  } as const
}
