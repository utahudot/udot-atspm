import type { EChartsOption, SeriesOption } from 'echarts'
import { buildTimeSpaceExportOption } from './timeSpaceChartExport'

describe('buildTimeSpaceExportOption', () => {
  it('restores the source title and stripped axis visuals while preserving live chart state', () => {
    const currentOption: EChartsOption = {
      animation: true,
      title: [],
      grid: {
        top: 30,
        right: 10,
      },
      xAxis: [
        {
          type: 'time',
          name: '',
          show: false,
          axisLabel: { show: false },
          axisLine: { show: false },
          axisTick: { show: false },
        },
        {
          type: 'value',
          position: 'top',
          name: '',
          show: false,
          axisLabel: { show: false },
          axisLine: { show: false },
          axisTick: { show: false },
          min: 0,
          max: 900,
        },
      ],
      dataZoom: [
        {
          type: 'slider',
          orient: 'horizontal',
          show: false,
          start: 20,
          end: 80,
        },
        {
          type: 'inside',
          start: 20,
          end: 80,
        },
      ],
      series: [
        {
          id: 'Cycles primary',
          type: 'line',
          data: [['2026-04-23T16:00:00Z', 0]],
        },
      ],
      legend: {
        show: false,
        selected: {
          'Cycles primary': false,
        },
      },
    }

    const sourceOption: EChartsOption = {
      title: [
        {
          text: 'Time Space Diagram - 50th Percentile',
          top: 0,
          textStyle: {
            fontSize: 18,
          },
        },
        {
          text: 'Wed, Feb 19, 2025 - 16:00-16:20',
          top: 27,
          textStyle: {
            fontSize: 15,
          },
        },
      ],
      grid: {
        top: 30,
      },
      xAxis: [
        {
          type: 'time',
          name: 'Time',
          show: true,
          axisLabel: { show: true },
          axisLine: { show: true },
          axisTick: { show: true },
        },
        {
          type: 'value',
          position: 'top',
          name: 'Time Since Start (seconds)',
          show: true,
          axisLabel: { show: true },
          axisLine: { show: true },
          axisTick: { show: true },
        },
      ],
      dataZoom: [
        {
          type: 'slider',
          orient: 'horizontal',
          show: true,
          start: 0,
          end: 100,
        },
        {
          type: 'inside',
          start: 0,
          end: 100,
        },
      ],
    }

    const exportOption = buildTimeSpaceExportOption(currentOption, sourceOption)
    const exportAxes = Array.isArray(exportOption.xAxis)
      ? exportOption.xAxis
      : [exportOption.xAxis]
    const exportZooms = Array.isArray(exportOption.dataZoom)
      ? exportOption.dataZoom
      : [exportOption.dataZoom]
    const exportGrids = Array.isArray(exportOption.grid)
      ? exportOption.grid
      : [exportOption.grid]
    const exportSeries = Array.isArray(exportOption.series)
      ? (exportOption.series as SeriesOption[])
      : []

    expect(exportOption.animation).toBe(false)
    expect(exportOption.title).toEqual(sourceOption.title)
    expect(exportAxes[0]).toMatchObject({
      name: 'Time',
      show: true,
      axisLabel: { show: true },
    })
    expect(exportAxes[1]).toMatchObject({
      name: 'Time Since Start (seconds)',
      position: 'top',
      show: true,
      min: 0,
      max: 900,
    })
    expect(exportZooms[0]).toMatchObject({
      type: 'slider',
      orient: 'horizontal',
      show: true,
      start: 20,
      end: 80,
    })
    expect(exportZooms[1]).toMatchObject({
      type: 'inside',
      start: 20,
      end: 80,
    })
    expect(exportGrids[0]).toMatchObject({
      right: 10,
      top: 57,
    })
    expect(exportSeries).toEqual(currentOption.series)
    expect(exportOption.legend).toEqual(currentOption.legend)
  })
})
