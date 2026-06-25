import { dateToTimestamp } from '@/utils/dateTime'
import { act, renderHook } from '@testing-library/react'
import type { ECharts, EChartsOption } from 'echarts'
import {
  formatSignedOffsetSeconds,
  getTimeSpaceLocationOffsetBadgeLayout,
  TIME_SPACE_LOCATION_AXIS_SERIES_ID,
} from '../transformers/timeSpaceTransformerBase'
import { useTimeSpaceHandler } from './timeSpace.handler'

const originalCanvasGetContext = HTMLCanvasElement.prototype.getContext

beforeAll(() => {
  Object.defineProperty(HTMLCanvasElement.prototype, 'getContext', {
    configurable: true,
    value: jest.fn(() => null),
  })
})

afterAll(() => {
  Object.defineProperty(HTMLCanvasElement.prototype, 'getContext', {
    configurable: true,
    value: originalCanvasGetContext,
  })
})

type MouseHandler = (event: { offsetX: number; offsetY: number }) => void
type ChartHandler = () => void

class MockChart {
  option: EChartsOption
  zrHandlers: Record<string, MouseHandler[]> = {}
  chartHandlers: Record<string, ChartHandler[]> = {}
  setOptionCalls: EChartsOption[] = []

  constructor(option: EChartsOption) {
    this.option = cloneOption(option)
  }

  getOption() {
    return this.option
  }

  setOption(nextOption: EChartsOption) {
    this.setOptionCalls.push(cloneOption(nextOption))

    if (!Array.isArray(nextOption.series)) {
      this.option = {
        ...this.option,
        ...nextOption,
      }
      return
    }

    const currentSeries = Array.isArray(this.option.series)
      ? this.option.series
      : []
    const updates = Array.isArray(nextOption.series) ? nextOption.series : []

    this.option = {
      ...this.option,
      ...nextOption,
      series: currentSeries.map((series) => {
        const id = typeof series?.id === 'string' ? series.id : undefined
        const update = updates.find(
          (candidate) =>
            typeof candidate?.id === 'string' && candidate.id === id
        )

        return update ? { ...series, ...update } : series
      }) as EChartsOption['series'],
    }
  }

  getZr() {
    return {
      on: (event: string, handler: MouseHandler) => {
        this.zrHandlers[event] ??= []
        this.zrHandlers[event].push(handler)
      },
      off: (event: string, handler: MouseHandler) => {
        this.zrHandlers[event] = (this.zrHandlers[event] ?? []).filter(
          (candidate) => candidate !== handler
        )
      },
    }
  }

  on(event: string, handler: ChartHandler) {
    this.chartHandlers[event] ??= []
    this.chartHandlers[event].push(handler)
  }

  off(event: string, handler: ChartHandler) {
    this.chartHandlers[event] = (this.chartHandlers[event] ?? []).filter(
      (candidate) => candidate !== handler
    )
  }

  convertToPixel(
    _finder: { xAxisIndex: number; yAxisIndex: number },
    value: [number, number]
  ) {
    return [0, value[1]]
  }

  convertFromPixel(
    _finder: { xAxisIndex: number; yAxisIndex: number },
    value: [number, number]
  ) {
    return value
  }

  replaceOption(nextOption: EChartsOption) {
    this.option = cloneOption(nextOption)
  }
}

function cloneOption(option: EChartsOption): EChartsOption {
  return JSON.parse(JSON.stringify(option)) as EChartsOption
}

function buildOption(): EChartsOption {
  return {
    grid: {
      left: 220,
    },
    series: [
      {
        id: 'Cycles 1 Eastbound',
        data: [['2026-03-20T00:00:00Z', 10, 0]],
      },
      {
        id: 'Cycles 2 Eastbound',
        data: [['2026-03-20T00:00:00Z', 20, 0]],
      },
      {
        id: TIME_SPACE_LOCATION_AXIS_SERIES_ID,
        data: [
          ['2026-03-20T00:00:00Z', 10, '1', null, 150, 0, 0, 0],
          ['2026-03-20T00:00:00Z', 20, '2', null, 150, 0, 0, 0],
        ],
      },
    ],
  }
}

