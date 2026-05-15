// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceLayout.ts
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
export const TIME_SPACE_MIN_SEGMENT = 2500
export const TIME_SPACE_DISPLAY_DISTANCE_UNITS_PER_PIXEL = 18
export const TIME_SPACE_Y_AXIS_EDGE_BUFFER_PX = 25
export const TIME_SPACE_Y_AXIS_PADDING =
  TIME_SPACE_Y_AXIS_EDGE_BUFFER_PX * TIME_SPACE_DISPLAY_DISTANCE_UNITS_PER_PIXEL
export const TIME_SPACE_MIN_ROW_HEIGHT_PX = 100
export const TIME_SPACE_DISPLAY_HEIGHT_BASE = 220
export const TIME_SPACE_CYCLE_CENTER_OFFSET = 150

export function getDisplayDistanceScale(distanceData: number[]): number {
  const segments = distanceData.map((value, index) => {
    if (index === 0) {
      return 0
    }

    return value - distanceData[index - 1]
  })

  const positiveSegments = segments.filter((segment) => segment > 0)
  const minSegment = positiveSegments.length ? Math.min(...positiveSegments) : 0

  return minSegment > 0 && minSegment < TIME_SPACE_MIN_SEGMENT
    ? TIME_SPACE_MIN_SEGMENT / minSegment
    : 1
}

export function getTimeSpaceChartHeight(
  minDisplayDistance: number,
  maxDisplayDistance: number,
  rowCount: number
): number {
  const minHeightFromRows = rowCount * TIME_SPACE_MIN_ROW_HEIGHT_PX
  const heightFromDistance =
    Math.ceil(
      (maxDisplayDistance -
        minDisplayDistance +
        TIME_SPACE_Y_AXIS_PADDING * 2) /
        TIME_SPACE_DISPLAY_DISTANCE_UNITS_PER_PIXEL
    ) + TIME_SPACE_DISPLAY_HEIGHT_BASE

  return Math.max(
    heightFromDistance,
    minHeightFromRows + TIME_SPACE_DISPLAY_HEIGHT_BASE
  )
}

export function getTimeSpacePhaseRowDistances(
  locationCenterDistances: number[]
) {
  return {
    primaryDistanceData: locationCenterDistances.map(
      (distance) => distance - TIME_SPACE_CYCLE_CENTER_OFFSET
    ),
    opposingDistanceData: [...locationCenterDistances]
      .reverse()
      .map((distance) => distance + TIME_SPACE_CYCLE_CENTER_OFFSET),
  }
}

export function getTimeLikeMs(value: unknown): number | null {
  if (typeof value === 'number' && Number.isFinite(value)) {
    return value
  }

  if (value instanceof Date) {
    const parsed = value.getTime()
    return Number.isFinite(parsed) ? parsed : null
  }

  if (typeof value === 'string') {
    const parsed = Date.parse(value)
    return Number.isFinite(parsed) ? parsed : null
  }

  return null
}

export function getChartTimespanMs(
  start: unknown,
  end: unknown
): number | null {
  const startMs = getTimeLikeMs(start)
  const endMs = getTimeLikeMs(end)

  if (startMs == null || endMs == null || endMs <= startMs) {
    return null
  }

  return endMs - startMs
}
