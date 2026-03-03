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
import { Color } from '@/features/charts/utils'
import { dateToTimestamp } from '@/utils/dateTime'
import {
  CustomSeriesRenderItemAPI,
  CustomSeriesRenderItemParams,
  CustomSeriesRenderItemReturn,
  GridComponentOption,
  SeriesOption,
} from 'echarts'

// export function generateCycles(
//   data: TimeSpaceResponseData,
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
  data: TimeSpaceResponseData,
  distanceData: number[],
  phaseType?: string
): SeriesOption[] {
  return data.map((phase, index) => {
    const distance = distanceData[index]
    const nextDistance = distanceData[index + 1] ?? distance + 300 // or whatever spacing you use
    const hasData = hasCycleData(phase.cycleAllEvents)

    const cycleEvents = hasData
      ? getCycleEvents(phase.cycleAllEvents, distance)
      : [[0, distance, 0]]

    return {
      name: `Cycles ${phaseType ?? ''}`,
      id: `Cycles ${phase.locationIdentifier} ${phaseType ?? ''}`,
      type: 'custom',
      clip: true,
      z: 5,
      data: cycleEvents,

      renderItem: (param, api): CustomSeriesRenderItemReturn => {
        if (!hasData) {
          // console.log(
          //   `${phase.locationIdentifier} has no cycles for ${phaseType}`
          // )
          return renderMissingCycle(api, param, distance, nextDistance)
          // return renderMissingCycle(param, distance)
        }

        return renderCycleSegment(api, cycleEvents, param.dataIndex)
      },
    }
  })
}

function renderCycleSegment(
  api: CustomSeriesRenderItemAPI,
  cycleEvents: any[],
  index: number
): CustomSeriesRenderItemReturn {
  if (index >= cycleEvents.length - 1) return

  const [x1, y1, v1] = [api.value(0), api.value(1), api.value(2)]

  const [x2, y2, v2] = [
    api.value(0, index + 1),
    api.value(1, index + 1),
    api.value(2, index + 1),
  ]

  const p1 = api.coord([x1, y1])
  const p2 = api.coord([new Date(x2).getTime(), y2])

  const fill = getCycleColor(v1)

  return {
    type: 'rect',
    cursor: 'pointer',
    shape: {
      x: p1[0],
      y: p1[1],
      width: p2[0] - p1[0],
      height: 10,
    },
    style: {
      fill,
      opacity: 0.6,
    },
  }
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
      height: 10,
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
//   data: TimeSpaceResponseData,
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
  data: TimeSpaceResponseData,
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

export function getLocationsLabelOption(
  data: TimeSpaceResponseData,
  distanceData: number[],
  grid: GridComponentOption
): SeriesOption {
  const gridLeft = (grid.left as number) ?? 0

  const GRID_GAP = 70 // distance from grid start (left) to the "label anchor" line
  const DOT_OFFSET = 10 // dot sits DOT_OFFSET to the right of the label anchor
  const CARD_GAP_TO_DOT = 12 // gap between card and dot

  const CARD_WIDTH = 180
  const CARD_RADIUS = 8

  const HEADER_H = 26
  const BODY_H = 48
  const CARD_H = HEADER_H + BODY_H

  const BODY_PAD_LEFT = 12

  const series: SeriesOption = {
    name: 'Location axis',
    type: 'custom',
    silent: true,
    clip: false,
    renderItem: (params, api) => {
      const idx = params.dataIndexInside ?? params.dataIndex
      const len = params.dataInsideLength ?? distanceData.length

      const [, y] = api.coord([api.value(0), api.value(1)])

      const xTextRight = gridLeft - GRID_GAP
      const xDot = xTextRight + DOT_OFFSET

      const cardRight = xDot - CARD_GAP_TO_DOT
      const cardLeft = cardRight - CARD_WIDTH
      const cardTop = y - CARD_H / 2
      const textX = cardLeft + BODY_PAD_LEFT

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
              width: CARD_WIDTH,
              height: CARD_H,
              r: CARD_RADIUS,
            },
            style: {
              fill: '#FFFFFF',
              stroke: '#D9DEE6',
              lineWidth: 1,
              shadowBlur: 6,
              shadowColor: 'rgba(0,0,0,0.08)',
              shadowOffsetX: 0,
              shadowOffsetY: 2,
            },
          },

          // Header background (grey)
          {
            type: 'rect',
            z2: 11,
            shape: {
              x: cardLeft,
              y: cardTop,
              width: CARD_WIDTH,
              height: HEADER_H,
              r: [CARD_RADIUS, CARD_RADIUS, 0, 0],
            },
            style: { fill: '#EEF1F5' },
          },

          // Identifier (centered in header)
          {
            type: 'text',
            z2: 20,
            style: {
              x: cardLeft + CARD_WIDTH / 2,
              y: cardTop + HEADER_H / 2,
              text: ident,
              textAlign: 'center',
              textVerticalAlign: 'middle',
              fill: '#111',
              fontSize: 14,
              fontWeight: 700,
            },
          },

          // Primary/secondary combined (wrap inside card)
          {
            type: 'text',
            z2: 20,
            style: {
              x: textX,
              y: cardTop + HEADER_H + 5,
              text: name,
              width: CARD_WIDTH - BODY_PAD_LEFT * 2,
              overflow: 'break',
              lineHeight: 18,
              textAlign: 'left',
              textVerticalAlign: 'top',
              fill: '#222',
              fontSize: 13,
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
  data: TimeSpaceResponseData,
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

export function generateCycleLabels(
  distanceData: number[],
  direction: string,
  gridLeft: number,
  linesByIndex?: Array<string[] | undefined>,
  expand: ExpandDir = 'down'
): SeriesOption {
  return {
    name: `Cycles ${direction}`,
    type: 'custom',
    renderItem: (params, api) => {
      const rowIndex = params.dataIndex
      const [, y] = api.coord([0, api.value(0)])
      const width = params.coordSys.width

      const fontSize = 10
      const lineGap = 20

      const extra = linesByIndex?.[rowIndex] ?? []
      const lines = [direction, ...extra] // direction always first

      // Anchor at the row baseline and choose whether the block grows up or down
      const startY = expand === 'down' ? 0 : -(lines.length * lineGap) - 15 // move up so the last line ends at y=0

      return {
        type: 'group',
        position: [width + 100, y],

        children: lines.map((text, i) => ({
          type: 'text',
          style: {
            backgroundColor: '#f2f2f2',
            padding: [8, 12],
            borderRadius: 6,
            textStyle: {
              fontWeight: 400,
              fontSize: 12,
              rich: {
                values: { fontWeight: 600 },
              },
            },
            x: gridLeft,
            y: startY + i * lineGap,
            text,
            fontSize,
          },
        })),
      }
    },
    data: distanceData,
  }
}
