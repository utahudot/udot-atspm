import { EChartsOption, init } from 'echarts'
import { useEffect, useRef } from 'react'

interface props {
  id: string
  existing: number
  positive: number
  negative: number
  remaining: number
}

export const PieChart = ({
  id,
  existing,
  positive,
  negative,
  remaining,
}: props) => {
  const chartRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const chartOptions: EChartsOption = {
      tooltip: {
        trigger: 'item',
      },
      series: [
        {
          type: 'pie',
          radius: '85%',
          labelLine: {
            show: false,
          },
          data: [
            {
              value: existing,
              name: 'Existing',
              itemStyle: { color: 'lightgreen' },
            },
            {
              value: positive,
              name: 'Positive',
              itemStyle: { color: 'green' },
            },
            { value: negative, name: 'Negative', itemStyle: { color: 'red' } },
            {
              value: remaining,
              name: 'Remaining',
              itemStyle: {
                color: 'grey',
                borderColor: '#fff',
              },
            },
          ],
        },
      ],
    }
    if (chartRef.current !== null) {
      const chart = init(chartRef.current, undefined, {
        width: '100%',
        height: '100%',
      })
      chart.setOption(chartOptions)
    }
  }, [existing, negative, positive, remaining])

  return (
    <div style={{ width: '100%', height: '100%' }} id={id} ref={chartRef} />
  )
}
