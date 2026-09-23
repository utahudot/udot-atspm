// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - speedOverTime.transformer.ts
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
  SpeedFromImpactDto,
  SpeedOverTimeDto,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import {
  createDataZoom,
  createGrid,
  createLegend,
  createSeries,
  createTooltip,
  createXAxis,
  createYAxis,
  transformSeriesData,
} from '@/features/charts/common/transformers'
import { ExtendedEChartsOption } from '@/features/charts/types'
import { Color, SolidLineSeriesSymbol } from '@/features/charts/utils'
import { createSpeedManagementTitle } from '@/features/charts/speedManagementTool/createSpeedManagementTitle'
import { SM_ChartType } from '@/features/speedManagementTool/api/getSMCharts'
import { SeriesOption } from 'echarts'

export default function transformSpeedOverTimeData(
  response: SpeedOverTimeDto,
  impactResponse?: SpeedFromImpactDto[]
) {
  const chart = transformData(response, impactResponse)
  return {
    type: SM_ChartType.SPEED_OVER_TIME,
    charts: [chart],
  }
}

export const dateFormat: Intl.DateTimeFormatOptions = {
  weekday: 'short',
  year: 'numeric',
  month: 'long',
  day: '2-digit',
}

function transformData(
  response: SpeedOverTimeDto,
  impactResponse: SpeedFromImpactDto
) {
  const dateRange =
    new Date(response?.startDate).toLocaleDateString('en-US', dateFormat) +
    ' - ' +
    new Date(response?.endDate).toLocaleDateString('en-US', dateFormat)

  const title = createSpeedManagementTitle({
    title: `Speed Over Time - ${response.segmentName}`,
    dateRange,
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

      const impactInfo = `Impacts: {info|${impact.impactTypes
        .map((type) => type.name)
        .join(', ')}}`

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
        y: 85,
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
    top: 80,
    left: 60,
    right: 240,
  })

  const dataZoom = createDataZoom()

  const tooltip = createTooltip()

  const seriesData = transformSeries(response)

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
  const seriesWithData = []
  if (seriesData.average.length > 0) {
    seriesWithData.push({
      name: averageSpeed,
      icon: SolidLineSeriesSymbol,
    })
  }

  if (seriesData.eightyFifth.length > 0) {
    seriesWithData.push({
      name: eightyFifthPercentile,
      icon: SolidLineSeriesSymbol,
    })
  }
  const legend = createLegend({ top: grid.top, data: seriesWithData })

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

const transformSeries = (response) => {
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
