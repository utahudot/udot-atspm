// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceChartGeometry.ts
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
import {
  formatSignedOffsetSeconds,
  getTimeSpaceLocationOffsetBadgeLayout,
  hasModifiedOffset,
  TIME_SPACE_LOCATION_CARD_LAYOUT,
} from '@/features/charts/timeSpaceDiagram/shared/transformers/timeSpaceTransformerBase'
import type { ECharts, EChartsOption, SeriesOption } from 'echarts'

export type LocationToggleButton = {
  left: number
  top: number
  location: string
}

export type OffsetResetButton = {
  active: boolean
  height: number
  left: number
  location: string
  top: number
  width: number
}

type LocationAxisDatum = {
  actualOffset: number | null
  distance: number
  location: string
  offset: number
  time: string | number
  userAdjustment: number
}

type GridRectProvider = ECharts & {
  getModel?: () => {
    getComponent?: (
      mainType: string,
      index: number
    ) => {
      coordinateSystem?: {
        getRect?: () => {
          x?: number
          width?: number
        }
      }
    }
  }
}

function getGridLeft(chart: ECharts, option?: EChartsOption) {
  const gridRect = (chart as GridRectProvider)
    ?.getModel?.()
    ?.getComponent?.('grid', 0)
    ?.coordinateSystem?.getRect?.()

  if (typeof gridRect?.x === 'number') {
    return gridRect.x
  }

  const grid = Array.isArray(option?.grid) ? option.grid[0] : option?.grid
  return typeof grid?.left === 'number' ? grid.left : 0
}

export function getGridRect(chart: ECharts, option?: EChartsOption) {
  let gridRect:
    | {
        x?: number
        width?: number
      }
    | undefined

  try {
    gridRect = (chart as GridRectProvider)
      ?.getModel?.()
      ?.getComponent?.('grid', 0)
      ?.coordinateSystem?.getRect?.()
  } catch {
    gridRect = undefined
  }

  if (typeof gridRect?.x === 'number' && typeof gridRect?.width === 'number') {
    return {
      x: gridRect.x,
      width: gridRect.width,
    }
  }

  const grid = Array.isArray(option?.grid) ? option.grid[0] : option?.grid
  const left = typeof grid?.left === 'number' ? grid.left : 0
  const right = typeof grid?.right === 'number' ? grid.right : 0
  let chartWidth = 0

  try {
    const dom = chart.getDom?.()
    if (dom instanceof HTMLElement && Number.isFinite(dom.clientWidth)) {
      chartWidth = dom.clientWidth
    } else {
      chartWidth = chart.getWidth()
    }
  } catch {
    chartWidth = 0
  }

  return {
    x: left,
    width: Math.max(0, chartWidth - left - right),
  }
}

function getLocationAxisData(option?: EChartsOption): LocationAxisDatum[] {
  const series = Array.isArray(option?.series)
    ? (option.series as Array<SeriesOption | null | undefined>).filter(
        (entry): entry is SeriesOption =>
          Boolean(entry && typeof entry === 'object')
      )
    : []

  const locationSeries = series.find((entry) => entry.name === 'Location axis')
  if (!locationSeries || !Array.isArray(locationSeries.data)) {
    return []
  }

  return locationSeries.data
    .map((item) => {
      const value = Array.isArray(item)
        ? item
        : Array.isArray((item as { value?: unknown[] })?.value)
          ? ((item as { value?: unknown[] }).value as unknown[])
          : null

      if (!value) return null

      const time = value[0]
      const distance = Number(value[1])
      const location = String(value[2] ?? '')
      const offset = Number(value[5] ?? 0)
      const rawActualOffset = value[6]
      const rawUserAdjustment = value[7]
      const actualOffset =
        rawActualOffset == null || rawActualOffset === ''
          ? null
          : Number(rawActualOffset)
      const userAdjustment =
        rawUserAdjustment == null || rawUserAdjustment === ''
          ? 0
          : Number(rawUserAdjustment)

      if (!location || !Number.isFinite(distance)) return null

      return {
        actualOffset: Number.isFinite(actualOffset) ? actualOffset : null,
        time: typeof time === 'string' || typeof time === 'number' ? time : '',
        distance,
        location,
        offset: Number.isFinite(offset) ? offset : 0,
        userAdjustment: Number.isFinite(userAdjustment) ? userAdjustment : 0,
      }
    })
    .filter((item): item is LocationAxisDatum => item !== null)
}

function getChartPixel(
  chart: ECharts,
  time: string | number,
  distance: number
): [number, number] | null {
  try {
    const pixel = chart.convertToPixel({ xAxisIndex: 0, yAxisIndex: 0 }, [
      time,
      distance,
    ])

    if (
      !Array.isArray(pixel) ||
      pixel.length < 2 ||
      !Number.isFinite(pixel[0]) ||
      !Number.isFinite(pixel[1])
    ) {
      return null
    }

    return [pixel[0], pixel[1]]
  } catch {
    return null
  }
}

export function buildLocationToggleButtons(
  chart: ECharts,
  option?: EChartsOption
): LocationToggleButton[] {
  const gridLeft = getGridLeft(chart, option)
  const locationAxisData = getLocationAxisData(option)

  if (!locationAxisData.length) {
    return []
  }

  const {
    gridGap,
    dotOffset,
    cardGapToDot,
    headerHeight,
    bodyHeight,
    headerActionSize,
    headerActionRight,
    headerActionOverlayOffsetX,
    headerActionOverlayOffsetY,
    verticalOffsetY,
  } = TIME_SPACE_LOCATION_CARD_LAYOUT

  const cardHeight = headerHeight + bodyHeight
  const xTextRight = gridLeft - gridGap
  const xDot = xTextRight + dotOffset
  const cardRight = xDot - cardGapToDot
  const iconLeft = cardRight - headerActionRight - headerActionSize

  return locationAxisData
    .map(({ distance, location, time }) => {
      const pixel = getChartPixel(chart, time, distance)
      if (!pixel) return null

      const [, y] = pixel

      if (!Number.isFinite(y)) {
        return null
      }

      const cardTop = y - cardHeight / 2 + verticalOffsetY

      return {
        location,
        left: iconLeft + headerActionOverlayOffsetX,
        top:
          cardTop +
          (headerHeight - headerActionSize) / 2 +
          headerActionOverlayOffsetY,
      }
    })
    .filter((item): item is LocationToggleButton => item !== null)
}

export function buildOffsetResetButtons(
  chart: ECharts,
  option?: EChartsOption
): OffsetResetButton[] {
  const gridLeft = getGridLeft(chart, option)
  const locationAxisData = getLocationAxisData(option)

  if (!locationAxisData.length) {
    return []
  }

  return locationAxisData
    .map(
      ({ actualOffset, distance, location, offset, time, userAdjustment }) => {
        if (!Number.isFinite(offset)) {
          return null
        }

        const pixel = getChartPixel(chart, time, distance)
        if (!pixel) return null

        const [, y] = pixel
        if (!Number.isFinite(y)) {
          return null
        }

        const badgeLayout = getTimeSpaceLocationOffsetBadgeLayout(
          gridLeft,
          y,
          formatSignedOffsetSeconds(offset),
          false
        )

        return {
          active: hasModifiedOffset(offset, actualOffset, userAdjustment),
          height: badgeLayout.highlightHeight,
          left: badgeLayout.highlightX,
          location,
          top: badgeLayout.highlightY,
          width: badgeLayout.highlightWidth,
        }
      }
    )
    .filter((item): item is OffsetResetButton => item !== null)
}
