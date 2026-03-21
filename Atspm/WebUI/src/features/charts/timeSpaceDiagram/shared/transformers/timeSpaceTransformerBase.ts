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
  RawTimeSpaceAverageData,
  TimeSpaceDetectorEvent,
  TimeSpaceUnwrappedData,
} from '@/features/charts/timeSpaceDiagram/shared/types'
import { Cycle } from '@/features/charts/timingAndActuation/types'
import { Color } from '@/features/charts/utils'
import { directionTypes as staticDirectionTypes } from '@/features/locations/components/editDetector/selectOptions'
import { getDirectionAccentColor } from '@/features/locations/utils/directionAccent'
import { dateToTimestamp } from '@/utils/dateTime'
import {
  CustomSeriesRenderItemAPI,
  CustomSeriesRenderItemParams,
  CustomSeriesRenderItemReturn,
  GridComponentOption,
  SeriesOption,
} from 'echarts'

// export function generateCycles(
//   data: TimeSpaceUnwrappedData,
//   distanceData: number[],
//   phaseType?: string
// ): SeriesOption[] {
//   const seriesOptions: SeriesOption[] = []
//   for (let i = 0; i < data.length; i++) {
//     const cycleEvents = getCycleEvents(data[i].cycleAllEvents, distanceData[i])
//     const seriesOption: SeriesOption = {
//       name: `Cycles ${phaseType?.length ? phaseType : ''}`,
//       id: `Cycles ${data[i].locationIdentifier} ${phaseType?.length ? phaseType : ''}`,
//       type: 'custom',
//       clip: true,
//       z: 5,
//       silent: true,
//       data: cycleEvents,
//       renderItem: (param, api): CustomSeriesRenderItemReturn => {
//         const i = param.dataIndex
//         if (!cycleEvents || i >= cycleEvents.length - 1) {
//           return
//         }
//         const nextIndex = i + 1

//         const [x1, y1, v1] = [api.value(0), api.value(1), api.value(2)]

//         const [x2, y2, v2] = [
//           api.value(0, nextIndex),
//           api.value(1, nextIndex),
//           api.value(2, nextIndex),
//         ]
//         const newX2 = new Date(x2).getTime()
//         const p1 = api.coord([x1, y1])
//         const p2 = api.coord([newX2, y2])
//         return {
//           type: 'rect',
//           shape: {
//             x: p1[0],
//             y: p1[1],
//             width: p2[0] - p1[0],
//             height: 10,
//           },
//           style: {
//             fill: getSegmentColor(v1 as number, v2 as number),
//             opacity: 0.9,
//           },
//         }
//       },
//     }
//     seriesOptions.push(seriesOption)
//   }
//   return seriesOptions
// }

type CycleIndication = {
  name: string
  codes: number[]
  color: string
}

export const CYCLE_INDICATIONS: readonly CycleIndication[] = [
  {
    name: 'Phase Begin Green (1)\nOverlap Begin Green (61)',
    codes: [1, 61],
    color: Color.Green,
  },
  {
    name: 'Phase Min Complete (3)\nOverlap Begin Trailing Green (Extension) (62)',
    codes: [3, 62],
    color: '#8ef08d',
  },
  {
    name: 'Phase Begin Yellow Clearance (8)\nBegin Overlap Yellow (63)',
    codes: [8, 63],
    color: Color.Yellow,
  },
  {
    name: 'Phase End Yellow Clearance (9)\nOverlap Begin Red Clearance (64)',
    codes: [9, 64],
    color: '#FF0000',
  },
  {
    name: 'Phase End Red Clearance (11)\nOverlap Off (Inactive with Red Indication) (65)',
    codes: [11, 65],
    color: '#f0807f',
  },
] as const

const CYCLE_SEGMENT_HEIGHT = 15
const CYCLE_DURATION_LABEL_FONT_SIZE = 10
const CYCLE_DURATION_LABEL_FILL = 'white'
const CYCLE_DURATION_LABEL_STROKE = 'black'
const CYCLE_DURATION_LABEL_STROKE_WIDTH = 1.5

function getCycleColor(value: number): string {
  const found = CYCLE_INDICATIONS.find((x) => x.codes.includes(value))
  return found?.color ?? '#999'
}

function getCycleEvents(
  data: { start: string; value: number }[] | null,
  distanceData: number
): [string, number, number][] {
  if (!data) return []
  return data.map((e) => [e.start, distanceData, e.value])
}

export function generateCycles(
  data: TimeSpaceUnwrappedData,
  distanceData: number[],
  phaseType?: string
): SeriesOption[] {
  return data.flatMap((phase, index) => {
    const distance = distanceData[index]
    const hasData = hasCycleData(phase.cycleAllEvents)

    const cycleEvents = hasData
      ? getCycleEvents(phase.cycleAllEvents, distance)
      : [[0, distance, 0]]

    const cycleName = `Cycles ${phaseType ?? ''}`
    const cycleDurationName = `Cycle Durations ${phaseType ?? ''}`
    const series: SeriesOption[] = [
      {
        name: cycleName,
        id: `Cycles ${phase.locationIdentifier} ${phaseType ?? ''}`,
        type: 'custom',
        clip: true,
        z: 5,
        data: cycleEvents,
        renderItem: (param, api): CustomSeriesRenderItemReturn => {
          if (!hasData) {
            return renderMissingCycle(api, param, distance)
          }

          return renderCycleSegment(api, cycleEvents, param.dataIndex)
        },
      },
    ]

    series.push({
      name: cycleDurationName,
      id: `Cycle Duration Labels ${phase.locationIdentifier} ${phaseType ?? ''}`,
      type: 'custom',
      clip: true,
      silent: true,
      z: 6,
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
            y: center[1] + CYCLE_SEGMENT_HEIGHT / 2,
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
    })

    return series
  })
}

