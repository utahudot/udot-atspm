// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - timeSpaceTransformerBase.ts
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
  getChartTimespanMs,
  getTimeLikeMs,
} from '@/features/charts/timeSpaceDiagram/core/math/timeSpaceLayout'
import type {
  TimeSpaceDetectorEvent,
  TimeSpaceUnwrappedData,
} from '@/features/charts/timeSpaceDiagram/shared/types'
import { Cycle } from '@/features/charts/timingAndActuation/types'
import { dateToTimestamp } from '@/utils/dateTime'
import type {
  CustomSeriesRenderItemAPI,
  CustomSeriesRenderItemParams,
  CustomSeriesRenderItemReturn,
  SeriesOption,
} from 'echarts'

export {
  TIME_SPACE_CYCLE_CENTER_OFFSET,
  TIME_SPACE_Y_AXIS_PADDING,
  getChartTimespanMs,
  getDisplayDistanceScale,
  getTimeLikeMs,
  getTimeSpaceChartHeight,
  getTimeSpacePhaseRowDistances,
} from '@/features/charts/timeSpaceDiagram/core/math/timeSpaceLayout'
export {
  formatSignedOffsetSeconds,
  getOffsetDeltaVisuals,
  hasModifiedOffset,
  normalizeOffsetToCycleLengthSeconds,
  offsetsMatch,
} from '@/features/charts/timeSpaceDiagram/core/offsets/timeSpaceOffsets'
export {
  buildIdentifierAndNameTitle,
  getDistancesLabelOption,
  getDraggableOffsetabelOption,
  getLocationsLabelOption,
  getOffsetAndProgramSplitLabel,
  getTimeSpaceLocationOffsetBadgeLayout,
  splitIdentifierAndDescription,
  TIME_SPACE_CARD_CONNECTOR_IGNORED_STROKE,
  TIME_SPACE_CARD_CONNECTOR_STROKE,
  TIME_SPACE_CARD_CONNECTOR_WIDTH,
  TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT,
  TIME_SPACE_LOCATION_AXIS_SERIES_ID,
  TIME_SPACE_LOCATION_CARD_LAYOUT,
  TIME_SPACE_PHASE_CONNECTOR_ARROW_SIZE,
  TIME_SPACE_PHASE_CONNECTOR_END_INSET,
  TIME_SPACE_PHASE_CONNECTOR_INNER_OFFSET,
  TIME_SPACE_PHASE_CONNECTOR_MIN_LENGTH,
} from '@/features/charts/timeSpaceDiagram/core/labels/timeSpaceLocationCards'
export {
  CYCLE_LABEL_SERIES_ID_PREFIX,
  generateCycleLabels,
} from '@/features/charts/timeSpaceDiagram/core/labels/timeSpaceCycleLabels'

type CycleIndication = {
  name: string
  codes: number[]
  color: string
}

export const CYCLE_INDICATIONS: readonly CycleIndication[] = [
  {
    name: 'Phase Begin Green (1)\nOverlap Begin Green (61)',
    codes: [1, 61],
    color: '#0CC078',
  },
  {
    name:
      'Phase Min Complete (3)\nOverlap Begin Trailing Green (Extension) (62)',
    codes: [3, 62],
    color: '#79DE79',
  },
  {
    name: 'Phase Begin Yellow Clearance (8)\nBegin Overlap Yellow (63)',
    codes: [8, 63],
    color: '#FCFC99',
  },
  {
    name: 'Phase End Yellow Clearance (9)\nOverlap Begin Red Clearance (64)',
    codes: [9, 64],
    color: '#FB6962',
  },
  {
    name: 'Phase End Red Clearance (11)\nOverlap Off (Inactive with Red Indication) (65)',
    codes: [11, 65],
    color: '#B34747',
  },
] as const

const CYCLE_SEGMENT_HEIGHT = 17
const CYCLE_BORDER_HEIGHT = 0.5
const CYCLE_DURATION_LABEL_FONT_SIZE = 10
const CYCLE_DURATION_LABEL_FILL = 'white'
const CYCLE_DURATION_LABEL_STROKE = 'black'
const CYCLE_DURATION_LABEL_STROKE_WIDTH = 1.5
const CYCLE_CONTINUATION_FILL = '#D5DBE3'

