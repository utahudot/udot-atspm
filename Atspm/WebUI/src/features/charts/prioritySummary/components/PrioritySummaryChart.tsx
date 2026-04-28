import { getPriorityDetailsReportData } from '@/api/reports/priority-details/priority-details'
import transformPriorityDetailsData from '@/features/charts/prioritySummary/priorityDetails.transformer'
import { dateToTimestamp } from '@/utils/dateTime'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import { Button } from '@mui/material'
import type { ECharts, EChartsOption, SetOptionOpts } from 'echarts'
import { init } from 'echarts'
import type { CSSProperties } from 'react'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'

export interface PrioritySummaryDetailsChartProps {
  id: string
  option: EChartsOption
  style?: CSSProperties
  settings?: SetOptionOpts
  loading?: boolean
  theme?: 'light' | 'dark'
}

export default function PrioritySummaryDetailsChart({
  id,
  option,
  style,
  settings,
  loading,
  theme,
}: PrioritySummaryDetailsChartProps) {
  const chartRef = useRef<HTMLDivElement>(null)
  const chartInstance = useRef<ECharts | null>(null)

  const [overrideOption, setOverrideOption] = useState<EChartsOption | null>(
    null
  )

  useEffect(() => {
    setOverrideOption(null)
  }, [option])

  const effectiveOption = overrideOption ?? option

  const isDrilled = useMemo(() => overrideOption != null, [overrideOption])

  const handleBack = useCallback((e: React.MouseEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setOverrideOption(null)
  }, [])

  const initChart = useCallback(() => {
    if (!chartRef.current) return

    chartInstance.current?.dispose()
    chartInstance.current = init(chartRef.current, theme, {
      useDirtyRect: true,
    })

    chartInstance.current.setOption(effectiveOption, settings)

    chartInstance.current.off('click')
    chartInstance.current.on('click', async (p) => {
      if (isDrilled) return

      const isPrioritySummaryBar =
        p?.seriesType === 'bar' &&
        typeof p?.seriesName === 'string' &&
        (p.seriesName.includes('TSP Request') ||
          p.seriesName.includes('TSP Service')) &&
        !p.seriesName.startsWith('__') // ignore hidden/extent series

      if (!isPrioritySummaryBar) return

      const d = p?.data
      if (!Array.isArray(d)) return

      const seriesIsRequest = p?.seriesName?.includes('TSP Request')

      const locationIdentifier = seriesIsRequest
        ? (d[4] as string)
        : (d[7] as string)
      const start = seriesIsRequest ? (d[5] as string) : (d[8] as string)
      const end = seriesIsRequest ? (d[6] as string) : (d[9] as string)

      if (!locationIdentifier || !start || !end) return

      try {
        chartInstance.current?.showLoading()

        const detailsResponse = await getPriorityDetailsReportData({
          locationIdentifier,
          start: dateToTimestamp(start),
          end: dateToTimestamp(end),
        })

        const transformed = transformPriorityDetailsData(detailsResponse)
        const nextOption = transformed?.data?.charts?.[0]?.chart

        if (nextOption) {
          setOverrideOption(nextOption)
          chartInstance.current?.setOption(nextOption, { notMerge: true })
        }
      } catch (err) {
        console.error('Priority Details drilldown failed:', err)
      } finally {
        chartInstance.current?.hideLoading()
      }
    })
  }, [effectiveOption, settings, theme, isDrilled])

  useEffect(() => {
    initChart()

    const resizeChart = () => chartInstance.current?.resize()
    window.addEventListener('resize', resizeChart)

    return () => {
      chartInstance.current?.dispose()
      window.removeEventListener('resize', resizeChart)
    }
  }, [initChart])

  useEffect(() => {
    if (!chartInstance.current) return
    chartInstance.current.setOption(effectiveOption, settings)
  }, [effectiveOption, settings])

  useEffect(() => {
    if (!chartInstance.current) return
    loading
      ? chartInstance.current.showLoading()
      : chartInstance.current.hideLoading()
  }, [loading])

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
      <div id={id} ref={chartRef} style={{ width: '100%', height: '100%' }} />

      {isDrilled && (
        <Button
          variant="contained"
          size="small"
          startIcon={<ArrowBackIcon fontSize="small" />}
          onClick={handleBack}
          style={{
            position: 'absolute',
            top: '85px',
            left: '10px',
            zIndex: 10,
          }}
        >
          Back
        </Button>
      )}
    </div>
  )
}
