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
  createGrid,
  createInfoString,
  createLegend,
  createPlans,
  createSeries,
  createTitle,
  createToolbox,
  createTooltip,
  createXAxis,
  createYAxis,
  formatExportFileName,
  transformSeriesData,
} from '@/features/charts/common/transformers'
import { ChartType } from '@/features/charts/common/types'
import {
  ColumnGroup,
  Labels,
  TableRow,
  TransformedChartResponse,
} from '@/features/charts/types'
import {
  Color,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import { addHours, format } from 'date-fns'
import { EChartsOption, SeriesOption } from 'echarts'
import {
  RawTurningMovementCountsData,
  RawTurningMovementCountsResponse,
} from './types'

export default function transformTurningMovementCountsData(
  response: RawTurningMovementCountsResponse
): TransformedChartResponse {
  const charts = response.data.charts.map((data) => {
    const chartOptions = transformData(
      data,
      response.data.peakHour,
      response.data.peakHourFactor
    )
    return {
      chart: chartOptions,
    }
  })

  charts.sort((a, b) => {
    const directionOrder = ['North', 'South', 'East', 'West']
    const movementOrder = ['Left', 'Thru', 'Right']

    const titleA = a.chart.displayProps.description
    const titleB = b.chart.displayProps.description

    const directionA = directionOrder.find((dir) => titleA.includes(dir)) || ''
    const directionB = directionOrder.find((dir) => titleB.includes(dir)) || ''

    const movementA = movementOrder.find((mov) => titleA.includes(mov)) || ''
    const movementB = movementOrder.find((mov) => titleB.includes(mov)) || ''

    const directionDiff =
      directionOrder.indexOf(directionA) - directionOrder.indexOf(directionB)
    if (directionDiff !== 0) return directionDiff

    return movementOrder.indexOf(movementA) - movementOrder.indexOf(movementB)
  })

  const directions = ['Eastbound', 'Westbound', 'Northbound', 'Southbound']
  const preferred = ['Left', 'Thru', 'Thru-Right', 'Right']

  const movementTypes = buildMovementTypeMap(
    response.data.table,
    preferred,
    directions
  )
  const labels = buildLabels(directions, movementTypes)
  const table = buildMatrix(response.data.table, directions, movementTypes)

  const peakRow = buildPeakHourRow(
    response.data.table,
    response.data.peakHour,
    directions,
    movementTypes
  )

  return {
    type: ChartType.TurningMovementCounts,
    data: {
      labels,
      table,
      charts,
      peakHour: response.data.peakHour
        ? {
            peakHourFactor: response.data.peakHourFactor,
            peakHourData: [peakRow],
          }
        : null,
    },
  }
}

function transformData(
  data: RawTurningMovementCountsData,
  peakHour: { key: string; value: number } | null,
  peakHourFactor: number | null
): EChartsOption {
  const { lanes, plans, totalHourlyVolumes } = data

  const info = createInfoString(
    ['Total Volume: ', `${data.totalVolume.toLocaleString()}`],
    [
      'Peak Hour: ',
      peakHour
        ? `${formatTime(peakHour.key)} - ${formatTime(addHours(new Date(peakHour.key), 1))}`
        : 'N/A',
    ],
    ['Peak Hour Volume: ', peakHour ? peakHour.value.toLocaleString() : 'N/A'],
    ['Peak Hour Factor: ', peakHourFactor?.toFixed(2) || 'N/A'],
    ['fLU: ', data.laneUtilizationFactor.toFixed(2)]
  )

  const titleHeader = `Turning Movement Counts\n${data.locationDescription} - ${data.direction} ${data.movementType} - ${data.laneType}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: info,
  })

  const xAxis = createXAxis(data.start, data.end)
  const yAxis = createYAxis(true, { name: 'Volume Per Hour' })

  const grid = createGrid({
    top: 180,
    left: 70,
    right: 180,
  })

  const legendData = [] as { name: string; icon: string }[]

  lanes.forEach((lane) => {
    legendData.push({
      name: `Lane ${lane.laneNumber}`,
      icon: SolidLineSeriesSymbol,
    })
  })

  const legend = createLegend({
    data: [
      { name: 'Total Volume', icon: SolidLineSeriesSymbol },
      ...legendData,
    ],
  })

  const dataZoom = createDataZoom([
    {
      type: 'slider',
      orient: 'vertical',
      right: 140,
      yAxisIndex: 0,
    },
  ])

  const toolbox = createToolbox(
    {
      title: formatExportFileName(titleHeader, data.start, data.end),
      dateRange,
    },
    data.locationIdentifier,
    ChartType.TurningMovementCounts
  )

  const tooltip = createTooltip()

  const colorValues = Object.values(Color)

  const series: SeriesOption[] = []

  if (lanes.length > 1) {
    series.push(
      ...createSeries({
        name: `Total Volume`,
        data: transformSeriesData(totalHourlyVolumes),
        type: 'line',
        color: Color.Red,
        tooltip: {
          valueFormatter: (val) => `${Math.round(val as number)} vph`,
        },
      })
    )
  }

  lanes.forEach((lane, i) => {
    series.push(
      ...createSeries({
        name: `Lane ${lane.laneNumber}`,
        data: transformSeriesData(lane.volume),
        type: 'line',
        color: colorValues[i % colorValues.length],
        tooltip: {
          valueFormatter: (val) => `${Math.round(val as number)} vph`,
        },
      })
    )
  })

  const plansSeries = createPlans(plans, yAxis.length)

  const displayProps = createDisplayProps({
    description: `${data.direction}${data.movementType}`,
  })

  const chartOptions: EChartsOption = {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    dataZoom,
    toolbox,
    tooltip,
    series: [...series, plansSeries],
    displayProps,
  }

  return chartOptions
}

function formatTime(timestamp: string) {
  return format(new Date(timestamp), 'HH:mm')
}

function buildMovementTypeMap(
  table: RawTurningMovementCountsResponse['data']['table'],
  preferredOrder: string[],
  directions: string[]
) {
  const map: Record<string, string[]> = {}
  directions.forEach((dir) => {
    const set = new Set(
      table.filter((d) => d.direction === dir).map((d) => d.movementType)
    )
    const arr = Array.from(set).sort((a, b) => {
      const ia = preferredOrder.indexOf(a)
      const ib = preferredOrder.indexOf(b)
      if (ia === -1 || ib === -1) return a.localeCompare(b)
      return ia - ib
    })
    map[dir] = arr
  })
  return map
}

function buildLabels(
  directions: string[],
  movementTypes: Record<string, string[]>
): Labels {
  const columnGroups: ColumnGroup[] = [{ title: null, columns: ['Hour'] }]

  directions.forEach((dir) => {
    columnGroups.push({
      title: dir,
      columns: [...movementTypes[dir], 'Total'],
    })
  })

  columnGroups.push({ title: null, columns: ['Bin Total'] })

  const flatColumns = columnGroups.flatMap((g) => g.columns)
  return { columnGroups, flatColumns }
}

function buildMatrix(
  table: RawTurningMovementCountsResponse['data']['table'],
  directions: string[],
  movementTypes: Record<string, string[]>
): TableRow[] {
  const timestamps = Array.from(
    new Set(table.flatMap((d) => d.volumes.map((v) => v.timestamp)))
  ).sort((a, b) => a.localeCompare(b))

  const valueAt = (dir: string, mt: string, ts: string) =>
    table
      .filter((r) => r.direction === dir && r.movementType === mt)
      .flatMap((r) => r.volumes)
      .find((v) => v.timestamp === ts)?.value ?? 0

  const rows: TableRow[] = timestamps.map((ts) => {
    const row: TableRow = [formatTime(ts)]
    let binTotal = 0

    directions.forEach((dir) => {
      let dirSum = 0
      movementTypes[dir].forEach((mt) => {
        const v = valueAt(dir, mt, ts)
        row.push(v)
        dirSum += v
      })
      row.push(dirSum)
      binTotal += dirSum
    })

    row.push(binTotal)
    return row
  })

  /* footer (grand totals) */
  const footer: TableRow = ['Total']
  let grand = 0

  directions.forEach((dir) => {
    let dirSum = 0
    movementTypes[dir].forEach((mt) => {
      const tot = timestamps.reduce((s, ts) => s + valueAt(dir, mt, ts), 0)
      footer.push(tot)
      dirSum += tot
    })
    footer.push(dirSum)
    grand += dirSum
  })

  footer.push(grand)
  rows.push(footer)
  return rows
}

function buildPeakHourRow(
  rawTable: RawTurningMovementCountsResponse['data']['table'],
  peakHour: { key: string; value: number } | null,
  directions: string[],
  movementTypes: Record<string, string[]>
): TableRow | null {
  if (!peakHour?.key) return null

  const valueAtPH = (dir: string, mt: string) =>
    rawTable.find((r) => r.direction === dir && r.movementType === mt)
      ?.peakHourVolume?.value ?? 0

  const start = new Date(peakHour.key)
  const desc = `${formatTime(start)} – ${formatTime(addHours(start, 1))}`

  const row: TableRow = [desc]
  let binTotal = 0

  directions.forEach((dir) => {
    let dirSum = 0
    movementTypes[dir].forEach((mt) => {
      const v = valueAtPH(dir, mt)
      row.push(v)
      dirSum += v
    })
    row.push(dirSum)
    binTotal += dirSum
  })

  row.push(binTotal)
  return row
}
