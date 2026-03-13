import { ApacheEChartsProps } from '@/features/charts/components/apacheEChart'
import { useTimeSpaceHandler } from '@/features/charts/timeSpaceDiagram/shared/handlers/timeSpace.handler'
import { TIME_SPACE_LOCATION_CARD_LAYOUT } from '@/features/charts/timeSpaceDiagram/shared/transformers/timeSpaceTransformerBase'
import VisibilityIcon from '@mui/icons-material/Visibility'
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff'
import { IconButton, Tooltip } from '@mui/material'
import type {
  ECharts,
  EChartsOption,
  GridComponentOption,
  LegendComponentOption,
  SeriesOption,
} from 'echarts'
import { init } from 'echarts'
import { useEffect, useMemo, useRef, useState } from 'react'
import { useGpxAnimationHandler } from '../handlers/gpxAnimation.handler'
import { GpxUploadOptions } from '../types'
import TimeSpaceSidebar from './TimeSpaceSidebar'

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

function getPrimaryLegend(
  option?: EChartsOption
): LegendSelectionProvider | undefined {
  if (!option?.legend) return undefined

  return Array.isArray(option.legend)
    ? (option.legend[0] as LegendSelectionProvider | undefined)
    : (option.legend as LegendSelectionProvider)
}

function getLegendSelectedMap(option?: EChartsOption): Record<string, boolean> {
  const selected = getPrimaryLegend(option)?.selected
  return selected ? { ...selected } : {}
}

function buildChartOptionWithSidebar(option: EChartsOption): EChartsOption {
  const primaryLegend = getPrimaryLegend(option)
  const hasLegendData =
    primaryLegend &&
    Array.isArray(primaryLegend.data) &&
    primaryLegend.data.length > 0

  if (!hasLegendData) {
    return option
  }

  const hideLegend = (legend: LegendComponentOption) => ({
    ...legend,
    show: false,
  })

  const nextLegend = Array.isArray(option.legend)
    ? option.legend.map((legend, index) =>
        index === 0 ? hideLegend(legend as LegendComponentOption) : legend
      )
    : hideLegend(primaryLegend)

  const shrinkGrid = (grid?: GridComponentOption) => {
    if (!grid) return grid

    return {
      ...grid,
      right: typeof grid.right === 'number' ? Math.min(grid.right, 36) : 36,
    }
  }

  const nextGrid = Array.isArray(option.grid)
    ? option.grid.map((grid, index) =>
        index === 0 ? shrinkGrid(grid as GridComponentOption) : grid
      )
    : shrinkGrid(option.grid as GridComponentOption | undefined)

  return {
    ...option,
    legend: nextLegend,
    grid: nextGrid,
  }
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
      const pixel = getChartPixel(chart, time, distance)
      if (!pixel) return null

      const [, y] = pixel

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

function getChartPixel(
  chart: ECharts,
  time: string | number,
  distance: number
): [number, number] | null {
  try {
    const pixel = chart.convertToPixel({ xAxisIndex: 0, yAxisIndex: 0 }, [
      time,
      distance,
    ])

    if (
      !Array.isArray(pixel) ||
      pixel.length < 2 ||
      !Number.isFinite(pixel[0]) ||
      !Number.isFinite(pixel[1])
    ) {
      return null
    }

    return [pixel[0], pixel[1]]
  } catch {
    return null
  }
}

type LegendSelectionProvider = LegendComponentOption & {
  selected?: Record<string, boolean>
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
  const [selectedSeries, setSelectedSeries] = useState<Record<string, boolean>>(
    () => getLegendSelectedMap(option)
  )
  const renderedOption = useMemo(
    () => buildChartOptionWithSidebar(option),
    [option]
  )

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
    setSelectedSeries(getLegendSelectedMap(option))
  }, [option])

  useEffect(() => {
    const inst = chartInstanceRef.current
    if (!inst || !renderedOption) return
    inst.setOption(renderedOption)
  }, [chart, renderedOption])

  useEffect(() => {
    if (!chart) return

    const syncSelectedSeries = () => {
      setSelectedSeries(getLegendSelectedMap(chart.getOption() as EChartsOption))
    }

    const handleLegendSelectionChange = (event: {
      selected?: Record<string, boolean>
    }) => {
      if (!event.selected) return
      setSelectedSeries({ ...event.selected })
    }

    syncSelectedSeries()
    chart.on('finished', syncSelectedSeries)
    chart.on('legendselectchanged', handleLegendSelectionChange)

    return () => {
      chart.off('finished', syncSelectedSeries)
      chart.off('legendselectchanged', handleLegendSelectionChange)
    }
  }, [chart, renderedOption])

  useEffect(() => {
    if (!chart || !renderedOption || !onToggleIgnoredLocation) {
      setLocationToggleButtons([])
      return
    }

    let rafId = 0

    const syncButtons = () => {
      rafId = window.requestAnimationFrame(() => {
        setLocationToggleButtons(
          buildLocationToggleButtons(chart, renderedOption)
        )
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
  }, [chart, renderedOption, onToggleIgnoredLocation])

  const handleToggleSeries = (seriesName: string) => {
    const inst = chartInstanceRef.current
    if (!inst) return

    const isSelected = selectedSeries[seriesName] !== false

    inst.dispatchAction({
      type: isSelected ? 'legendUnSelect' : 'legendSelect',
      name: seriesName,
    })

    setSelectedSeries((current) => ({
      ...current,
      [seriesName]: !isSelected,
    }))
  }

  return (
    <div
      style={{
        width: '100%',
        height: '100%',
        position: 'relative',
        display: 'flex',
        ...style,
      }}
    >
      <div
        style={{
          flex: 1,
          minWidth: 0,
          height: '100%',
          position: 'relative',
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

      <TimeSpaceSidebar
        option={option}
        selectedSeries={selectedSeries}
        onToggleSeries={handleToggleSeries}
      />
    </div>
  )
}
