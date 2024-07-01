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

const specialCasesCharts = [
  ChartType.ApproachSpeed,
  ChartType.GreenTimeUtilization,
  ChartType.PreemptionDetails,
]

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
  const midpointTime = new Date((startTime + endTime) / 2).toISOString()

  return midpointTime
}

export function createPlans<T extends BasePlan>(
  plans: T[],
  planYAxisIndex: number,
  options?: PlanOptions<T>,
  yLineLength?: number,
  xAxisIndex?: number
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

      if (value === null || value === undefined) continue

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
      y: yLineLength ? yLineLength : 145,
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
      backgroundColor: '#f0f0f0',
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

type titleProps = {
  title: string
  dateRange: string
  info?: string
}

export function createTitle({ title, dateRange, info }: titleProps) {
  const option: TitleComponentOption = {
    textStyle: {
      rich: {
        title: {
          fontSize: 20,
          fontWeight: 'bold',
          lineHeight: 24,
        },
        dateTime: {
          fontSize: 14,
          fontWeight: 500,
          lineHeight: 30,
        },
      },
    },
    subtextStyle: {
      rich: {
        description: {
          fontStyle: 'italic',
        },
        values: {
          fontStyle: 'italic',
          fontWeight: 'bold',
        },
      },
    },
    text:
      dateRange !== ''
        ? `{title|${title}}\n{dateTime|${dateRange}}`
        : `{title|${title}}`,
    padding: 5,
  }

  if (info) {
    option.subtext = `\n${info}`
  }

  return option
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
    nameGap: 40,
    alignTicks: true,
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

export function createDataZoom() {
  const dataZoom: DataZoomComponentOption[] = [
    {
      type: 'slider',
      filterMode: 'none',
      show: true,
      minSpan: 0.2,
    },
    {
      type: 'inside',
      filterMode: 'none',
      show: true,
      minSpan: 0.2,
    },
  ]

  return dataZoom
}

export function createToolbox(
  { title, dateRange }: titleProps,
  locationIdentifier: string,
  type: ChartType | ToolType
) {
  // const toolbox: ToolboxComponentOption = {
  //   feature: {
  //     saveAsImage: {},
  //     feature: {
  //       dataZoom: {
  //         yAxisIndex: 'none',
  //       },
  //     },
  //     dataView: specialCasesCharts.includes(type)
  //       ? undefined
  //       : {
  //           readOnly: true,
  //           optionToContent: function (opt) {
  //             return generateDataView(
  //               opt,
  //               { title, dateRange },
  //               locationIdentifier
  //             )
  //           },
  //         },
  //   },
  // }

  const toolbox: ToolboxComponentOption = {
    feature: {
      saveAsImage: {},
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

export function createLegend(legendConfig?: Partial<LegendComponentOption>) {
  const defaultLegend: LegendComponentOption = {
    orient: 'vertical' as const,
    top: 'middle',
    right: 0,
  }

  const legend = {
    ...defaultLegend,
    ...legendConfig,
  }

  return legend
}

export function createTooltip(tooltip?: TooltipComponentOption) {
  const defaultTooltip = {
    trigger: 'axis',
  }
  return {
    ...defaultTooltip,
    ...tooltip,
  } as TooltipComponentOption
}

export function createGrid(grid: GridComponentOption) {
  const defaultGrid: GridComponentOption = {
    show: true,
    bottom: 95,
  }

  return { ...defaultGrid, ...grid }
}

export function createInfoString(...info: string[][]) {
  let infoString = ''

  for (const line of info) {
    infoString += `{description|${line[0]}} {values|${line[1]}}  •  `
  }

  return infoString.slice(0, -4) // remove the last "  •  " from the string
}

interface CreateDisplayProps {
  description: string
  detectorEvents?: string[]
  numberOfLocations?: number
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  [key: string]: any
}

export function createDisplayProps(props: CreateDisplayProps) {
  const defaultProps = {
    height: 700,
  }
  const displayProps = { ...defaultProps, ...props }
  return displayProps
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