function renderCycleSegment(
  api: CustomSeriesRenderItemAPI,
  cycleEvents: any[],
  index: number
): CustomSeriesRenderItemReturn {
  if (index >= cycleEvents.length - 1) return

  const [x1, y1, v1] = [api.value(0), api.value(1), api.value(2)]

  const [x2, y2] = [api.value(0, index + 1), api.value(1, index + 1)]

  const p1 = api.coord([x1, y1])
  const p2 = api.coord([new Date(x2).getTime(), y2])
  const width = p2[0] - p1[0]

  const fill = getCycleColor(v1 as number)
  return {
    type: 'rect',
    shape: {
      x: p1[0],
      y: p1[1],
      width,
      height: CYCLE_SEGMENT_HEIGHT,
    },
    style: {
      fill,
      opacity: 0.6,
    },
  }
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
  cycleEvents: any[]
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

    const midpointMs = startMs + (endMs - startMs) / 2

    return [[midpointMs, y as number, label]]
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
  const coordSys = param.coordSys as any
  const y = api.coord([0, distance])[1]

  return {
    type: 'rect',
    shape: {
      x: coordSys.x,
      y,
      width: coordSys.width,
      height: CYCLE_SEGMENT_HEIGHT,
    },
    style: {
      fill: '#d0d0d0',
      opacity: 0.75,
    },
  }
}

// function renderMissingCycle(
//   api: CustomSeriesRenderItemAPI,
//   param: CustomSeriesRenderItemParams,
//   distance: number,
//   nextDistance: number
// ): CustomSeriesRenderItemReturn {
//   const coordSys = param.coordSys as any

//   const y1 = api.coord([0, distance])[1]
//   const y2 = api.coord([0, nextDistance])[1]

//   const y = Math.min(y1, y2)
//   const height = Math.abs(y2 - y1)

//   return {
//     type: 'rect',
//     shape: {
//       x: coordSys.x,
//       y,
//       width: coordSys.width,
//       height: 10,
//     },
//     style: {
//       fill: '#d0d0d0',
//       opacity: 0.35,
//     },
//   }
// }

// export function generateCycles(
//   data: TimeSpaceUnwrappedData,
//   distanceData: number[],
//   phaseType?: string
// ): SeriesOption[] {
//   const series: SeriesOption[] = []

//   const greenBands = getBandData(data, distanceData, 1)
//   const yellowBands = getBandData(data, distanceData, 8)
//   const redBands = getBandData(data, distanceData, 9)

//   for (const band of bandSpecs) {
//     series.push({
//       type: 'custom',
//       name: `Cycles ${phaseType ?? ''}`,
//       renderItem: renderCycleBand,

//       itemStyle: {
//         color: band.color,
//         opacity: 0.8,
//       },

//       encode: {
//         x: [1, 2], // start & end time
//         y: 0, // distance index
//       },

//       data: band.items,
//     })
//   }

//   return series
// }

// function toTimestamp(dt: string): number {
//   return new Date(dt).getTime()
// }

// function getBandData(
//   data: TimeSpaceUnwrappedData,
//   distanceData: number[],
//   value: number
// ) {
//   const bands: Array<{ name: string; value: number[] }> = []

//   data.forEach((location, index) => {
//     if (!location.cycleAllEvents?.length) return

//     const cycles = location.cycleAllEvents
//     const startIndex = cycles.findIndex((e) => e.value === value)
//     if (startIndex < 0) return

//     for (let i = startIndex; i < cycles.length; i += 3) {
//       const startTimeStr = cycles[i].start
//       const endTimeStr =
//         i === cycles.length - 1 ? location.end : cycles[i + 1].start

//       const startTime = toTimestamp(startTimeStr)
//       const endTime = toTimestamp(endTimeStr)

//       bands.push({
//         name: `${value}`,
//         value: [
//           index, // y-axis category index
//           startTime, // x-axis start (timestamp)
//           endTime, // x-axis end (timestamp)
//         ],
//       })
//     }
//   })

//   return bands
// }

// function renderCycleBand(params, api) {
//   const yIndex = api.value(0)
//   const xStart = api.value(1)
//   const xEnd = api.value(2)

//   const startCoord = api.coord([xStart, yIndex])
//   const endCoord = api.coord([xEnd, yIndex])

//   // band thickness in Y units:
//   const height = api.size([0, 1])[1] * 5 // adjust width (% of row height)

//   const rect = graphic.clipRectByRect(
//     {
//       x: startCoord[0],
//       y: startCoord[1] - height / 2,
//       width: endCoord[0] - startCoord[0],
//       height,
//     },
//     {
//       x: params.coordSys.x,
//       y: params.coordSys.y,
//       width: params.coordSys.width,
//       height: params.coordSys.height,
//     }
//   )

//   return (
//     rect && {
//       type: 'rect',
//       shape: rect,
//       style: api.style(),
//     }
//   )
// }

function getDataByValue(
  data: TimeSpaceUnwrappedData,
  distanceData: number[],
  value: number
) {
  return data.reduce((result, location, index: number) => {
    if (location.cycleAllEvents?.length) {
      const cycles = location.cycleAllEvents
      const startingIndex = location.cycleAllEvents.findIndex(
        (event) => event.value === value
      )
      for (let i = startingIndex; i < cycles.length; i += 3) {
        const currPoint = [cycles[i].start, distanceData[index]]
        let nextPoint: any[]
        if (i === cycles.length - 1) {
          nextPoint = [location.end, distanceData[index]]
        } else {
          nextPoint = [cycles[i + 1].start, distanceData[index]]
        }

        result.push(...[currPoint, nextPoint, null])
      }
    }
    return result
  }, [] as any[])
}

