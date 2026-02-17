// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - transformers.ts
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
  BasePlan,
  ChartType,
  DataPoint,
  MarkAreaData,
  PlanData,
  PlanOptions,
  ToolType,
} from '@/features/charts/common/types'
import { Color } from '@/features/charts/utils'
import { format } from 'date-fns'
import {
  CustomSeriesRenderItemAPI,
  DataZoomComponentOption,
  GridComponentOption,
  LegendComponentOption,
  MarkAreaComponentOption,
  SeriesOption,
  TitleComponentOption,
  ToolboxComponentOption,
  TooltipComponentOption,
  XAXisComponentOption,
  YAXisComponentOption,
} from 'echarts'

export function transformSeriesData(
  dataPoints: DataPoint[]
): (string | number)[][] {
  const series: (string | number)[][] = []
  for (const dataPoint of dataPoints) {
    const values: (string | number)[] = [
      dataPoint.timestamp,
      dataPoint.value.toFixed(2),
    ]
    series.push(values)
  }
  return series
}

export function createSeries(...seriesInputs: SeriesOption[]) {
  const defaultProperties: SeriesOption = {}

  seriesInputs.map((seriesInput) => {
    if (seriesInput.type === 'line') {
      seriesInput.symbol = 'line'
      seriesInput.symbolSize = 0
    }
  })

  return seriesInputs.map((seriesInput) => ({
    ...defaultProperties,
    ...seriesInput,
  })) as SeriesOption[]
}

function getMidpointTimestamp(start: string, end: string): string {
  const startTime = new Date(start).getTime()
  const endTime = new Date(end).getTime()
  return new Date((startTime + endTime) / 2).toISOString()
}

export function createPlans<T extends BasePlan>(
  plans: T[],
  planYAxisIndex: number,
  options?: PlanOptions<T>,
  yLineLength?: number,
  xAxisIndex?: number,
  backgroundColor?: string
): SeriesOption {
  const markAreaData: MarkAreaData[] = []
  const planData: PlanData[] = []

  for (let i = 0; i < plans.length; i++) {
    const plan = []
    const planColor = i % 2 === 0 ? Color.PlanA : Color.PlanB

    const startTime = new Date(plans[i].start).toISOString()
    const endTime = new Date(plans[i].end).toISOString()

    let planInfo = `{plan|${plans[i].planDescription}}`

    for (const option in options) {
      const key = option as keyof Omit<T, keyof BasePlan>
      const value = plans[i][key]

      if (value == null) continue

      const formatter = options[key]
      planInfo += `\n{info|${formatter(value)}}`
    }

    const middleTime = getMidpointTimestamp(startTime, endTime)

    planData.push([middleTime, 1, planInfo])

    plan.push({
      xAxis: startTime,
      itemStyle: {
        color: planColor,
      },
    })

    plan.push({
      xAxis: endTime,
    })

    markAreaData.push(plan as MarkAreaData)
  }

  const yAxisIndex = planYAxisIndex - 1

  const plansSeries: SeriesOption = {
    name: 'Plans',
    type: 'scatter',
    symbol: 'roundRect',
    symbolSize: 3,
    yAxisIndex: yAxisIndex,
    xAxisIndex: xAxisIndex ? xAxisIndex : undefined,
    color: Color.Grey,
    data: planData,
    silent: true,
    tooltip: {
      show: false,
    },
    labelLayout: {
      y: yLineLength ? yLineLength : 130,
      moveOverlap: 'shiftX',
      hideOverlap: plans.length > 10,
      draggable: true,
    },
    labelLine: {
      show: true,
      lineStyle: {
        color: '#bbb',
      },
    },
    label: {
      show: true,
      color: '#000',
      opacity: 1,
      fontSize: 9,
      padding: 8,
      borderRadius: 5,
      minMargin: 10,
      align: 'right',
      backgroundColor: backgroundColor ? backgroundColor : '#f0f0f0',
      rich: {
        plan: {
          fontSize: 9,
          fontWeight: 'bold',
          align: 'left',
        },
        info: {
          fontSize: 9,
          align: 'left',
        },
      },
      formatter(params) {
        return (params.data as PlanData)[2]
      },
    },
    markArea: {
      data: markAreaData,
    } as MarkAreaComponentOption,
  }

  return plansSeries
}

interface TitleProps {
  title: string | string[]
  location?: string
  dateRange?: string
  info?: string
  invertColors?: boolean
}

