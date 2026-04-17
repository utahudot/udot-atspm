import type { ECharts, EChartsOption } from 'echarts'
import {
  buildOffsetResetButtons,
  getRequestedLegendVisibility,
} from './TimeSpaceEChart'

describe('getRequestedLegendVisibility', () => {
  const directionRoleBySeriesName = new Map([
    ['Cycles EB', 'primary' as const],
    ['Cycle Durations EB', 'primary' as const],
    ['Green Bands EB', 'primary' as const],
    ['Lane by Lane Count EB', 'primary' as const],
    ['Cycles WB', 'opposing' as const],
  ])

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

describe('buildOffsetResetButtons', () => {
  function buildChart() {
    return {
      convertToPixel: () => [50, 60],
      getModel: () => ({
        getComponent: () => ({
          coordinateSystem: {
            getRect: () => ({
              x: 100,
              width: 200,
            }),
          },
        }),
      }),
    } as unknown as ECharts
  }

  it('returns reset buttons only when the offset row is actually modified', () => {
    const option: EChartsOption = {
      series: [
        {
          id: 'Location axis',
          name: 'Location axis',
          type: 'custom',
          data: [
            ['2026-03-20T00:00:00Z', 10, '6192', 'Main', 132, 89, 88, 1],
            ['2026-03-20T00:00:00Z', 20, '7001', 'Main', 132, 93, 93, 0],
            ['2026-03-20T00:00:00Z', 30, '8123', 'Main', 120, 93, 93, 120],
          ],
        },
      ],
    }

    const buttons = buildOffsetResetButtons(buildChart(), option)

    expect(buttons).toHaveLength(3)
    expect(buttons.map((button) => [button.location, button.active])).toEqual([
      ['6192', true],
      ['7001', false],
      ['8123', true],
    ])
  })
})
