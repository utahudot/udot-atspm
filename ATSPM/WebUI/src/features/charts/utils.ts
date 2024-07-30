import { DataZoomComponentOption, ECharts } from 'echarts'
import { ChartType, MarkAreaData, PlanData } from './common/types'

export enum Color {
  Blue = '#0072B2',
  Green = '#009E73',
  Orange = '#E69F00',
  Yellow = '#F0E442',
  Pink = '#CC79A7',
  LightBlue = '#56B4E9',
  Red = '#D55E00',
  PlanA = 'rgba(0, 0, 0, 0)',
  PlanB = 'rgba(125, 125, 125, 0.1)',
  Grey = '#808080 ',
  Black = '#000000',
  White = '#FFFFFF',
  BrightRed = '#d73131',
}

export const dateFormat: Intl.DateTimeFormatOptions = {
  weekday: 'short',
  year: 'numeric',
  month: 'long',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  second: '2-digit',
  hour12: false,
}

export const xSvgSymbol =
  'path://M640 320L512 192 320 384 128 192 0 320l192 192L0 704l128 128 192-192 192 192 128-128L448 512 640 320z'

export const triangleSvgSymbol =
  'path://M7.93189 1.24806C7.84228 1.09446 7.67783 1 7.5 1C7.32217 1 7.15772 1.09446 7.06811 1.24806L0.0681106 13.2481C-0.0220988 13.4027 -0.0227402 13.5938 0.0664289 13.749C0.155598 13.9043 0.320967 14 0.5 14H14.5C14.679 14 14.8444 13.9043 14.9336 13.749C15.0227 13.5938 15.0221 13.4027 14.9319 13.2481L7.93189 1.24806Z'

export const DashedLineSeriesSymbol =
  'path://M180 1000 l0 -80 200 0 200 0 0 80 0 80 -200 0 -200 0 0 -80z, M810 1000 l0 -80 200 0 200 0 0 80 0 80 -200 0 -200 0 0 -80zm, M1440 1000 l0 -80 200 0 200 0 0 80 0 80 -200 0 -200 0 0 -80z'

export const DottedLineSeriesSymbol =
  'path://M335 1316 c-63 -28 -125 -122 -125 -191 0 -71 62 -164 127 -192 18 -7 58 -13 90 -13 72 0 125 28 168 88 27 39 30 52 30 117 0 65 -3 78 -30 117 -43 61 -95 88 -170 87 -33 0 -73 -6 -90 -13z, M1035 1313 c-76 -40 -115 -103 -115 -188 0 -121 85 -205 205 -205 121 0 205 84 205 205 0 84 -39 148 -112 186 -46 24 -140 25 -183 2z, M1714 1298 c-61 -42 -94 -102 -94 -173 0 -71 33 -131 94 -172 41 -28 57 -33 107 -33 76 0 115 16 161 68 76 84 76 190 0 274 -46 52 -85 68 -161 68 -50 0 -66 -5 -107 -32z'

export const SolidLineSeriesSymbol =
  'path://M180 1000 l0 -20 200 0 200 0 0 20 0 20 -200 0 -200 0 0 -20z'

export function formatChartDateTimeRange(startDate: string, endDate: string) {
  return `${new Date(startDate).toLocaleString(
    'en-US',
    dateFormat
  )} - ${new Date(endDate).toLocaleString('en-US', dateFormat)}`
}

export function adjustPlanPositions(chart: ECharts) {
  const options = chart.getOption()

  if (!Array.isArray(options.dataZoom) || !Array.isArray(options.series)) return

  const viewRange = extractViewRange(options.dataZoom[0])

  const planSeries = options.series[options.series.length - 1]

  if (!planSeries.markArea || !planSeries.markArea.data) return

  const planData: PlanData[] = planSeries.data

  planData.forEach((_, i) => {
    const markData = planSeries.markArea.data[i]
    adjustMarkDataForViewRange(markData, viewRange, planData[i])
  })

  chart.setOption(options)
}

function extractViewRange(dataZoom: DataZoomComponentOption): {
  start: number
  end: number
} {
  return {
    start: new Date(dataZoom.startValue as string).getTime(),
    end: new Date(dataZoom.endValue as string).getTime(),
  }
}

// const determineDecimals = (binWidth: number) => {
//   if (binWidth > 40) return 2
//   if (binWidth > 25) return 1
//   return null
// }

function adjustMarkDataForViewRange(
  markData: MarkAreaData,
  viewRange: { start: number; end: number },
  planData: PlanData
) {
  const planStart = new Date(markData[0].xAxis).getTime()
  const planEnd = new Date(markData[1].xAxis).getTime()

  const adjustedStart = Math.max(planStart, viewRange.start)

  const adjustedEnd = Math.min(planEnd, viewRange.end)

  planData[0] = new Date((adjustedStart + adjustedEnd) / 2).toISOString()
}

export function handleGreenTimeUtilizationDataZoom(chart: ECharts) {
  const options = chart.getOption()
  const startValue = options.dataZoom[0].startValue
  const pixelPositionStart = chart.convertToPixel({ xAxisIndex: 0 }, startValue)
  const pixelPositionNext = chart.convertToPixel(
    { xAxisIndex: 0 },
    startValue + 1
  )
  const binPixelWidth = pixelPositionNext - pixelPositionStart
  const heatmapIndex = (options.series as SeriesOption[]).findIndex(
    (s) => s.type === 'heatmap'
  )
  const newMaxValue = determineYAxisMax(options, heatmapIndex, 3)
  const labelFormatter = computeLabelFormatter(binPixelWidth)
  const seriesUpdateTemplate = (options.series as SeriesOption[]).map(
    () => ({})
  )
  seriesUpdateTemplate[heatmapIndex] = {
    label: {
      formatter: labelFormatter,
    },
  }
  chart.setOption({
    yAxis: [{ max: newMaxValue }],
    series: seriesUpdateTemplate,
  })
}

function determineYAxisMax(options, heatmapIndex, binSize: number) {
  const heatmapData = (options.series[heatmapIndex] as any).data
  const maxHeatmapValue = Math.max(...heatmapData.map((item: any) => item[2]))

  // Get max index from heatmap Y-values (which correspond to categories)
  const maxHeatmapIndex = Math.max(...heatmapData.map((item: any) => item[1]))
  const maxCategoryValue = (maxHeatmapIndex + 1) * binSize

  const currentYAxisMax = options.yAxis[0]?.max ?? 0

  const calculatedMax = Math.max(maxHeatmapValue, maxCategoryValue)

  return calculatedMax > currentYAxisMax ? calculatedMax : currentYAxisMax
}
function computeLabelFormatter(binPixelWidth) {
  const determineDecimals = (binWidth: number) => {
    if (binWidth > 40) return 2
    if (binWidth > 25) return 1
    return null
  }

  const decimals = determineDecimals(binPixelWidth)

  return (params) => {
    const value = params.value[2]
    if (decimals !== null) {
      return parseFloat(value).toFixed(decimals)
    } else {
      return ' '
    }
  }
}

export const getDisplayNameFromChartType = (chartType: ChartType) => {
  // Split the chartType string into words, assuming PascalCase or camelCase formatting
  const words = chartType.replace(/([A-Z])/g, ' $1').trim()

  // Capitalize the first letter of each word and join them with spaces
  const displayName = words
    .split(' ')
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ')

  return displayName
}
