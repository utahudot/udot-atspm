// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - rampMetering.transformer.test.ts
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
import { ChartType } from '@/features/charts/common/types'
import type {
  CustomSeriesRenderItemAPI,
  CustomSeriesRenderItemParams,
  EChartsOption,
} from 'echarts'
import { Color } from '../utils'
import transformRampMeteringData from './rampMetering.transformer'
import type {
  DescriptionWithDataPoints,
  RawRampMeteringResponse,
} from './types'

type RenderedSeries = {
  name: string
  color?: string
  renderItem: (
    params: CustomSeriesRenderItemParams,
    api: CustomSeriesRenderItemAPI
  ) => { children: { style: { lineDash?: string } }[] }
}

const lane = (
  description: string,
  value: number
): DescriptionWithDataPoints => ({
  description,
  value: [
    { timestamp: '2026-10-06T16:00:00', value },
    { timestamp: '2026-10-06T17:00:00', value: 0 },
  ],
})

const transform = (
  lanesActiveRate: DescriptionWithDataPoints[],
  lanesBaseRate: DescriptionWithDataPoints[]
): EChartsOption[] => {
  const response: RawRampMeteringResponse = {
    type: ChartType.RampMetering,
    data: {
      locationIdentifier: '2126',
      locationDescription:
        '#2126 - University Ave (SR-189) SB Meter & I-15 MP 263',
      start: '2026-10-06T00:00:00',
      end: '2026-10-06T23:59:00',
      mainlineAvgFlow: [],
      mainlineAvgOcc: [],
      mainlineAvgSpeed: [],
      lanesActiveRate,
      lanesBaseRate,
      startUpWarning: [],
      shutdownWarning: [],
      lanesQueueOnAndOffEvents: [],
    },
  }
  return transformRampMeteringData(response).data.charts.map(
    ({ chart }) => chart as EChartsOption
  )
}

const findSeries = (chart: EChartsOption, name: string) =>
  (chart.series as RenderedSeries[]).find(
    (series) => series.name === name
  ) as RenderedSeries

const lineDashOf = (series: RenderedSeries) => {
  const api = {
    coord: (value: unknown) => value,
    visual: () => series.color,
  } as unknown as CustomSeriesRenderItemAPI
  const group = series.renderItem(
    { dataIndex: 0 } as CustomSeriesRenderItemParams,
    api
  )
  return group.children[0].style.lineDash
}

describe('transformRampMeteringData lane rates', () => {
  // Queue Override, Volume and Speed draw both active and base rates
  const ratesCharts = (charts: EChartsOption[]) => charts.slice(1)

  it('gives each lane its own color, shared by its active and base rates', () => {
    const lanes = ['1', '2', '3', '4']
    const charts = transform(
      lanes.map((id) => lane(id, 500)),
      lanes.map((id) => lane(id, 600))
    )

    ratesCharts(charts).forEach((chart) => {
      const colors = lanes.map((id) => {
        const active = findSeries(chart, `Active Rate: ${id}`).color
        expect(findSeries(chart, `Base Rate: ${id}`).color).toBe(active)
        return active
      })
      expect(new Set(colors).size).toBe(lanes.length)
      expect(colors).not.toContain(Color.Yellow)
      expect(colors).not.toContain(Color.LightBlue)
    })
  })

  it('matches lanes by number when a lane has only a base rate', () => {
    const charts = transform(
      [lane('1', 500), lane('2', 500)],
      [lane('1', 600), lane('2', 600), lane('3', 600)]
    )

    ratesCharts(charts).forEach((chart) => {
      ;['1', '2'].forEach((id) =>
        expect(findSeries(chart, `Base Rate: ${id}`).color).toBe(
          findSeries(chart, `Active Rate: ${id}`).color
        )
      )
      const baseOnlyColor = findSeries(chart, 'Base Rate: 3').color
      expect(baseOnlyColor).not.toBe(findSeries(chart, 'Active Rate: 1').color)
      expect(baseOnlyColor).not.toBe(findSeries(chart, 'Active Rate: 2').color)
    })
  })

  it('draws base rates dashed and active rates solid', () => {
    const charts = transform(
      [lane('1', 500), lane('2', 500)],
      [lane('1', 600), lane('2', 600)]
    )

    ratesCharts(charts).forEach((chart) => {
      expect(lineDashOf(findSeries(chart, 'Active Rate: 1'))).toBeUndefined()
      expect(lineDashOf(findSeries(chart, 'Base Rate: 1'))).toBe('dashed')
    })
  })
})
