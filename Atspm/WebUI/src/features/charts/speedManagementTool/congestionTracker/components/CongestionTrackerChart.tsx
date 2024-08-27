import { ChartType } from '@/features/charts/common/types'
import { transformCongestionTrackerData } from '@/features/charts/speedManagementTool/congestionTracker/congestionTracker.transformer'
import { ToggleButton, ToggleButtonGroup } from '@mui/material'
import type { ECharts, EChartsOption, SetOptionOpts } from 'echarts'
import { init } from 'echarts'
import type { CSSProperties } from 'react'
import { useEffect, useRef, useState } from 'react'

export interface CongestionChartProps {
  id: string
  option: EChartsOption
  chartType?: ChartType
  style?: CSSProperties
  settings?: SetOptionOpts
  loading?: boolean
  theme?: 'light' | 'dark'
  hideInteractionMessage?: boolean
}

export default function CongestionChart({
  id,
  option,
  chartType,
  style,
  settings,
  loading,
  theme,
}: CongestionChartProps) {
  const chartRef = useRef<HTMLDivElement>(null)
  const chartInstance = useRef<ECharts | null>(null)

  const [currentView, setCurrentView] = useState<'week' | 'month'>('month')

  const initChart = () => {
    if (chartRef.current !== null) {
      chartInstance.current = init(chartRef.current, theme, {
        useDirtyRect: true,
      })

      chartInstance.current.setOption(option, settings)
    }
  }

  useEffect(() => {
    initChart()

    const resizeChart = () => {
      chartInstance.current?.resize()
    }
    window.addEventListener('resize', resizeChart)

    return () => {
      chartInstance.current?.dispose()
      window.removeEventListener('resize', resizeChart)
    }
  }, [theme, chartType])

  useEffect(() => {
    if (chartInstance.current) {
      loading
        ? chartInstance.current.showLoading()
        : chartInstance.current.hideLoading()
    }
  }, [loading])

  const handleViewChange = (
    _: React.MouseEvent<HTMLElement>,
    newView: 'week' | 'month'
  ) => {
    if (newView === null) return

    const updatedOption = transformCongestionTrackerData(
      option.response,
      newView
    )
    chartInstance.current?.setOption(updatedOption, {
      replaceMerge: ['title'],
    })
    setCurrentView(newView)
  }

  return (
    <div
      style={{
        position: 'relative',
        width: '100%',
        height: '100%',
        ...style,
      }}
      role="presentation"
      aria-hidden="true"
    >
      <div
        id={id}
        ref={chartRef}
        style={{
          width: '100%',
          height: '100%',
        }}
      />
      <ToggleButtonGroup
        value={currentView}
        exclusive
        onChange={handleViewChange}
        aria-label="View toggle"
        size="small"
        style={{
          position: 'absolute',
          top: '10px',
          right: '10px',
          zIndex: 10,
        }}
      >
        <ToggleButton size="small" value="month" aria-label="Month view">
          Month
        </ToggleButton>
        <ToggleButton size="small" value="week" aria-label="Week view">
          Week
        </ToggleButton>
      </ToggleButtonGroup>
    </div>
  )
}
