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
  TimeSpaceResponseData,
} from '@/features/charts/timeSpaceDiagram/shared/types'
import { Cycle } from '@/features/charts/timingAndActuation/types'
import { Color } from '@/features/charts/utils'
import { dateToTimestamp } from '@/utils/dateTime'
import { CustomSeriesRenderItemReturn, SeriesOption } from 'echarts'

export function generateCycles(
  data: TimeSpaceResponseData,
  distanceData: number[],
  colorMap: Map<number, string>,
  phaseType?: string
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []
  for (let i = 0; i < data.length; i++) {
    const cycleEvents = getCycleEvents(data[i].cycleAllEvents, distanceData[i])
    const seriesOption: SeriesOption = {
      name: `Cycles ${phaseType?.length ? phaseType : ''}`,
      id: `Cycles ${data[i].locationIdentifier} ${phaseType?.length ? phaseType : ''}`,
      type: 'custom',
      clip: true,
      z: 5,
      silent: true,
      data: cycleEvents,
      renderItem: (param, api): CustomSeriesRenderItemReturn => {
        const i = param.dataIndex
        if (!cycleEvents || i >= cycleEvents.length - 1) {
          return
        }
        const nextIndex = i + 1

        const [x1, y1, v1] = [api.value(0), api.value(1), api.value(2)]

        const [x2, y2, v2] = [
          api.value(0, nextIndex),
          api.value(1, nextIndex),
          api.value(2, nextIndex),
        ]
        const newX2 = new Date(x2).getTime()
        const p1 = api.coord([x1, y1])
        const p2 = api.coord([newX2, y2])
        return {
          type: 'rect',
          shape: {
            x: p1[0],
            y: p1[1],
            width: p2[0] - p1[0],
            height: 10,
          },
          style: {
            fill: getSegmentColor(v1 as number, v2 as number),
            opacity: 0.9,
          },
        }
      },
    }
    seriesOptions.push(seriesOption)
  }
  return seriesOptions
}

function getSegmentColor(from: number, to: number): string {
  if (from === 1 && to === 3) return 'lightgreen'
  if (from === 3 && to === 8) return 'green'
  if (from === 1 && to === 8) return 'green'
  if (from === 8 && to === 9) return 'yellow'
  if (from === 9 && to === 1) return 'red'
  return '#999' // fallback
}

// export function generateCycles(
//   data: TimeSpaceResponseData,
//   distanceData: number[],
//   colorMap: Map<number, string>,
//   phaseType?: string
// ): SeriesOption[] {
//   const series: SeriesOption[] = []

//   const greenBands = getBandData(data, distanceData, 1)
//   const yellowBands = getBandData(data, distanceData, 8)
//   const redBands = getBandData(data, distanceData, 9)

//   const bandSpecs = [
//     { items: greenBands, color: colorMap.get(1) },
//     { items: yellowBands, color: colorMap.get(8) },
//     { items: redBands, color: colorMap.get(9) },
//   ]

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
//   data: TimeSpaceResponseData,
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

function getCycleEvents(
  data: Cycle[] | null,
  distanceData: number
): [string, number, number][] {
  if (!data) return

  return data.map((e) => [e.start, distanceData, e.value])
}

function getDataByValue(
  data: TimeSpaceResponseData,
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
  data: TimeSpaceResponseData,
  distanceData: number[],
  phaseType?: string,
  isPrimary?: boolean
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []
  for (let i = 0; i < data.length; i++) {
    const location = data[i]
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
          location.calculatedDistanceToNext,
          location.speed,
          x1 as string
        )
        const nextPointFinalX = getArrivalTime(
          location.calculatedDistanceToNext,
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
            opacity: 0.2,
            fill: 'green',
          },
        }
      },
    }
    seriesOptions.push(seriesOption)
  }
  return seriesOptions
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
//   data: TimeSpaceResponseData,
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
  data: TimeSpaceResponseData,
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

