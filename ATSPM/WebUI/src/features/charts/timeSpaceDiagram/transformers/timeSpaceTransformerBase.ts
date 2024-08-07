import { SeriesOption } from 'echarts'
import { RawTimeSpaceAverageData, TimeSpaceResponseData } from '../types'

export function generateCycles(
  data: TimeSpaceResponseData,
  distanceData: number[],
  colorMap: Map<number, string>,
  phaseType?: string
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []
  const bandTypes = [1, 8, 9]
  const greenEventOptions = getDataByValue(data, distanceData, 1)
  const yellowEventOptions = getDataByValue(data, distanceData, 8)
  const redEventOptions = getDataByValue(data, distanceData, 9)

  for (let i = 0; i < 3; i++) {
    const seriesOption: SeriesOption = {
      name: `Cycles ${phaseType?.length ? phaseType : ''}`,
      type: 'line',
      symbol: 'none',
      z: 5,
      lineStyle: {
        width: 6,
        color: colorMap.get(bandTypes[i]),
      },
      data:
        bandTypes[i] === 1
          ? greenEventOptions
          : bandTypes[i] === 8
            ? yellowEventOptions
            : redEventOptions,
    }
    seriesOptions.push(seriesOption)
  }

  // seriesOptions.push(labelSeriesOption)
  return seriesOptions
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
  phaseType?: string
): SeriesOption {
  const dataPoints = getGreenEventsDataPoints(data, distanceData)

  return {
    name: `Green Bands ${phaseType?.length ? phaseType : ''}`,
    type: 'custom',
    data: dataPoints,
    clip: true,
    renderItem: function (params, api) {
      if (params.context.rendered) {
        return
      }
      params.context.rendered = true
      let points = []
      const polygons: any[] = []
      const color = 'green'
      for (let i = 0; i < dataPoints.length; i++) {
        if (dataPoints[i] === null) {
          polygons.push({
            type: 'polygon',
            transition: ['shape'],
            shape: {
              points: points,
            },
            style: {
              z: -1,
              opacity: 0.2,
              fill: color,
            },
          })
          points = []
        } else {
          points.push(api.coord(dataPoints[i]))
        }
      }
      return {
        type: 'group',
        children: polygons,
      }
    },
  }
}

function getGreenEventsDataPoints(
  data: TimeSpaceResponseData,
  distanceData: number[]
) {
  return data.reduce((result, location, index) => {
    if (location.greenTimeEvents) {
      const greenEvents = location.greenTimeEvents
      for (let i = 0; i < greenEvents.length; ) {
        const currPoint = greenEvents[i]
        const nextPoint = greenEvents[i + 1]
        if (i === 0 && currPoint.isDetectorOn === false) {
          result.push(
            [location.start, distanceData[index]],
            [currPoint.initialX, distanceData[index]],
            [currPoint.finalX, distanceData[index + 1]],
            [location.start, distanceData[index + 1]],
            null
          )
          i++
        } else if (
          i === greenEvents.length - 1 &&
          currPoint.isDetectorOn === true
        ) {
          result.push(
            [currPoint.initialX, distanceData[index]],
            [location.end, distanceData[index]],
            [location.end, distanceData[index + 1]],
            [currPoint.finalX, distanceData[index + 1]],
            null
          )
          i++
        } else if (currPoint.isDetectorOn === false) {
          i++
        } else {
          result.push(
            ...[
              [currPoint.initialX, distanceData[index]],
              [nextPoint.initialX, distanceData[index]],
              [nextPoint.finalX, distanceData[index + 1]],
              [currPoint.finalX, distanceData[index + 1]],
              null,
            ]
          )
          i += 2
        }
      }
    }
    return result
  }, [] as any)
}

export function getLocationsLabelOption(
  data: TimeSpaceResponseData,
  distanceData: number[]
): SeriesOption {
  return {
    name: `Labels location`,
    type: 'custom',
    renderItem: (params, api) => {
      const [, y] = api.coord([api.value(0), api.value(1)])
      return {
        type: 'group',
        position: [0, y],
        children: [
          {
            type: 'path',
            shape: {
              d: 'M0,0 L0,-20 L30,-20 C42,-20 38,-1 50,-1 L70,-1 L30,0 Z',
              x: 0,
              y: -20,
              width: 90,
              height: 20,
              layout: 'cover',
            },
            style: {
              fill: 'lightgreen',
              opacity: 0.7,
            },
          },
          {
            type: 'text',
            style: {
              x: 22,
              y: -1,
              textVerticalAlign: 'bottom',
              textAlign: 'center',
              text: api.value(2).toString(),
              textFill: '#000',
              fontSize: 15,
            },
          },
        ],
      }
    },
    data: distanceData.map((distance, index) => [
      data[index].start,
      distance,
      data[index].locationIdentifier,
    ]),
  }
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
  distanceData: number[]
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
              x: 50,
              y: y - 10,
              text:
                params.dataIndex !== dataPoints.length - 1
                  ? api.value(2).toString() +
                    ' ft' +
                    '\n' +
                    api.value(3).toString() +
                    'mph'
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

export function generatePrimaryCycleLabels(
  distanceData: number[],
  primaryDirection: string
): SeriesOption {
  return {
    name: `Cycles ${primaryDirection}`,
    type: 'custom',
    renderItem: (params: any, api) => {
      const [, y] = api.coord([0, api.value(0)])
      const width = params.coordSys.width
      return {
        type: 'group',
        position: [width + 100, y],
        children: [
          {
            type: 'text',
            style: {
              x: 15,
              y: -4,
              textAlign: 'center',
              text: primaryDirection,
              textFill: '#000',
              fontSize: 10,
            },
          },
        ],
      }
    },
    data: distanceData,
  }
}

export function generateOpposingCycleLabels(
  reverseDistanceData: number[],
  opposingDirection: string
): SeriesOption {
  return {
    name: `Cycles ${opposingDirection}`,
    type: 'custom',
    renderItem: (params: any, api) => {
      const [, y] = api.coord([0, api.value(0)])
      const width = params.coordSys.width
      return {
        type: 'group',
        position: [width + 100, y],
        children: [
          {
            type: 'text',
            style: {
              x: 15,
              y: -7,
              textAlign: 'center',
              text: opposingDirection,
              textFill: '#000',
              fontSize: 10,
            },
          },
        ],
      }
    },
    data: reverseDistanceData,
  }
}
