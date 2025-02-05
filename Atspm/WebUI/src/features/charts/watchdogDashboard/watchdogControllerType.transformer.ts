// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - watchdogControllerType.transformer.ts
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
import { Color, lightenColor } from '@/features/charts/utils'
import { EChartsOption } from 'echarts'

interface RawWatchdogData {
  name: string
  model: {
    name: string
    firmware: {
      name: string
      issueType: {
        name: string
        counts: number
      }[]
    }[]
  }[]
}

interface TransformedWatchdogData {
  name: string
  itemStyle: { color: string }
  children: {
    name: string
    itemStyle: { color: string }
    children: {
      name: string
      itemStyle: { color: string }
      children: {
        name: string
        value: number
        itemStyle: { color: string }
      }[]
    }[]
  }[]
}

interface TransformedData {
  sunburst: EChartsOption
  legendData: { name: string; color: string; selected: boolean }[]
  showUnconfiguredData: boolean
}

const controllerColors = [
  Color.Blue,
  Color.Green,
  Color.Orange,
  Color.Yellow,
  Color.Pink,
  Color.LightBlue,
  Color.Red,
]

export default function transformWatchdogControllerTypeData(
  response: RawWatchdogData[],
  deselectedItems: string[] = [],
  showUnconfiguredData = false
): TransformedData {
  const transformedData = transformData(
    response,
    deselectedItems,
    showUnconfiguredData
  )

  // Create legend data from all items, regardless of selection
  const legendData = response.map((item, index) => ({
    name: item.name,
    color: controllerColors[index % controllerColors.length],
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
        if (depth === 0) layer = 'Total Controller Types Count'
        else if (depth === 1) layer = 'Product'
        else if (depth === 2) layer = 'Model'
        else if (depth === 3) layer = 'Firmware'
        else if (depth === 4) layer = 'Issue Type'

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
          show: true, // Show labels if there's enough space
          rotate: 0,
          overflow: 'truncate',
        },
        emphasis: {
          label: {
            show: true, // Always show label on hover
            fontWeight: 'bold',
          },
        },
        labelLayout: {
          hideOverlap: true, // Hide labels only if they overlap
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
  deselectedItems: string[],
  showUnconfiguredData: boolean
): TransformedWatchdogData[] {
  const filteredData = data.filter(
    (item) => !deselectedItems.includes(item.name)
  )

  const totalIssueCount = filteredData.reduce(
    (sum, item) =>
      sum +
      item.model.reduce(
        (modelSum, model) =>
          modelSum +
          model.firmware.reduce(
            (fwSum, fw) =>
              fwSum +
              fw.issueType.reduce(
                (issueSum, issue) =>
                  showUnconfiguredData ||
                  (issue.name !== 'UnconfiguredDetector' &&
                    issue.name !== 'UnconfiguredApproach')
                    ? issueSum + issue.counts
                    : issueSum,
                0
              ),
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
    const controllerIssueCount = item.model.reduce(
      (sum, model) =>
        sum +
        model.firmware.reduce(
          (fwSum, fw) =>
            fwSum +
            fw.issueType.reduce(
              (issueSum, issue) =>
                showUnconfiguredData ||
                (issue.name !== 'UnconfiguredDetector' &&
                  issue.name !== 'UnconfiguredApproach')
                  ? issueSum + issue.counts
                  : issueSum,
              0
            ),
          0
        ),
      0
    )

    const controllerPercentage = (
      (controllerIssueCount / totalIssueCount) *
      100
    ).toFixed(1)

    return {
      name: `${item.name}\n${controllerPercentage}%`,
      itemStyle: {
        color: controllerColors[originalIndex % controllerColors.length],
      },
      children: item.model.map((model) => ({
        name: model.name,
        itemStyle: {
          color: lightenColor(
            controllerColors[originalIndex % controllerColors.length],
            15
          ),
        },
        children: model.firmware.map((fw) => ({
          name: fw.name,
          itemStyle: {
            color: lightenColor(
              controllerColors[originalIndex % controllerColors.length],
              30
            ),
          },
          children: fw.issueType
            .filter(
              (issue) =>
                showUnconfiguredData ||
                (issue.name !== 'UnconfiguredDetector' &&
                  issue.name !== 'UnconfiguredApproach')
            )
            .map((issue) => {
              const issuePercentage = (
                (issue.counts / controllerIssueCount) *
                100
              ).toFixed(1)
              return {
                name: `${issue.name}\n${issuePercentage}%`,
                value: issue.counts,
                itemStyle: {
                  color: lightenColor(
                    controllerColors[originalIndex % controllerColors.length],
                    45
                  ),
                  borderColor: '#ffffff',
                  borderWidth: 1,
                },
              }
            }),
        })),
      })),
    }
  })
}
