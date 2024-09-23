import {
  createDataZoom,
  createGrid,
  createLegend,
  createSeries,
  createTitle,
  createToolbox,
  createTooltip,
  createXAxis,
  createYAxis,
  lightenColor,
  transformSeriesData,
} from '@/features/charts/common/transformers'
import { ExtendedEChartsOption } from '@/features/charts/types'
import { Color, formatChartDateTimeRange } from '@/features/charts/utils'
import { SM_ChartType } from '@/features/speedManagementTool/api/getSMCharts'
import { SeriesOption } from 'echarts'

type DataPoint = {
  timestamp: string
  value: number
}

export type RawDataQualitySegment = {
  segmentId: string
  segmentName: string
  startingMilePoint: number
  endingMilePoint: number
  dataPoints: DataPoint[]
}

export type RawDataQualitySource = {
  sourceId: number
  name: string
  segments: RawDataQualitySegment[]
  startDate: string
  endDate: string
}

export type RawDataQualityResponse = RawDataQualitySource[]

export default function transformDataQualityData(
  response: RawDataQualityResponse
) {
  // Check if only one segment exists across all sources
  const allSegments = response.flatMap((source) => source.segments)
  const uniqueSegmentIds = [
    ...new Set(allSegments.map((segment) => segment.segmentId)),
  ]

  if (uniqueSegmentIds.length === 1) {
    // If only one unique segment exists, combine data from all sources into a single chart
    const combinedChart = transformSingleSegmentAcrossSources(
      response,
      uniqueSegmentIds[0]
    )
    return {
      type: SM_ChartType.DATA_QUALITY,
      charts: [combinedChart],
    }
  }

  const charts = response
    .map((source) => {
      const chartOptions = transformData(source)
      return chartOptions ? { chart: chartOptions } : null
    })
    .filter(Boolean)

  return {
    type: SM_ChartType.DATA_QUALITY,
    charts,
  }
}

function transformSingleSegmentAcrossSources(
  response: RawDataQualityResponse,
  segmentId: string
): { chart: ExtendedEChartsOption } | null {
  const segmentsWithData = response
    .map((source) =>
      source.segments.find((segment) => segment.segmentId === segmentId)
    )
    .filter(
      (segment) => segment && segment.dataPoints.length > 0
    ) as RawDataQualitySegment[]

  if (segmentsWithData.length === 0) return null

  const dateRange = formatChartDateTimeRange(
    response[0].startDate,
    response[0].endDate,
    'date'
  )

  const title = createTitle({
    title: `Data Quality - ${segmentsWithData[0].segmentName} (All Sources)`,
    dateRange,
  })

  const xAxis = createXAxis()
  const yAxis = createYAxis(true, {
    name: 'Confidence Percent',
    max: 100,
    axisLabel: { formatter: '{value}%' },
  })

  const grid = createGrid({
    top: 70,
    left: 80,
    right: 150,
  })

  const legend = createLegend({
    data: response.map((source) => source.name),
  })

  const dataZoom = createDataZoom()
  const toolbox = createToolbox()
  const tooltip = createTooltip()

  const getSourceColor = (sourceName: string, index: number) => {
    switch (sourceName) {
      case 'ClearGuide':
        return Color.Green
      case 'ATSPM':
        return Color.Blue
      case 'PeMS':
        return Color.Red
      default:
        const availableColors = [
          Color.Blue,
          Color.Red,
          Color.Yellow,
          Color.Orange,
          Color.Green,
          Color.Pink,
        ]
        const colorIndex = index % availableColors.length
        const baseColor = availableColors[colorIndex]
        const lightenFactor = Math.floor(index / availableColors.length) * 10
        return lightenFactor > 0
          ? lightenColor(baseColor, lightenFactor)
          : baseColor
    }
  }

  const seriesData: SeriesOption[] = response.map((source, index) => {
    const segment = source.segments.find((seg) => seg.segmentId === segmentId)
    return {
      name: source.name,
      data: transformSeriesData(segment!.dataPoints),
      type: 'line',
      color: getSourceColor(source.name, index),
      tooltip: {
        valueFormatter: (value) => Math.round(value as number).toLocaleString(),
      },
    }
  })

  const series = createSeries(...seriesData)

  const chartOptions: ExtendedEChartsOption = {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    dataZoom,
    toolbox,
    tooltip,
    series,
  }

  return { chart: chartOptions }
}

function transformData(
  source: RawDataQualitySource
): ExtendedEChartsOption | null {
  const segmentsWithData = source.segments.filter(
    (segment) => segment.dataPoints.length > 0
  )

  if (segmentsWithData.length === 0) return null

  const dateRange = formatChartDateTimeRange(
    source.startDate,
    source.endDate,
    'date'
  )

  const title = createTitle({
    title: `Data Quality - ${source.name}`,
    dateRange,
  })

  const xAxis = createXAxis()
  const yAxis = createYAxis(true, {
    name: 'Confidence Percent',
    max: 100,
    axisLabel: { formatter: '{value}%' },
  })

  const grid = createGrid({
    top: 70,
    left: 80,
    right: 150,
  })

  const legend = createLegend({
    data: segmentsWithData.map((segment) => segment.segmentName),
  })

  const dataZoom = createDataZoom()
  const toolbox = createToolbox()
  const tooltip = createTooltip()

  const availableColors = [
    Color.Blue,
    Color.Red,
    Color.Yellow,
    Color.Orange,
    Color.Green,
    Color.Pink,
  ]

  const getColor = (index: number) => {
    const colorIndex = index % availableColors.length
    const baseColor = availableColors[colorIndex]

    // If we've cycled through the list, start lightening the color
    const lightenFactor = Math.floor(index / availableColors.length) * 10

    return lightenFactor > 0
      ? lightenColor(baseColor, lightenFactor)
      : baseColor
  }

  const seriesData: SeriesOption[] = segmentsWithData.map((segment, index) => ({
    name: segment.segmentName,
    data: transformSeriesData(segment.dataPoints),
    type: 'line',
    color: getColor(index),
    tooltip: {
      valueFormatter: (value) => Math.round(value as number).toLocaleString(),
    },
  }))

  const series = createSeries(...seriesData)

  const chartOptions: ExtendedEChartsOption = {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    dataZoom,
    toolbox,
    tooltip,
    series,
  }

  return chartOptions
}
