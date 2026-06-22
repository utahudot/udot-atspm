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
export const TIME_SPACE_MIN_SEGMENT = 2000
export const TIME_SPACE_DISPLAY_DISTANCE_UNITS_PER_PIXEL = 18
export const TIME_SPACE_Y_AXIS_EDGE_BUFFER_PX = 25
export const TIME_SPACE_Y_AXIS_PADDING =
  TIME_SPACE_Y_AXIS_EDGE_BUFFER_PX * TIME_SPACE_DISPLAY_DISTANCE_UNITS_PER_PIXEL
export const TIME_SPACE_MIN_ROW_HEIGHT_PX = 100
export const TIME_SPACE_DISPLAY_HEIGHT_BASE = 220
export const TIME_SPACE_GRID_TOP = 30
export const TIME_SPACE_GRID_BOTTOM = 80
export const TIME_SPACE_CYCLE_SEGMENT_HEIGHT_PX = 17
export const TIME_SPACE_CYCLE_CENTER_OFFSET =
  (TIME_SPACE_CYCLE_SEGMENT_HEIGHT_PX / 2) *
  TIME_SPACE_DISPLAY_DISTANCE_UNITS_PER_PIXEL
export const TIME_SPACE_SEQUENCE_SEGMENT = TIME_SPACE_MIN_SEGMENT
export const TIME_SPACE_HYBRID_BIN_STEP = 500

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

export function getSequenceDistanceData(rowCount: number): number[] {
  return Array.from(
    { length: Math.max(0, rowCount) },
    (_, index) => index * TIME_SPACE_SEQUENCE_SEGMENT
  )
}

export function getHybridDistanceData(rawDistanceData: number[]): number[] {
  if (!rawDistanceData.length) {
    return []
  }

  const rawSegments = rawDistanceData.map((distance, index) => {
    if (index === 0) {
      return 0
    }

    return distance - rawDistanceData[index - 1]
  })
  const positiveSegments = rawSegments.filter((segment) => segment > 0)
  const shortestSegment = positiveSegments.length
    ? Math.min(...positiveSegments)
    : 0

  if (shortestSegment <= 0) {
    return getSequenceDistanceData(rawDistanceData.length)
  }

  const displayBaseSegment = Math.max(shortestSegment, TIME_SPACE_MIN_SEGMENT)
  const displayDistanceData = [0]

  for (let index = 1; index < rawDistanceData.length; index++) {
    const rawSegment = rawSegments[index]
    const displaySegment = getHybridDisplaySegment(
      rawSegment,
      shortestSegment,
      displayBaseSegment
    )
    displayDistanceData.push(displayDistanceData[index - 1] + displaySegment)
  }

  return displayDistanceData
}

function getHybridDisplaySegment(
  rawSegment: number,
  shortestSegment: number,
  displayBaseSegment: number
) {
  const shortMax = shortestSegment + TIME_SPACE_HYBRID_BIN_STEP
  const mediumMax = shortMax + TIME_SPACE_HYBRID_BIN_STEP
  const longMax = mediumMax + TIME_SPACE_HYBRID_BIN_STEP

  if (rawSegment <= shortMax) {
    return displayBaseSegment
  }

  if (rawSegment <= mediumMax) {
    return displayBaseSegment + TIME_SPACE_HYBRID_BIN_STEP
  }

  if (rawSegment <= longMax) {
    return displayBaseSegment + TIME_SPACE_HYBRID_BIN_STEP * 2.5
  }

  return displayBaseSegment + TIME_SPACE_HYBRID_BIN_STEP * 4
}