export const TIME_SPACE_CONTINUATION_NODE_NAME = 'time-space-continuation'
export const TIME_SPACE_MOVEMENT_SERIES_Z = 1
export const TIME_SPACE_CYCLE_SERIES_Z = 5
export const TIME_SPACE_CYCLE_LABEL_SERIES_Z = 6

const TIME_SPACE_CYCLE_CONTINUATION_SERIES_Z = 4
const TIME_SPACE_MOVEMENT_ELEMENT_Z2 = 1
const TIME_SPACE_CYCLE_ELEMENT_Z2 = 5

function getCycleColor(value: number): string {
  const found = CYCLE_INDICATIONS.find((entry) => entry.codes.includes(value))
  return found?.color ?? '#999'
}

function getCycleEvents(
  data: { start: string; value: number }[] | null,
  distance: number
): [string, number, number][] {
  if (!data) return []
  return data.map((event) => [event.start, distance, event.value])
}

export function generateCycles(
  data: TimeSpaceUnwrappedData,
  distanceData: number[],
  phaseType?: string,
  idScope = 'default'
): SeriesOption[] {
  return data.flatMap((phase, index) => {
    if (phase.isIgnoredLocation) {
      return []
    }

    const distance = distanceData[index]
    const hasData = hasCycleData(phase.cycleAllEvents)
    const cycleEvents = hasData
      ? getCycleEvents(phase.cycleAllEvents, distance)
      : [[0, distance, 0]]
    const cycleRailData = [[phase.start, distance]]

    const cycleName = `Cycles ${phaseType ?? ''}`
    const cycleDurationName = `Cycle Durations ${phaseType ?? ''}`
    const series: SeriesOption[] = [
      {
        name: cycleName,
        id: `Cycle Continuation ${phase.locationIdentifier} ${phaseType ?? ''} row-${index} ${idScope}`,
        type: 'custom',
        clip: true,
        silent: true,
        z: TIME_SPACE_CYCLE_CONTINUATION_SERIES_Z,
        data: [[phase.start, distance, phase.end]],
        renderItem: (_param, api): CustomSeriesRenderItemReturn => {
          const timespanMs = getChartTimespanMs(api.value(0), api.value(2))
          const center = api.coord([api.value(0), api.value(1)])

          if (
            timespanMs == null ||
            !Array.isArray(center) ||
            center.length < 2 ||
            !Number.isFinite(center[0]) ||
            !Number.isFinite(center[1])
          ) {
            return
          }

          return {
            type: 'group',
            children: [
              buildCycleBandGroup(
                center[0] - timespanMs,
                center[1],
                timespanMs,
                getCycleContinuationPatternFill(),
                1
              ),
              buildCycleBandGroup(
                center[0] + timespanMs,
                center[1],
                timespanMs,
                getCycleContinuationPatternFill(),
                1
              ),
            ],
          }
        },
      },
      {
        name: cycleName,
        id: `Cycle Rail ${phase.locationIdentifier} ${phaseType ?? ''} row-${index} ${idScope}`,
        type: 'custom',
        clip: true,
        silent: true,
        z: TIME_SPACE_CYCLE_CONTINUATION_SERIES_Z,
        data: cycleRailData,
        renderItem: (param, api): CustomSeriesRenderItemReturn =>
          renderCycleRailBand(param, api),
      },
      {
        name: cycleName,
        id: `Cycles ${phase.locationIdentifier} ${phaseType ?? ''} row-${index} ${idScope}`,
        type: 'custom',
        clip: true,
        z: TIME_SPACE_CYCLE_SERIES_Z,
        data: cycleEvents,
        renderItem: (param, api): CustomSeriesRenderItemReturn => {
          if (!hasData) {
            return renderMissingCycle(api, param, distance)
          }

          return renderCycleSegment(api, cycleEvents, param.dataIndex)
        },
      },
      {
        name: cycleDurationName,
        id: `Cycle Duration Labels ${phase.locationIdentifier} ${phaseType ?? ''} row-${index} ${idScope}`,
        type: 'custom',
        clip: true,
        silent: true,
        z: TIME_SPACE_CYCLE_LABEL_SERIES_Z,
        data: hasData ? getCycleDurationLabelData(cycleEvents) : [],
        renderItem: (_param, api): CustomSeriesRenderItemReturn => {
          const midX = api.value(0) as number
          const y = api.value(1) as number
          const label = String(api.value(2))
          const center = api.coord([midX, y])

          return {
            type: 'text',
            z2: 20,
            style: {
              x: center[0],
              y: center[1],
              text: label,
              fill: CYCLE_DURATION_LABEL_FILL,
              stroke: CYCLE_DURATION_LABEL_STROKE,
              lineWidth: CYCLE_DURATION_LABEL_STROKE_WIDTH,
              fontSize: CYCLE_DURATION_LABEL_FONT_SIZE,
              fontWeight: 600,
              textAlign: 'center',
              textVerticalAlign: 'middle',
            },
          }
        },
      },
    ]

    return series
  })
}

