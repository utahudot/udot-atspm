import {
  createDataZoom,
  createGrid,
  createLegend,
  createSeries,
  createTitle,
  createTooltip,
  createXAxis,
  createYAxis,
  transformSeriesData,
} from '@/features/charts/common/transformers'
import { ExtendedEChartsOption } from '@/features/charts/types'
import { Color, SolidLineSeriesSymbol } from '@/features/charts/utils'
import { SpeedOverDistanceResponse } from '@/features/speedManagementTool/api/getSpeedOverDistanceChart'
import { SeriesOption } from 'echarts'

export default function transformSpeedOverTimeData(
  response: SpeedOverDistanceResponse,
  impactResponse?: any
) {
  console.log(impactResponse)
  const title = createTitle({
    title: `Speed Over Time\n${
      response.segmentName
    } (between MP ${response.startingMilePoint.toFixed(
      1
    )} and MP ${response.endingMilePoint.toFixed(1)})`,
    dateRange: '',
  })

  // todo add date range
  interface Impact {
    description: string
    start: string
    end: string
    impactTypes: { name: string }[]
  }

  function transformImpactData(impactResponse: {
    impacts: Impact[] | null
  }): SeriesOption | null {
    if (!impactResponse.impacts || impactResponse.impacts.length === 0) {
      return null
    }

    const planData: [string, number, string][] = []

    impactResponse.impacts.forEach((impact) => {
      const startTime = new Date(impact.start).toISOString()
      const endTime = new Date(impact.end).toISOString()
      const middleTime = new Date(
        (new Date(startTime).getTime() + new Date(endTime).getTime()) / 2
      ).toISOString()

      const impactInfo = `{plan|${
        impact.description
      }}\n{info|${impact.impactTypes.map((type) => type.name).join(', ')}}`

      planData.push([middleTime, 1, impactInfo])
    })

    const impactSeries: SeriesOption = {
      name: 'Impacts',
      type: 'scatter',
      symbol: 'roundRect',
      symbolSize: 3,
      yAxisIndex: 1,
      color: Color.Grey,
      data: planData,
      silent: true,
      tooltip: {
        show: false,
      },
      labelLayout: {
        y: 60,
        moveOverlap: 'shiftX',
        hideOverlap: impactResponse.impacts.length > 10,
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
          return (params.data as [string, number, string])[2]
        },
      },
      markArea: {
        data: impactResponse.impacts.map((impact) => [
          { xAxis: impact.start },
          { xAxis: impact.end },
        ]),
        itemStyle: {
          color: Color.Grey,
          opacity: 0.2,
        },
      },
    }

    return impactSeries
  }

  const averageSpeed = 'Average Speed (mph)'
  const eightyFifthPercentile = '85th Percentile Speed (mph)'

  const xAxis = createXAxis()

  const yAxis = createYAxis(true, { name: 'Speed (mph)' })

  const grid = createGrid({
    top: 120,
    left: 60,
    right: 210,
  })

  const legend = createLegend({
    data: [
      { name: averageSpeed, icon: SolidLineSeriesSymbol },
      { name: eightyFifthPercentile, icon: SolidLineSeriesSymbol },
    ],
  })

  const dataZoom = createDataZoom()

  const tooltip = createTooltip()

  const seriesData = transformData(response)

  const series = createSeries(
    {
      name: averageSpeed,
      data: transformSeriesData(seriesData.average),
      type: 'line',
      color: Color.Blue,
    },
    {
      name: eightyFifthPercentile,
      data: transformSeriesData(seriesData.eightyFifth),
      type: 'line',
      color: Color.Red,
    }
  )
  const impactSeries = impactResponse
    ? transformImpactData(impactResponse)
    : null

  const chartOptions: ExtendedEChartsOption = {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    tooltip,
    series: impactSeries ? [...series, impactSeries] : series,
    dataZoom,
  }

  return chartOptions
}

function transformData(response) {
  const result = {
    average: [],
    eightyFifth: [],
  }

  response.data.forEach((entry) => {
    const { series } = entry
    const { average, eightyFifth } = series

    if (average && Array.isArray(average)) {
      average.forEach((item) => {
        result.average.push({ timestamp: item.timestamp, value: item.value })
      })
    }

    if (eightyFifth && Array.isArray(eightyFifth)) {
      eightyFifth.forEach((item) => {
        result.eightyFifth.push({
          timestamp: item.timestamp,
          value: item.value,
        })
      })
    }
  })

  return result
}
