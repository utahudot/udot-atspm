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
  compareTurningMovementDirections,
  getAvailableTurningMovementDirections,
  normalizeTurningMovementDirection,
} from './directions'
import {
  RawTurningMovementCountsData,
  RawTurningMovementCountsResponse,
} from './types'

export default function transformTurningMovementCountsData(
  response: RawTurningMovementCountsResponse
): TransformedChartResponse {
  const charts = response.data.charts
    .slice()
    .sort((a, b) => {
      const directionDiff = compareTurningMovementDirections(
        a.direction,
        b.direction
      )
      if (directionDiff !== 0) return directionDiff

      return compareMovementTypes(a.movementType, b.movementType)
    })
    .map((data) => ({
      chart: transformData(data),
    }))

  const directions = getAvailableTurningMovementDirections(
    response.data.table.map((row) => row.direction)
  )
  const preferred = ['Left', 'Thru-Left', 'Thru', 'Thru-Right', 'Right']

  const movementTypes = buildMovementTypeMap(
    response.data.table,
    preferred,
    directions
  )
  const labels = buildLabels(directions, movementTypes)

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
      table: response.data.table,
      charts,
      peakHour:
        response.data.peakHour && peakRow
          ? {
              peakHourFactor: response.data.peakHourFactor,
              peakHourData: [peakRow],
            }
          : null,
    },
  }
}

function transformData(data: RawTurningMovementCountsData): EChartsOption {
  const {
    lanes,
    plans,
    peakHour,
    peakHourFactor,
    peakHourVolume,
    laneUtilizationFactor,
  } = data
  const totalHourlyVolumes = data.totalHourlyVolumes ?? []

  const info = createInfoString(
    ['Total Volume: ', `${data.totalVolume.toLocaleString()}`],
    ['Peak Hour: ', peakHour ?? 'N/A'],
    ['Peak Hour Volume: ', peakHourVolume.toLocaleString() ?? 'N/A'],
    ['Peak Hour Factor: ', peakHourFactor?.toFixed(2) ?? 'N/A'],
    ['fLU: ', laneUtilizationFactor.toFixed(2)]
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

function formatTime(timestamp: string | Date) {
  return format(new Date(timestamp), 'HH:mm')
}

function compareMovementTypes(a: string, b: string) {
  const movementOrder = ['Left', 'Thru-Left', 'Thru', 'Thru-Right', 'Right']
  const orderA = movementOrder.indexOf(a)
  const orderB = movementOrder.indexOf(b)

  if (orderA !== orderB) {
    if (orderA === -1) return 1
    if (orderB === -1) return -1
    return orderA - orderB
  }

  return a.localeCompare(b)
}

function buildMovementTypeMap(
  table: RawTurningMovementCountsResponse['data']['table'],
  preferredOrder: string[],
  directions: string[]
) {
  const map: Record<string, string[]> = {}
  directions.forEach((dir) => {
    const set = new Set(
      table
        .filter((d) => normalizeTurningMovementDirection(d.direction) === dir)
        .map((d) => d.movementType)
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

function buildPeakHourRow(
  rawTable: RawTurningMovementCountsResponse['data']['table'],
  peakHour: { key: string; value: number } | null,
  directions: string[],
  movementTypes: Record<string, string[]>
): TableRow | null {
  if (!peakHour?.key) return null

  const valueAtPH = (dir: string, mt: string) =>
    rawTable.find(
      (r) =>
        normalizeTurningMovementDirection(r.direction) === dir &&
        r.movementType === mt
    )?.peakHourVolume?.value ?? 0

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
