import { supportsBinStepLineToggle } from '@/features/charts/common/chartFeatureFlags'
import { ChartType } from '@/features/charts/common/types'
import {
  adjustPlanPositions,
  handleGreenTimeUtilizationDataZoom,
} from '@/features/charts/utils'
import { useChartsStore } from '@/stores/charts'
import type {
  DataZoomComponentOption,
  ECharts,
  EChartsOption,
  SeriesOption,
  SetOptionOpts,
} from 'echarts'
import { connect, init } from 'echarts'
import type { CSSProperties } from 'react'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'

type LineStep = boolean | 'start' | 'middle' | 'end'
type LineSeriesOption = SeriesOption & {
  data?: unknown
  step?: LineStep
  binStepLineToggle?: boolean
}

export interface ApacheEChartsProps {
  id: string
  option: EChartsOption
  chartType?: ChartType
  style?: CSSProperties
  settings?: SetOptionOpts
  loading?: boolean
  theme?: 'light' | 'dark'
  hideInteractionMessage?: boolean
  resetKey?: boolean
}

function asArray<T>(value: T | T[] | undefined): T[] | undefined {
  if (value == null) return undefined
  return Array.isArray(value) ? value : [value]
}

function getPrimaryTitleText(option: EChartsOption) {
  const title = Array.isArray(option.title) ? option.title[0] : option.title
  return (title as { text?: string } | undefined)?.text
}

function getPrimaryGrid(option: EChartsOption) {
  const grid = Array.isArray(option.grid) ? option.grid[0] : option.grid
  return (
    (grid as
      | {
          top?: string | number
          left?: string | number
          right?: string | number
          bottom?: string | number
        }
      | undefined) ?? {}
  )
}

function getPrimaryXAxisMax(option: EChartsOption) {
  const xAxis = Array.isArray(option.xAxis) ? option.xAxis[0] : option.xAxis
  return (xAxis as { max?: unknown } | undefined)?.max
}

function getPrimaryHorizontalDataZoomEndValue(option: EChartsOption) {
  const dataZoom = asArray(
    option.dataZoom as
      | DataZoomComponentOption
      | DataZoomComponentOption[]
      | undefined
  )
  const horizontalZoom = dataZoom?.find(
    (zoom) => (zoom.orient ?? 'horizontal') === 'horizontal'
  )

  return horizontalZoom?.endValue
}

function getStepExtensionTarget(option: EChartsOption) {
  return getPrimaryHorizontalDataZoomEndValue(option) ?? getPrimaryXAxisMax(option)
}

function getDataPointX(dataPoint: unknown) {
  if (Array.isArray(dataPoint)) return dataPoint[0]

  if (dataPoint && typeof dataPoint === 'object' && 'value' in dataPoint) {
    const value = (dataPoint as { value?: unknown }).value
    if (Array.isArray(value)) return value[0]
  }

  return undefined
}

function cloneDataPointAtX(dataPoint: unknown, xAxisMax: unknown) {
  if (Array.isArray(dataPoint)) {
    const nextPoint = [...dataPoint]
    nextPoint[0] = xAxisMax
    return nextPoint
  }

  if (dataPoint && typeof dataPoint === 'object' && 'value' in dataPoint) {
    const value = (dataPoint as { value?: unknown }).value
    if (Array.isArray(value)) {
      return {
        ...dataPoint,
        value: [xAxisMax, ...value.slice(1)],
      }
    }
  }

  return undefined
}

function getComparableAxisValue(value: unknown) {
  if (typeof value === 'number' && Number.isFinite(value)) return value
  if (value instanceof Date) return value.getTime()
  if (typeof value !== 'string') return undefined

  const parsedDate = Date.parse(value)
  if (Number.isFinite(parsedDate)) return parsedDate

  const parsedNumber = Number(value)
  return Number.isFinite(parsedNumber) ? parsedNumber : undefined
}

function isAtOrBeyondAxisMax(dataPointX: unknown, xAxisMax: unknown) {
  const dataPointValue = getComparableAxisValue(dataPointX)
  const maxValue = getComparableAxisValue(xAxisMax)

  if (dataPointValue != null && maxValue != null) {
    return dataPointValue >= maxValue
  }

  return dataPointX === xAxisMax
}

function extendStepDataToXAxisMax(data: unknown, xAxisMax: unknown) {
  if (!Array.isArray(data) || data.length === 0 || xAxisMax == null) {
    return data
  }

  const lastPoint = data[data.length - 1]
  const lastPointX = getDataPointX(lastPoint)

  if (lastPointX == null || isAtOrBeyondAxisMax(lastPointX, xAxisMax)) {
    return data
  }

  const finalPoint = cloneDataPointAtX(lastPoint, xAxisMax)
  return finalPoint == null ? data : [...data, finalPoint]
}

function getSetOptionSettings(
  settings: SetOptionOpts | undefined,
  replaceSeries: boolean
) {
  if (!replaceSeries) return settings

  const replaceMerge = asArray(settings?.replaceMerge)
  return {
    ...settings,
    replaceMerge: replaceMerge?.includes('series')
      ? replaceMerge
      : [...(replaceMerge ?? []), 'series'],
  } satisfies SetOptionOpts
}

