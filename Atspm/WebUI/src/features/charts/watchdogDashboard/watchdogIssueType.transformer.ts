// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - watchdogIssueType.transformer.ts
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
import { EChartsOption } from 'echarts'
import { Color, lightenColor } from '../utils'

interface RawWatchdogData {
  issueType: number
  products: {
    manufacturer: string
    model: string
    firmware: string
    counts: number
  }[]
  name: string
}

interface TransformedWatchdogData {
  name: string
  itemStyle: { color: string }
  children: {
    name: string
    itemStyle: { color: string }
    value: number
  }[]
}

interface TransformedData {
  sunburst: EChartsOption
  bar: EChartsOption
}

//barchart

const issueTypeColors = [
  Color.Blue,
  Color.Green,
  Color.Orange,
  Color.Yellow,
  Color.Pink,
  Color.LightBlue,
  Color.Red,
]

interface TransformedData {
  sunburst: EChartsOption
  legendData: { name: string; color: string; selected: boolean }[]
}

export default function transformWatchdogIssueTypeData(
  response: RawWatchdogData[],
  deselectedItems: string[] = []
): TransformedData {
  const transformedData = transformData(response, deselectedItems)

  // Create legend data from all items, regardless of selection
  const legendData = response.map((item, index) => ({
    name: item.name,
    color: issueTypeColors[index % issueTypeColors.length],
    selected: !deselectedItems.includes(item.name),
  }))

  const chart: EChartsOption = {
    tooltip: {
      trigger: 'item',
      formatter: (params) => {
        const { data, treePathInfo } = params
        const depth = treePathInfo.length - 1

        // Determine the layer type based on the depth in the hierarchy
        let layer = ''
        if (depth === 0) layer = 'Total Issue Types Count'
        else if (depth === 1) layer = 'Issue Type'
        else if (depth === 2) layer = 'Product'
        else if (depth === 3) layer = 'Model'
        else if (depth === 4) layer = 'Firmware'

        // Display layer, name, and value
        return `${layer}: ${data.name}<br/>Count: ${data.value || 'N/A'}`
      },
    },
    series: [
      {
        type: 'sunburst',
        data: transformedData,
        radius: [60, '90%'],
        itemStyle: {
          borderRadius: 5,
          borderWidth: 2,
        },
        label: {
          show: true,
          rotate: 0,
          overflow: 'truncate',
        },
        emphasis: {
          label: {
            show: true,
            fontWeight: 'bold',
          },
        },
        labelLayout: {
          hideOverlap: true,
        },
      },
    ],
  }

  return {
    sunburst: chart,
    legendData: legendData,
  }
}

function transformData(
  data: RawWatchdogData[],
  deselectedItems: string[]
): TransformedWatchdogData[] {
  const filteredData = data.filter(
    (item) => !deselectedItems.includes(item.name)
  )

  const totalIssueCount = filteredData.reduce(
    (sum, item) =>
      sum +
      item.products.reduce(
        (productSum, product) =>
          productSum +
          product.model.reduce(
            (modelSum, model) =>
              modelSum +
              model.firmware.reduce((fwSum, fw) => fwSum + fw.counts, 0),
            0
          ),
        0
      ),
    0
  )

  return filteredData.map((item) => {
    const originalIndex = data.findIndex(
      (originalItem) => originalItem.name === item.name
    )
    const issueTypeCount = item.products.reduce(
      (sum, product) =>
        sum +
        product.model.reduce(
          (modelSum, model) =>
            modelSum +
            model.firmware.reduce((fwSum, fw) => fwSum + fw.counts, 0),
          0
        ),
      0
    )

    const issueTypePercentage = (
      (issueTypeCount / totalIssueCount) *
      100
    ).toFixed(1)

    return {
      name: `${item.name}\n${issueTypePercentage}%`,
      itemStyle: {
        color: issueTypeColors[originalIndex % issueTypeColors.length],
      },
      children: item.products.map((product) => {
        const productCount = product.model.reduce(
          (sum, model) =>
            sum + model.firmware.reduce((fwSum, fw) => fwSum + fw.counts, 0),
          0
        )

        return {
          name: `${product.name}`,
          itemStyle: {
            color: lightenColor(
              issueTypeColors[originalIndex % issueTypeColors.length],
              15
            ),
          },
          children: product.model.map((model) => {
            const modelCount = model.firmware.reduce(
              (sum, fw) => sum + fw.counts,
              0
            )

            return {
              name: `${model.name}`,
              itemStyle: {
                color: lightenColor(
                  issueTypeColors[originalIndex % issueTypeColors.length],
                  30
                ),
              },
              children: model.firmware.map((fw) => {
                const fwPercentage = (
                  (fw.counts / issueTypeCount) *
                  100
                ).toFixed(1)
                return {
                  name: `${fw.name}\n${fwPercentage}%`,
                  value: fw.counts,
                  itemStyle: {
                    color: lightenColor(
                      issueTypeColors[originalIndex % issueTypeColors.length],
                      45
                    ),
                    borderColor: '#ffffff',
                    borderWidth: 1,
                  },
                }
              }),
            }
          }),
        }
      }),
    }
  })
}