function renderCycleSegment(
  api: CustomSeriesRenderItemAPI,
  cycleEvents: Array<[string, number, number]>,
  index: number
): CustomSeriesRenderItemReturn {
  if (index >= cycleEvents.length - 1) return

  const [x1, y1, v1] = [api.value(0), api.value(1), api.value(2)]
  const [x2, y2] = [api.value(0, index + 1), api.value(1, index + 1)]

  const p1 = api.coord([x1, y1])
  const x2Ms = getTimeLikeMs(x2)
  if (x2Ms == null) {
    return
  }
  const p2 = api.coord([x2Ms, y2])

  return buildCycleBandGroup(
    p1[0],
    p1[1],
    p2[0] - p1[0],
    getCycleColor(v1 as number),
    1
  )
}

function getCycleDurationLabel(startTime: unknown, endTime: unknown): string {
  const startMs = Date.parse(String(startTime))
  const endMs = Date.parse(String(endTime))

  if (
    !Number.isFinite(startMs) ||
    !Number.isFinite(endMs) ||
    endMs <= startMs
  ) {
    return ''
  }

  const durationSeconds = Math.round((endMs - startMs) / 1000)
  return durationSeconds > 0 ? durationSeconds.toString() : ''
}

function getCycleDurationLabelData(
  cycleEvents: Array<[string, number, number]>
): Array<[number, number, string]> {
  return cycleEvents.flatMap((event, index) => {
    if (index >= cycleEvents.length - 1) return []

    const [startTime, y] = event
    const [endTime] = cycleEvents[index + 1]
    const startMs = Date.parse(String(startTime))
    const endMs = Date.parse(String(endTime))
    const label = getCycleDurationLabel(startTime, endTime)

    if (!label || !Number.isFinite(startMs) || !Number.isFinite(endMs)) {
      return []
    }

    return [[startMs + (endMs - startMs) / 2, y as number, label]]
  })
}

function hasCycleData(cycleAllEvents: Cycle[] | null): boolean {
  return Array.isArray(cycleAllEvents) && cycleAllEvents.length > 1
}

function renderMissingCycle(
  api: CustomSeriesRenderItemAPI,
  param: CustomSeriesRenderItemParams,
  distance: number
): CustomSeriesRenderItemReturn {
  const coordSys = param.coordSys as { x: number; width: number }
  const y = api.coord([0, distance])[1]

  return buildCycleBackgroundBandGroup(coordSys.x, y, coordSys.width, '#E0E0E0')
}

function renderCycleRailBand(
  param: CustomSeriesRenderItemParams,
  api: CustomSeriesRenderItemAPI
): CustomSeriesRenderItemReturn {
  const yValue = api.value(1)
  if (!Number.isFinite(yValue as number)) {
    return
  }

  const coordSys = param.coordSys as { x?: number; width?: number } | undefined
  if (!coordSys || !Number.isFinite(coordSys.x) || !Number.isFinite(coordSys.width)) {
    return
  }

  const center = api.coord([api.value(0), yValue])
  if (
    !Array.isArray(center) ||
    center.length < 2 ||
    !Number.isFinite(center[1])
  ) {
    return
  }

  return buildCycleContinuationBandGroup(
    coordSys.x as number,
    center[1],
    coordSys.width as number
  )
}