function buildOptionWithBaseOffset(): EChartsOption {
  return {
    grid: {
      left: 220,
    },
    series: [
      {
        id: 'Cycles 1 Eastbound',
        data: [['2026-03-20T00:00:00Z', 10, 0]],
      },
      {
        id: 'Cycles 2 Eastbound',
        data: [['2026-03-20T00:00:00Z', 20, 0]],
      },
      {
        id: TIME_SPACE_LOCATION_AXIS_SERIES_ID,
        data: [
          ['2026-03-20T00:00:00Z', 10, '1', null, 150, 7, 0, 0],
          ['2026-03-20T00:00:00Z', 20, '2', null, 150, 0, 0, 0],
        ],
      },
    ],
  }
}

function buildOptionWithEquivalentCycleOffset(): EChartsOption {
  return {
    grid: {
      left: 220,
    },
    series: [
      {
        id: 'Cycles 1 Eastbound',
        data: [['2026-03-20T00:00:00Z', 10, 0]],
      },
      {
        id: TIME_SPACE_LOCATION_AXIS_SERIES_ID,
        data: [['2026-03-20T00:00:00Z', 10, '1', null, 120, 93, 93, 0]],
      },
    ],
  }
}

function buildOptionWithChartTimespanLimit(): EChartsOption {
  return {
    xAxis: [
      {
        min: '2026-03-20T00:00:00Z',
        max: '2026-03-20T00:01:00Z',
      },
    ],
    grid: {
      left: 220,
    },
    series: [
      {
        id: 'Cycles 1 Eastbound',
        data: [['2026-03-20T00:00:00Z', 10, 0]],
      },
      {
        id: TIME_SPACE_LOCATION_AXIS_SERIES_ID,
        data: [['2026-03-20T00:00:00Z', 10, '1', null, 150, 0, 0, 0]],
      },
    ],
  }
}

function buildOptionWithNullSeries(): EChartsOption {
  const option = buildOption()

  return {
    ...option,
    series: [
      null,
      ...(Array.isArray(option.series) ? option.series : []),
    ] as unknown as EChartsOption['series'],
  }
}

function buildOptionWithAssociatedSeries(): EChartsOption {
  return {
    series: [
      {
        id: 'Cycles 1 Eastbound',
        data: [['2026-03-20T00:00:00Z', 10, 0]],
      },
      {
        id: 'Cycles 2 Eastbound',
        data: [['2026-03-20T00:00:00Z', 20, 0]],
      },
      {
        id: 'Cycle Duration Labels 1 Eastbound',
        data: [[Date.parse('2026-03-20T00:00:05Z'), 10, '5']],
      },
      {
        id: 'Green Bands 1 Eastbound row-0 primary',
        data: [
          ['2026-03-20T00:00:01Z', 10],
          ['2026-03-20T00:00:03Z', 10],
        ],
      },
      {
        id: 'PI 1 Eastbound row-0 primary',
        data: [['2026-03-20T00:00:00Z', '2026-03-20T00:00:04Z', 21, 10]],
      },
      {
        id: 'LLC 1 Eastbound row-0 primary',
        data: [
          ['2026-03-20T00:00:01Z', 10],
          ['2026-03-20T00:00:03Z', 12],
          null,
        ],
      },
      {
        id: 'AC 1 Eastbound row-0 primary',
        data: [
          ['2026-03-20T00:00:02Z', 8],
          ['2026-03-20T00:00:04Z', 10],
          null,
        ],
      },
      {
        id: 'SBP 1 Eastbound row-0 primary',
        data: [
          ['2026-03-20T00:00:03Z', 10],
          ['2026-03-20T00:00:05Z', 10],
        ],
      },
      {
        id: 'Left Turn 1 Eastbound row-0 primary',
        data: [
          ['2026-03-20T00:00:01Z', 10],
          ['2026-03-20T00:00:03Z', 12],
          null,
        ],
      },
      {
        id: 'Right Turn 1 Eastbound row-0 primary',
        data: [
          ['2026-03-20T00:00:02Z', 10],
          ['2026-03-20T00:00:04Z', 12],
          null,
        ],
      },
      {
        id: 'SRM 1 bus-42 0 Eastbound row-0 primary',
        data: [['2026-03-20T00:00:01Z', 10]],
      },
      {
        id: 'Early Green 1 row-0 primary',
        data: [['2026-03-20T00:00:01Z', 10]],
      },
      {
        id: 'Extend Green 1 row-0 primary',
        data: [['2026-03-20T00:00:02Z', 10]],
      },
      {
        id: 'TSP Request 1 row-0 primary',
        data: [['2026-03-20T00:00:03Z', '2026-03-20T00:00:05Z', 10]],
      },
      {
        id: 'TSP Service 1 row-0 primary',
        data: [['2026-03-20T00:00:04Z', '2026-03-20T00:00:06Z', 10]],
      },
      {
        id: 'PI 2 Eastbound row-1 primary',
        data: [['2026-03-20T00:00:10Z', '2026-03-20T00:00:14Z', 21, 20]],
      },
      {
        id: 'LLC 2 Eastbound row-1 primary',
        data: [
          ['2026-03-20T00:00:11Z', 20],
          ['2026-03-20T00:00:13Z', 22],
          null,
        ],
      },
      {
        id: 'TSP Request 2 row-1 primary',
        data: [['2026-03-20T00:00:13Z', '2026-03-20T00:00:15Z', 20]],
      },
      {
        id: TIME_SPACE_LOCATION_AXIS_SERIES_ID,
        data: [
          ['2026-03-20T00:00:00Z', 10, '1', null, 150, 0],
          ['2026-03-20T00:00:00Z', 20, '2', null, 300, 0],
        ],
      },
    ],
  }
}

