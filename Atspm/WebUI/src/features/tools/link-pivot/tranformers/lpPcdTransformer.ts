// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - lpPcdTransformer.ts
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
  createInfoString,
  createTitle,
} from '@/features/charts/common/transformers'
import { ToolType } from '@/features/charts/common/types'
import { transformPcdData } from '@/features/charts/purdueCoordinationDiagram/transformers'
import { StandardChart, TransformedToolResponse } from '@/features/charts/types'
import { RawLpPcdData } from '../types'

export default function transformlpPcdData(
  response: RawLpPcdData,
  pcdChartType: string
): TransformedToolResponse {
  const charts = transformData(response, pcdChartType)
  return {
    type: ToolType.LpPcd,
    data: {
      charts: charts,
    },
  }
}

function transformData(
  data: RawLpPcdData,
  pcdChartType: string
): StandardChart[] {
  const pcdDatas = data.pcd.map((pcd, i: number) =>
    transformPcdData(pcd, i !== 0 ? 80 : 90)
  )
  // const chartOptions =

  return pcdDatas.map((pcdData, index) => {
    const info = createInfoString([
      `${data.pcd[index].phaseDescription}   `,
      `Arrivals on Green:  ${data.pcd[index].percentArrivalOnGreen}%`,
    ])
    pcdData.dataZoom = [
      {
        type: 'inside',
        filterMode: 'none',
        show: true,
      },
    ]
    pcdData.xAxis = {
      ...pcdData.xAxis,
      minorTick: { show: false },
      axisLabel: { hideOverlap: true, rotate: 25 },
    }

    if (index !== 0) {
      const title = createTitle({
        title: '',
        dateRange: '',
        info,
      })
      pcdData.title = title
      pcdData.legend = { data: [] }
      pcdData.grid = {
        ...pcdData.grid,
        top: 150,
        bottom: 150,
      }
      pcdData.dataZoom.push({
        type: 'slider',
        filterMode: 'none',
        show: true,
        height: 25,
        top: 400,
      })
    } else {
      const title = createTitle({
        title: `${pcdChartType} \nTotal AOG - ${data.totalAog} | ${data.totalPAog}%`,
        dateRange: '',
        info,
      })
      pcdData.title = title
      pcdData.grid = {
        ...pcdData.grid,
        top: 150,
        bottom: 150,
      }
      pcdData.dataZoom.push({
        type: 'slider',
        filterMode: 'none',
        show: true,
        height: 25,
        top: 400,
      })
    }
    return {
      chart: pcdData,
    }
  })
}
