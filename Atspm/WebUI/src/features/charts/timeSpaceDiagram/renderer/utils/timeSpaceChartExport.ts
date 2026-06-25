// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceChartExport.ts
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
import type {
  DataZoomComponentOption,
  EChartsOption,
  GridComponentOption,
  TitleComponentOption,
  XAXisComponentOption,
} from 'echarts'

const EXPORT_TITLE_GAP_PX = 12
const DEFAULT_TITLE_FONT_SIZE = 18

function normalizeToArray<T>(value: T | T[] | undefined): T[] {
  if (value == null) {
    return []
  }

  return Array.isArray(value) ? value : [value]
}

function getNumericValue(value: unknown) {
  if (typeof value === 'number' && Number.isFinite(value)) {
    return value
  }

  if (typeof value === 'string') {
    const parsed = Number.parseFloat(value)
    return Number.isFinite(parsed) ? parsed : null
  }

  return null
}

function getTitlePaddingVertical(
  padding: TitleComponentOption['padding']
) {
  if (typeof padding === 'number' && Number.isFinite(padding)) {
    return padding * 2
  }

  if (!Array.isArray(padding) || padding.length === 0) {
    return 0
  }

  if (padding.length === 1) {
    return padding[0] * 2
  }

  if (padding.length === 2) {
    return padding[0] * 2
  }

  if (padding.length === 3) {
    return padding[0] + padding[2]
  }

  return padding[0] + padding[2]
}

function getTitleFontSize(entry: TitleComponentOption) {
  const directFontSize = getNumericValue(entry.textStyle?.fontSize)
  if (directFontSize != null) {
    return directFontSize
  }

  const richStyles = entry.textStyle?.rich
  if (richStyles && typeof richStyles === 'object') {
    for (const richStyle of Object.values(richStyles)) {
      const richFontSize = getNumericValue(richStyle?.fontSize)
      if (richFontSize != null) {
        return richFontSize
      }
    }
  }

  return DEFAULT_TITLE_FONT_SIZE
}

function estimateTitleEntryBottom(entry: TitleComponentOption) {
  const top = getNumericValue(entry.top) ?? 0
  const fontSize = getTitleFontSize(entry)
  const paddingVertical = getTitlePaddingVertical(entry.padding)
  const lineCount = String(entry.text ?? '')
    .split('\n')
    .filter(Boolean).length || 1

  return top + fontSize * 1.2 * lineCount + paddingVertical
}

function getRequiredExportGridTop(sourceOption: EChartsOption) {
  const titleEntries = normalizeToArray(sourceOption.title)
  if (titleEntries.length === 0) {
    return null
  }

  const titleBottom = titleEntries.reduce((maxBottom, entry) => {
    if (!entry || typeof entry !== 'object') {
      return maxBottom
    }

    return Math.max(maxBottom, estimateTitleEntryBottom(entry))
  }, 0)

  return Math.ceil(titleBottom + EXPORT_TITLE_GAP_PX)
}

function mergeAxisForExport(
  currentAxis: XAXisComponentOption | undefined,
  sourceAxis: XAXisComponentOption | undefined
) {
  if (!currentAxis) {
    return sourceAxis
  }

  if (!sourceAxis) {
    return currentAxis
  }

  return {
    ...currentAxis,
    ...sourceAxis,
  }
}

function restoreSliderVisibility(
  currentZoom: DataZoomComponentOption | undefined,
  sourceZoom: DataZoomComponentOption | undefined
) {
  if (!currentZoom) {
    return sourceZoom
  }

  if (
    currentZoom.type === 'slider' &&
    (currentZoom.orient ?? 'horizontal') === 'horizontal'
  ) {
    return {
      ...sourceZoom,
      ...currentZoom,
      show: true,
    }
  }

  return {
    ...sourceZoom,
    ...currentZoom,
  }
}

function mergeGridForExport(
  currentGrid: GridComponentOption | undefined,
  sourceGrid: GridComponentOption | undefined,
  requiredTop: number | null
) {
  if (!currentGrid && !sourceGrid) {
    return undefined
  }

  const mergedGrid = {
    ...sourceGrid,
    ...currentGrid,
  }

  if (requiredTop == null) {
    return mergedGrid
  }

  const currentTop = getNumericValue(mergedGrid.top) ?? 0

  return {
    ...mergedGrid,
    top: Math.max(currentTop, requiredTop),
  }
}

export function buildTimeSpaceExportOption(
  currentOption: EChartsOption,
  sourceOption: EChartsOption
): EChartsOption {
  const currentAxes = normalizeToArray(currentOption.xAxis)
  const sourceAxes = normalizeToArray(sourceOption.xAxis)
  const axisCount = Math.max(currentAxes.length, sourceAxes.length)

  const nextXAxis =
    axisCount > 0
      ? Array.from({ length: axisCount }, (_, index) =>
          mergeAxisForExport(currentAxes[index], sourceAxes[index])
        )
      : currentOption.xAxis

  const currentZooms = normalizeToArray(currentOption.dataZoom)
  const sourceZooms = normalizeToArray(sourceOption.dataZoom)
  const zoomCount = Math.max(currentZooms.length, sourceZooms.length)

  const nextDataZoom =
    zoomCount > 0
      ? Array.from({ length: zoomCount }, (_, index) =>
          restoreSliderVisibility(currentZooms[index], sourceZooms[index])
        )
      : currentOption.dataZoom

  const requiredGridTop = getRequiredExportGridTop(sourceOption)
  const currentGrids = normalizeToArray(currentOption.grid)
  const sourceGrids = normalizeToArray(sourceOption.grid)
  const gridCount = Math.max(currentGrids.length, sourceGrids.length)

  const nextGrid =
    gridCount > 0
      ? Array.from({ length: gridCount }, (_, index) =>
          mergeGridForExport(
            currentGrids[index],
            sourceGrids[index],
            requiredGridTop
          )
        )
      : currentOption.grid

  return {
    ...currentOption,
    animation: false,
    title: sourceOption.title ?? currentOption.title,
    xAxis: nextXAxis,
    dataZoom: nextDataZoom,
    grid: nextGrid,
  }
}
