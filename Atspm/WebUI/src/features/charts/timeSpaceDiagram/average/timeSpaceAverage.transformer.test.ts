// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceAverage.transformer.test.ts
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
import { ToolType } from '@/features/charts/common/types'
import type { EChartsOption, SeriesOption } from 'echarts'
import type {
  RawTimeSpaceAverageData,
  RawTimeSpaceDiagramResponse,
} from '../shared/types'
import transformTimeSpaceAverageData from './timeSpaceAverage.transformer'

function buildAverageLocation(
  phaseType: 'Primary' | 'Opposing',
  overrides: Partial<RawTimeSpaceAverageData> = {}
): RawTimeSpaceAverageData {
  return {
    start: '2026-04-07T08:00:00Z',
    end: '2026-04-07T09:00:00Z',
    locationIdentifier:
      phaseType === 'Primary' ? 'primary-location' : 'opposing-location',
    locationDescription: `${phaseType} location`,
    phaseNumber: phaseType === 'Primary' ? 2 : 6,
    phaseNumberSort: phaseType === 'Primary' ? '2' : '6',
    distanceToNextLocation: 100,
    distanceToPreviousLocation: 0,
    speed: 35,
    approachId: phaseType === 'Primary' ? 1 : 2,
    approachDescription: phaseType === 'Primary' ? 'NB Through' : 'SB Through',
    phaseType,
    order: phaseType === 'Primary' ? 0 : 0,
    calculatedDistanceToNext: 100,
    calculatedDistanceToPrevious: 0,
    isIgnoredLocation: false,
    offset: 0,
    cycleLength: 120,
    programmedSplit: 60,
    coordinatedPhases: true,
    greenTimeEvents: [],
    cycleAllEvents: [
      { start: '2026-04-07T08:00:00Z', value: 1 },
      { start: '2026-04-07T08:00:30Z', value: 3 },
      { start: '2026-04-07T08:01:00Z', value: 8 },
      { start: '2026-04-07T08:01:30Z', value: 9 },
      { start: '2026-04-07T08:02:00Z', value: 11 },
    ],
    ...overrides,
  }
}

describe('transformTimeSpaceAverageData', () => {
  it('returns transformed chart data when wrapped results are successful', () => {
    const response: RawTimeSpaceDiagramResponse = {
      type: ToolType.TimeSpaceAverage,
      data: [
        {
          isSuccess: true,
          error: null,
          result: buildAverageLocation('Primary'),
        },
        {
          isSuccess: true,
          error: null,
          result: buildAverageLocation('Opposing'),
        },
      ],
    }

    const result = transformTimeSpaceAverageData(response)

    expect(result.type).toBe(ToolType.TimeSpaceAverage)
    expect(result.data.chart).toBeDefined()
  })

  it('builds the same header and axis scaffolding used by the historic chart shell', () => {
    const response: RawTimeSpaceDiagramResponse = {
      type: ToolType.TimeSpaceAverage,
      data: [
        {
          isSuccess: true,
          error: null,
          result: buildAverageLocation('Primary'),
        },
        {
          isSuccess: true,
          error: null,
          result: buildAverageLocation('Opposing'),
        },
      ],
    }

    const result = transformTimeSpaceAverageData(response)
    const chart = result.data.chart as EChartsOption
    const titleEntries = Array.isArray(chart.title)
      ? chart.title
      : [chart.title]
    const xAxes = Array.isArray(chart.xAxis) ? chart.xAxis : [chart.xAxis]
    const toolbox = Array.isArray(chart.toolbox)
      ? chart.toolbox[0]
      : chart.toolbox
    const series = Array.isArray(chart.series)
      ? (chart.series as SeriesOption[])
      : []

    expect(titleEntries[0]).toMatchObject({
      text: 'Time Space Diagram - 50th Percentile',
    })
    expect(String(titleEntries[1]?.text ?? '')).toMatch(
      /^[A-Z][a-z]{2}, [A-Z][a-z]{2} \d{1,2}, \d{4} - \d{2}:\d{2}-\d{2}:\d{2}$/
    )
    expect(xAxes).toHaveLength(3)
    expect(xAxes[2]).toMatchObject({
      position: 'top',
      name: 'Time Since Start (seconds)',
    })
    expect(toolbox).toMatchObject({
      feature: {
        restore: {},
      },
    })
    expect(
      series.some((entry) => entry.name === 'Labels offset and program split')
    ).toBe(false)
  })

  it('returns errors and empty chart when all wrapped results fail', () => {
    const response: RawTimeSpaceDiagramResponse = {
      type: ToolType.TimeSpaceAverage,
      data: [
        {
          isSuccess: false,
          error: 'Primary failed',
          result: null,
        },
        {
          isSuccess: false,
          error: 'Opposing failed',
          result: null,
        },
      ],
    }

    const result = transformTimeSpaceAverageData(response)

    expect(result.type).toBe(ToolType.TimeSpaceAverage)
    expect(result.errors).toEqual(['Primary failed', 'Opposing failed'])
    expect(result.data.chart).toEqual({})
  })

  it('creates 3 cycle-related series per non-ignored location (rail, cycles, duration labels)', () => {
    const response: RawTimeSpaceDiagramResponse = {
      type: ToolType.TimeSpaceAverage,
      data: [
        {
          isSuccess: true,
          error: null,
          result: buildAverageLocation('Primary'),
        },
        {
          isSuccess: true,
          error: null,
          result: buildAverageLocation('Opposing'),
        },
      ],
    }

    const result = transformTimeSpaceAverageData(response)
    const chart = result.data.chart as EChartsOption
    const series = Array.isArray(chart.series)
      ? (chart.series as SeriesOption[])
      : []

    const primaryCycleSeries = series.filter((entry) =>
      String(entry.id).includes('primary')
    )
    const opposingCycleSeries = series.filter((entry) =>
      String(entry.id).includes('opposing')
    )

    expect(
      primaryCycleSeries.some((entry) =>
        String(entry.id).startsWith('Cycle Rail ')
      )
    ).toBe(true)
    expect(
      primaryCycleSeries.some((entry) => String(entry.id).startsWith('Cycles '))
    ).toBe(true)
    expect(
      primaryCycleSeries.some((entry) =>
        String(entry.id).startsWith('Cycle Duration Labels ')
      )
    ).toBe(true)

    expect(
      opposingCycleSeries.some((entry) =>
        String(entry.id).startsWith('Cycle Rail ')
      )
    ).toBe(true)
    expect(
      opposingCycleSeries.some((entry) =>
        String(entry.id).startsWith('Cycles ')
      )
    ).toBe(true)
    expect(
      opposingCycleSeries.some((entry) =>
        String(entry.id).startsWith('Cycle Duration Labels ')
      )
    ).toBe(true)
  })
})
