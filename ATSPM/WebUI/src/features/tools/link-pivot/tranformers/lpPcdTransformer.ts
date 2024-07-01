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
