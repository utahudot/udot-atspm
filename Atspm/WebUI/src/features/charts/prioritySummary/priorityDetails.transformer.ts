// #region license
// Copyright 2025 Utah Departement of Transportation
// for WebUI - priorityDetails.transformer.ts
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

import {
  DetectorEventBase,
  DetectorEventDto,
  IndianaEvent,
  PriorityDetailsResult,
} from '@/api/reports'
import {
  createDisplayProps,
  createGrid,
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
import { buildPriorityOverlay } from '@/features/charts/prioritySummary/priorityDetails.priorityOverlay'
import { TransformedChartResponse } from '@/features/charts/types'
import {
  Color,
  formatChartDateTimeRange,
  hLineSvgSymbol,
  triangleSvgSymbol,
} from '@/features/charts/utils'
import {
  CustomSeriesRenderItemAPI,
  CustomSeriesRenderItemParams,
  graphic,
  type EChartsOption,
  type SeriesOption,
} from 'echarts'

type CycleIndication = {
  name: string
  codes: number[]
  color: string
}

const TSP_CODES = {
  EarlyGreen: 113,
  ExtendGreen: 114,
} as const

const CYCLE_INDICATIONS = [
  {
    name: 'Phase Begin Green (1)\nOverlap Begin Green (61)',
    codes: [1, 61],
    color: Color.Green,
  },
  {
    name: 'Phase Min Complete (3)\nOverlap Begin Trailing Green (Extension) (62)',
    codes: [3, 62],
    color: '#8ef08d',
  },
  {
    name: 'Phase Begin Yellow Clearance (8)\nBegin Overlap Yellow (63)',
    codes: [8, 63],
    color: Color.Yellow,
  },
  {
    name: 'Phase End Yellow Clearance (9)\nOverlap Begin Red Clearance (64)',
    codes: [9, 64],
    color: '#FF0000',
  },
  {
    name: 'Phase End Red Clearance (11)\nOverlap Off (Inactive with Red Indication) (65)',
    codes: [11, 65],
    color: '#f0807f',
  },
] as const satisfies readonly CycleIndication[]

type IndicationName = (typeof CYCLE_INDICATIONS)[number]['name']

function getIndicationDetails(
  value: number
): { name: IndicationName; color: string } | null {
  const found = CYCLE_INDICATIONS.find((x) => x.codes.includes(value))
  return found
    ? { name: found.name as IndicationName, color: found.color }
    : null
}

export interface RawPriorityDetailsResponse {
  data: PriorityDetailsResult[]
}

export default function transformPriorityDetailsData(
  response: RawPriorityDetailsResponse
): TransformedChartResponse {
  const rows = response ?? []

  const chart = transformCyclesOnly(rows)

  return {
    type: ChartType.PriorityDetails,
    data: {
      charts: [{ chart }],
    },
  }
}

function transformCyclesOnly(rows: PriorityDetailsResult[]): EChartsOption {
  const first = rows[0]
  const locationDescription = first?.locationDescription ?? ''
  const locationIdentifier = first?.locationIdentifier ?? ''

  const { chartStartIso, chartEndIso } = getChartRangeMs(rows)

  const dateRange = formatChartDateTimeRange(chartStartIso, chartEndIso)

  const title = createTitle({
    title: 'TSP Events with all Phase Indications',
    location: locationDescription,
    dateRange,
  })

  const xAxis = {
    type: 'time',
    show: false,
    min: chartStartIso,
    max: chartEndIso,
  }

  const xAxisBottom = createXAxis(chartStartIso, chartEndIso)
  const categories = buildRowCategories(rows)

  const {
    gridTop,
    yAxisTop,
    series: prioritySeries,
    legendItems,
  } = buildPriorityOverlay(rows)

  yAxisTop.gridIndex = 0

  const yAxisBottom = createYAxis(false, {
    type: 'category',
    name: 'Phase Number',
    gridIndex: 1,
    boundaryGap: true,
    axisPointer: {
      show: true,
      type: 'shadow',
      triggerTooltip: false,
    },
    splitLine: {
      show: true,
      lineStyle: { color: '#000000' },
    },
    data: categories,
  })

  const gridBottom = createGrid({
    top: 230,
    left: 65,
    right: 330,
  })

  const dataZoom: EChartsOption['dataZoom'] = [
    {
      type: 'inside',
      show: true,
      filterMode: 'weakFilter',
      minSpan: 0.2,
    },
    {
      type: 'slider',
      show: true,
      filterMode: 'weakFilter',
      minSpan: 0.2,
      xAxisIndex: [0, 1],
    },
  ]

  const toolbox = createToolbox(
    {
      title: formatExportFileName(
        `Priority Details\n${locationDescription}`,
        chartStartIso,
        chartEndIso
      ),
      dateRange,
    },
    locationIdentifier,
    ChartType.PriorityDetails
  )

  const tooltip = createTooltip({ trigger: 'item', confine: true })

  const series = createSeries()

  /* For some reason the datazoom will act funny if the first 
  scatter series has a single value. This is just to prevent that.*/
  series.push({
    name: '__extent__',
    type: 'scatter',
    xAxisIndex: 0,
    yAxisIndex: 0,
    data: [
      [chartStartIso, 0],
      [chartEndIso, 0],
    ],
    symbolSize: 0,
    itemStyle: { opacity: 0 },
    tooltip: { show: false },
    silent: true,
    z: -1,
  })
  series.push(...prioritySeries)
  series.push(...buildCycleTimelineSeries(rows, categories))
  series.push(...buildCycleEventMarkersOnCyclesSeries(rows, categories))
  series.push(...buildDetectorOnOffOnCyclesSeries(rows, categories))

  const legend = createLegend({
    top: gridTop.top,
    data: [
      ...legendItems,
      ...CYCLE_INDICATIONS.map((x) => ({
        name: x.name,
        itemStyle: { color: x.color },
      })),
      {
        name: 'Detection Event',
        icon: 'image:///DetectionEvent.svg',
      },
    ],
  })

  const displayProps = createDisplayProps({
    description: 'Priority Details',
  })

  const chartOptions: EChartsOption = {
    title,
    grid: [gridTop, gridBottom],
    xAxis: [
      { ...xAxis, gridIndex: 0 },
      { ...xAxisBottom, gridIndex: 1 },
    ],
    yAxis: [
      { ...yAxisTop[0], gridIndex: 0 },
      { ...yAxisBottom[0], gridIndex: 1 },
    ],
    legend,
    // dataZoom,
    toolbox,
    tooltip,
    animation: false,
    series,
    displayProps,
  }

  return chartOptions
}

function buildRowCategories(rows: PriorityDetailsResult[]): string[] {
  const out: string[] = []
  const seen = new Set<string>()

  for (const r of rows) {
    const name = categoryOfRow(r)
    if (seen.has(name)) continue
    seen.add(name)
    out.push(name)
  }

  return out
}

function categoryOfRow(r: PriorityDetailsResult): string {
  return r.isPhaseOverLap ? `O${r.phaseNumber}` : `${r.phaseNumber}`
}

function getChartRangeMs(rows: PriorityDetailsResult[]) {
  if (!rows.length) {
    return {
      chartStartMs: 0,
      chartEndMs: 0,
      chartStartIso: '',
      chartEndIso: '',
    }
  }

  let min = Date.parse(rows[0].start)
  let max = Date.parse(rows[0].end)

  for (const r of rows) {
    const s = Date.parse(r.start)
    const e = Date.parse(r.end)
    if (!Number.isNaN(s) && s < min) min = s
    if (!Number.isNaN(e) && e > max) max = e
  }

  return {
    chartStartMs: min,
    chartEndMs: max,
    chartStartIso: new Date(min).toISOString(),
    chartEndIso: new Date(max).toISOString(),
  }
}

function buildCycleTimelineSeries(
  rows: PriorityDetailsResult[],
  categories: string[]
): SeriesOption[] {
  const indexByCategory = new Map<string, number>()
  categories.forEach((c, i) => indexByCategory.set(c, i))

  const buckets = new Map<
    IndicationName,
    Array<{
      name: string
      value: [number, number, number, number]
      itemStyle: { color: string }
    }>
  >()

  for (const ind of CYCLE_INDICATIONS) {
    buckets.set(ind.name as IndicationName, [])
  }

  for (const row of rows) {
    const yCat = categoryOfRow(row)
    const rowIndex = indexByCategory.get(yCat)
    if (rowIndex === undefined) continue

    const cycles = row.cycleEvents ?? []
    const rowEndMs = Date.parse(row.end)

    for (let i = 0; i < cycles.length; i++) {
      const curr = cycles[i]
      const next = i < cycles.length - 1 ? cycles[i + 1] : null

      const details = getIndicationDetails(curr.value)
      if (!details) continue

      const startMs = Date.parse(curr.start)
      const endMs = next ? Date.parse(next.start) : rowEndMs
      if (!Number.isFinite(startMs) || !Number.isFinite(endMs)) continue
      if (endMs <= startMs) continue

      const arr = buckets.get(details.name)
      if (!arr) continue

      arr.push({
        name: details.name,
        value: [rowIndex, startMs, endMs, endMs - startMs],
        itemStyle: { color: details.color },
      })
    }
  }

  return CYCLE_INDICATIONS.map((ind) => {
    const name = ind.name as IndicationName
    return {
      name,
      type: 'custom',
      xAxisIndex: 1,
      yAxisIndex: 1,
      renderItem: renderCycleRect,
      tooltip: { show: false },
      encode: { x: [1, 2], y: 0 },
      data: buckets.get(name) ?? [],
      itemStyle: { color: ind.color },
      z: 1,
    } as SeriesOption
  })
}

function renderCycleRect(
  params: CustomSeriesRenderItemParams,
  api: CustomSeriesRenderItemAPI
) {
  const categoryIndex = api.value(0)
  const start = api.coord([api.value(1), categoryIndex])
  const end = api.coord([api.value(2), categoryIndex])

  const height = api.size([0, 1])[1] * 0.6

  const rectShape = graphic.clipRectByRect(
    {
      x: start[0],
      y: start[1] - height / 2,
      width: end[0] - start[0],
      height,
    },
    {
      x: params.coordSys.x,
      y: params.coordSys.y,
      width: params.coordSys.width,
      height: params.coordSys.height,
    }
  )

  return (
    rectShape && {
      type: 'rect',
      transition: ['shape'],
      shape: rectShape,
      style: api.style(),
    }
  )
}

function buildCycleEventMarkersOnCyclesSeries(
  rows: PriorityDetailsResult[],
  categories: string[]
): SeriesOption[] {
  const indexByCategory = new Map<string, number>()
  categories.forEach((c, i) => indexByCategory.set(c, i))

  const earlyGreens: Array<[string, number]> = []
  const extendGreens: Array<[string, number]> = []

  // Dedupe (same event can appear on multiple rows depending on payload shape)
  const seen = new Set<string>()

  for (const row of rows ?? []) {
    const rowIndex = indexByCategory.get(categoryOfRow(row))
    if (rowIndex == null) continue

    const tspEvents = (row.tspEvents ?? []) as IndianaEvent[]
    if (!tspEvents.length) continue

    for (const e of tspEvents) {
      if (
        e.eventCode !== TSP_CODES.EarlyGreen &&
        e.eventCode !== TSP_CODES.ExtendGreen
      ) {
        continue
      }

      const tMs = Date.parse(e.timestamp)
      if (!Number.isFinite(tMs)) continue

      if (tMs < Date.parse(row.start) || tMs > Date.parse(row.end)) continue

      const k = `${e.eventCode}|${rowIndex}|${e.timestamp}`
      if (seen.has(k)) continue
      seen.add(k)

      if (e.eventCode === TSP_CODES.EarlyGreen)
        earlyGreens.push([e.timestamp, rowIndex])
      else extendGreens.push([e.timestamp, rowIndex])
    }
  }

  const common = {
    type: 'scatter' as const,
    xAxisIndex: 1,
    yAxisIndex: 1,
    symbolSize: 9,
    itemStyle: { color: Color.Black },
    z: 6,
    tooltip: {
      show: true,
      formatter: (p: any) => {
        const t = p?.value?.[0]
        const phase = categories[p?.value?.[1]] ?? ''
        return `${p.seriesName}<br/>${phase}<br/>${t ?? ''}`
      },
    },
  }

  const out: SeriesOption[] = []

  if (earlyGreens.length) {
    out.push({
      ...common,
      name: 'Early Green (113)',
      symbol: 'circle',
      data: earlyGreens,
    })
  }

  if (extendGreens.length) {
    out.push({
      ...common,
      name: 'Extend Green (114)',
      symbol: triangleSvgSymbol,
      data: extendGreens,
    })
  }

  return out
}

type BasicDetectors = {
  name: string
  events: DetectorEventBase[]
}

function buildDetectorOnOffOnCyclesSeries(
  rows: PriorityDetailsResult[],
  categories: string[]
): SeriesOption[] {
  const indexByCategory = new Map<string, number>()
  categories.forEach((c, i) => indexByCategory.set(c, i))

  type seriesDataPoint = {
    value: [string, number]
    detectorName: string | undefined | null
    on: string | null
    off: string | null | undefined
  }

  const onSeriesData: seriesDataPoint[] = []
  const offSeriesData: seriesDataPoint[] = []
  const lineSeriesData: seriesDataPoint[][] = []

  for (const row of rows ?? []) {
    const rowLabel = categoryOfRow(row)
    const rowIndex = indexByCategory.get(rowLabel)
    if (rowIndex == null) continue

    const detectors = row.priorityAndPreemptionEvents as
      | DetectorEventDto[]
      | undefined
    if (!detectors?.length) continue

    const basic = detectors.map((d) => ({
      name: d.name,
      events: d.events,
    }))

    basic.forEach((detector) => {
      detector?.events?.forEach((event) => {
        if (!event.detectorOn) return

        onSeriesData.push({
          value: [event.detectorOn, rowIndex],
          detectorName: detector.name,
          on: event.detectorOn,
          off: event.detectorOff,
        })

        if (!event.detectorOff) return

        offSeriesData.push({
          value: [event.detectorOff, rowIndex],
          detectorName: detector.name,
          on: event.detectorOn,
          off: event.detectorOff,
        })

        // Only connect when both exist
        if (event.detectorOn && event.detectorOff) {
          lineSeriesData.push([
            {
              coord: [event.detectorOn, rowIndex],
              detectorName: detector.name,
              on: event.detectorOn,
              off: event.detectorOff,
            },
            {
              coord: [event.detectorOff, rowIndex],
              detectorName: detector.name,
              on: event.detectorOn,
              off: event.detectorOff,
            },
          ])
        }
      })
    })
  }

  const formatTooltip = (p: any) => {
    const d = p?.data
    if (!d) return ''
    const phase = categories[d.value?.[1]] ?? ''
    return [
      `<b>${d.detectorName}</b>`,
      phase,
      `On: ${d.on ?? ''}`,
      `Off: ${d.off ?? ''}`,
    ].join('<br/>')
  }

  const seriesName = 'Detection Event'

  return [
    {
      name: seriesName,
      type: 'scatter',
      xAxisIndex: 1,
      yAxisIndex: 1,
      tooltip: { confine: true, formatter: formatTooltip },
      large: true,
      symbol: 'circle',
      symbolSize: 6,
      symbolOffset: [0, 0.5],
      itemStyle: { color: 'black' },
      z: 8,
      data: onSeriesData,
    },
    {
      name: seriesName,
      type: 'scatter',
      xAxisIndex: 1,
      yAxisIndex: 1,
      large: true,
      tooltip: { confine: true, formatter: formatTooltip },
      symbol: hLineSvgSymbol,
      symbolSize: [2, 8],
      symbolOffset: [0, 0.5],
      itemStyle: { color: Color.Black },
      z: 8,
      data: offSeriesData,
    },
    {
      name: seriesName,
      type: 'lines',
      xAxisIndex: 1,
      yAxisIndex: 1,
      large: true,
      coordinateSystem: 'cartesian2d',
      lineStyle: {
        color: 'black',
        width: 2,
      },
      z: 7,
      tooltip: {},
      data: lineSeriesData,
    },
  ]
}
