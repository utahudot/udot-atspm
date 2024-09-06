import { EChartsOption } from 'echarts'
import { Color, lightenColor } from '@/features/charts/utils'

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
    showUnconfiguredData: boolean = false
  ): TransformedData {
    const transformedData = transformData(response, deselectedItems, showUnconfiguredData)
    
    // Create legend data from all items, regardless of selection
    const legendData = response.map((item, index) => ({
      name: item.name,
      color: controllerColors[index % controllerColors.length],
      selected: !deselectedItems.includes(item.name)
    }))
  
    const chart: EChartsOption = {
      series: [
        {
          type: 'sunburst',
          data: transformedData,
          radius: [60, '90%'],
          itemStyle: {
            borderRadius: 5,
            borderWidth: 2
          },
          label: {
            rotate: 0
          }
        },
      ],
    }
  
    return {
      sunburst: chart,
      legendData: legendData
    }
  }
  
  function transformData(data: RawWatchdogData[], deselectedItems: string[], showUnconfiguredData: boolean): TransformedWatchdogData[] {
    const filteredData = data.filter(item => !deselectedItems.includes(item.name));
  
    const totalIssueCount = filteredData.reduce((sum, item) => 
      sum + item.model.reduce((modelSum, model) =>
        modelSum + model.firmware.reduce((fwSum, fw) =>
          fwSum + fw.issueType.reduce((issueSum, issue) => 
            showUnconfiguredData || (issue.name !== "UnconfiguredDetector" && issue.name !== "UnconfiguredApproach") ? issueSum + issue.counts : issueSum, 0), 0), 0), 0);
  
    return filteredData.map((item) => {
      const originalIndex = data.findIndex(originalItem => originalItem.name === item.name);
      const controllerIssueCount = item.model.reduce((sum, model) =>
        sum + model.firmware.reduce((fwSum, fw) =>
          fwSum + fw.issueType.reduce((issueSum, issue) => 
            showUnconfiguredData || (issue.name !== "UnconfiguredDetector" && issue.name !== "UnconfiguredApproach") ? issueSum + issue.counts : issueSum, 0), 0), 0);
      
      const controllerPercentage = ((controllerIssueCount / totalIssueCount) * 100).toFixed(1);
  
      return {
        name: `${item.name}\n${controllerPercentage}%`,
        itemStyle: { color: controllerColors[originalIndex % controllerColors.length] },
        children: item.model.map((model) => ({
          name: model.name,
          itemStyle: {
            color: lightenColor(controllerColors[originalIndex % controllerColors.length], 15),
          },
          children: model.firmware.map((fw) => ({
            name: fw.name,
            itemStyle: {
              color: lightenColor(controllerColors[originalIndex % controllerColors.length], 30),
            },
            children: fw.issueType
              .filter(issue => showUnconfiguredData || (issue.name !== "UnconfiguredDetector" && issue.name !== "UnconfiguredApproach"))
              .map((issue) => {
                const issuePercentage = ((issue.counts / controllerIssueCount) * 100).toFixed(1);
                return {
                  name: `${issue.name}\n${issuePercentage}%`,
                  value: issue.counts,
                  itemStyle: {
                    color: lightenColor(controllerColors[originalIndex % controllerColors.length], 45),
                    borderColor: '#ffffff',
                    borderWidth: 1,
                  },
                };
              })
          }))
        })),
      };
    });
  }