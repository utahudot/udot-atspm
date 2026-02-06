import { ApacheEChartsProps } from '@/features/charts/components/apacheEChart'
import { useTimeSpaceHandler } from '@/features/charts/timeSpaceDiagram/shared/handlers/timeSpace.handler'
import type { ECharts } from 'echarts'
import { init } from 'echarts'
import { useEffect, useRef, useState } from 'react'
import { useGpxAnimationHandler } from '../handlers/gpxAnimation.handler'
import { GpxUploadOptions } from '../types'

export interface TimeSpaceChartProps extends ApacheEChartsProps {
  gpxEntries?: GpxUploadOptions[]
  ignoredLocations?: string[]
}

export default function TimeSpaceEChart(prop: TimeSpaceChartProps) {
  const { id, option, style, theme, gpxEntries, ignoredLocations } = prop

  const chartRef = useRef<HTMLDivElement>(null)
  const chartInstanceRef = useRef<ECharts | null>(null)
  const [chart, setChart] = useState<ECharts | null>(null)

  useTimeSpaceHandler(chart)
  // useTimeSpaceIgnoreLocationHandler(chart, ignoredLocations)
  const animator = useGpxAnimationHandler(
    chart,
    gpxEntries as GpxUploadOptions[]
  )

  useEffect(() => {
    if (gpxEntries) {
      animator.play()
      // animator.stepForward()
    }
  }, [animator, gpxEntries])

  useEffect(() => {
    const dom = chartRef.current
    if (!dom) return

    chartInstanceRef.current?.dispose()

    const c = init(dom, theme, { useDirtyRect: true })
    chartInstanceRef.current = c
    setChart(c)
    if (option)
      c.setOption(option, {
        notMerge: true,
        replaceMerge: ['series', 'xAxis', 'yAxis'],
      })

    const resizeChart = () => c.resize()
    window.addEventListener('resize', resizeChart)

    return () => {
      window.removeEventListener('resize', resizeChart)
      c.dispose()
      chartInstanceRef.current = null
      setChart(null)
    }
  }, [theme, option])

  useEffect(() => {
    if (!chartInstanceRef.current || !option) return
    chartInstanceRef.current.setOption(option, { notMerge: true })
  }, [option])

  return (
    <div
      id={id}
      ref={chartRef}
      style={{
        width: '100%',
        height: '100%',
        ...style,
      }}
    />
  )
}