export function generateGreenEventLines(
  data: TimeSpaceUnwrappedData,
  distanceData: number[],
  phaseType?: string,
  isPrimary?: boolean
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []
  for (let i = 0; i < data.length; i++) {
    const location = data[i]
    if (location.isIgnoredLocation) continue
    // const distanceToNext = getEffectiveDistanceToNext(data, i, isPrimary)
    if (!location.greenTimeEvents) continue
    const dataPoints = getGreenEventsDataPoints(
      location.greenTimeEvents,
      distanceData[i],
      location.start,
      location.end
    )
    const seriesOption: SeriesOption = {
      name: `Green Bands ${phaseType?.length ? phaseType : ''}`,
      id: `Green Bands ${data[i].locationIdentifier} ${phaseType?.length ? phaseType : ''}`,
      type: 'custom',
      data: dataPoints,
      clip: true,
      animation: false,
      renderItem: function (params, api) {
        const i = params.dataIndex
        if (!dataPoints || i >= dataPoints.length - 1 || i % 2 !== 0) {
          return
        }
        const distanceToNext = isPrimary
          ? location.calculatedDistanceToNext
          : -location.calculatedDistanceToNext

        const nextIndex = i + 1

        const [x1, y1] = [api.value(0), api.value(1)]
        const [x2, y2] = [api.value(0, nextIndex), api.value(1, nextIndex)]

        const currPointFinalX = getArrivalTime(
          Math.abs(distanceToNext),
          location.speed,
          x1 as string
        )
        const nextPointFinalX = getArrivalTime(
          Math.abs(distanceToNext),
          location.speed,
          x2 as string
        )
        const points = [
          api.coord([x1, y1]),
          api.coord([x2, y2]),
          api.coord([nextPointFinalX, (y2 as number) + distanceToNext]),
          api.coord([currPointFinalX, (y1 as number) + distanceToNext]),
        ]
        return {
          type: 'polygon',
          transition: ['shape'],
          shape: {
            points: points,
          },
          style: {
            z: -1,
            opacity: 0.3,
            fill: CYCLE_INDICATIONS[0].color,
          },
        }
      },
    }
    seriesOptions.push(seriesOption)
  }
  return seriesOptions
}

export function getEffectiveDistanceToNext(
  data: TimeSpaceUnwrappedData,
  index: number,
  isPrimary?: boolean
): number {
  let totalDistance = data[index].calculatedDistanceToNext

  const nextIndex = index + 1
  // while (
  //   nextIndex < data.length &&
  //   !hasCycleData(data[nextIndex].cycleAllEvents)
  // ) {
  if (nextIndex < data.length)
    totalDistance += data[nextIndex].calculatedDistanceToNext
  //   nextIndex++
  // }

  return isPrimary ? totalDistance : -totalDistance
}

function getGreenEventsDataPoints(
  greenEvents: TimeSpaceDetectorEvent[],
  currDistance: number,
  start: string,
  end: string
) {
  const result = []
  for (let i = 0; i < greenEvents.length; ) {
    const currPoint = greenEvents[i]
    const nextPoint = greenEvents[i + 1]
    if (i === 0 && currPoint.isDetectorOn === false) {
      result.push([start, currDistance], [currPoint.initialX, currDistance])
      i++
    } else if (
      i === greenEvents.length - 1 &&
      currPoint.isDetectorOn === true
    ) {
      result.push([currPoint.initialX, currDistance], [end, currDistance])
      i++
    } else if (currPoint.isDetectorOn === false) {
      i++
    } else {
      result.push(
        ...[
          [currPoint.initialX, currDistance],
          [nextPoint.initialX, currDistance],
        ]
      )
      i += 2
    }
  }

  return result
}

// function getGreenEventsDataPoints(
//   data: TimeSpaceUnwrappedData,
//   distanceData: number[]
// ) {
//   return data.reduce((result, location, index) => {
//     if (location.greenTimeEvents) {
//       const greenEvents = location.greenTimeEvents
//       for (let i = 0; i < greenEvents.length; ) {
//         const currPoint = greenEvents[i]
//         const nextPoint = greenEvents[i + 1]
//         const currPointFinalX = getArrivalTime(
//           location.distanceToNextLocation,
//           location.speed,
//           currPoint.initialX
//         )
//         const nextPointFinalX = getArrivalTime(
//           location.distanceToNextLocation,
//           location.speed,
//           nextPoint.initialX
//         )
//         if (i === 0 && currPoint.isDetectorOn === false) {
//           result.push(
//             [location.start, distanceData[index]],
//             [currPoint.initialX, distanceData[index]]
//             // [currPointFinalX, distanceData[index + 1]],
//             // [location.start, distanceData[index + 1]],
//             // null
//           )
//           i++
//         } else if (
//           i === greenEvents.length - 1 &&
//           currPoint.isDetectorOn === true
//         ) {
//           result.push(
//             [currPoint.initialX, distanceData[index]],
//             [location.end, distanceData[index]]
//             // [location.end, distanceData[index + 1]],
//             // [currPointFinalX, distanceData[index + 1]],
//             // null
//           )
//           i++
//         } else if (currPoint.isDetectorOn === false) {
//           i++
//         } else {
//           result.push(
//             ...[
//               [currPoint.initialX, distanceData[index]],
//               [nextPoint.initialX, distanceData[index]],
//               // [nextPointFinalX, distanceData[index + 1]],
//               // [currPointFinalX, distanceData[index + 1]],
//               // null,
//             ]
//           )
//           i += 2
//         }
//       }
//     }
//     return result
//   }, [] as any)
// }

function getArrivalTime(
  distanceToNextLocation: number,
  speed: number,
  currentDetectorOn: Date | string
): string {
  const start = new Date(currentDetectorOn)
  const speedInFeetPerSecond = getSpeedInFeetPerSecond(speed)
  const timeToTravelSeconds = distanceToNextLocation / speedInFeetPerSecond

  const arrivalMs = start.getTime() + timeToTravelSeconds * 1000

  return dateToTimestamp(new Date(arrivalMs))
}

function getSpeedInFeetPerSecond(speed: number): number {
  return (speed * 5280) / 3600
}