export function getDistanceAtDisplayCoordinate(
  rawDistanceData: number[],
  displayDistanceData: number[],
  rawDistance: number
): number {
  if (!rawDistanceData.length || !displayDistanceData.length) {
    return rawDistance
  }

  if (rawDistanceData.length === 1 || displayDistanceData.length === 1) {
    return displayDistanceData[0] ?? rawDistance
  }

  const lastIndex = rawDistanceData.length - 1

  if (rawDistance <= rawDistanceData[0]) {
    return extrapolateDisplayDistance(
      rawDistanceData,
      displayDistanceData,
      0,
      1,
      rawDistance
    )
  }

  for (let index = 1; index < rawDistanceData.length; index++) {
    if (rawDistance <= rawDistanceData[index]) {
      return interpolateDisplayDistance(
        rawDistanceData,
        displayDistanceData,
        index - 1,
        index,
        rawDistance
      )
    }
  }

  return extrapolateDisplayDistance(
    rawDistanceData,
    displayDistanceData,
    lastIndex - 1,
    lastIndex,
    rawDistance
  )
}

function interpolateDisplayDistance(
  rawDistanceData: number[],
  displayDistanceData: number[],
  startIndex: number,
  endIndex: number,
  rawDistance: number
) {
  const rawStart = rawDistanceData[startIndex]
  const rawEnd = rawDistanceData[endIndex]
  const displayStart = displayDistanceData[startIndex]
  const displayEnd = displayDistanceData[endIndex]
  const rawSpan = rawEnd - rawStart

  if (!Number.isFinite(rawSpan) || Math.abs(rawSpan) < 0.0001) {
    return displayStart
  }

  const ratio = (rawDistance - rawStart) / rawSpan
  return displayStart + ratio * (displayEnd - displayStart)
}

function extrapolateDisplayDistance(
  rawDistanceData: number[],
  displayDistanceData: number[],
  startIndex: number,
  endIndex: number,
  rawDistance: number
) {
  const rawStart = rawDistanceData[startIndex]
  const rawEnd = rawDistanceData[endIndex]
  const displayStart = displayDistanceData[startIndex]
  const displayEnd = displayDistanceData[endIndex]
  const rawSpan = rawEnd - rawStart

  if (!Number.isFinite(rawSpan) || Math.abs(rawSpan) < 0.0001) {
    return displayStart
  }

  return (
    displayStart +
    ((rawDistance - rawStart) / rawSpan) * (displayEnd - displayStart)
  )
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
  const cycleCenterOffset = getCycleCenterOffsetDataUnits(
    locationCenterDistances
  )

  return {
    primaryDistanceData: locationCenterDistances.map(
      (distance) => distance - cycleCenterOffset
    ),
    opposingDistanceData: [...locationCenterDistances]
      .reverse()
      .map((distance) => distance + cycleCenterOffset),
  }
}

function getCycleCenterOffsetDataUnits(locationCenterDistances: number[]) {
  if (!locationCenterDistances.length) {
    return TIME_SPACE_CYCLE_CENTER_OFFSET
  }

  const minCenterDistance = Math.min(...locationCenterDistances)
  const maxCenterDistance = Math.max(...locationCenterDistances)
  const targetCenterOffsetPx = TIME_SPACE_CYCLE_SEGMENT_HEIGHT_PX / 2
  let centerOffsetDataUnits = TIME_SPACE_CYCLE_CENTER_OFFSET

  for (let index = 0; index < 5; index++) {
    const minDisplayDistance = minCenterDistance - centerOffsetDataUnits
    const maxDisplayDistance = maxCenterDistance + centerOffsetDataUnits
    const chartHeight = getTimeSpaceChartHeight(
      minDisplayDistance,
      maxDisplayDistance,
      locationCenterDistances.length
    )
    const plotHeight =
      chartHeight - TIME_SPACE_GRID_TOP - TIME_SPACE_GRID_BOTTOM
    const axisRange =
      maxDisplayDistance -
      minDisplayDistance +
      TIME_SPACE_Y_AXIS_PADDING * 2

    if (plotHeight <= 0 || axisRange <= 0) {
      return TIME_SPACE_CYCLE_CENTER_OFFSET
    }

    centerOffsetDataUnits = (axisRange / plotHeight) * targetCenterOffsetPx
  }

  return centerOffsetDataUnits
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
