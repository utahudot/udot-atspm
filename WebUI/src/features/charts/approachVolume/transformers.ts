import {
  createDataZoom,
  createDisplayProps,
  createGrid,
  createInfoString,
  createLegend,
  createSeries,
  createTitle,
  createToolbox,
  createTooltip,
  createXAxis,
  createYAxis,
  transformSeriesData,
} from '@/features/charts/common/transformers'
import { ChartType } from '@/features/charts/common/types'
import { TransformedApproachVolumeResponse } from '@/features/charts/types'
import {
  Color,
  DashedLineSeriesSymbol,
  SolidLineSeriesSymbol,
  formatChartDateTimeRange,
} from '@/features/charts/utils'
import { EChartsOption } from 'echarts'
import { RawApproachVolumeData, RawApproachVolumeResponse } from './types'

const directionAbbreviations: { [key: string]: string } = {
  Unknown: 'NA',
  Northbound: 'NB',
  Southbound: 'SB',
  Eastbound: 'EB',
  Westbound: 'WB',
  Northeast: 'NE',
  Northwest: 'NW',
  Southeast: 'SE',
  Southwest: 'SW',
}

export default function transformApproachVolumeData(
  response: RawApproachVolumeResponse
): TransformedApproachVolumeResponse {
  const transformedData = response.data.map((data) => {
    const chartOptions = transformData(data)

    return {
      chart: chartOptions,
      table: {
        ...data.summaryData,
        primaryDirectionName: data.primaryDirectionName,
        opposingDirectionName: data.opposingDirectionName,
      },
    }
  })

  return {
    type: ChartType.ApproachVolume,
    data: {
      charts: transformedData,
    },
  }
}

function transformData(data: RawApproachVolumeData) {
  const {
    primaryDirectionVolumes,
    opposingDirectionVolumes,
    combinedDirectionVolumes,
    primaryDFactors,
    opposingDFactors,
  } = data

  const distanceFromStopBarLanguage =
    data.distanceFromStopBar === 0
      ? 'at stopbar'
      : `located ${data.distanceFromStopBar} ft. upstream of stopbar`

  const info = createInfoString([
    `${data.detectorType} ${distanceFromStopBarLanguage}`,
    '',
  ])

  const titleHeader = `Approach Volume\n${data.locationDescription} - ${data.primaryDirectionName}/${data.opposingDirectionName}`
  const dateRange = formatChartDateTimeRange(data.start, data.end)

  const title = createTitle({
    title: titleHeader,
    dateRange,
    info: info,
  })

  const yAxis = createYAxis(
    false,
    { name: 'Volume (Vehicles per Hour)', nameGap: 50 },
    { name: 'Directional Split' }
  )

  const xAxis = createXAxis(data.start, data.end)
  const grid = createGrid({
    top: 150,
    left: 65,
    right: 250,
  })

  const combinedValueText = 'Combined Volume'
  const dFactorText = 'D-Factor'

  const legend = createLegend({
    data: [
      { name: data.primaryDirectionName, icon: SolidLineSeriesSymbol },
      { name: data.opposingDirectionName, icon: SolidLineSeriesSymbol },
      { name: combinedValueText, icon: SolidLineSeriesSymbol },
      {
        name: `${data.primaryDirectionName} ${dFactorText}`,
        icon: DashedLineSeriesSymbol,
      },
      {
        name: `${data.opposingDirectionName} ${dFactorText}`,
        icon: DashedLineSeriesSymbol,
      },
    ],
  })

  const dataZoom = createDataZoom()

  const toolbox = createToolbox(
    { title: titleHeader, dateRange },
    data.locationIdentifier,
    ChartType.ApproachVolume
  )

  const tooltip = createTooltip()

  const series = createSeries(
    {
      name: data.primaryDirectionName,
      data: transformSeriesData(primaryDirectionVolumes),
      type: 'line',
      color: Color.Red,
      tooltip: {
        valueFormatter: (val) =>
          `${Math.round(val as number).toLocaleString()} vph`,
      },
    },
    {
      name: data.opposingDirectionName,
      data: transformSeriesData(opposingDirectionVolumes),
      type: 'line',
      color: Color.Blue,
      tooltip: {
        valueFormatter: (val) =>
          `${Math.round(val as number).toLocaleString()} vph`,
      },
    },
    {
      name: combinedValueText,
      data: transformSeriesData(combinedDirectionVolumes),
      type: 'line',
      color: Color.Green,
      tooltip: {
        valueFormatter: (val) =>
          `${Math.round(val as number).toLocaleString()} vph`,
      },
    },
    {
      name: `${data.primaryDirectionName} ${dFactorText}`,
      data: transformSeriesData(primaryDFactors),
      type: 'line',
      color: Color.Red,
      yAxisIndex: 1,
      lineStyle: {
        type: 'dashed',
      },
    },
    {
      name: `${data.opposingDirectionName} ${dFactorText}`,
      data: transformSeriesData(opposingDFactors),
      type: 'line',
      color: Color.Blue,
      yAxisIndex: 1,
      lineStyle: {
        type: 'dashed',
      },
    }
  )

  const displayProps = createDisplayProps({
    description: `${directionAbbreviations[data.primaryDirectionName]}/${
      directionAbbreviations[data.opposingDirectionName]
    } - ${data.detectorType}`,
  })

  const chartOptions: EChartsOption = {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    dataZoom,
    toolbox,
    tooltip,
    series,
    displayProps,
  }

  return chartOptions
}
