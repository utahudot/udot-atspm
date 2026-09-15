import { Box } from '@mui/material'
import type { ECharts, EChartsOption } from 'echarts'
import { init } from 'echarts'
import { useEffect, useMemo, useRef } from 'react'
import type { TimeOfDayChartDetailTarget } from '../../transformers'

export type TimeOfDayScheduleView = 'proposed' | 'existing'

interface TimeOfDayEChartProps {
  option: EChartsOption
  selectedSeries: Record<string, boolean>
  showPercentAxis: boolean
  selectedDetail?: TimeOfDayChartDetailTarget
  onSelectDetail: (detailKey: string) => void
  onToggleScheduleView: (view: TimeOfDayScheduleView) => void
}

const chartTitle = 'Corridor Time-of-Day Analysis'
const mutedScheduleColor = '#94A3B8'
const scheduleRailSeriesNames = new Set([
  'Proposed schedule rail',
  'Existing schedule rail',
])
const scheduleViewByRailSeriesName = new Map<string, TimeOfDayScheduleView>([
  ['Proposed schedule rail', 'proposed'],
  ['Existing schedule rail', 'existing'],
])
const scheduleViewByAxisLabel = new Map<string, TimeOfDayScheduleView>([
  ['Proposed', 'proposed'],
  ['Existing', 'existing'],
])
const scheduleRailSeriesNameByView: Record<TimeOfDayScheduleView, string> = {
  proposed: 'Proposed schedule rail',
  existing: 'Existing schedule rail',
}

interface TimeOfDayChartEventParams {
  seriesName?: unknown
  componentType?: unknown
  value?: unknown
  data?: { detailKey?: unknown }
}

const getScheduleViewFromChartEvent = (params: unknown) => {
  const chartParams = params as TimeOfDayChartEventParams | undefined
  if (typeof chartParams?.seriesName === 'string') {
    const scheduleView = scheduleViewByRailSeriesName.get(
      chartParams.seriesName
    )
    if (scheduleView) return scheduleView
  }

  return chartParams?.componentType === 'yAxis' &&
    typeof chartParams.value === 'string'
    ? scheduleViewByAxisLabel.get(chartParams.value)
    : undefined
}

