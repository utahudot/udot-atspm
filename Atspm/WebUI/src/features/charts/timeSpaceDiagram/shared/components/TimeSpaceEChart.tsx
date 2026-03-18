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
  const { id, option, style, theme, gpxEntries } = prop

  const chartRef = useRef<HTMLDivElement>(null)
  const chartInstanceRef = useRef<ECharts | null>(null)
  const [chart, setChart] = useState<ECharts | null>(null)

  useTimeSpaceHandler(chart)

  const animator = useGpxAnimationHandler(
    chart,
    gpxEntries as GpxUploadOptions[]
  )

  useEffect(() => {
    if (gpxEntries) animator.play()
  }, [animator, gpxEntries])

  useEffect(() => {
    const dom = chartRef.current
    if (!dom) return

    chartInstanceRef.current?.dispose()

    const c = init(dom, theme, { useDirtyRect: true })
    chartInstanceRef.current = c
    setChart(c)

    // ✅ ResizeObserver: resize CURRENT instance (don’t close over c)
    const ro = new ResizeObserver(() => {
      const inst = chartInstanceRef.current
      const el = chartRef.current
      if (!inst || !el) return
      const { width, height } = el.getBoundingClientRect()
      if (width > 0 && height > 0) inst.resize()
    })
    ro.observe(dom)

    // ✅ Window resize: resize CURRENT instance
    const onWindowResize = () => {
      chartInstanceRef.current?.resize()
    }
    window.addEventListener('resize', onWindowResize)

    return () => {
      window.removeEventListener('resize', onWindowResize)
      ro.disconnect()
      c.dispose()
      chartInstanceRef.current = null
      setChart(null)
    }
  }, [theme])

  // ✅ Apply option updates without re-init
  useEffect(() => {
    const inst = chartInstanceRef.current
    if (!inst || !option) return
    inst.setOption(option, { notMerge: true })
  })

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