function applyBinStepLinePreference(
  option: EChartsOption,
  chartType: ChartType | undefined,
  showBinStepLines: boolean
) {
  if (!supportsBinStepLineToggle(chartType)) return option

  const series = option.series
  const seriesList = asArray(series)
  if (!seriesList) return option
  const xAxisMax = getStepExtensionTarget(option)

  return {
    ...option,
    series: seriesList.map((seriesOption) => {
      if (seriesOption.type !== 'line') return seriesOption
      const lineSeriesOption = seriesOption as LineSeriesOption
      const { binStepLineToggle, ...echartsLineSeriesOption } = lineSeriesOption
      if (binStepLineToggle !== true) return echartsLineSeriesOption

      const seriesStep = lineSeriesOption.step ?? 'end'

      return {
        ...echartsLineSeriesOption,
        data: showBinStepLines
          ? extendStepDataToXAxisMax(lineSeriesOption.data, xAxisMax)
          : lineSeriesOption.data,
        step: showBinStepLines ? seriesStep : false,
      } as SeriesOption
    }),
  }
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
  const {
    activeChart,
    setActiveChart,
    syncZoom,
    showBinStepLines,
    yAxisMaxStore,
  } = useChartsStore()
  const [isHovered, setIsHovered] = useState(false)
  const [isScrolling, setIsScrolling] = useState(false)
  const chartInstance = useRef<ECharts | null>(null)

  const isActive = activeChart === id || hideInteractionMessage
  const shouldReplaceSeries = supportsBinStepLineToggle(chartType)
  const effectiveOption = useMemo(
    () => applyBinStepLinePreference(option, chartType, showBinStepLines),
    [option, chartType, showBinStepLines]
  )
  const setOptionSettings = useMemo(
    () => getSetOptionSettings(settings, shouldReplaceSeries),
    [settings, shouldReplaceSeries]
  )

  const initChart = useCallback(() => {
    if (chartRef.current !== null) {
      chartInstance.current = init(chartRef.current, theme, {
        useDirtyRect: true,
      })

      if (syncZoom || chartType === ChartType.TimingAndActuation) {
        chartInstance.current.group = 'group1'
        connect('group1')
      }

      if (chartType === ChartType.GreenTimeUtilization) {
        chartInstance.current.on('datazoom', () =>
          handleGreenTimeUtilizationDataZoom(chartInstance.current!)
        )
      } else {
        chartInstance.current.on('datazoom', () =>
          adjustPlanPositions(chartInstance.current!)
        )
      }
    }
  }, [theme, chartType, syncZoom])

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
  }, [initChart])

  useEffect(() => {
    if (chartInstance.current) {
      const adjustedDataZoom = asArray(
        effectiveOption.dataZoom as
          | DataZoomComponentOption
          | DataZoomComponentOption[]
          | undefined
      )?.map((zoom) => ({
        ...zoom,
        endValue: yAxisMaxStore != null ? yAxisMaxStore : zoom.endValue,
        disabled: !isActive,
        zoomLock: !isActive,
      }))

      // Use adjusted dataZoom in the chart options
      const updatedOption: EChartsOption = {
        ...effectiveOption,
        dataZoom: adjustedDataZoom,
        series: asArray(
          effectiveOption.series as SeriesOption | SeriesOption[] | undefined
        )?.map((series) => ({
          ...series,
          silent: !isActive,
        })),
      }

      // Apply the updated option to the chart
      chartInstance.current.setOption(updatedOption, setOptionSettings)
    }
  }, [
    effectiveOption,
    setOptionSettings,
    theme,
    chartType,
    syncZoom,
    isActive,
    yAxisMaxStore,
  ])

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

  useEffect(() => {
    const handleSaveAsImage = (event: Event) => {
      const customEvent = event as CustomEvent<{
        text: string
      }>
      const clickedChart = customEvent.detail.text
      const currentChart = chartInstance.current
      if (!clickedChart || !currentChart) return

      const chartOptions = currentChart.getOption() as EChartsOption
      if (getPrimaryTitleText(chartOptions) !== clickedChart) return

      // Temporarily remove grouping to prevent all charts from saving
      const originalGroup = currentChart.group
      currentChart.group = ''

      const imageURL = currentChart.getDataURL({
        type: 'png',
        pixelRatio: 2,
        backgroundColor: '#fff',
      })
      const link = document.createElement('a')
      link.href = imageURL
      link.download = `${clickedChart}.png`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link) // Clean up

      // Restore the group after saving
      setTimeout(() => {
        if (clickedChart) {
          currentChart.group = originalGroup
        }
      }, 100)
    }

    window.addEventListener('saveChartImage', handleSaveAsImage)

    return () => {
      window.removeEventListener('saveChartImage', handleSaveAsImage)
    }
  }, [])

  const handleActivate = () => {
    if (!isActive) {
      setActiveChart(id)
      if (chartInstance.current) {
        chartInstance.current.setOption(
          {
            ...effectiveOption,
            dataZoom: asArray(
              effectiveOption.dataZoom as
                | DataZoomComponentOption
                | DataZoomComponentOption[]
                | undefined
            )?.map((zoom) => ({
              ...zoom,
              disabled: false,
              zoomLock: false,
            })),
            series: asArray(
              effectiveOption.series as SeriesOption | SeriesOption[] | undefined
            )?.map((series) => ({
              ...series,
              silent: false,
            })),
          },
          setOptionSettings
        )
      }
    }
  }

  const grid = getPrimaryGrid(effectiveOption)

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
              top: grid.top || 0,
              left: grid.left || 0,
              right: grid.right || 0,
              bottom: grid.bottom || 0,
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
                top: grid.top || 0,
                left: grid.left || 0,
                right: grid.right || 0,
                bottom: grid.bottom || 0,
                // outline: '2px solid #0060df80',
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
