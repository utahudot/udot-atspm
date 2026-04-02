import { renderHook, act } from '@testing-library/react'
import type { ECharts, EChartsOption } from 'echarts'
import { TIME_SPACE_LOCATION_AXIS_SERIES_ID } from '../transformers/timeSpaceTransformerBase'
import { useTimeSpaceHandler } from './timeSpace.handler'
import { dateToTimestamp } from '@/utils/dateTime'

type MouseHandler = (event: { offsetX: number; offsetY: number }) => void
type ChartHandler = () => void

class MockChart {
  option: EChartsOption
  zrHandlers: Record<string, MouseHandler[]> = {}
  chartHandlers: Record<string, ChartHandler[]> = {}

  constructor(option: EChartsOption) {
    this.option = cloneOption(option)
  }

  getOption() {
    return this.option
  }

  setOption(nextOption: EChartsOption) {
    if (!Array.isArray(nextOption.series)) {
      this.option = {
        ...this.option,
        ...nextOption,
      }
      return
    }

    const currentSeries = Array.isArray(this.option.series) ? this.option.series : []
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
          ['2026-03-20T00:00:00Z', 10, '1', null, null, 0],
          ['2026-03-20T00:00:00Z', 20, '2', null, null, 0],
        ],
      },
    ],
  }
}

function buildOptionWithNullSeries(): EChartsOption {
  const option = buildOption()

  return {
    ...option,
    series: [null, ...(Array.isArray(option.series) ? option.series : [])] as
      unknown as EChartsOption['series'],
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
        id: 'PI 1 Eastbound row-0 primary',
        data: [['2026-03-20T00:00:00Z', '2026-03-20T00:00:04Z', 21, 10]],
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
        data: [['2026-03-20T00:00:01Z', -90]],
      },
      {
        id: 'Extend Green 1 row-0 primary',
        data: [['2026-03-20T00:00:02Z', -90]],
      },
      {
        id: 'TSP Request 1 row-0 primary',
        data: [
          ['2026-03-20T00:00:03Z', -50],
          ['2026-03-20T00:00:05Z', -50],
          null,
        ],
      },
      {
        id: 'TSP Service 1 row-0 primary',
        data: [
          ['2026-03-20T00:00:04Z', 10],
          ['2026-03-20T00:00:06Z', 10],
          null,
        ],
      },
      {
        id: 'PI 2 Eastbound row-1 primary',
        data: [['2026-03-20T00:00:10Z', '2026-03-20T00:00:14Z', 21, 20]],
      },
      {
        id: 'TSP Request 2 row-1 primary',
        data: [
          ['2026-03-20T00:00:13Z', -40],
          ['2026-03-20T00:00:15Z', -40],
          null,
        ],
      },
      {
        id: TIME_SPACE_LOCATION_AXIS_SERIES_ID,
        data: [
          ['2026-03-20T00:00:00Z', 10, '1', null, null, 0],
          ['2026-03-20T00:00:00Z', 20, '2', null, null, 0],
        ],
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

function getSeriesData(chart: MockChart, id: string) {
  const series = Array.isArray(chart.option.series) ? chart.option.series : []
  return series.find((entry) => entry?.id === id)?.data
}

function shiftTimestamp(value: string, offsetMs: number) {
  return dateToTimestamp(new Date(new Date(value).getTime() + offsetMs))
}

describe('useTimeSpaceHandler', () => {
  it('clears cached offsets after an external reset before the next drag', () => {
    const initialOption = buildOption()
    const chart = new MockChart(initialOption)
    const { rerender } = renderHook(
      ({ syncVersion }) => useTimeSpaceHandler(chart as unknown as ECharts, syncVersion),
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
      ({ syncVersion }) => useTimeSpaceHandler(chart as unknown as ECharts, syncVersion),
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

  it('drags the cycle overlays that belong to the selected row', () => {
    const chart = new MockChart(buildOptionWithAssociatedSeries())
    const offsetMs = 2000

    renderHook(() => useTimeSpaceHandler(chart as unknown as ECharts, 0))

    dragGroup(chart, 10, offsetMs)

    expect(getSeriesData(chart, 'Cycle Duration Labels 1 Eastbound')).toEqual([
      [Date.parse('2026-03-20T00:00:07Z'), 10, '5'],
    ])
    expect(getSeriesData(chart, 'PI 1 Eastbound row-0 primary')).toEqual([
      [
        shiftTimestamp('2026-03-20T00:00:00Z', offsetMs),
        shiftTimestamp('2026-03-20T00:00:04Z', offsetMs),
        21,
        10,
      ],
    ])
    expect(getSeriesData(chart, 'Left Turn 1 Eastbound row-0 primary')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:01Z', offsetMs), 10],
      [shiftTimestamp('2026-03-20T00:00:03Z', offsetMs), 12],
      null,
    ])
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
      [shiftTimestamp('2026-03-20T00:00:01Z', offsetMs), -90],
    ])
    expect(getSeriesData(chart, 'Extend Green 1 row-0 primary')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:02Z', offsetMs), -90],
    ])
    expect(getSeriesData(chart, 'TSP Request 1 row-0 primary')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:03Z', offsetMs), -50],
      [shiftTimestamp('2026-03-20T00:00:05Z', offsetMs), -50],
      null,
    ])
    expect(getSeriesData(chart, 'TSP Service 1 row-0 primary')).toEqual([
      [shiftTimestamp('2026-03-20T00:00:04Z', offsetMs), 10],
      [shiftTimestamp('2026-03-20T00:00:06Z', offsetMs), 10],
      null,
    ])
    expect(getSeriesData(chart, 'PI 2 Eastbound row-1 primary')).toEqual([
      ['2026-03-20T00:00:10Z', '2026-03-20T00:00:14Z', 21, 20],
    ])
    expect(getSeriesData(chart, 'TSP Request 2 row-1 primary')).toEqual([
      ['2026-03-20T00:00:13Z', -40],
      ['2026-03-20T00:00:15Z', -40],
      null,
    ])
    expect(getLocationAxisOffsets(chart)).toEqual([2, 0])
  })
})
