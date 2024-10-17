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
  
  function transformData(data: RawWatchdogData[], deselectedItems: string[]): TransformedWatchdogData[] {
    const filteredData = data.filter(item => !deselectedItems.includes(item.name));
  
    const totalIssueCount = filteredData.reduce((sum, item) => 
      sum + item.products.reduce((productSum, product) => 
        productSum + product.model.reduce((modelSum, model) => 
          modelSum + model.firmware.reduce((fwSum, fw) => fwSum + fw.counts, 0), 0), 0), 0);
  
    return filteredData.map((item) => {
      const originalIndex = data.findIndex(originalItem => originalItem.name === item.name);
      const issueTypeCount = item.products.reduce((sum, product) => 
        sum + product.model.reduce((modelSum, model) => 
          modelSum + model.firmware.reduce((fwSum, fw) => fwSum + fw.counts, 0), 0), 0);
  
      const issueTypePercentage = ((issueTypeCount / totalIssueCount) * 100).toFixed(1);
  
      return {
        name: `${item.name}\n${issueTypePercentage}%`,
        itemStyle: { color: issueTypeColors[originalIndex % issueTypeColors.length] },
        children: item.products.map((product) => {
          const productCount = product.model.reduce((sum, model) => 
            sum + model.firmware.reduce((fwSum, fw) => fwSum + fw.counts, 0), 0);
  
          return {
            name: `${product.name}`,
            itemStyle: { 
              color: lightenColor(issueTypeColors[originalIndex % issueTypeColors.length], 15),
            },
            children: product.model.map((model) => {
              const modelCount = model.firmware.reduce((sum, fw) => sum + fw.counts, 0);
  
              return {
                name: `${model.name}`,
                itemStyle: {
                  color: lightenColor(issueTypeColors[originalIndex % issueTypeColors.length], 30),
                },
                children: model.firmware.map((fw) => {
                  const fwPercentage = ((fw.counts / issueTypeCount) * 100).toFixed(1);
                  return {
                    name: `${fw.name}\n${fwPercentage}%`,
                    value: fw.counts,
                    itemStyle: {
                      color: lightenColor(issueTypeColors[originalIndex % issueTypeColors.length], 45),
                      borderColor: '#ffffff',
                      borderWidth: 1
                    },
                  };
                })
              };
            })
          };
        }),
      };
    });
  }