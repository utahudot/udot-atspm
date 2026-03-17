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

const CYCLE_SEGMENT_HEIGHT = 12
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
  gridGap: 70,
  dotOffset: 10,
  cardGapToDot: 12,
  cardWidth: 200,
  cardRadius: 4,
  headerHeight: 44,
  bodyHeight: 30,
  bodyPaddingLeft: 12,
  bodyPaddingRight: 12,
  headerActionSize: 12,
  headerActionRight: 10,
} as const

export const TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT = {
  cardWidth: 90,
  cardRadius: 2,
  headerHeight: 18,
  cardGapFromPlot: 18,
  cycleGapY: 2,
  verticalOffsetY: -4,
  connectorOffsetYUp: -7,
  connectorInsetNearCard: 8,
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
  } = TIME_SPACE_LOCATION_CARD_LAYOUT
  const CARD_H = headerHeight + bodyHeight

  const series: SeriesOption = {
    name: 'Location axis',
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
      const cardTop = y - CARD_H / 2
      const textX = cardLeft + bodyPaddingLeft
      const iconLeft = cardRight - headerActionRight - headerActionSize
      const dividerX = iconLeft - 8
      const titleWidth = Math.max(0, dividerX - textX - 8)

      const children: any[] = []

      const lineColor = Color.LightBlue

      if (idx === 0 && len > 1) {
        const last = len - 1
        const [, yTop] = api.coord([api.value(0, 0), api.value(1, 0)])
        const [, yBottom] = api.coord([api.value(0, last), api.value(1, last)])

        children.push({
          type: 'line',
          shape: { x1: xDot, y1: yTop, x2: xDot, y2: yBottom },
          style: { stroke: Color.PlanB, lineWidth: 3 },
          z2: 1,
        })
      }

      const location = data.find(
        (loc) => loc.locationIdentifier === api.value(2).toString()
      )

      // Circle node
      children.push({
        type: 'circle',
        shape: { cx: xDot, cy: y, r: 7 },
        style: { fill: '#fff', stroke: lineColor, lineWidth: 3 },
        z2: 3,
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
      const detailText = `Cycle Length: ${location?.cycleLength ?? 'N/A'}`

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
              fill: '#FFFFFF',
              stroke: '#D9DEE6',
              lineWidth: 1,
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
            style: { fill: '#EEF1F5' },
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
                  fill: '#111',
                  fontSize: 11,
                  fontWeight: 700,
                },
                name: {
                  fill: '#111',
                  fontSize: 11,
                  fontWeight: 400,
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
              stroke: '#CBD5E1',
              lineWidth: 1,
            },
          },
          {
            type: 'text',
            z2: 20,
            style: {
              x: textX,
              y: cardTop + headerHeight + 6,
              text: detailText,
              width: cardWidth - bodyPaddingLeft - bodyPaddingRight,
              overflow: 'break',
              lineHeight: 13,
              textAlign: 'left',
              textVerticalAlign: 'top',
              fill: '#374151',
              fontSize: 11,
              fontWeight: 500,
            },
          },
        ],
      })

      return { type: 'group', children }
    },

    data: distanceData.map((distance, index) => [
      data[index].start,
      distance,
      data[index].locationIdentifier,
      data[index].locationDescription,
    ]),
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
  const dataPoints = distanceData.map((distance, index) => [
    data[index].end,
    distance,
    index !== distanceData.length - 1 ? data[index].distanceToNextLocation : '',
    index !== distanceData.length - 1 ? data[index].speed : '',
  ])
  return {
    name: `Labels distance`,
    type: 'custom',
    renderItem: (params, api) => {
      const [, y] = api.coord([
        0,
        (api.value(1) as number) + (api.value(2) as number) / 2,
      ])
      return {
        type: 'group',
        children: [
          {
            type: 'text',
            style: {
              x: gridLeft - 45,
              y: y - 10,
              text:
                params.dataIndex !== dataPoints.length - 1
                  ? api.value(2).toLocaleString() +
                    ' ft' +
                    '\n' +
                    api.value(3).toString() +
                    ' mph'
                  : '',
              textFill: '#000',
              fontSize: 10,
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

type ExpandDir = 'up' | 'down'

type StaticDirectionTypeKey = keyof typeof staticDirectionTypes

const CARDINAL_DIRECTION_SVG_PATHS = ['m5 12 7-7 7 7', 'M12 19V5'] as const

const DIAGONAL_DIRECTION_SVG_PATHS = ['M17 17 7 7', 'M17 7H7v10'] as const

const DIRECTION_ICON_DATA_URLS = new Map<StaticDirectionTypeKey, string>()

function getDirectionIconDataUrl(
  directionKey: StaticDirectionTypeKey
): string | null {
  const cached = DIRECTION_ICON_DATA_URLS.get(directionKey)
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
    '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="#111111" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">',
    `<g transform="rotate(${svgConfig.rotationDeg} 12 12)">`,
    ...paths.map((path) => `<path d="${path}"/>`),
    '</g>',
    '</svg>',
  ].join('')

  const dataUrl = `data:image/svg+xml;utf8,${encodeURIComponent(svg)}`
  DIRECTION_ICON_DATA_URLS.set(directionKey, dataUrl)
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

export const CYCLE_LABEL_SERIES_ID_PREFIX = 'Cycle Labels '

export function generateCycleLabels(
  distanceData: number[],
  direction: string,
  _gridLeft = 0,
  linesByIndex?: Array<string[] | undefined>,
  expand: ExpandDir = 'down'
): SeriesOption {
  void _gridLeft

  const {
    cardWidth,
    cardRadius,
    headerHeight,
    cardGapFromPlot,
    cycleGapY,
    verticalOffsetY,
    connectorOffsetYUp,
    connectorInsetNearCard,
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
      const plotRight = coordSys.x + coordSys.width
      const cardLeft = coordSys.x + coordSys.width + cardGapFromPlot

      const headerText = direction
      const headerDirectionKey = getDirectionTypeKey(direction)
      const headerIconDataUrl = getDirectionIconDataUrl(headerDirectionKey)
      const detailLines = (linesByIndex?.[rowIndex] ?? []).filter(Boolean)
      const detailText = detailLines.join('\n')
      const bodyHeight = detailLines.length
        ? Math.max(
            minBodyHeight,
            detailLines.length * lineHeight + bodyPaddingY * 2
          )
        : 0
      const cardHeight = headerHeight + bodyHeight
      const cardTop =
        (expand === 'down'
          ? y + CYCLE_SEGMENT_HEIGHT / 2 + cycleGapY
          : y - cardHeight - cycleGapY) + verticalOffsetY
      const bodyTop = cardTop + headerHeight
      const cycleAnchorY =
        y +
        CYCLE_SEGMENT_HEIGHT / 2 +
        (expand === 'up' ? connectorOffsetYUp : -7)
      const cardCenterY = cardTop + cardHeight / 2
      const connectorX = cardLeft - connectorInsetNearCard
      const textX = cardLeft + bodyPaddingX
      const iconSize = 10
      const headerTextX = textX + (headerIconDataUrl ? iconSize + 3 : 0)

      return {
        type: 'group',
        children: [
          {
            type: 'polyline',
            z2: 9,
            shape: {
              points: [
                [plotRight, cycleAnchorY],
                [connectorX, cycleAnchorY],
                [connectorX, cardCenterY],
                [cardLeft, cardCenterY],
              ],
            },
            style: {
              stroke: '#6B7280',
              lineWidth: 1,
              fill: undefined,
              lineJoin: 'round',
              lineCap: 'round',
            },
          },
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
              width: cardWidth,
              height: headerHeight,
              r: bodyHeight > 0 ? [cardRadius, cardRadius, 0, 0] : cardRadius,
            },
            style: { fill: '#EEF1F5' },
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
                    fill: '#111',
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
              fill: '#111',
              fontSize: 10,
              fontWeight: 700,
            },
          },
          ...(detailText
            ? [
                {
                  type: 'text' as const,
                  z2: 20,
                  style: {
                    x: textX,
                    y: bodyTop + bodyPaddingY,
                    text: detailText,
                    width: cardWidth - bodyPaddingX * 2,
                    overflow: 'break',
                    lineHeight,
                    textAlign: 'left',
                    textVerticalAlign: 'top',
                    fill: '#222',
                    fontSize: 10,
                    fontWeight: 500,
                  },
                },
              ]
            : []),
        ],
      }
    },
    data: distanceData,
  } satisfies SeriesOption
}