export function createTitle({
  title,
  location,
  dateRange,
  info,
  invertColors,
}: TitleProps): TitleComponentOption[] {
  const titles: TitleComponentOption[] = []

  // Row 1: main title
  titles.push({
    left: 10,
    top: 0,
    text: Array.isArray(title) ? title.join(' • ') : title,
    textStyle: {
      fontSize: 18,
      fontWeight: 600,
      color: '#1f1f1f',
    },
  })

  // Row 2: location
  if (location) {
    titles.push({
      left: 10,
      top: 27,
      text: location,
      textStyle: {
        fontSize: 15,
        fontWeight: 400,
        color: '#2b2b2b',
      },
    })
  }

  // Row 3: date range
  if (dateRange) {
    titles.push({
      left: 10,
      top: 52,
      text: `{dateTime|${dateRange}}`,
      textStyle: {
        rich: {
          dateTime: {
            fontSize: 12,
            fontWeight: 450,
            color: '#6b6b6b',
          },
        },
      },
    })
  }

  // Row 4: grey info strip
  if (info) {
    titles.push({
      left: 10,
      right: 10,
      top: 82,
      text: info,
      backgroundColor: invertColors ? Color.White : '#f2f2f2',
      padding: [8, 12],
      borderRadius: 6,
      textStyle: {
        fontWeight: 400,
        fontSize: 12,
        rich: {
          values: { fontWeight: 600 },
        },
      },
    })
  }

  return titles
}

const planYAxis: YAXisComponentOption = {
  name: 'Plans',
  type: 'value',
  max: 1,
  show: false,
}

export function createYAxis(
  hasPlans: boolean,
  ...configs: YAXisComponentOption[]
): YAXisComponentOption[] {
  const defaultYAxis: YAXisComponentOption = {
    type: 'value',
    nameLocation: 'middle',
    nameGap: 30,
    min: 0,
    alignTicks: true,
    axisLabel: {
      formatter: (val) => {
        return typeof val === 'number' ? Math.round(val).toString() : val
      },
    },
  }

  const yAxisConfigs: YAXisComponentOption[] = []

  for (const config of configs) {
    yAxisConfigs.push({ ...defaultYAxis, ...config })
  }

  if (hasPlans) {
    yAxisConfigs.push(planYAxis)
  }

  return yAxisConfigs
}

export function createXAxis(start?: string, end?: string) {
  const xAxis: XAXisComponentOption = {
    type: 'time',
    min: start,
    max: end,
    name: 'Time',
    nameGap: 30,
    nameLocation: 'middle',
    splitNumber: 10,
    minorTick: {
      show: true,
      splitNumber: 2,
    },
  }

  return xAxis
}

export function createDataZoom(
  overrides?: DataZoomComponentOption[]
): DataZoomComponentOption[] {
  const commonDefaults = {
    filterMode: 'none',
    show: true,
    minSpan: 0.2,
  } as const

  // our two “built-in” defaults
  const base: DataZoomComponentOption[] = [
    { type: 'slider', ...commonDefaults }, // horizontal
    { type: 'inside', ...commonDefaults }, // drag/scroll
  ]

  if (!overrides || overrides.length === 0) {
    return base
  }

  // 1) Merge any overrides for the two built-ins
  const mergedBase = base.map((def) => {
    const match = overrides.find(
      (o) =>
        o.type === def.type &&
        (o.orient ?? 'horizontal') === (def.orient ?? 'horizontal')
    )
    return match ? { ...def, ...match } : def
  })

  // 2) Extras = overrides that didn’t match those two
  //    For a vertical-slider override, fold in the shared defaults only
  const extras = overrides
    .filter(
      (o) =>
        !base.some(
          (def) =>
            o.type === def.type &&
            (o.orient ?? 'horizontal') === (def.orient ?? 'horizontal')
        )
    )
    .map((o) =>
      o.type === 'slider' && (o.orient ?? 'horizontal') === 'vertical'
        ? { orient: 'vertical' as const, ...commonDefaults, ...o }
        : o
    )

  return [...mergedBase, ...extras]
}

