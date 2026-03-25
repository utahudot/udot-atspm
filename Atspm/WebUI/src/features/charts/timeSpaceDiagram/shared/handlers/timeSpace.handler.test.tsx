import { renderHook, act } from '@testing-library/react'
import type { ECharts, EChartsOption } from 'echarts'
import { TIME_SPACE_LOCATION_AXIS_SERIES_ID } from '../transformers/timeSpaceTransformerBase'
import { useTimeSpaceHandler } from './timeSpace.handler'

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
    const updates = nextOption.series

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
      }),
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
})
