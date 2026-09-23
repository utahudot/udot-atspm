// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - transformers.test.ts
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
import { ChartType } from '@/features/charts/common/types'
import type { TransformedTurningMovementCountsResponse } from '../types'
import transformTurningMovementCountsData from './turningMovementCounts.transformer'
import {
  RawTurningMovementCountsData,
  RawTurningMovementCountsResponse,
} from './types'

type ChartWithDisplayProps = {
  displayProps: {
    description: string
  }
}

type ChartWithTitle = {
  title?: { text?: string }[]
}

const buildChart = (
  movementType: string,
  overrides: Partial<RawTurningMovementCountsData> = {}
): RawTurningMovementCountsData => ({
  locationIdentifier: '1001',
  locationDescription: 'Main St & 100 S',
  start: '2026-04-01T08:00:00',
  end: '2026-04-01T09:00:00',
  direction: 'Northbound',
  laneType: 'Vehicle',
  movementType,
  plans: [],
  lanes: [
    {
      laneNumber: 1,
      movementType,
      volume: [{ timestamp: '2026-04-01T08:00:00', value: 12 }],
      laneType: 1,
    },
  ],
  totalHourlyVolumes: [{ timestamp: '2026-04-01T08:00:00', value: 12 }],
  totalVolume: 12,
  peakHour: '08:00 - 09:00',
  peakHourVolume: 12,
  peakHourFactor: 1,
  laneUtilizationFactor: 1,
  ...overrides,
})

describe('transformTurningMovementCountsData', () => {
  it('sorts combined movements correctly and builds labels and peak hour rows', () => {
    const response: RawTurningMovementCountsResponse = {
      type: ChartType.TurningMovementCounts,
      data: {
        charts: [
          buildChart('Right'),
          buildChart('Thru + Thru-Right'),
          buildChart('Thru'),
        ],
        table: [
          {
            direction: 'Northbound',
            movementType: 'Right',
            laneType: 'Vehicle',
            volume: [],
            peakHourVolume: { value: 30 },
          },
          {
            direction: 'Northbound',
            movementType: 'Thru + Thru-Right',
            laneType: 'Vehicle',
            volume: [],
            peakHourVolume: { value: 20 },
          },
          {
            direction: 'Northbound',
            movementType: 'Thru',
            laneType: 'Vehicle',
            volume: [],
            peakHourVolume: { value: 10 },
          },
        ],
        peakHourFactor: 0.92,
        peakHour: { key: '2026-04-01T08:00:00', value: 60 },
      },
    }

    const result = transformTurningMovementCountsData(
      response
    ) as TransformedTurningMovementCountsResponse
    const chartDescriptions = result.data.charts.map(
      (chart) => (chart.chart as ChartWithDisplayProps).displayProps.description
    )

    expect(chartDescriptions).toEqual([
      'NorthboundThru',
      'NorthboundThru + Thru-Right',
      'NorthboundRight',
    ])

    expect(result.data.labels.columnGroups).toEqual([
      { title: null, columns: ['Hour'] },
      {
        title: 'Northbound',
        columns: ['Thru', 'Thru + Thru-Right', 'Right', 'Total'],
      },
      { title: null, columns: ['Bin Total'] },
    ])

    expect(result.data.peakHour).toEqual({
      peakHourFactor: 0.92,
      peakHourData: [['08:00 - 09:00', 10, 20, 30, 60, 60]],
    })
    expect(result.data.displayProps?.exportFileName).toBe(
      'Turning_Movement_Counts_Main_St_100_S_2026-04-01_08-00_to_2026-04-01_09-00'
    )
  })

  it('formats nullable chart summary values as N/A', () => {
    const response: RawTurningMovementCountsResponse = {
      type: ChartType.TurningMovementCounts,
      data: {
        charts: [
          buildChart('Thru', {
            peakHour: null,
            peakHourVolume: null,
            peakHourFactor: null,
            laneUtilizationFactor: null,
          }),
        ],
        table: [
          {
            direction: 'Northbound',
            movementType: 'Thru',
            laneType: 'Vehicle',
            volume: [],
            peakHourVolume: null,
          },
        ],
        peakHourFactor: null,
        peakHour: null,
      },
    }

    const result = transformTurningMovementCountsData(
      response
    ) as TransformedTurningMovementCountsResponse
    const chart = result.data.charts[0].chart as ChartWithTitle
    const infoText = chart.title?.find((title) =>
      title.text?.includes('Total Volume')
    )?.text

    expect(infoText).toContain('Peak Hour:  {values|N/A}')
    expect(infoText).toContain('Peak Hour Volume:  {values|N/A}')
    expect(infoText).toContain('Peak Hour Factor:  {values|N/A}')
    expect(infoText).toContain('fLU:  {values|N/A}')
  })
})