export default function TimeOfDayEChart({
  option,
  selectedSeries,
  showPercentAxis,
  selectedDetail,
  onSelectDetail,
  onToggleScheduleView,
}: TimeOfDayEChartProps) {
  const chartElementRef = useRef<HTMLDivElement>(null)
  const chartRef = useRef<ECharts | null>(null)
  const onSelectDetailRef = useRef(onSelectDetail)
  const onToggleScheduleViewRef = useRef(onToggleScheduleView)

  useEffect(() => {
    onSelectDetailRef.current = onSelectDetail
    onToggleScheduleViewRef.current = onToggleScheduleView
  }, [onSelectDetail, onToggleScheduleView])

  useEffect(() => {
    const element = chartElementRef.current
    if (!element) return

    const chart = init(element, undefined, { useDirtyRect: true })
    chartRef.current = chart
    const handleChartClick = (params: unknown) => {
      const scheduleView = getScheduleViewFromChartEvent(params)
      if (scheduleView) {
        onToggleScheduleViewRef.current(scheduleView)
        return
      }

      const chartParams = params as TimeOfDayChartEventParams | undefined
      const detailKey = chartParams?.data?.detailKey
      if (typeof detailKey === 'string' && detailKey) {
        onSelectDetailRef.current(detailKey)
      }
    }
    const setScheduleRowEmphasis = (
      scheduleView: TimeOfDayScheduleView,
      emphasized: boolean
    ) => {
      chart.dispatchAction({
        type: emphasized ? 'highlight' : 'downplay',
        seriesName: scheduleRailSeriesNameByView[scheduleView],
        dataIndex: 0,
      })
    }
    const handleChartMouseOver = (params: unknown) => {
      const scheduleView = getScheduleViewFromChartEvent(params)
      if (scheduleView) setScheduleRowEmphasis(scheduleView, true)
    }
    const handleChartMouseOut = (params: unknown) => {
      const scheduleView = getScheduleViewFromChartEvent(params)
      if (scheduleView) setScheduleRowEmphasis(scheduleView, false)
    }
    const handleChartGlobalOut = () => {
      setScheduleRowEmphasis('proposed', false)
      setScheduleRowEmphasis('existing', false)
    }
    let lastWidth = element.clientWidth
    let lastHeight = element.clientHeight
    const resizeChart = () => {
      const width = element.clientWidth
      const height = element.clientHeight
      if (!width || !height || (width === lastWidth && height === lastHeight)) {
        return
      }

      lastWidth = width
      lastHeight = height
      chart.resize({
        width,
        height,
        animation: { duration: 0 },
        silent: true,
      })
    }
    const resizeObserver =
      typeof ResizeObserver === 'undefined'
        ? undefined
        : new ResizeObserver(resizeChart)

    chart.on('click', handleChartClick)
    chart.on('mouseover', handleChartMouseOver)
    chart.on('mouseout', handleChartMouseOut)
    chart.on('globalout', handleChartGlobalOut)
    resizeObserver?.observe(element)
    window.addEventListener('resize', resizeChart)

    return () => {
      chart.off('click', handleChartClick)
      chart.off('mouseover', handleChartMouseOver)
      chart.off('mouseout', handleChartMouseOut)
      chart.off('globalout', handleChartGlobalOut)
      resizeObserver?.disconnect()
      window.removeEventListener('resize', resizeChart)
      chart.dispose()
      chartRef.current = null
    }
  }, [])

  useEffect(() => {
    chartRef.current?.setOption(option, {
      notMerge: true,
    })
  }, [option])

  const scheduleWindowSeries = useMemo(() => {
    const seriesOptions = Array.isArray(option.series)
      ? option.series
      : option.series
        ? [option.series]
        : []

    return seriesOptions.flatMap((seriesOption) => {
      const series = seriesOption as {
        id?: string
        name?: string
        markArea?: { data?: unknown[] }
      }
      if (
        series.name !== 'Proposed plan windows' &&
        series.name !== 'Existing plan windows'
      ) {
        return []
      }

      return [
        {
          id: series.id,
          markArea: {
            ...series.markArea,
            data: selectedSeries[series.name]
              ? (series.markArea?.data ?? [])
              : [],
          },
        },
      ]
    })
  }, [option, selectedSeries])

  const scheduleRailSeries = useMemo(() => {
    const seriesOptions = Array.isArray(option.series)
      ? option.series
      : option.series
        ? [option.series]
        : []

    return seriesOptions.flatMap((seriesOption) => {
      const series = seriesOption as {
        id?: string
        name?: string
        data?: unknown[]
      }
      if (!series.name || !scheduleRailSeriesNames.has(series.name)) return []

      const selected = selectedSeries[series.name] ?? true
      const data = (series.data ?? []).map((datum) => {
        if (selected || !Array.isArray(datum)) return datum

        const mutedDatum = [...datum]
        mutedDatum[4] = mutedScheduleColor
        return mutedDatum
      })

      return [
        {
          id: series.id,
          data,
          silent: false,
          cursor: 'pointer',
        },
      ]
    })
  }, [option, selectedSeries])

  useEffect(() => {
    const chart = chartRef.current
    if (!chart) return

    Object.entries(selectedSeries).forEach(([seriesName, visible]) => {
      const keepScheduleRailVisible = scheduleRailSeriesNames.has(seriesName)
      chart.dispatchAction({
        type:
          visible || keepScheduleRailVisible
            ? 'legendSelect'
            : 'legendUnSelect',
        name: seriesName,
      })
    })
    chart.setOption({
      series: [
        ...scheduleWindowSeries,
        ...scheduleRailSeries,
      ] as EChartsOption['series'],
      yAxis: [
        {},
        {
          show: showPercentAxis,
          name: showPercentAxis ? 'Cross Traffic (%)' : '',
        },
      ],
    })
  }, [
    scheduleRailSeries,
    scheduleWindowSeries,
    selectedSeries,
    showPercentAxis,
  ])

  useEffect(() => {
    const chart = chartRef.current
    if (!chart) return

    chart.dispatchAction({ type: 'downplay' })
    if (!selectedDetail) return

    const target = {
      seriesName: selectedDetail.seriesName,
      dataIndex: selectedDetail.dataIndex,
    }
    chart.dispatchAction({ type: 'highlight', ...target })
    chart.dispatchAction({ type: 'showTip', ...target })
  }, [selectedDetail])

  useEffect(() => {
    const handleSaveAsImage = (event: Event) => {
      const requestedTitle = (event as CustomEvent<{ text?: string }>).detail
        ?.text
      const chart = chartRef.current
      if (requestedTitle !== chartTitle || !chart) return

      const imageUrl = chart.getDataURL({
        type: 'png',
        pixelRatio: 2,
        backgroundColor: '#fff',
      })
      const link = document.createElement('a')
      link.href = imageUrl
      link.download = `${chartTitle}.png`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
    }

    window.addEventListener('saveChartImage', handleSaveAsImage)
    return () => window.removeEventListener('saveChartImage', handleSaveAsImage)
  }, [])

  return (
    <Box
      ref={chartElementRef}
      role="img"
      aria-label="Corridor time-of-day analysis chart"
      sx={{ width: '100%', height: '100%' }}
    />
  )
}
