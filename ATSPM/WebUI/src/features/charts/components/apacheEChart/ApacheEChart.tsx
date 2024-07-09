import { ChartType } from '@/features/charts/common/types'
import {
  adjustPlanPositions,
  handleGreenTimeUtilizationDataZoom,
} from '@/features/charts/utils'
import { useSidebarStore } from '@/stores/sidebar'
import type { ECharts, EChartsOption, SetOptionOpts } from 'echarts'
import { connect, init } from 'echarts'
import type { CSSProperties } from 'react'
import { useEffect, useRef, useState } from 'react'

export interface ApacheEChartsProps {
  id: string
  option: EChartsOption
  chartType?: ChartType
  style?: CSSProperties
  settings?: SetOptionOpts
  loading?: boolean
  theme?: 'light' | 'dark'
}

export default function ApacheEChart({
  id,
  option,
  chartType,
  style,
  settings,
  loading,
  theme,
}: ApacheEChartsProps) {
  const chartRef = useRef<HTMLDivElement>(null)
  const { isSidebarOpen } = useSidebarStore()
  const [isActivated, setIsActivated] = useState(false)
  const [isHovered, setIsHovered] = useState(false)
  const [isScrolling, setIsScrolling] = useState(false)
  const chartInstance = useRef<ECharts | null>(null)

  const initChart = () => {
    if (chartRef.current !== null) {
      chartInstance.current = init(chartRef.current, theme, {
        useDirtyRect: true,
      })

      // Set initial options with zooming disabled
      const disabledZoomOption: EChartsOption = {
        ...option,
        dataZoom: (option.dataZoom as any[])?.map((zoom) => ({
          ...zoom,
          disabled: true,
          zoomLock: true,
        })),
        toolbox: {
          feature: {
            dataZoom: { show: false },
            brush: { type: 'rect', show: false },
          },
        },
        series: (option.series as any[])?.map((series) => ({
          ...series,
          silent: true,
        })),
      }

      chartInstance.current.setOption(disabledZoomOption, settings)

      if (chartType === ChartType.GreenTimeUtilization) {
        chartInstance.current.on('datazoom', () =>
          handleGreenTimeUtilizationDataZoom(chartInstance.current!)
        )
      } else if (chartType === ChartType.TimingAndActuation) {
        chartInstance.current.group = 'group1'
        connect('group1')
      } else {
        chartInstance.current.on('datazoom', () =>
          adjustPlanPositions(chartInstance.current!)
        )
      }
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
    setTimeout(() => {
      chartInstance.current?.resize()
    }, 500)
  }, [isSidebarOpen])

  useEffect(() => {
    if (chartInstance.current) {
      const updatedOption: EChartsOption = {
        ...option,
        dataZoom: (option.dataZoom as any[])?.map((zoom) => ({
          ...zoom,
          disabled: !isActivated,
          zoomLock: !isActivated,
        })),
        toolbox: {
          feature: {
            dataZoom: { show: isActivated },
            brush: { type: 'rect', show: isActivated },
          },
        },
        series: (option.series as any[])?.map((series) => ({
          ...series,
          silent: !isActivated,
        })),
      }
      chartInstance.current.setOption(updatedOption, settings)
    }
  }, [option, settings, theme, isActivated])

  useEffect(() => {
    if (chartInstance.current) {
      loading
        ? chartInstance.current.showLoading()
        : chartInstance.current.hideLoading()
    }
  }, [loading])

  useEffect(() => {
    let scrollTimeout: NodeJS.Timeout

    const handleScroll = () => {
      setIsScrolling(true)
      clearTimeout(scrollTimeout)
      scrollTimeout = setTimeout(() => {
        setIsScrolling(false)
      }, 1200)
    }

    window.addEventListener('scroll', handleScroll)

    return () => {
      window.removeEventListener('scroll', handleScroll)
      clearTimeout(scrollTimeout)
    }
  }, [])

  const handleActivate = () => {
    if (!isActivated) {
      setIsActivated(true)
      if (chartInstance.current) {
        chartInstance.current.setOption({
          ...option,
          dataZoom: (option.dataZoom as any[])?.map((zoom) => ({
            ...zoom,
            disabled: false,
            zoomLock: false,
          })),
          toolbox: {
            feature: {
              dataZoom: { show: true },
              brush: { type: 'rect', show: true },
            },
          },
          series: (option.series as any[])?.map((series) => ({
            ...series,
            silent: false,
          })),
        })
      }
    }
  }

  return (
    <div
      style={{
        position: 'relative',
        width: '100%',
        height: '100%',
        ...style,
      }}
      onMouseLeave={() => {
        // setIsActivated(false)
      }}
    >
      <div id={id} ref={chartRef} style={{ width: '100%', height: '100%' }} />
      {!isActivated && isScrolling && (
        <div
          style={{
            position: 'absolute',
            top: option.grid.top,
            left: option.grid.left,
            right: option.grid.right,
            bottom: option.grid.bottom,
          }}
          onMouseEnter={() => setIsHovered(true)}
          onMouseLeave={() => {
            setIsHovered(false)
          }}
          onClick={handleActivate}
        >
          {!isActivated && isHovered && isScrolling && (
            <div
              style={{
                width: '100%',
                height: '100%',
                background: 'rgba(0, 0, 0, 0.2)',
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                color: 'white',
                fontSize: '16px',
                zIndex: 1,
              }}
            >
              Click to enable zoom
            </div>
          )}
        </div>
      )}
    </div>
  )
}
