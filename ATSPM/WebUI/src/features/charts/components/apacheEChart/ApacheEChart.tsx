import { ChartType } from '@/features/charts/common/types'
import {
  adjustPlanPositions,
  handleGreenTimeUtilizationDataZoom,
} from '@/features/charts/utils'
import { useSidebarStore } from '@/stores/sidebar'
import type { ECharts, EChartsOption, SetOptionOpts } from 'echarts'
import { connect, getInstanceByDom, init } from 'echarts'
import type { CSSProperties } from 'react'
import { useEffect, useRef } from 'react'

export interface ApacheEChartsProps {
  id: string
  option: EChartsOption
  chartType?: ChartType
  style?: CSSProperties
  settings?: SetOptionOpts
  loading?: boolean
  theme?: 'light' | 'dark'
  // eventHandlers?: { [K in keyof ElementEvent]?: ElementEvent[K] }
}

export default function ApacheEChart({
  id,
  option,
  chartType,
  style,
  settings,
  loading,
  theme,
}: // eventHandlers,
ApacheEChartsProps) {
  const chartRef = useRef<HTMLDivElement>(null)
  const { isSidebarOpen } = useSidebarStore()

  useEffect(() => {
    let chart: ECharts
    if (chartRef.current !== null) {
      chart = init(chartRef.current, theme, { useDirtyRect: true })
      if (chartType === ChartType.GreenTimeUtilization) {
        chart.on('datazoom', () => handleGreenTimeUtilizationDataZoom(chart)) // TODO: fix this
      } else if (chartType === ChartType.TimingAndActuation) {
        chart.group = 'group1'
        connect('group1')
      } else {
        chart.on('datazoom', () => adjustPlanPositions(chart))
      }
    }

    const resizeChart = () => {
      chart?.resize()
    }
    window.addEventListener('resize', resizeChart)

    return () => {
      chart?.dispose()
      window.removeEventListener('resize', resizeChart)
    }
  }, [theme, chartType])

  useEffect(() => {
    if (chartRef.current !== null) {
      const chart = getInstanceByDom(chartRef.current)
      setTimeout(() => {
        chart?.resize()
      }, 500)
    }
  }, [isSidebarOpen])

  useEffect(() => {
    if (chartRef.current !== null) {
      const chart = getInstanceByDom(chartRef.current)
      if (!chart) {
        throw new Error('Internal error: chart instance not found')
      }
      chart.setOption(option, settings)
    }
  }, [option, settings, theme])

  useEffect(() => {
    if (chartRef.current !== null) {
      const chart = getInstanceByDom(chartRef.current)

      if (!chart) {
        throw new Error('Internal error: chart instance not found')
      }

      loading ? chart.showLoading() : chart.hideLoading()
    }
  }, [loading, theme])

  useEffect(() => {
    const chart = chartRef.current ? getInstanceByDom(chartRef.current) : null
    if (chart) {
      const resizeTimer = setTimeout(() => {
        chart.resize()
      }, 500) // 0.5 seconds delay

      return () => clearTimeout(resizeTimer)
    }
  }, [isSidebarOpen])

  return <div id={id} ref={chartRef} style={{ ...style }} />
}