export function getLocationsLabelOption(
  data: TimeSpaceResponseData,
  distanceData: number[]
): SeriesOption {
  // x = width of longest label line
  const x = getLongestLabelLineWidth(data)

  // per your spec:
  // text right edge at x
  // dot at x+20
  // grid starts at x+40
  const DOT_OFFSET = 10
  const GRID_GAP = 70

  const series: SeriesOption = {
    name: 'Location axis',
    type: 'custom',
    silent: true,
    clip: false,
    renderItem: (params, api) => {
      const idx = params.dataIndexInside ?? params.dataIndex
      const len = params.dataInsideLength ?? distanceData.length
      const coordSys = params.coordSys as any

      const [, y] = api.coord([api.value(0), api.value(1)])

      // coordSys.x === grid.left (the start of the plot area)
      // Therefore:
      //   xTextRight = coordSys.x - GRID_GAP === x
      //   xDot       = coordSys.x - (GRID_GAP - DOT_OFFSET) === x+20
      const xTextRight = coordSys.x - GRID_GAP
      const xDot = coordSys.x - (GRID_GAP - DOT_OFFSET)

      const children: any[] = []

      // Draw the vertical connector once
      if (idx === 0 && len > 1) {
        const last = len - 1
        const [, yTop] = api.coord([api.value(0, 0), api.value(1, 0)])
        const [, yBottom] = api.coord([api.value(0, last), api.value(1, last)])

        children.push({
          type: 'line',
          shape: {
            x1: xDot,
            y1: yTop,
            x2: xDot,
            y2: yBottom,
          },
          style: {
            stroke: Color.Orange,
            lineWidth: 3,
          },
          z2: 1,
        })
      }

      // Circle node
      children.push({
        type: 'circle',
        shape: { cx: xDot, cy: y, r: 7 },
        style: { fill: '#fff', stroke: Color.Orange, lineWidth: 3 },
        z2: 2,
      })

      // Label text
      const ident = String(api.value(2) ?? '')
      const { primary, secondary } = splitPrimarySecondary(
        String(api.value(3) ?? '')
      )

      const lineGap = 14
      const blockTop = y - lineGap

      children.push({
        type: 'group',
        children: [
          // Line 1: identifier (bold)
          {
            type: 'text',
            style: {
              x: xTextRight,
              y: blockTop,
              text: ident,
              textAlign: 'right',
              textVerticalAlign: 'middle',
              fill: '#111',
              fontSize: 14,
              fontWeight: 700,
            },
            z2: 2,
          },

          // Line 2: primary + "&"
          {
            type: 'text',
            style: {
              x: xTextRight,
              y: blockTop + lineGap,
              text: primary ? `${primary} &` : '',
              textAlign: 'right',
              textVerticalAlign: 'middle',
              fill: '#333',
              fontSize: 12,
              fontWeight: 400,
            },
            z2: 2,
          },

          // Line 3: secondary
          {
            type: 'text',
            style: {
              x: xTextRight,
              y: blockTop + lineGap * 2,
              text: secondary ?? '',
              textAlign: 'right',
              textVerticalAlign: 'middle',
              fill: '#333',
              fontSize: 12,
              fontWeight: 400,
            },
            z2: 2,
          },
        ],
      })

      return { type: 'group', children }
    },

    data: distanceData.map((distance, index) => [
      data[index].start, // any valid x for coord calc
      distance,
      data[index].locationIdentifier,
      data[index].locationDescription,
    ]),
  }

  // Optional: expose x too (handy for debugging)
  ;(series as any).gridLeft = x // == coordSys.x - 40

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
  data: TimeSpaceResponseData,
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
              x: gridLeft + 35,
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
  data: TimeSpaceResponseData,
  distanceData: number[],
  phaseType?: string
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
        0,
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
        const textY = y - 6

        const offsetValue = api.value(3)
        const fontSize = 10
        const text = `Offset: ${offsetValue}s`

        const textHeight = fontSize
        const lineHeight = textHeight * 2

        const lineX = textX - 6
        const textCenterY = textY - textHeight / 2 + 10
        const lineY1 = textCenterY - lineHeight / 2
        const lineY2 = textCenterY + lineHeight / 2

        return {
          type: 'group',
          children: [
            {
              type: 'line',
              shape: { x1: lineX, y1: lineY1, x2: lineX, y2: lineY2 },
              style: { stroke: '#000', lineWidth: 1 },
            },
            {
              type: 'text',
              style: {
                x: textX,
                y: textY,
                text,
                textFill: '#000',
                fontSize,
              },
            },
          ],
        }
      },
      clip: false,
    }
    seriesOptions.push(seriesOption)
  }
  return seriesOptions
}

export function generateCycleLabels(
  distanceData: number[],
  direction: string,
  gridLeft: number
): SeriesOption {
  return {
    name: `Cycles ${direction}`,
    type: 'custom',
    renderItem: (params, api) => {
      const [, y] = api.coord([0, api.value(0)])
      const width = params.coordSys.width
      return {
        type: 'group',
        position: [width + 100, y],
        children: [
          {
            type: 'text',
            style: {
              x: gridLeft,
              y: 0,
              textAlign: 'center',
              text: direction,
              fontSize: 10,
            },
          },
        ],
      }
    },
    data: distanceData,
  }
}
