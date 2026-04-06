import { act, renderHook } from '@testing-library/react'
import type { ECharts, EChartsOption, SeriesOption } from 'echarts'
import type { GpxUploadOptions } from '../types'
import {
  mergeChartSeriesWithGpxOverlays,
  useGpxAnimationHandler,
} from './gpxAnimation.handler'

class MockChart {
  option: EChartsOption & { displayProps?: { locations: string[] } }

  constructor(option: EChartsOption & { displayProps?: { locations: string[] } }) {
    this.option = cloneOption(option)
  }

  getOption() {
    return this.option
  }

  setOption(nextOption: EChartsOption) {
    this.option = {
      ...this.option,
      ...nextOption,
      series: Array.isArray(nextOption.series)
        ? cloneSeries(nextOption.series as Array<SeriesOption | null | undefined>)
        : this.option.series,
    }
  }
}

function cloneSeries(series: Array<SeriesOption | null | undefined>) {
  return JSON.parse(JSON.stringify(series)) as EChartsOption['series']
}

function cloneOption(
  option: EChartsOption & { displayProps?: { locations: string[] } }
) {
  return JSON.parse(JSON.stringify(option)) as EChartsOption & {
    displayProps?: { locations: string[] }
  }
}

function buildChartOption(): EChartsOption & {
  displayProps: { locations: string[] }
} {
  return {
    displayProps: {
      locations: ['5600', '5700'],
    },
    yAxis: [
      {
        data: [0, 300],
      },
    ],
    series: [
      {
        id: 'Cycles EB',
        name: 'Cycles EB',
        type: 'line',
        data: [['2025-02-17T13:00:00', 0]],
      },
      {
        id: 'Location axis',
        name: 'Location axis',
        type: 'custom',
        data: [],
      },
      {
        id: 'gpx-stale',
        name: 'GPX stale',
        type: 'line',
        data: [['2025-02-17T13:00:00', 10]],
      },
      {
        id: 'srm-stale-bus-1-0',
        name: 'SRM stale',
        type: 'line',
        data: [['2025-02-17T13:01:00', 12]],
      },
    ],
  }
}

function buildUploads(): GpxUploadOptions[] {
  return [
    {
      id: 'upload-1',
      startLocation: '5600',
      endLocation: '5700',
      parsedData: [
        { time: '2025-02-17T13:06:00', distance: 0 },
        { time: '2025-02-17T13:07:00', distance: 35 },
      ],
    },
  ]
}

function getSeriesIds(chart: MockChart) {
  return (Array.isArray(chart.option.series) ? chart.option.series : [])
    .map((series) => (typeof series?.id === 'string' ? series.id : ''))
    .filter(Boolean)
}

function getSeriesById(chart: MockChart, id: string) {
  return (Array.isArray(chart.option.series) ? chart.option.series : []).find(
    (series) => series?.id === id
  )
}

describe('gpxAnimation handler', () => {
  it('replaces only GPX overlay series and preserves the base diagram series', () => {
    const merged = mergeChartSeriesWithGpxOverlays(
      [
        { id: 'Cycles EB', type: 'line', data: [] },
        { id: 'gpx-stale', type: 'line', data: [] },
        { id: 'srm-stale-1', type: 'line', data: [] },
        { id: 'Location axis', type: 'custom', data: [] },
      ],
      [{ id: 'gpx-upload-1', type: 'line', data: [] }]
    )

    expect(merged.map((series) => series.id)).toEqual([
      'Cycles EB',
      'Location axis',
      'gpx-upload-1',
    ])
  })

  it('keeps the time-space series visible when GPX overlays are applied and removes stale overlays when cleared', () => {
    const chart = new MockChart(buildChartOption())
    const { result, rerender } = renderHook(
      ({ uploads }) =>
        useGpxAnimationHandler(chart as unknown as ECharts, uploads),
      {
        initialProps: {
          uploads: buildUploads(),
        },
      }
    )

    act(() => {
      result.current.play()
    })

    expect(getSeriesIds(chart)).toEqual([
      'Cycles EB',
      'Location axis',
      'gpx-upload-1',
    ])
    expect(getSeriesById(chart, 'gpx-upload-1')?.name).toBe('GPX Tracks')

    rerender({ uploads: [] })

    act(() => {
      result.current.play()
    })

    expect(getSeriesIds(chart)).toEqual(['Cycles EB', 'Location axis'])
  })
})
