import { ChartType } from '@/features/charts/common/types'
import {
  adjustPlanPositions,
  handleGreenTimeUtilizationDataZoom,
} from '@/features/charts/utils'
import { useChartsStore } from '@/stores/charts'
import type {
  DataZoomComponentOption,
  DatasetComponentOption,
  ECharts,
  EChartsOption,
  SeriesOption,
  SetOptionOpts,
} from 'echarts'
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
  hideInteractionMessage?: boolean
}

export default function ApacheEChart({
  id,
  option,
  chartType,
  style,
  settings,
  loading,
  theme,
  hideInteractionMessage = false,
}: ApacheEChartsProps) {
  const chartRef = useRef<HTMLDivElement>(null)
  const { activeChart, setActiveChart } = useChartsStore()
  const [isHovered, setIsHovered] = useState(false)
  const [isScrolling, setIsScrolling] = useState(false)
  const chartInstance = useRef<ECharts | null>(null)

  const isActive = activeChart === id || hideInteractionMessage

  const initChart = () => {
    if (chartRef.current !== null) {
      chartInstance.current = init(chartRef.current, theme, {
        useDirtyRect: true,
      })

      // Set initial options with zooming disabled
      const disabledZoomOption: EChartsOption = {
        ...option,
        dataZoom: (option.dataZoom as DataZoomComponentOption[])?.map(
          (zoom) => ({
            ...zoom,
            disabled: true,
            zoomLock: true,
          })
        ),
        series: (option.series as SeriesOption[])?.map((series) => ({
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
    if (chartInstance.current) {
      const updatedOption: EChartsOption = {
        ...option,
        dataZoom: (option.dataZoom as DataZoomComponentOption[])?.map(
          (zoom) => ({
            ...zoom,
            disabled: !isActive,
            zoomLock: !isActive,
          })
        ),
        series: (option.series as SeriesOption[])?.map((series) => ({
          ...series,
          silent: !isActive,
        })),
      }
      chartInstance.current.setOption(updatedOption, settings)
    }
  }, [option, settings, theme, isActive])

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
      }, 700)
    }

    window.addEventListener('scroll', handleScroll)

    return () => {
      window.removeEventListener('scroll', handleScroll)
      clearTimeout(scrollTimeout)
    }
  }, [])

  const handleActivate = () => {
    if (!isActive) {
      setActiveChart(id)
      if (chartInstance.current) {
        chartInstance.current.setOption({
          ...option,
          dataZoom: (option.dataZoom as DatasetComponentOption[])?.map(
            (zoom) => ({
              ...zoom,
              disabled: false,
              zoomLock: false,
            })
          ),
          series: (option.series as SeriesOption[])?.map((series) => ({
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
      role="presentation"
      aria-hidden="true"
      onClick={handleActivate}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      <div
        id={id}
        ref={chartRef}
        style={{
          width: '100%',
          height: '100%',
        }}
      />
      {!hideInteractionMessage && (
        <>
          <div
            style={{
              position: 'absolute',
              top: option?.grid?.top || 0,
              left: option?.grid?.left || 0,
              right: option?.grid?.right || 0,
              bottom: option?.grid?.bottom || 0,
              background: 'rgba(0, 0, 0, 0.3)',
              display: 'flex',
              visibility:
                !isActive && isHovered && isScrolling ? 'visible' : 'hidden',
              justifyContent: 'center',
              alignItems: 'center',
              color: 'white',
              fontSize: '24px',
              zIndex: 1,
              textShadow: '0 0 2px black',
            }}
          >
            Click to enable zoom
          </div>
          {isActive && (
            <div
              style={{
                display: isActive ? 'block' : 'none',
                position: 'absolute',
                top: option?.grid?.top || 0,
                left: option?.grid?.left || 0,
                right: option?.grid?.right || 0,
                bottom: option?.grid?.bottom || 0,
                outline: '2px solid #0060df80',
                zIndex: 1,
                pointerEvents: 'none',
              }}
            />
          )}
        </>
      )}
    </div>
  )
}