export function formatExportFileName(
  title: string,
  startDate: string,
  endDate: string
) {
  // Clean the title
  const cleanedTitle = title
    .normalize('NFKC')
    .replace(/[#&:\-/ ]/g, '_')
    .split('_')
    .filter((s) => s.length > 0)
    .join('_')

  return (
    cleanedTitle +
    '_' +
    format(startDate, 'yyyy-MM-dd_HH-mm') +
    '_to_' +
    format(endDate, 'yyyy-MM-dd_HH-mm')
  )
}

export function createToolbox(
  { title, dateRange }: titleProps,
  locationIdentifier: string,
  type: ChartType | ToolType
) {
  const toolbox: ToolboxComponentOption = {
    feature: {
      saveAsImage: { name: title },
      dataView: {
        readOnly: true,
      },
    },
  }
  return toolbox
}

// function generateDataView(
//   opt: any,
//   { title, dateRange }: titleProps,
//   locationIdentifier: string
// ) {
//   const extractedData: DataViewTable[] = opt.series.reduce(
//     (acc: DataViewTable[], series: any) => {
//       const detectorEvent = 'circle'
//       const seriesData: DataViewTable[] = cleanSeriesData(
//         series,
//         opt,
//         detectorEvent,
//         locationIdentifier
//       )
//       if (seriesData?.length) return [...acc, ...seriesData]

//       return [...acc]
//     },
//     []
//   )
//   extractedData.sort((a, b) => a.timeStamp?.localeCompare(b.timeStamp))
//   const plans = opt.series.find((series: any) => series.name === 'Plans')
//   let plansData: DataViewSeriesData[] = []
//   if (plans !== null && plans !== undefined) {
//     plansData = getPlansData(plans)
//   }
//   const content = DisplayDataViewComponent(
//     title,
//     dateRange,
//     extractedData,
//     plansData
//   )
//   return content
// }

// function getPlansData(plans: any): DataViewSeriesData[] {
//   return plans.data.map((plan: any, index: number) => {
//     const cleanValue = plan[2]?.replace(/\n/g, '').replace(/\|/g, ':')
//     return {
//       start: plans.markArea.data[index][0]?.xAxis,
//       end: plans.markArea.data[index][1]?.xAxis,
//       value: cleanValue,
//     }
//   })
// }

// function DisplayDataViewComponent(
//   title: string,
//   dateRange: string,
//   extractedData: DataViewTable[],
//   plansData: DataViewSeriesData[]
// ) {
//   return `
//   <div style="display:flex; user-select: text;">

//     <div style="margin-right:10px;">
//       <h3 style="margin-bottom: 0">${title}</h3>
//       <h3>${dateRange}</h3>
//     </div>
//   </div>
//   ${
//     plansData.length
//       ? `
//     <div style="margin-bottom: 12px;">
//       <h3 style="margin-bottom: 0">Plans</h3>
//       <table style="border-collapse: collapse; user-select: text; ">
//         <tr>
//         <th style="border: 1px solid #000; padding: 5px;">Start</th>
//         <th style="border: 1px solid #000; padding: 5px;">End</th>
//         <th style="border: 1px solid #000; padding: 5px;">Value</th>
//         </tr>
//         ${plansData
//           .map(
//             (data) =>
//               `
//               <tr>
//                 <td style="border: 1px solid #000; padding: 5px;">${data.start}</td>
//                 <td style="border: 1px solid #000; padding: 5px;">${data.end}</td>
//                 <td style="border: 1px solid #000; padding: 5px;">${data.value}</td>
//               </tr>
//             `
//           )
//           .join('')}
//       </table>
//     </div>
//   `
//       : ''
//   }
//   <div style="display:flex;">
//     <table style="border-collapse: collapse; user-select: text;">
//       <tr>
//       <th style="border: 1px solid #000; padding: 5px;">Location Identifier</th>
//       <th style="border: 1px solid #000; padding: 5px;">Event Code</th>
//       <th style="border: 1px solid #000; padding: 5px;">Time Stamp</th>
//       <th style="border: 1px solid #000; padding: 5px;">Value</th>
//       </tr>
//       ${extractedData
//         .map((data: DataViewTable) =>
//           data.eventCode === 'Plans'
//             ? ``
//             : `
//         <tr>
//         <td style="border: 1px solid #000; padding: 5px;">${data.locationIdentifier}</td>
//         <td style="border: 1px solid #000; padding: 5px;">${data.eventCode}</td>
//         <td style="border: 1px solid #000; padding: 5px;">${data.timeStamp}</td>
//         <td style="border: 1px solid #000; padding: 5px;">${data.value}</td>
//         </tr>
//       `
//         )
//         .join('')}
//     </table>
//   </div>
// `
// }

// function cleanSeriesData(
//   series: any,
//   opt: any,
//   detectorEvent: string,
//   locationIdentifier: string
// ): DataViewTable[] {
//   return series?.data
//     ?.map((point: any) => {
//       if (point !== null) {
//         let isDetectorEvents = false
//         const detectorEvents: string[] = opt.displayProps?.detectorEvents
//         if (detectorEvents && detectorEvents.includes(series.name)) {
//           isDetectorEvents = true
//         }

//         let eventCode: string
//         if (!isDetectorEvents) {
//           eventCode = series.name
//         } else {
//           eventCode = detectorEvent == point?.symbol ? 'On' : 'Off'
//         }

//         let value: string
//         if (series.name === 'Plans') {
//           value = point?.value ? point.value[2] : point[2]
//         } else {
//           value = point?.value ? point.value[1] : point[1]
//         }

//         return {
//           locationIdentifier,
//           timeStamp: point?.value ? point.value[0] : point[0],
//           eventCode,
//           value,
//         }
//       }
//     })
//     .filter((data: any) => data !== undefined)
// }

// {
//   "orient": "vertical",
//   "top": "middle",
//   "right": 8,
//   "backgroundColor": "#f2f2f2",
//       "borderRadius": 6,

//   "padding": [10, 12],
//   "itemGap": 14,
//   "data": [
//     {
//       "name": "Approach Delay\nPer Vehicle\n(per second)",
//       "icon": "path://M180 1000 l0 -20 200 0 200 0 0 20 0 20 -200 0 -200 0 0 -20z"
//     },
//     {
//       "name": "Approach Delay\n(per hour)",
//       "icon": "path://M180 1000 l0 -20 200 0 200 0 0 20 0 20 -200 0 -200 0 0 -20z"
//     }
//   ]
// }

export function createLegend(legendConfig?: Partial<LegendComponentOption>) {
  const defaultLegend: LegendComponentOption = {
    orient: 'vertical' as const,
    top: 'middle',
    right: 0,
    backgroundColor: '#f2f2f2',
    borderRadius: 6,
    padding: [10, 12],
    itemGap: 14,
  }

  return {
    ...defaultLegend,
    ...legendConfig,
  } as LegendComponentOption
}

export function createTooltip(tooltip?: TooltipComponentOption) {
  const defaultTooltip: TooltipComponentOption = {
    trigger: 'axis',
    valueFormatter: (v: unknown) => {
      if (v == null) return ''
      if (typeof v === 'number' && Number.isFinite(v)) return v.toFixed(1)

      const n = typeof v === 'string' && v.trim() !== '' ? Number(v) : NaN
      return Number.isFinite(n) ? n.toFixed(1) : String(v)
    },
  }

  return {
    ...defaultTooltip,
    ...tooltip,
  } as TooltipComponentOption
}

export function createGrid<T extends Partial<GridComponentOption>>(
  grid: T
): GridComponentOption & T {
  const defaultGrid: GridComponentOption = {
    show: true,
    bottom: 95,
  }

  return { ...defaultGrid, ...grid }
}

export function createInfoString(...info: string[][]) {
  let infoString = ''

  for (const line of info) {
    infoString += `${line[0]} {values|${line[1]}}   •   `
  }

  return infoString.slice(0, -7) // remove the last "   •   " from the string
}

interface CreateDisplayProps {
  description?: string
  detectorEvents?: string[]
  numberOfLocations?: number
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  [key: string]: any
}

export function createDisplayProps(props: CreateDisplayProps) {
  const defaultProps = {
    height: 550,
  }
  return { ...defaultProps, ...props }
}

export function formatDataPointForStepView(
  data: DataPoint[],
  endTime: string
): any[] {
  const dataWithAddedPoints = data.reduce(
    (accumulator: DataPoint[], dataPoint: DataPoint, index: number) => {
      accumulator.push(dataPoint)
      if (index !== data.length - 1) {
        accumulator.push({
          value: dataPoint.value,
          timestamp: data[index + 1].timestamp,
        })
      } else {
        if (dataPoint.value !== 0) {
          accumulator.push({
            value: dataPoint.value,
            timestamp: endTime,
          })
        }
      }
      return accumulator
    },
    []
  )

  const pairs: any[] = []

  for (let i = 0; i < dataWithAddedPoints.length - 1; i++) {
    const currentPoint = dataWithAddedPoints[i]
    const nextPoint = dataWithAddedPoints[i + 1]

    if (currentPoint.value === 0 && nextPoint.value === 0) {
      pairs.push(null)
      continue
    }

    pairs.push([
      [currentPoint.timestamp, currentPoint.value],
      [nextPoint.timestamp, nextPoint.value],
    ])
  }

  return pairs
}

export interface ChildObject {
  type: string
  shape: { [key: string]: any } // Define shape properties as needed
  style: { [key: string]: any } // Define style properties as needed
}

export function createPolyLines(
  dataForView: any[],
  api: CustomSeriesRenderItemAPI,
  lineType?: string
) {
  let points: any[] = []
  const polylines: any[] = []
  dataForView.forEach((pair: any[]) => {
    if (pair === null) {
      polylines.push({
        type: 'polyline',
        shape: {
          points,
        },
        style: {
          lineDash: lineType,
          fill: 'none',
          stroke: api.visual('color') as string,
          lineWidth: 2,
        },
      })
      points = []
    } else {
      const pointA = api.coord(pair[0])
      const pointB = api.coord(pair[1])
      points.push(pointA, pointB)
    }
  })

  if (points.length) {
    polylines.push({
      type: 'polyline',
      shape: {
        points,
      },
      style: {
        lineDash: lineType,
        fill: 'none',
        stroke: api.visual('color') as string,
        lineWidth: 2,
      },
    })
    points = []
  }
  return polylines
}
