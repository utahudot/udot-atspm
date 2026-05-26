// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceChartScaffold.ts
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
import { createXAxis } from '@/features/charts/common/transformers'
import type {
  DataZoomComponentOption,
  EChartsOption,
  GridComponentOption,
  LegendComponentOption,
  SeriesOption,
  TitleComponentOption,
  ToolboxComponentOption,
  XAXisComponentOption,
  YAXisComponentOption,
} from 'echarts'

export function buildTimeSpaceTopSecondsAxis(
  chartStartMs: number,
  chartEndMs: number
) {
  const totalSeconds = Math.floor((chartEndMs - chartStartMs) / 1000)

  return {
    type: 'value',
    position: 'top',
    nameGap: 25,
    min: 0,
    max: totalSeconds,
    name: 'Time Since Start (seconds)',
    nameLocation: 'middle',
    minInterval: 60,
    maxInterval: 60,
    minorTick: { show: true, splitNumber: 4 },
    axisLabel: {
      formatter: (value: number) => String(Math.round(value / 1) * 1),
    },
  } as const
}

export function buildTimeSpaceXAxis(
  start: string,
  end: string,
  chartStartMs: number,
  chartEndMs: number
) {
  return [
    {
      ...createXAxis(start, end),
      axisLine: {
        onZero: false,
      },
    },
    { min: 0, max: 1, show: false },
    buildTimeSpaceTopSecondsAxis(chartStartMs, chartEndMs),
  ] as XAXisComponentOption[]
}

export function buildTimeSpaceGrid(
  overrides?: Partial<GridComponentOption>
): GridComponentOption {
  return {
    top: 30,
    left: 220,
    right: 195,
    bottom: 80,
    show: true,
    borderWidth: 1,
    ...overrides,
  }
}

export function buildTimeSpaceDataZoom(
  chartStartMs: number,
  chartEndMs: number
): DataZoomComponentOption[] {
  const timeDiffHours = (chartEndMs - chartStartMs) / 3_600_000

  if (timeDiffHours > 6) {
    return [
      {
        type: 'slider',
        filterMode: 'filter',
        show: true,
        start: 0,
        end: 10,
        maxSpan: 10,
        minSpan: 0.2,
      },
    ]
  }

  return [
    {
      type: 'slider',
      filterMode: 'none',
      show: true,
    },
  ]
}

export function buildTimeSpaceToolbox(saveAsImageName: string) {
  return {
    feature: {
      saveAsImage: { name: saveAsImageName },
      dataView: {
        readOnly: true,
      },
      restore: {},
    },
  } satisfies ToolboxComponentOption
}

interface BuildTimeSpaceChartScaffoldProps {
  title: TitleComponentOption[]
  xAxis: XAXisComponentOption[]
  yAxis: YAXisComponentOption[]
  grid: GridComponentOption
  dataZoom: DataZoomComponentOption[]
  legend: LegendComponentOption
  toolbox: ToolboxComponentOption
  series: SeriesOption[]
  displayProps: Record<string, unknown>
  animation?: boolean
}

export function buildTimeSpaceChartScaffold({
  title,
  xAxis,
  yAxis,
  grid,
  dataZoom,
  legend,
  toolbox,
  series,
  displayProps,
  animation = true,
}: BuildTimeSpaceChartScaffoldProps): EChartsOption {
  return {
    title,
    xAxis,
    yAxis,
    grid,
    dataZoom,
    legend,
    toolbox,
    animation,
    series,
    displayProps,
    responsive: true,
    maintainAspectRatio: false,
  }
}
