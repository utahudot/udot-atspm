// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceLegend.test.ts
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
import type { ECharts, EChartsOption } from 'echarts'
import {
  getLegendSelectedMap,
  getRequestedLegendVisibility,
  syncRequestedLegendSelections,
  withSyntheticLegendEntry,
} from './timeSpaceLegend'

class MockChart {
  constructor(private readonly option: EChartsOption) {}

  dispatchedActions: Array<{ type: string; name: string }> = []

  getOption() {
    return this.option
  }

  dispatchAction(action: { type: string; name: string }) {
    this.dispatchedActions.push(action)
  }
}

describe('timeSpaceLegend utilities', () => {
  const directionRoleBySeriesName = new Map([
    ['Cycles EB', 'primary' as const],
    ['Cycle Durations EB', 'primary' as const],
    ['Green Bands EB', 'primary' as const],
    ['Lane by Lane Count EB', 'primary' as const],
    ['Cycles WB', 'opposing' as const],
  ])

  describe('withSyntheticLegendEntry', () => {
    it('adds a synthetic entry to the primary legend and marks it selected by default', () => {
      const option: EChartsOption = {
        legend: [
          {
            data: ['Cycles EB'],
            selected: {
              'Cycles EB': true,
            },
          },
          {
            data: ['Cycles WB'],
          },
        ],
      }

      const nextOption = withSyntheticLegendEntry(option, 'GPX Tracks')

      expect(nextOption?.legend).toEqual([
        {
          data: ['Cycles EB', 'GPX Tracks'],
          selected: {
            'Cycles EB': true,
            'GPX Tracks': true,
          },
        },
        {
          data: ['Cycles WB'],
        },
      ])
    })

    it('does not duplicate an existing legend entry and honors explicit default selection state', () => {
      const option: EChartsOption = {
        legend: {
          data: ['Cycles EB', 'GPX Tracks'],
          selected: {
            'Cycles EB': true,
            'GPX Tracks': false,
          },
        },
      }

      const nextOption = withSyntheticLegendEntry(option, 'GPX Tracks', false)

      expect(nextOption).toEqual(option)
    })
  })

  describe('getLegendSelectedMap', () => {
    it('returns an empty map when the chart has no selected legend state', () => {
      expect(getLegendSelectedMap({})).toEqual({})
    })

    it('clones the selected map instead of returning the original object', () => {
      const option: EChartsOption = {
        legend: {
          selected: {
            'Cycles EB': true,
          },
        },
      }

      const selected = getLegendSelectedMap(option)
      selected['Cycles EB'] = false

      expect(getLegendSelectedMap(option)).toEqual({
        'Cycles EB': true,
      })
    })
  })

  describe('getRequestedLegendVisibility', () => {
    it('hides a suppressed direction without overwriting the underlying requested state', () => {
      const requestedSelections = {
        'Cycles EB': true,
        'Green Bands EB': true,
        'Lane by Lane Count EB': false,
      }

      expect(
        getRequestedLegendVisibility(
          'Cycles EB',
          requestedSelections,
          { primary: true },
          directionRoleBySeriesName
        )
      ).toBe(false)

      expect(
        getRequestedLegendVisibility(
          'Green Bands EB',
          requestedSelections,
          { primary: true },
          directionRoleBySeriesName
        )
      ).toBe(false)

      expect(
        getRequestedLegendVisibility(
          'Lane by Lane Count EB',
          requestedSelections,
          {},
          directionRoleBySeriesName
        )
      ).toBe(false)
    })

    it('restores the previously requested series state when the direction is unsuppressed', () => {
      const requestedSelections = {
        'Cycles EB': true,
        'Green Bands EB': true,
        'Lane by Lane Count EB': false,
      }

      expect(
        getRequestedLegendVisibility(
          'Cycles EB',
          requestedSelections,
          {},
          directionRoleBySeriesName
        )
      ).toBe(true)

      expect(
        getRequestedLegendVisibility(
          'Green Bands EB',
          requestedSelections,
          {},
          directionRoleBySeriesName
        )
      ).toBe(true)

      expect(
        getRequestedLegendVisibility(
          'Lane by Lane Count EB',
          requestedSelections,
          {},
          directionRoleBySeriesName
        )
      ).toBe(false)
    })

    it('keeps cycle duration labels hidden when their parent cycles are off', () => {
      const requestedSelections = {
        'Cycles EB': false,
        'Cycle Durations EB': true,
      }

      expect(
        getRequestedLegendVisibility(
          'Cycle Durations EB',
          requestedSelections,
          {},
          directionRoleBySeriesName
        )
      ).toBe(false)
    })
  })

  describe('syncRequestedLegendSelections', () => {
    it('dispatches only the select and unselect actions needed to match requested visibility', () => {
      const chart = new MockChart({
        legend: {
          data: [
            'Cycles EB',
            'Cycle Durations EB',
            'Green Bands EB',
            'Lane by Lane Count EB',
            'Cycles WB',
          ],
          selected: {
            'Cycles EB': true,
            'Cycle Durations EB': true,
            'Green Bands EB': false,
            'Lane by Lane Count EB': false,
            'Cycles WB': false,
          },
        },
      })

      syncRequestedLegendSelections(
        chart as unknown as ECharts,
        {
          'Cycles EB': true,
          'Cycle Durations EB': true,
          'Green Bands EB': true,
          'Lane by Lane Count EB': true,
          'Cycles WB': true,
        },
        { primary: true },
        directionRoleBySeriesName
      )

      expect(chart.dispatchedActions).toEqual([
        {
          type: 'legendUnSelect',
          name: 'Cycles EB',
        },
        {
          type: 'legendUnSelect',
          name: 'Cycle Durations EB',
        },
        {
          type: 'legendSelect',
          name: 'Cycles WB',
        },
      ])
    })
  })
})