function buildCycleBandGroup(
  x: number,
  centerY: number,
  width: number,
  fill: string,
  opacity: number
): CustomSeriesRenderItemReturn {
  const y = centerY - CYCLE_SEGMENT_HEIGHT / 2

  return {
    type: 'group',
    emphasisDisabled: true,
    children: [
      {
        type: 'rect',
        z2: TIME_SPACE_CYCLE_ELEMENT_Z2,
        shape: {
          x,
          y,
          width,
          height: CYCLE_SEGMENT_HEIGHT,
        },
        style: {
          fill,
          opacity,
        },
      },
      {
        type: 'rect',
        z2: TIME_SPACE_CYCLE_ELEMENT_Z2 + 1,
        shape: {
          x,
          y,
          width,
          height: CYCLE_BORDER_HEIGHT,
        },
        style: {
          fill: '#000',
          opacity: 1,
        },
      },
      {
        type: 'rect',
        z2: TIME_SPACE_CYCLE_ELEMENT_Z2 + 1,
        shape: {
          x,
          y: y + CYCLE_SEGMENT_HEIGHT - CYCLE_BORDER_HEIGHT,
          width,
          height: CYCLE_BORDER_HEIGHT,
        },
        style: {
          fill: '#000',
          opacity: 1,
        },
      },
    ],
  }
}

function buildCycleBackgroundBandGroup(
  x: number,
  centerY: number,
  width: number,
  fill: string
): CustomSeriesRenderItemReturn {
  const y = centerY - CYCLE_SEGMENT_HEIGHT / 2

  return {
    type: 'rect',
    emphasisDisabled: true,
    z2: TIME_SPACE_CYCLE_ELEMENT_Z2 - 1,
    shape: {
      x,
      y,
      width,
      height: CYCLE_SEGMENT_HEIGHT,
    },
    style: {
      fill,
      opacity: 1,
    },
  }
}

export function getCycleContinuationPatternFill() {
  return CYCLE_CONTINUATION_FILL
}

function buildCycleContinuationBandGroup(
  x: number,
  centerY: number,
  width: number
): CustomSeriesRenderItemReturn {
  return buildCycleBackgroundBandGroup(
    x,
    centerY,
    width,
    CYCLE_CONTINUATION_FILL
  )
}

export function generateGreenEventLines(
  data: TimeSpaceUnwrappedData,
  distanceData: number[],
  phaseType?: string,
  isPrimary?: boolean,
  distanceScale = 1,
  idScope = 'default'
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []

  for (let i = 0; i < data.length; i++) {
    const location = data[i]
    if (location.isIgnoredLocation || !location.greenTimeEvents) continue

    const dataPoints = getGreenEventsDataPoints(
      location.greenTimeEvents,
      distanceData[i],
      location.start,
      location.end
    )

    seriesOptions.push({
      name: `Green Bands ${phaseType?.length ? phaseType : ''}`,
      id: `Green Bands ${data[i].locationIdentifier} ${
        phaseType?.length ? phaseType : ''
      } row-${i} ${idScope}`,
      type: 'custom',
      data: dataPoints,
      clip: true,
      animation: false,
      silent: true,
      selectedMode: false,
      tooltip: { show: false },
      z: TIME_SPACE_MOVEMENT_SERIES_Z,
      renderItem: (params, api) => {
        const pointIndex = params.dataIndex
        if (!dataPoints || pointIndex >= dataPoints.length - 1 || pointIndex % 2 !== 0) {
          return
        }

        const travelDistanceToNext = isPrimary
          ? location.calculatedDistanceToNext
          : -location.calculatedDistanceToNext
        const displayDistanceToNext = travelDistanceToNext * distanceScale
        const nextIndex = pointIndex + 1

        const [x1, y1] = [api.value(0), api.value(1)]
        const [x2, y2] = [api.value(0, nextIndex), api.value(1, nextIndex)]
        const x1Ms = getTimeLikeMs(x1)
        const x2Ms = getTimeLikeMs(x2)
        const currPointFinalX = getArrivalTime(
          Math.abs(travelDistanceToNext),
          location.speed,
          x1 as string
        )
        const nextPointFinalX = getArrivalTime(
          Math.abs(travelDistanceToNext),
          location.speed,
          x2 as string
        )
        const currPointFinalMs = getTimeLikeMs(currPointFinalX)
        const nextPointFinalMs = getTimeLikeMs(nextPointFinalX)

        if (
          x1Ms == null ||
          x2Ms == null ||
          currPointFinalMs == null ||
          nextPointFinalMs == null
        ) {
          return
        }

        const chartTimespanMs = getChartTimespanMs(location.start, location.end)

        const buildPoints = (shiftMs = 0) => [
          api.coord([x1Ms + shiftMs, y1]),
          api.coord([x2Ms + shiftMs, y2]),
          api.coord([
            nextPointFinalMs + shiftMs,
            (y2 as number) + displayDistanceToNext,
          ]),
          api.coord([
            currPointFinalMs + shiftMs,
            (y1 as number) + displayDistanceToNext,
          ]),
        ]

        return chartTimespanMs == null
          ? buildGreenBandPolygon(buildPoints(), false, isPrimary)
          : {
              type: 'group',
              emphasisDisabled: true,
              children: [
                buildGreenBandPolygon(
                  buildPoints(-chartTimespanMs),
                  true,
                  isPrimary
                ),
                buildGreenBandPolygon(buildPoints(), false, isPrimary),
                buildGreenBandPolygon(
                  buildPoints(chartTimespanMs),
                  true,
                  isPrimary
                ),
              ],
            }
      },
    })
  }

  return seriesOptions
}