function buildOptionWithMissingCycleLength(): EChartsOption {
  return {
    series: [
      {
        id: 'Cycles 1 Eastbound',
        data: [['2026-03-20T00:00:00Z', 10, 0]],
      },
      {
        id: TIME_SPACE_LOCATION_AXIS_SERIES_ID,
        data: [['2026-03-20T00:00:00Z', 10, '1', null, null, 0, 0]],
      },
    ],
  }
}

function dragGroup(chart: MockChart, groupY: number, offsetMs: number) {
  act(() => {
    chart.zrHandlers.mousedown?.[0]({ offsetX: 0, offsetY: groupY })
  })

  act(() => {
    chart.zrHandlers.mousemove?.[0]({ offsetX: offsetMs, offsetY: groupY })
  })

  act(() => {
    chart.zrHandlers.mouseup?.[0]({ offsetX: offsetMs, offsetY: groupY })
  })
}

function doubleClickOffsetBox(chart: MockChart, locationId: string) {
  const series = Array.isArray(chart.option.series) ? chart.option.series : []
  const locationAxis = series.find(
    (entry) => entry?.id === TIME_SPACE_LOCATION_AXIS_SERIES_ID
  )
  const data = Array.isArray(locationAxis?.data) ? locationAxis.data : []
  const datum = data.find(
    (entry) => Array.isArray(entry) && String(entry[2] ?? '') === locationId
  )

  if (!Array.isArray(datum)) {
    throw new Error(`Could not find location axis datum for ${locationId}`)
  }

  const grid = Array.isArray(chart.option.grid)
    ? chart.option.grid[0]
    : chart.option.grid
  const gridLeft =
    typeof grid?.left === 'number' ? grid.left : Number(grid?.left ?? 0)
  const badgeLayout = getTimeSpaceLocationOffsetBadgeLayout(
    gridLeft,
    Number(datum[1]),
    formatSignedOffsetSeconds(Number(datum[5] ?? 0)),
    false
  )

  act(() => {
    chart.zrHandlers.dblclick?.[0]({
      offsetX: badgeLayout.highlightX + badgeLayout.highlightWidth / 2,
      offsetY: badgeLayout.highlightY + badgeLayout.highlightHeight / 2,
    })
  })
}

function getLocationAxisOffsets(chart: MockChart): number[] {
  const series = Array.isArray(chart.option.series) ? chart.option.series : []
  const locationAxis = series.find(
    (entry) => entry?.id === TIME_SPACE_LOCATION_AXIS_SERIES_ID
  )
  const data = Array.isArray(locationAxis?.data) ? locationAxis.data : []

  return data.map((datum) =>
    Array.isArray(datum) && typeof datum[5] === 'number' ? datum[5] : 0
  )
}

function getLocationAxisUserAdjustments(chart: MockChart): number[] {
  const series = Array.isArray(chart.option.series) ? chart.option.series : []
  const locationAxis = series.find(
    (entry) => entry?.id === TIME_SPACE_LOCATION_AXIS_SERIES_ID
  )
  const data = Array.isArray(locationAxis?.data) ? locationAxis.data : []

  return data.map((datum) =>
    Array.isArray(datum) && typeof datum[7] === 'number' ? datum[7] : 0
  )
}

function getSeriesData(chart: MockChart, id: string) {
  const series = Array.isArray(chart.option.series) ? chart.option.series : []
  return series.find((entry) => entry?.id === id)?.data
}

