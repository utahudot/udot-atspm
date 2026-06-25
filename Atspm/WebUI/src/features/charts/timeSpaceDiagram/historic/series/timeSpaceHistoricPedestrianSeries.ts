// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceHistoricPedestrianSeries.ts
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
import { Color } from '@/features/charts/utils'
import { RawTimeSpaceHistoricData } from '@/features/charts/timeSpaceDiagram/shared/types'
import { PedestrianInterval } from '@/features/charts/timingAndActuation/types'
import { CustomSeriesRenderItemReturn, SeriesOption } from 'echarts'

const PEDESTRIAN_LINE_WIDTH = 0.8
const PEDESTRIAN_LINE_OFFSET_PX = 13
const PEDESTRIAN_ZIGZAG_AMPLITUDE = 2
const PEDESTRIAN_ZIGZAG_STEP_PX = 3
const PEDESTRIAN_CLEARANCE_DOT_PATTERN = [1, 3]
const PEDESTRIAN_BOUNDARY_TICK_HALF_HEIGHT = PEDESTRIAN_ZIGZAG_AMPLITUDE

export function generatePedestrianIntervalLines(
  data: RawTimeSpaceHistoricData[],
  distanceData: number[],
  phaseType?: string,
  idScope = 'default'
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []
  const directionMultiplier = idScope === 'opposing' ? -1 : 1

  data.forEach((location, i) => {
    if (!location.pedestrianIntervals?.length) return

    const pedData = generatePedData(
      location.pedestrianIntervals,
      distanceData[i]
    )

    const series: SeriesOption = {
      name: `Pedestrian Interval ${phaseType?.length ? phaseType : ''}`,
      id: `PI ${location.locationIdentifier} ${phaseType ?? ''} row-${i} ${idScope}`,
      type: 'custom',
      clip: true,
      data: pedData,
      encode: {
        x: [1, 2],
        y: [3],
      },
      z: 6,
      renderItem: (_param, api): CustomSeriesRenderItemReturn => {
        const x1 = api.value(0)
        const x2 = api.value(1)
        const interval = api.value(2)
        const distance = api.value(3)
        const p1 = api.coord([x1, distance])
        const p2 = api.coord([x2, distance])
        const y = p1[1] + directionMultiplier * PEDESTRIAN_LINE_OFFSET_PX

        return createPedestrianIntervalShape(
          interval as number,
          p1[0],
          p2[0],
          y
        )
      },
    }

    seriesOptions.push(series)
  })

  return seriesOptions
}

function generatePedData(
  pedestrianIntervals: PedestrianInterval[],
  distance: number
) {
  return pedestrianIntervals.map((interval, index, array) => {
    const startCoord = interval.start
    const endCoord =
      index < array.length - 1 ? array[index + 1].start : startCoord

    return [startCoord, endCoord, interval.value, distance]
  })
}

function createPedestrianIntervalShape(
  intervalValue: number,
  startX: number,
  endX: number,
  y: number
): CustomSeriesRenderItemReturn {
  const baseStyle = {
    stroke: Color.Black,
    lineWidth: PEDESTRIAN_LINE_WIDTH,
    fill: 'none',
    lineCap: 'round',
    lineJoin: 'round',
  }
  const boundaryTick = {
    type: 'line' as const,
    shape: {
      x1: startX,
      y1: y - PEDESTRIAN_BOUNDARY_TICK_HALF_HEIGHT,
      x2: startX,
      y2: y + PEDESTRIAN_BOUNDARY_TICK_HALF_HEIGHT,
    },
    style: baseStyle,
  }
  let intervalShape

  if (intervalValue === 22 || intervalValue === 68) {
    intervalShape = {
      type: 'line' as const,
      shape: { x1: startX, y1: y, x2: endX, y2: y },
      style: { ...baseStyle, lineDash: PEDESTRIAN_CLEARANCE_DOT_PATTERN },
    }
  } else if (intervalValue === 23 || intervalValue === 69) {
    intervalShape = {
      type: 'polyline' as const,
      shape: {
        points: buildPedestrianZigZagPoints(startX, endX, y),
      },
      style: baseStyle,
    }
  } else {
    intervalShape = {
      type: 'line' as const,
      shape: { x1: startX, y1: y, x2: endX, y2: y },
      style: baseStyle,
    }
  }

  return {
    type: 'group',
    children: [intervalShape, boundaryTick],
  }
}

function buildPedestrianZigZagPoints(
  startX: number,
  endX: number,
  baseY: number
): Array<[number, number]> {
  const width = endX - startX
  if (Math.abs(width) <= PEDESTRIAN_ZIGZAG_STEP_PX) {
    return [
      [startX, baseY],
      [endX, baseY],
    ]
  }

  const segmentCount = Math.max(
    3,
    Math.ceil(Math.abs(width) / PEDESTRIAN_ZIGZAG_STEP_PX)
  )
  const step = width / segmentCount
  const points: Array<[number, number]> = [[startX, baseY]]

  for (let i = 1; i < segmentCount; i++) {
    const x = startX + step * i
    const nextY =
      baseY +
      (i % 2 === 0 ? PEDESTRIAN_ZIGZAG_AMPLITUDE : -PEDESTRIAN_ZIGZAG_AMPLITUDE)
    points.push([x, nextY])
  }

  points.push([endX, baseY])

  return points
}