function buildGreenBandPolygon(
  points: number[][],
  isContinuation: boolean,
  isPrimary?: boolean
): CustomSeriesRenderItemReturn {
  return {
    type: 'polygon',
    ...(isContinuation ? { name: TIME_SPACE_CONTINUATION_NODE_NAME } : null),
    z2: isContinuation
      ? TIME_SPACE_MOVEMENT_ELEMENT_Z2 - 1
      : TIME_SPACE_MOVEMENT_ELEMENT_Z2,
    focus: 'none',
    transition: ['shape'],
    emphasisDisabled: true,
    shape: {
      points,
    },
    style: isContinuation
      ? {
          fill: getCycleContinuationPatternFill(),
        }
      : {
          opacity: isPrimary ? 0.3 : 0.2,
          fill: isPrimary ? '#4f9bac' : '#202d30',
        },
  }
}

export function getEffectiveDistanceToNext(
  data: TimeSpaceUnwrappedData,
  index: number,
  isPrimary?: boolean
): number {
  let totalDistance = data[index].calculatedDistanceToNext

  const nextIndex = index + 1
  if (nextIndex < data.length) {
    totalDistance += data[nextIndex].calculatedDistanceToNext
  }

  return isPrimary ? totalDistance : -totalDistance
}

function getGreenEventsDataPoints(
  greenEvents: TimeSpaceDetectorEvent[],
  currentDistance: number,
  start: string,
  end: string
) {
  const result = []

  for (let i = 0; i < greenEvents.length; ) {
    const currentPoint = greenEvents[i]
    const nextPoint = greenEvents[i + 1]

    if (i === 0 && currentPoint.isDetectorOn === false) {
      result.push([start, currentDistance], [currentPoint.initialX, currentDistance])
      i++
    } else if (
      i === greenEvents.length - 1 &&
      currentPoint.isDetectorOn === true
    ) {
      result.push([currentPoint.initialX, currentDistance], [end, currentDistance])
      i++
    } else if (currentPoint.isDetectorOn === false) {
      i++
    } else {
      result.push(
        [currentPoint.initialX, currentDistance],
        [nextPoint.initialX, currentDistance]
      )
      i += 2
    }
  }

  return result
}

function getArrivalTime(
  distanceToNextLocation: number,
  speed: number,
  currentDetectorOn: Date | string
): string {
  const start = new Date(currentDetectorOn)
  const speedInFeetPerSecond = getSpeedInFeetPerSecond(speed)
  const timeToTravelSeconds = distanceToNextLocation / speedInFeetPerSecond

  return dateToTimestamp(
    new Date(start.getTime() + timeToTravelSeconds * 1000)
  )
}

function getSpeedInFeetPerSecond(speed: number): number {
  return (speed * 5280) / 3600
}