function getLatestSeriesUpdate(chart: MockChart, id: string) {
  const calls = [...chart.setOptionCalls].reverse()

  for (const call of calls) {
    const series = Array.isArray(call.series) ? call.series : []
    const update = series.find((entry) => entry?.id === id)
    if (update) {
      return update
    }
  }

  return null
}

function shiftTimestamp(value: string, offsetMs: number) {
  return dateToTimestamp(new Date(new Date(value).getTime() + offsetMs))
}

describe('useTimeSpaceHandler', () => {
  it('does not drag cycle overlays when cycle dragging is disabled', () => {
    const chart = new MockChart(buildOption())

    renderHook(() =>
      useTimeSpaceHandler(chart as unknown as ECharts, 0, {
        enableCycleDragging: false,
      })
    )

    dragGroup(chart, 10, 5000)

    expect(getLocationAxisOffsets(chart)).toEqual([0, 0])
    expect(getSeriesData(chart, 'Cycles 1 Eastbound')).toEqual([
      ['2026-03-20T00:00:00Z', 10, 0],
    ])
    expect(chart.setOptionCalls).toHaveLength(0)
  })

  it('clears cached offsets after an external reset before the next drag', () => {
    const initialOption = buildOption()
    const chart = new MockChart(initialOption)
    const { rerender } = renderHook(
      ({ syncVersion }) =>
        useTimeSpaceHandler(chart as unknown as ECharts, syncVersion),
      {
        initialProps: {
          syncVersion: 0,
        },
      }
    )

    dragGroup(chart, 10, 5000)
    expect(getLocationAxisOffsets(chart)).toEqual([5, 0])

    chart.replaceOption(initialOption)
    rerender({ syncVersion: 1 })

    dragGroup(chart, 20, 3000)
    expect(getLocationAxisOffsets(chart)).toEqual([0, 3])
  })

  it('ignores null series entries after the chart option is rebuilt', () => {
    const chart = new MockChart(buildOption())
    const { rerender } = renderHook(
      ({ syncVersion }) =>
        useTimeSpaceHandler(chart as unknown as ECharts, syncVersion),
      {
        initialProps: {
          syncVersion: 0,
        },
      }
    )

    chart.replaceOption(buildOptionWithNullSeries())

    expect(() => rerender({ syncVersion: 1 })).not.toThrow()

    dragGroup(chart, 10, 2000)
    expect(getLocationAxisOffsets(chart)).toEqual([2, 0])
  })

  it('does not throw when the chart temporarily returns a null option', () => {
    const chart = new MockChart(buildOption())
    const { rerender } = renderHook(
      ({ syncVersion }) =>
        useTimeSpaceHandler(chart as unknown as ECharts, syncVersion),
      {
        initialProps: {
          syncVersion: 0,
        },
      }
    )

    chart.replaceOption(null as unknown as EChartsOption)

    expect(() => rerender({ syncVersion: 1 })).not.toThrow()
  })

  it('drags the cycle overlays that belong to the selected row', () => {
    const chart = new MockChart(buildOptionWithAssociatedSeries())
    const offsetMs = 2000

    renderHook(() => useTimeSpaceHandler(chart as unknown as ECharts, 0))

    dragGroup(chart, 10, offsetMs)

    expect(getSeriesData(chart, 'Cycle Duration Labels 1 Eastbound')).toEqual([
      [Date.parse('2026-03-20T00:00:07Z'), 10, '5'],
    ])
    expect(getSeriesData(chart, 'Green Bands 1 Eastbound row-0 primary')).toEqual(
      [
        [shiftTimestamp('2026-03-20T00:00:01Z', offsetMs), 10],
        [shiftTimestamp('2026-03-20T00:00:03Z', offsetMs), 10],
      ]
    )
    expect(getSeriesData(chart, 'PI 1 Eastbound row-0 primary')).toEqual([
      [
        shiftTimestamp('2026-03-20T00:00:00Z', offsetMs),
        shiftTimestamp('2026-03-20T00:00:04Z', offsetMs),
        21,
        10,
      ],
    ])
    expect(getSeriesData(chart, 'LLC 1 Eastbound row-0 primary')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:01Z', offsetMs), 10],
      [shiftTimestamp('2026-03-20T00:00:03Z', offsetMs), 12],
      null,
    ])
    expect(getSeriesData(chart, 'AC 1 Eastbound row-0 primary')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:02Z', offsetMs), 8],
      [shiftTimestamp('2026-03-20T00:00:04Z', offsetMs), 10],
      null,
    ])
    expect(getSeriesData(chart, 'SBP 1 Eastbound row-0 primary')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:03Z', offsetMs), 10],
      [shiftTimestamp('2026-03-20T00:00:05Z', offsetMs), 10],
    ])
    expect(getSeriesData(chart, 'Left Turn 1 Eastbound row-0 primary')).toEqual(
      [
        [shiftTimestamp('2026-03-20T00:00:01Z', offsetMs), 10],
        [shiftTimestamp('2026-03-20T00:00:03Z', offsetMs), 12],
        null,
      ]
    )
    expect(
      getSeriesData(chart, 'Right Turn 1 Eastbound row-0 primary')
    ).toEqual([
      [shiftTimestamp('2026-03-20T00:00:02Z', offsetMs), 10],
      [shiftTimestamp('2026-03-20T00:00:04Z', offsetMs), 12],
      null,
    ])
    expect(
      getSeriesData(chart, 'SRM 1 bus-42 0 Eastbound row-0 primary')
    ).toEqual([[shiftTimestamp('2026-03-20T00:00:01Z', offsetMs), 10]])
    expect(getSeriesData(chart, 'Early Green 1 row-0 primary')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:01Z', offsetMs), 10],
    ])
    expect(getSeriesData(chart, 'Extend Green 1 row-0 primary')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:02Z', offsetMs), 10],
    ])
    expect(getSeriesData(chart, 'TSP Request 1 row-0 primary')).toEqual([
      [
        shiftTimestamp('2026-03-20T00:00:03Z', offsetMs),
        shiftTimestamp('2026-03-20T00:00:05Z', offsetMs),
        10,
      ],
    ])
    expect(getSeriesData(chart, 'TSP Service 1 row-0 primary')).toEqual([
      [
        shiftTimestamp('2026-03-20T00:00:04Z', offsetMs),
        shiftTimestamp('2026-03-20T00:00:06Z', offsetMs),
        10,
      ],
    ])
    expect(getSeriesData(chart, 'PI 2 Eastbound row-1 primary')).toEqual([
      ['2026-03-20T00:00:10Z', '2026-03-20T00:00:14Z', 21, 20],
    ])
    expect(getSeriesData(chart, 'LLC 2 Eastbound row-1 primary')).toEqual([
      ['2026-03-20T00:00:11Z', 20],
      ['2026-03-20T00:00:13Z', 22],
      null,
    ])
    expect(getSeriesData(chart, 'TSP Request 2 row-1 primary')).toEqual([
      ['2026-03-20T00:00:13Z', '2026-03-20T00:00:15Z', 20],
    ])
    expect(getLocationAxisOffsets(chart)).toEqual([2, 0])
  })

  it('disables update animation for drag-shifted series', () => {
    const chart = new MockChart(buildOptionWithAssociatedSeries())

    renderHook(() => useTimeSpaceHandler(chart as unknown as ECharts, 0))

    dragGroup(chart, 10, 2000)

    const laneByLaneUpdate = getLatestSeriesUpdate(
      chart,
      'LLC 1 Eastbound row-0 primary'
    ) as {
      animation?: unknown
      animationDurationUpdate?: unknown
      animationDelayUpdate?: unknown
    } | null
    const stopBarUpdate = getLatestSeriesUpdate(
      chart,
      'SBP 1 Eastbound row-0 primary'
    ) as {
      animation?: unknown
      animationDurationUpdate?: unknown
      animationDelayUpdate?: unknown
    } | null
    const locationAxisUpdate = getLatestSeriesUpdate(
      chart,
      TIME_SPACE_LOCATION_AXIS_SERIES_ID
    ) as {
      animation?: unknown
      animationDurationUpdate?: unknown
      animationDelayUpdate?: unknown
    } | null

    expect(laneByLaneUpdate).toMatchObject({
      animation: false,
      animationDurationUpdate: 0,
      animationDelayUpdate: 0,
    })
    expect(stopBarUpdate).toMatchObject({
      animation: false,
      animationDurationUpdate: 0,
      animationDelayUpdate: 0,
    })
    expect(locationAxisUpdate).toMatchObject({
      animation: false,
      animationDurationUpdate: 0,
      animationDelayUpdate: 0,
    })
  })

  it('keeps the full rightward drag on the cycle overlays while wrapping the displayed offset', () => {
    const chart = new MockChart(buildOption())

    renderHook(() => useTimeSpaceHandler(chart as unknown as ECharts, 0))

    dragGroup(chart, 10, 160000)

    expect(getLocationAxisOffsets(chart)).toEqual([10, 0])
    expect(getSeriesData(chart, 'Cycles 1 Eastbound')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:00Z', 160000), 10, 0],
    ])
  })

  it('keeps the full leftward drag on the cycle overlays while wrapping the displayed offset', () => {
    const chart = new MockChart(buildOption())

    renderHook(() => useTimeSpaceHandler(chart as unknown as ECharts, 0))

    dragGroup(chart, 10, -160000)

    expect(getLocationAxisOffsets(chart)).toEqual([-10, 0])
    expect(getSeriesData(chart, 'Cycles 1 Eastbound')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:00Z', -160000), 10, 0],
    ])
  })

  it('wraps within a 180 second cycle when the cycle length is not found', () => {
    const chart = new MockChart(buildOptionWithMissingCycleLength())

    renderHook(() => useTimeSpaceHandler(chart as unknown as ECharts, 0))

    dragGroup(chart, 10, 200000)

    expect(getLocationAxisOffsets(chart)).toEqual([20])
    expect(getSeriesData(chart, 'Cycles 1 Eastbound')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:00Z', 200000), 10, 0],
    ])
  })

  it('wraps only the displayed current offset when a drag goes past the cycle length', () => {
    const chart = new MockChart(buildOption())

    renderHook(() => useTimeSpaceHandler(chart as unknown as ECharts, 0))

    dragGroup(chart, 10, 155000)

    expect(getLocationAxisOffsets(chart)).toEqual([5, 0])
    expect(getSeriesData(chart, 'Cycles 1 Eastbound')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:00Z', 155000), 10, 0],
    ])
  })

  it('keeps full-cycle drags marked as modified even when the displayed offset matches the base value', () => {
    const chart = new MockChart(buildOptionWithEquivalentCycleOffset())

    renderHook(() => useTimeSpaceHandler(chart as unknown as ECharts, 0))

    dragGroup(chart, 10, 120000)

    expect(getLocationAxisOffsets(chart)).toEqual([93])
    expect(getLocationAxisUserAdjustments(chart)).toEqual([120])
    expect(getSeriesData(chart, 'Cycles 1 Eastbound')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:00Z', 120000), 10, 0],
    ])

    dragGroup(chart, 10, -120000)

    expect(getLocationAxisOffsets(chart)).toEqual([93])
    expect(getLocationAxisUserAdjustments(chart)).toEqual([0])
  })

  it('limits raw dragging to one chart timespan in either direction', () => {
    const chart = new MockChart(buildOptionWithChartTimespanLimit())

    renderHook(() => useTimeSpaceHandler(chart as unknown as ECharts, 0))

    dragGroup(chart, 10, 120000)

    expect(getLocationAxisOffsets(chart)).toEqual([60])
    expect(getLocationAxisUserAdjustments(chart)).toEqual([60])
    expect(getSeriesData(chart, 'Cycles 1 Eastbound')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:00Z', 60000), 10, 0],
    ])

    dragGroup(chart, 10, -180000)

    expect(getLocationAxisOffsets(chart)).toEqual([-60])
    expect(getLocationAxisUserAdjustments(chart)).toEqual([-60])
    expect(getSeriesData(chart, 'Cycles 1 Eastbound')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:00Z', -60000), 10, 0],
    ])
  })

  it('resets a dragged offset back to the base offset when the offset badge is double clicked', () => {
    const chart = new MockChart(buildOption())

    renderHook(() => useTimeSpaceHandler(chart as unknown as ECharts, 0))

    dragGroup(chart, 10, 5000)
    expect(getLocationAxisOffsets(chart)).toEqual([5, 0])

    doubleClickOffsetBox(chart, '1')

    expect(getLocationAxisOffsets(chart)).toEqual([0, 0])
    expect(getSeriesData(chart, 'Cycles 1 Eastbound')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:00Z', 0), 10, 0],
    ])
  })

  it('resets a non-zero current offset back to its base offset on double click', () => {
    const chart = new MockChart(buildOptionWithBaseOffset())

    renderHook(() => useTimeSpaceHandler(chart as unknown as ECharts, 0))

    expect(getLocationAxisOffsets(chart)).toEqual([7, 0])

    doubleClickOffsetBox(chart, '1')

    expect(getLocationAxisOffsets(chart)).toEqual([0, 0])
    expect(getSeriesData(chart, 'Cycles 1 Eastbound')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:00Z', -7000), 10, 0],
    ])
  })
})