function splitPrimarySecondary(desc: string | undefined) {
  const raw = (desc ?? '').trim()

  // remove leading "#1234 - " (or "1234 - ")
  const noId = raw.replace(/^\s*#?\d+\s*-\s*/, '')

  // split on first " & "
  const [primary, secondary = ''] = noId.split(/\s*&\s*/, 2)

  return {
    primary: (primary ?? '').trim(),
    secondary: (secondary ?? '').trim(),
  }
}

type FontSpec = {
  ident: string
  line: string
}

const DEFAULT_FONTS: FontSpec = {
  ident: '700 14px Inter, system-ui, -apple-system, Segoe UI, Roboto, Arial',
  line: '400 12px Inter, system-ui, -apple-system, Segoe UI, Roboto, Arial',
}

function measureTextWidth(text: string, font: string): number {
  if (!text) return 0

  // SSR / non-browser fallback
  if (typeof document === 'undefined') return Math.min(500, text.length * 7)

  const canvas =
    (measureTextWidth as any)._canvas ||
    ((measureTextWidth as any)._canvas = document.createElement('canvas'))
  const ctx = canvas.getContext('2d')
  if (!ctx) return Math.min(500, text.length * 7)

  ctx.font = font
  return ctx.measureText(text).width
}

function getLongestLabelLineWidth(
  data: TimeSpaceUnwrappedData,
  fonts: FontSpec = DEFAULT_FONTS
): number {
  let max = 0

  for (const row of data) {
    const ident = String((row as any).locationIdentifier ?? '')
    const { primary, secondary } = splitPrimarySecondary(
      String((row as any).locationDescription ?? '')
    )

    const line1 = ident
    const line2 = primary ? `${primary} &` : ''
    const line3 = secondary ?? ''

    max = Math.max(
      max,
      measureTextWidth(line1, fonts.ident),
      measureTextWidth(line2, fonts.line),
      measureTextWidth(line3, fonts.line)
    )
  }

  return Math.ceil(max)
}

export const TIME_SPACE_LOCATION_CARD_LAYOUT = {
  gridGap: 35,
  dotOffset: 36,
  cardGapToDot: 18,
  cardWidth: 200,
  cardRadius: 4,
  verticalOffsetY: 15,
  headerHeight: 44,
  bodyHeight: 30,
  bodyPaddingLeft: 12,
  bodyPaddingRight: 12,
  headerActionSize: 12,
  headerActionRight: 10,
  headerActionOverlayOffsetX: 15,
  headerActionOverlayOffsetY: 15,
} as const

export const TIME_SPACE_LOCATION_AXIS_SERIES_ID = 'Location axis'

const TIME_SPACE_DISTANCE_VALUE_CARD_WIDTH = 96

export const TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT = {
  cardWidth: 90,
  cardRadius: 2,
  headerHeight: 18,
  cardGapFromPlot: 5,
  cardGapBetween: 5,
  verticalOffsetY: -12,
  bodyPaddingX: 7,
  bodyPaddingY: 4,
  lineHeight: 13,
  minBodyHeight: 16,
} as const

export function getLocationsLabelOption(
  data: TimeSpaceUnwrappedData,
  distanceData: number[],
  grid: GridComponentOption
): SeriesOption {
  const gridLeft = (grid.left as number) ?? 0

  const {
    gridGap,
    dotOffset,
    cardGapToDot,
    cardWidth,
    cardRadius,
    headerHeight,
    bodyHeight,
    bodyPaddingLeft,
    bodyPaddingRight,
    headerActionSize,
    headerActionRight,
    verticalOffsetY,
  } = TIME_SPACE_LOCATION_CARD_LAYOUT
  const CARD_H = headerHeight + bodyHeight

  const series: SeriesOption = {
    id: TIME_SPACE_LOCATION_AXIS_SERIES_ID,
    name: TIME_SPACE_LOCATION_AXIS_SERIES_ID,
    type: 'custom',
    silent: true,
    clip: false,
    renderItem: (params, api) => {
      const idx = params.dataIndexInside ?? params.dataIndex
      const len = params.dataInsideLength ?? distanceData.length

      const [, y] = api.coord([api.value(0), api.value(1)])

      const xTextRight = gridLeft - gridGap
      const xDot = xTextRight + dotOffset

      const cardRight = xDot - cardGapToDot
      const cardLeft = cardRight - cardWidth
      const xLine = cardRight + (gridLeft - cardRight) / 2
      const cardTop = y - CARD_H / 2 + verticalOffsetY
      const textX = cardLeft + bodyPaddingLeft
      const iconLeft = cardRight - headerActionRight - headerActionSize
      const dividerX = iconLeft - 8
      const titleWidth = Math.max(0, dividerX - textX - 8)

      const children: any[] = []

      if (idx === 0 && len > 1) {
        const last = len - 1
        const [, yTop] = api.coord([api.value(0, 0), api.value(1, 0)])
        const [, yBottom] = api.coord([api.value(0, last), api.value(1, last)])

        children.push({
          type: 'line',
          shape: { x1: xLine, y1: yTop, x2: xLine, y2: yBottom },
          style: { stroke: Color.PlanB, lineWidth: 3 },
          z2: 1,
        })
      }

      const location = data.find(
        (loc) => loc.locationIdentifier === api.value(2).toString()
      )
      const isIgnored = Boolean(location?.isIgnoredLocation)

      children.push({
        type: 'line',
        shape: { x1: xLine, y1: y, x2: gridLeft, y2: y },
        style: {
          stroke: isIgnored ? '#D8E0E8' : '#CBD5E1',
          lineWidth: 2,
        },
        z2: 2,
      })

      children.push({
        type: 'circle',
        shape: { cx: xLine, cy: y, r: 4 },
        style: {
          fill: isIgnored ? '#CBD5E1' : Color.LightBlue,
          stroke: '#FFFFFF',
          lineWidth: 1.5,
        },
        z2: 4,
      })

      const ident = String(api.value(2) ?? '')
      const { primary, secondary } = splitPrimarySecondary(
        String(api.value(3) ?? '')
      )

      const name =
        primary && secondary
          ? `${primary} & ${secondary}`
          : primary || secondary || ''
      const titleText =
        ident && name
          ? `{ident|${ident}}{name| - ${name}}`
          : ident
            ? `{ident|${ident}}`
            : `{name|${name}}`
      const cycleLengthValue = api.value(4)
      const offsetValue = Number(api.value(5) ?? 0)
      const isOffsetModified = Number.isFinite(offsetValue) && offsetValue !== 0
      const formattedOffset = Number.isFinite(offsetValue)
        ? Number.isInteger(offsetValue)
          ? offsetValue.toString()
          : offsetValue.toFixed(1)
        : '0'
      const bodyTop = cardTop + headerHeight
      const bodyContentWidth = cardWidth - bodyPaddingLeft - bodyPaddingRight
      const metricGap = 8
      const metricWidth = (bodyContentWidth - metricGap) / 2
      const cycleMetricX = textX
      const offsetMetricX = cycleMetricX + metricWidth + metricGap
      const bodyDividerX = cycleMetricX + metricWidth + metricGap / 2
      const metricRowY = bodyTop + bodyHeight / 2
      const metricInnerPadding = 7
      const metricLabelWidth = metricWidth * 0.48
      const metricValueWidth = metricWidth * 0.42
      const cycleText = `${cycleLengthValue ?? 'N/A'}`
      const offsetText = `${formattedOffset}s`
      const offsetValueFont =
        '700 11px Inter, system-ui, -apple-system, Segoe UI, Roboto, Arial'
      const offsetValueTextWidth = measureTextWidth(offsetText, offsetValueFont)
      const offsetValueTextRightX =
        offsetMetricX + metricWidth - metricInnerPadding
      const offsetValueHighlightPaddingX = 6
      const offsetValueHighlightWidth = Math.min(
        metricWidth - metricInnerPadding,
        Math.max(
          26,
          Math.ceil(offsetValueTextWidth + offsetValueHighlightPaddingX * 2)
        )
      )
      const offsetValueHighlightHeight = 18
      const offsetValueHighlightCenterX =
        offsetValueTextRightX - offsetValueTextWidth / 2
      const offsetValueHighlightX =
        offsetValueHighlightCenterX - offsetValueHighlightWidth / 2
      const offsetValueHighlightY =
        metricRowY - offsetValueHighlightHeight / 2

      children.push({
        type: 'group',
        z2: 2,
        children: [
          // Outer card background (white)
          {
            type: 'rect',
            z2: 10,
            shape: {
              x: cardLeft,
              y: cardTop,
              width: cardWidth,
              height: CARD_H,
              r: cardRadius,
            },
            style: {
              fill: isIgnored ? '#F8FAFC' : '#FFFFFF',
              stroke: isIgnored ? '#D5DCE5' : '#D9DEE6',
              lineWidth: 1,
              opacity: isIgnored ? 0.82 : 1,
            },
          },

          // Header background (grey)
          {
            type: 'rect',
            z2: 11,
            shape: {
              x: cardLeft,
              y: cardTop,
              width: cardWidth,
              height: headerHeight,
              r: bodyHeight > 0 ? [cardRadius, cardRadius, 0, 0] : cardRadius,
            },
            style: {
              fill: isIgnored ? '#F1F5F9' : '#EEF1F5',
              opacity: isIgnored ? 0.88 : 1,
            },
          },

          // Identifier and location name combined inside the header
          {
            type: 'text',
            z2: 20,
            style: {
              x: textX,
              y: cardTop + 8,
              text: titleText,
              width: titleWidth,
              overflow: 'break',
              lineHeight: 14,
              textAlign: 'left',
              textVerticalAlign: 'top',
              rich: {
                ident: {
                  fill: isIgnored ? '#334155' : '#111',
                  fontSize: 11,
                  fontWeight: 700,
                  opacity: isIgnored ? 0.6 : 1,
                },
                name: {
                  fill: isIgnored ? '#64748B' : '#111',
                  fontSize: 11,
                  fontWeight: 400,
                  opacity: isIgnored ? 0.72 : 1,
                },
              },
            },
          },
          {
            type: 'line',
            z2: 20,
            shape: {
              x1: dividerX,
              y1: cardTop + 7,
              x2: dividerX,
              y2: cardTop + headerHeight - 7,
            },
            style: {
              stroke: isIgnored ? '#D8E0E8' : '#CBD5E1',
              lineWidth: 1,
            },
          },
          {
            type: 'text',
            z2: 20,
            style: {
              x: cycleMetricX + metricInnerPadding,
              y: metricRowY,
              text: 'Cycle',
              width: metricLabelWidth,
              overflow: 'truncate',
              textAlign: 'left',
              textVerticalAlign: 'middle',
              fill: isIgnored ? '#94A3B8' : '#64748B',
              fontSize: 10,
              fontWeight: 500,
            },
          },
          {
            type: 'text',
            z2: 20,
            style: {
              x: cycleMetricX + metricWidth - metricInnerPadding,
              y: metricRowY,
              text: cycleText,
              width: metricValueWidth,
              overflow: 'truncate',
              textAlign: 'right',
              textVerticalAlign: 'middle',
              fill: isIgnored ? '#64748B' : '#111827',
              fontSize: 11,
              fontWeight: 700,
            },
          },
          {
            type: 'line',
            z2: 20,
            shape: {
              x1: bodyDividerX,
              y1: bodyTop + 6,
              x2: bodyDividerX,
              y2: bodyTop + bodyHeight - 6,
            },
            style: {
              stroke: isIgnored ? '#E2E8F0' : '#E5EAF1',
              lineWidth: 1,
            },
          },
          ...(isOffsetModified
            ? [
                {
                  type: 'rect' as const,
                  z2: 19,
                  shape: {
                    x: offsetValueHighlightX,
                    y: offsetValueHighlightY,
                    width: offsetValueHighlightWidth,
                    height: offsetValueHighlightHeight,
                    r: 4,
                  },
                  style: {
                    fill: isIgnored
                      ? 'rgba(86, 180, 233, 0.08)'
                      : 'rgba(86, 180, 233, 0.14)',
                    stroke: isIgnored
                      ? 'rgba(86, 180, 233, 0.14)'
                      : 'rgba(86, 180, 233, 0.24)',
                    lineWidth: 1,
                  },
                },
              ]
            : []),
          {
            type: 'text',
            z2: 20,
            style: {
              x: offsetMetricX + metricInnerPadding,
              y: metricRowY,
              text: 'Offset',
              width: metricLabelWidth,
              overflow: 'truncate',
              textAlign: 'left',
              textVerticalAlign: 'middle',
              fill: isIgnored ? '#94A3B8' : '#64748B',
              fontSize: 10,
              fontWeight: 500,
            },
          },
          {
            type: 'text',
            z2: 20,
            style: {
              x: offsetMetricX + metricWidth - metricInnerPadding,
              y: metricRowY,
              text: offsetText,
              width: metricValueWidth,
              overflow: 'truncate',
              textAlign: 'right',
              textVerticalAlign: 'middle',
              fill: isOffsetModified
                ? isIgnored
                  ? '#5A88A8'
                  : '#0F5B8D'
                : isIgnored
                  ? '#64748B'
                  : '#0F172A',
              fontSize: 11,
              fontWeight: 700,
            },
          },
        ],
      })

      return { type: 'group', children }
    },

    data: distanceData.map((distance, index) => {
      const location = data[index]
      const offset =
        'offset' in location && typeof location.offset === 'number'
          ? location.offset
          : 0

      return [
        location.start,
        distance,
        location.locationIdentifier,
        location.locationDescription,
        location.cycleLength,
        offset,
      ]
    }),
  }

  return series
}

export function getOffsetAndProgramSplitLabel(
  primaryPhaseData: RawTimeSpaceAverageData[],
  opposingPhaseData: RawTimeSpaceAverageData[],
  distanceData: number[],
  primaryDirection: string,
  opposingDirection: string,
  endDate: string
): SeriesOption {
  return {
    name: `Labels offset and program split`,
    type: 'custom',
    renderItem: (params: any, api) => {
      const [x, y] = api.coord([api.value(0), api.value(1)])
      const width = params.coordSys.width
      return {
        type: 'group',
        position: [width + 140, y + 11],
        children: [
          {
            type: 'text',
            style: {
              x: 60,
              y: 10,
              textVerticalAlign: 'bottom',
              textAlign: 'center',
              text:
                'Cycle Length: ' +
                api.value(2).toString() +
                's\n' +
                `Offset (${primaryDirection}: ${api.value(
                  3
                )}s | ${opposingDirection}: ${api.value(5)}s)\n` +
                `Split (${primaryDirection}: ${api.value(
                  4
                )}s | ${opposingDirection}: ${api.value(6)}s)\n`,
              textFill: '#000',
              fontSize: 10,
            },
          },
        ],
      }
    },
    data: distanceData.map((distance, index) => [
      endDate,
      distance,
      primaryPhaseData[index].cycleLength,
      primaryPhaseData[index].offset,
      primaryPhaseData[index].programmedSplit,
      opposingPhaseData[distanceData.length - 1 - index].offset,
      opposingPhaseData[distanceData.length - 1 - index].programmedSplit,
    ]),
  }
}

export function getDistancesLabelOption(
  data: TimeSpaceUnwrappedData,
  distanceData: number[],
  gridLeft: number
): SeriesOption {
  const { gridGap, dotOffset, cardGapToDot, verticalOffsetY } =
    TIME_SPACE_LOCATION_CARD_LAYOUT
  const dataPoints = distanceData.map((distance, index) => [
    data[index].end,
    distance,
    index !== distanceData.length - 1 ? data[index].distanceToNextLocation : '',
    index !== distanceData.length - 1 ? data[index].speed : '',
  ])
  return {
    name: `Labels distance`,
    type: 'custom',
    z: 4,
    renderItem: (params, api) => {
      if (params.dataIndex === dataPoints.length - 1) {
        return
      }

      const xDot = gridLeft - gridGap + dotOffset
      const cardRight = xDot - cardGapToDot
      const xLine = cardRight + (gridLeft - cardRight) / 2
      const valueCardWidth = TIME_SPACE_DISTANCE_VALUE_CARD_WIDTH
      const valueCardHeight = 26
      const valueCardRight = cardRight
      const valueCardLeft = valueCardRight - valueCardWidth
      const dividerX = valueCardLeft + valueCardWidth / 2
      const distanceText = `${(api.value(2) as number).toLocaleString()} ft`
      const speedText = `${api.value(3)} mph`
      const [, rawY] = api.coord([
        0,
        (api.value(1) as number) + (api.value(2) as number) / 2,
      ])
      const y = rawY + verticalOffsetY

      return {
        type: 'group',
        children: [
          {
            type: 'line',
            shape: {
              x1: xLine,
              y1: y,
              x2: valueCardRight,
              y2: y,
            },
            style: {
              stroke: Color.PlanB,
              lineWidth: 3,
            },
          },
          {
            type: 'rect',
            shape: {
              x: valueCardLeft,
              y: y - valueCardHeight / 2,
              width: valueCardWidth,
              height: valueCardHeight,
              r: 4,
            },
            style: {
              fill: 'rgba(86, 180, 233, 0.14)',
              stroke: 'rgba(86, 180, 233, 0.38)',
              lineWidth: 1,
            },
          },
          {
            type: 'line',
            shape: {
              x1: dividerX,
              y1: y - 8,
              x2: dividerX,
              y2: y + 8,
            },
            style: {
              stroke: 'rgba(86, 180, 233, 0.34)',
              lineWidth: 1,
            },
          },
          {
            type: 'text',
            style: {
              x: valueCardLeft + valueCardWidth / 4,
              y,
              text: distanceText,
              textFill: '#000',
              fontSize: 10,
              fontWeight: 600,
              textAlign: 'center',
              textVerticalAlign: 'middle',
            },
          },
          {
            type: 'text',
            style: {
              x: valueCardLeft + (valueCardWidth * 3) / 4,
              y,
              text: speedText,
              textFill: '#2B4C68',
              fontSize: 10,
              textAlign: 'center',
              textVerticalAlign: 'middle',
            },
          },
        ],
      }
    },
    data: dataPoints,
  }
}

export function getDraggableOffsetabelOption(
  data: TimeSpaceUnwrappedData,
  distanceData: number[],
  phaseType?: string,
  extraLinesByIndex?: Array<string[] | undefined> // optional future lines per row
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []

  for (let i = 0; i < data.length; i++) {
    const location = data[i]
    const distance = distanceData[i]

    const dataPoint: [string, number, number, number][] = [
      [
        location.end,
        distance,
        i !== distanceData.length - 1 ? location.distanceToNextLocation : 0,
        0, // offset value
      ],
    ]

    const seriesOption: SeriesOption = {
      name: `Offset amount`,
      id: `Offset ${location.locationIdentifier} ${phaseType?.length ? phaseType : ''}`,
      type: 'custom',
      data: dataPoint,
      renderItem: (params, api) => {
        const coordSys = params.coordSys
        const [, y] = api.coord([0, api.value(1) as number])

        const textX = coordSys.x + coordSys.width + 40
        const offsetValue = api.value(3)

        const fontSize = 10
        const lineGap = 12

        const extra = extraLinesByIndex?.[params.dataIndex] ?? []
        const lines = [`Offset: ${offsetValue}s`, ...extra]

        // Anchor block so it sits nicely next to the row.
        const blockTopY = y - 6

        // vertical divider should span the full block height
        const lineX = textX - 6
        const lineY1 = blockTopY - 2
        const lineY2 = blockTopY + (lines.length - 1) * lineGap + fontSize + 2

        return {
          type: 'group',
          children: [
            {
              type: 'line',
              shape: { x1: lineX, y1: lineY1, x2: lineX, y2: lineY2 },
              style: { stroke: '#000', lineWidth: 1 },
            },
            ...lines.map((text, idx) => ({
              type: 'text',
              style: {
                x: textX,
                y: blockTopY + idx * lineGap,
                text,
                textFill: '#000',
                fontSize,
              },
            })),
          ],
        }
      },
      clip: false,
    }

    seriesOptions.push(seriesOption)
  }

  return seriesOptions
}

type LabelColumn = 'left' | 'right'

type StaticDirectionTypeKey = keyof typeof staticDirectionTypes

const CARDINAL_DIRECTION_SVG_PATHS = ['m5 12 7-7 7 7', 'M12 19V5'] as const

const DIAGONAL_DIRECTION_SVG_PATHS = ['M17 17 7 7', 'M17 7H7v10'] as const

const DIRECTION_ICON_DATA_URLS = new Map<string, string>()

function getDirectionIconDataUrl(
  directionKey: StaticDirectionTypeKey,
  strokeColor = '#111111'
): string | null {
  const cacheKey = `${directionKey}:${strokeColor}`
  const cached = DIRECTION_ICON_DATA_URLS.get(cacheKey)
  if (cached) {
    return cached
  }

  const svgConfig = staticDirectionTypes[directionKey].chartSvg
  if (!svgConfig) {
    return null
  }

  const paths =
    svgConfig.family === 'diagonal'
      ? DIAGONAL_DIRECTION_SVG_PATHS
      : CARDINAL_DIRECTION_SVG_PATHS

  const svg = [
    `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="${strokeColor}" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">`,
    `<g transform="rotate(${svgConfig.rotationDeg} 12 12)">`,
    ...paths.map((path) => `<path d="${path}"/>`),
    '</g>',
    '</svg>',
  ].join('')

  const dataUrl = `data:image/svg+xml;utf8,${encodeURIComponent(svg)}`
  DIRECTION_ICON_DATA_URLS.set(cacheKey, dataUrl)
  return dataUrl
}

function getDirectionTypeKey(directionLabel: string): StaticDirectionTypeKey {
  const token = directionLabel.trim().split(/\s+/)[0]?.toUpperCase() ?? ''

  if ((token as StaticDirectionTypeKey) in staticDirectionTypes) {
    return token as StaticDirectionTypeKey
  }

  const prefixMatch = (
    Object.keys(staticDirectionTypes) as StaticDirectionTypeKey[]
  ).find((key) => key !== 'NA' && token.startsWith(key))

  return prefixMatch ?? 'NA'
}

function extractPercentValue(text: string): number | null {
  const match = text.match(/(\d+(?:\.\d+)?)%/)
  if (!match) {
    return null
  }

  const value = Number(match[1])
  if (!Number.isFinite(value)) {
    return null
  }

  return Math.max(0, Math.min(100, value))
}

export const CYCLE_LABEL_SERIES_ID_PREFIX = 'Cycle Labels '

export function generateCycleLabels(
  distanceData: number[],
  direction: string,
  _gridLeft = 0,
  headerTextByIndex?: Array<string | undefined>,
  linesByIndex?: Array<string[] | undefined>,
  column: LabelColumn = 'left',
  ignoredByIndex?: boolean[]
): SeriesOption {
  void _gridLeft

  const {
    cardWidth,
    cardRadius,
    headerHeight,
    cardGapFromPlot,
    cardGapBetween,
    verticalOffsetY,
    bodyPaddingX,
    bodyPaddingY,
    lineHeight,
    minBodyHeight,
  } = TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT

  return {
    id: `${CYCLE_LABEL_SERIES_ID_PREFIX}${direction}`,
    name: `Cycles ${direction}`,
    type: 'custom',
    silent: true,
    clip: false,
    z: 7,
    renderItem: (params, api) => {
      const rowIndex = params.dataIndex
      const [, y] = api.coord([0, api.value(0)])
      const coordSys = params.coordSys as { x: number; width: number }
      const isIgnored = Boolean(ignoredByIndex?.[rowIndex])
      const anchorY = y + CYCLE_SEGMENT_HEIGHT / 2 + verticalOffsetY
      const primaryCardLeft = coordSys.x + coordSys.width + cardGapFromPlot
      const cardLeft =
        column === 'left'
          ? primaryCardLeft
          : primaryCardLeft + cardWidth + cardGapBetween

      const headerText = headerTextByIndex?.[rowIndex]?.trim() || direction
      const headerAccentColor = getDirectionAccentColor(headerText)
      const headerDirectionKey = getDirectionTypeKey(headerText)
      const headerIconDataUrl = getDirectionIconDataUrl(
        headerDirectionKey,
        isIgnored ? '#94A3B8' : '#111111'
      )
      const detailLines = (linesByIndex?.[rowIndex] ?? []).filter(Boolean)
      const bodyHeight = detailLines.length
        ? Math.max(
            minBodyHeight,
            detailLines.length * lineHeight + bodyPaddingY * 2
          )
        : 0
      const cardHeight = headerHeight + bodyHeight
      const cardTop = anchorY - cardHeight / 2
      const bodyTop = cardTop + headerHeight
      const textX = cardLeft + bodyPaddingX
      const iconSize = 10
      const headerTextX = textX + (headerIconDataUrl ? iconSize + 3 : 0)
      const detailPieRadius = 4.5
      const detailPieCenterX =
        cardLeft + cardWidth - bodyPaddingX - detailPieRadius
      const detailTextWidth = Math.max(
        0,
        detailPieCenterX - textX - detailPieRadius - 6
      )

      return {
        type: 'group',
        children: [
          {
            type: 'rect',
            z2: 10,
            shape: {
              x: cardLeft,
              y: cardTop,
              width: cardWidth,
              height: cardHeight,
              r: cardRadius,
            },
            style: {
              fill: '#FFFFFF',
              stroke: '#D9DEE6',
              lineWidth: 1,
            },
          },
          {
            type: 'rect',
            z2: 11,
            shape: {
              x: cardLeft,
              y: cardTop,
              width: 3,
              height: cardHeight,
              r: bodyHeight > 0 ? [cardRadius, 0, 0, cardRadius] : cardRadius,
            },
            style: {
              fill: headerAccentColor,
              opacity: isIgnored ? 0.5 : 1,
            },
          },
          {
            type: 'rect',
            z2: 12,
            shape: {
              x: cardLeft + 3,
              y: cardTop,
              width: cardWidth - 3,
              height: headerHeight,
              r:
                bodyHeight > 0
                  ? [0, cardRadius, 0, 0]
                  : [0, cardRadius, cardRadius, 0],
            },
            style: {
              fill: isIgnored ? '#F1F5F9' : '#EEF1F5',
              opacity: isIgnored ? 0.88 : 1,
            },
          },
          ...(headerIconDataUrl
            ? [
                {
                  type: 'image' as const,
                  z2: 20,
                  style: {
                    x: textX,
                    y: cardTop + (headerHeight - iconSize) / 2,
                    image: headerIconDataUrl,
                    width: iconSize,
                    height: iconSize,
                    opacity: isIgnored ? 0.4 : 1,
                  },
                },
              ]
            : [
                {
                  type: 'text' as const,
                  z2: 20,
                  style: {
                    x: textX,
                    y: cardTop + headerHeight / 2,
                    text: '?',
                    textAlign: 'left',
                    textVerticalAlign: 'middle',
                    fill: isIgnored ? '#94A3B8' : '#111',
                    fontSize: 10,
                    fontWeight: 700,
                  },
                },
              ]),
          {
            type: 'text',
            z2: 20,
            style: {
              x: headerTextX,
              y: cardTop + headerHeight / 2,
              text: headerText,
              textAlign: 'left',
              textVerticalAlign: 'middle',
              fill: isIgnored ? '#94A3B8' : '#111',
              fontSize: 10,
            },
          },
          ...detailLines.flatMap((line, index) => {
            const percentValue = extractPercentValue(line)
            const lineY = bodyTop + bodyPaddingY + index * lineHeight
            const pieChildren =
              percentValue === null
                ? []
                : [
                    {
                      type: 'circle' as const,
                      z2: 20,
                      shape: {
                        cx: detailPieCenterX,
                        cy: lineY + lineHeight / 2 - 1,
                        r: detailPieRadius,
                      },
                      style: {
                        fill: '#E2E8F0',
                        opacity: isIgnored ? 0.45 : 1,
                      },
                    },
                    ...(percentValue > 0
                      ? [
                          {
                            type: 'sector' as const,
                            z2: 21,
                            shape: {
                              cx: detailPieCenterX,
                              cy: lineY + lineHeight / 2 - 1,
                              r: detailPieRadius,
                              r0: 0,
                              startAngle: -Math.PI / 2,
                              endAngle:
                                -Math.PI / 2 +
                                (percentValue / 100) * Math.PI * 2,
                              clockwise: true,
                            },
                            style: {
                              fill: Color.Black,
                              opacity: isIgnored ? 0.55 : 1,
                            },
                          },
                        ]
                      : []),
                  ]

            return [
              {
                type: 'text' as const,
                z2: 20,
                style: {
                  x: textX,
                  y: lineY,
                  text: line,
                  width: detailTextWidth,
                  overflow: 'truncate',
                  lineHeight,
                  textAlign: 'left',
                  textVerticalAlign: 'top',
                  fill: isIgnored ? '#94A3B8' : '#222',
                  fontSize: 10,
                  fontWeight: 500,
                },
              },
              ...pieChildren,
            ]
          }),
        ],
      }
    },
    data: distanceData,
  } satisfies SeriesOption
}
