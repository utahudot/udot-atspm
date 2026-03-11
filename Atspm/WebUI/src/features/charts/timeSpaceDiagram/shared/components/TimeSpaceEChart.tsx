import { ApacheEChartsProps } from '@/features/charts/components/apacheEChart'
import { useTimeSpaceHandler } from '@/features/charts/timeSpaceDiagram/shared/handlers/timeSpace.handler'
import { TIME_SPACE_LOCATION_CARD_LAYOUT } from '@/features/charts/timeSpaceDiagram/shared/transformers/timeSpaceTransformerBase'
import VisibilityIcon from '@mui/icons-material/Visibility'
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff'
import { IconButton, Tooltip } from '@mui/material'
import type { ECharts, EChartsOption, SeriesOption } from 'echarts'
import { init } from 'echarts'
import { useEffect, useRef, useState } from 'react'
import { useGpxAnimationHandler } from '../handlers/gpxAnimation.handler'
import { GpxUploadOptions } from '../types'

export interface TimeSpaceChartProps extends ApacheEChartsProps {
  gpxEntries?: GpxUploadOptions[]
  ignoredLocations?: string[]
  onToggleIgnoredLocation?: (location: string) => void
}

type LocationToggleButton = {
  left: number
  top: number
  location: string
}

type LocationAxisDatum = {
  distance: number
  location: string
  time: string | number
}

type GridRectProvider = ECharts & {
  getModel?: () => {
    getComponent?: (
      mainType: string,
      index: number
    ) => {
      coordinateSystem?: {
        getRect?: () => {
          x?: number
        }
      }
    }
  }
}

function getGridLeft(chart: ECharts, option?: EChartsOption) {
  const gridRect = (chart as GridRectProvider)
    ?.getModel?.()
    ?.getComponent?.('grid', 0)
    ?.coordinateSystem?.getRect?.()

  if (typeof gridRect?.x === 'number') {
    return gridRect.x
  }

  const grid = Array.isArray(option?.grid) ? option.grid[0] : option?.grid
  return typeof grid?.left === 'number' ? grid.left : 0
}

function getLocationAxisData(option?: EChartsOption): LocationAxisDatum[] {
  const series = Array.isArray(option?.series)
    ? (option.series as SeriesOption[])
    : []

  const locationSeries = series.find((entry) => entry.name === 'Location axis')
  if (!locationSeries || !Array.isArray(locationSeries.data)) {
    return []
  }

  return locationSeries.data
    .map((item) => {
      const value = Array.isArray(item)
        ? item
        : Array.isArray((item as { value?: unknown[] })?.value)
          ? ((item as { value?: unknown[] }).value as unknown[])
          : null

      if (!value) return null

      const time = value[0]
      const distance = Number(value[1])
      const location = String(value[2] ?? '')

      if (!location || !Number.isFinite(distance)) return null

      return {
        time: typeof time === 'string' || typeof time === 'number' ? time : '',
        distance,
        location,
      }
    })
    .filter((item): item is LocationAxisDatum => item !== null)
}

function buildLocationToggleButtons(
  chart: ECharts,
  option?: EChartsOption
): LocationToggleButton[] {
  const gridLeft = getGridLeft(chart, option)
  const locationAxisData = getLocationAxisData(option)

  if (!locationAxisData.length) {
    return []
  }

  const {
    gridGap,
    dotOffset,
    cardGapToDot,
    headerHeight,
    bodyHeight,
    headerActionSize,
    headerActionRight,
  } = TIME_SPACE_LOCATION_CARD_LAYOUT

  const cardHeight = headerHeight + bodyHeight
  const xTextRight = gridLeft - gridGap
  const xDot = xTextRight + dotOffset
  const cardRight = xDot - cardGapToDot
  const iconLeft = cardRight - headerActionRight - headerActionSize

  return locationAxisData
    .map(({ distance, location, time }) => {
      const [, y] = chart.convertToPixel({ xAxisIndex: 0, yAxisIndex: 0 }, [
        time,
        distance,
      ]) as [number, number]

      if (!Number.isFinite(y)) {
        return null
      }

      const cardTop = y - cardHeight / 2

      return {
        location,
        left: iconLeft,
        top: cardTop + (headerHeight - headerActionSize) / 2,
      }
    })
    .filter((item): item is LocationToggleButton => item !== null)
}

export default function TimeSpaceEChart(prop: TimeSpaceChartProps) {
  const {
    id,
    option,
    style,
    theme,
    gpxEntries,
    ignoredLocations = [],
    onToggleIgnoredLocation,
  } = prop

  const chartRef = useRef<HTMLDivElement>(null)
  const chartInstanceRef = useRef<ECharts | null>(null)
  const [chart, setChart] = useState<ECharts | null>(null)
  const [locationToggleButtons, setLocationToggleButtons] = useState<
    LocationToggleButton[]
  >([])

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

    const ro = new ResizeObserver(() => {
      const inst = chartInstanceRef.current
      const el = chartRef.current
      if (!inst || !el) return
      const { width, height } = el.getBoundingClientRect()
      if (width > 0 && height > 0) inst.resize()
    })
    ro.observe(dom)

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
      setLocationToggleButtons([])
    }
  }, [theme])

  useEffect(() => {
    const inst = chartInstanceRef.current
    if (!inst || !option) return
    inst.setOption(option)
  }, [chart, option])

  useEffect(() => {
    if (!chart || !option || !onToggleIgnoredLocation) {
      setLocationToggleButtons([])
      return
    }

    let rafId = 0

    const syncButtons = () => {
      rafId = window.requestAnimationFrame(() => {
        setLocationToggleButtons(buildLocationToggleButtons(chart, option))
      })
    }

    syncButtons()

    chart.on('finished', syncButtons)
    chart.on('restore', syncButtons)
    window.addEventListener('resize', syncButtons)

    return () => {
      window.cancelAnimationFrame(rafId)
      chart.off('finished', syncButtons)
      chart.off('restore', syncButtons)
      window.removeEventListener('resize', syncButtons)
    }
  }, [chart, option, onToggleIgnoredLocation])

  return (
    <div
      style={{
        width: '100%',
        height: '100%',
        position: 'relative',
        ...style,
      }}
    >
      <div
        id={id}
        ref={chartRef}
        style={{
          width: '100%',
          height: '100%',
        }}
      />

      {locationToggleButtons.length > 0 && onToggleIgnoredLocation && (
        <div
          style={{
            position: 'absolute',
            inset: 0,
            pointerEvents: 'none',
          }}
        >
          {locationToggleButtons.map((button) => {
            const isIgnored = ignoredLocations.includes(button.location)
            const title = isIgnored
              ? `Show location ${button.location}`
              : `Ignore location ${button.location}`

            return (
              <Tooltip key={button.location} title={title} placement="top">
                <IconButton
                  size="small"
                  onClick={() => onToggleIgnoredLocation(button.location)}
                  sx={{
                    pointerEvents: 'auto',
                    position: 'absolute',
                    left: `${button.left}px`,
                    top: `${button.top}px`,
                    p: 0,
                    height: '10px',
                    color: isIgnored ? '#6B7280' : '#1F2937',
                    '&:hover': {
                      backgroundColor: 'rgba(15, 23, 42, 0.08)',
                    },
                  }}
                >
                  {isIgnored ? (
                    <VisibilityOffIcon sx={{ fontSize: '18px' }} />
                  ) : (
                    <VisibilityIcon sx={{ fontSize: '18px' }} />
                  )}
                </IconButton>
              </Tooltip>
            )
          })}
        </div>
      )}
    </div>
  )
}
