import { TimeSpaceChartHeader } from '@/features/charts/timeSpaceDiagram/renderer/components/TimeSpaceChartHeader'
import {
  TimeSpaceLocationToggleOverlay,
  TimeSpaceOffsetResetOverlay,
  TimeSpaceStickyBottomAxisOverlay,
  TimeSpaceStickyTopAxisOverlay,
} from '@/features/charts/timeSpaceDiagram/renderer/components/TimeSpaceChartOverlays'
import { TIME_SPACE_GUIDE_WIDTH } from '@/features/charts/timeSpaceDiagram/renderer/sidebar/timeSpaceSidebar.constants'
import {
  getSidebarDirectionControls,
  hasTimeSpaceStyleContent,
} from '@/features/charts/timeSpaceDiagram/renderer/sidebar/timeSpaceSidebarModel'
import {
  type TimeSpaceChartRendererProps,
  type TimeSpaceRendererDirectionRole,
  type TimeSpaceRendererTab,
} from '@/features/charts/timeSpaceDiagram/renderer/types/timeSpaceRenderer.types'
import {
  buildLocationToggleButtons,
  buildOffsetResetButtons,
  getGridRect,
  type LocationToggleButton,
  type OffsetResetButton,
} from '@/features/charts/timeSpaceDiagram/renderer/utils/timeSpaceChartGeometry'
import {
  buildChartOptionWithSidebar,
  CHART_CONTENT_PADDING,
  FULLSCREEN_PADDING_BOTTOM,
  FULLSCREEN_PADDING_X,
  getCssLength,
  GUIDE_EASING,
  GUIDE_TRANSITION_MS,
} from '@/features/charts/timeSpaceDiagram/renderer/utils/timeSpaceChartLayout'
import { buildTimeSpaceExportOption } from '@/features/charts/timeSpaceDiagram/renderer/utils/timeSpaceChartExport'
import {
  extractHeaderContent,
  getChartDownloadName,
} from '@/features/charts/timeSpaceDiagram/renderer/utils/timeSpaceChartHeader'
import {
  getLegendSelectedMap,
  syncRequestedLegendSelections,
  withSyntheticLegendEntry,
} from '@/features/charts/timeSpaceDiagram/renderer/utils/timeSpaceLegend'
import {
  buildStickyBottomAxisOption,
  extractBottomAxisConfig,
  extractTopAxisConfig,
  formatTopAxisTickLabel,
  getAxisPixel,
  getAxisValueFromPixel,
  getHorizontalSliderZoomState,
  stripBottomAxisVisuals,
  stripSliderDataZoomVisuals,
  stripTopAxisVisuals,
  type StickyBottomAxis,
  type StickyTopAxis,
} from '@/features/charts/timeSpaceDiagram/renderer/utils/timeSpaceStickyAxes'
import { useTimeSpaceHandler } from '@/features/charts/timeSpaceDiagram/shared/handlers/timeSpace.handler'
import {
  CYCLE_LABEL_SERIES_ID_PREFIX,
  TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT,
} from '@/features/charts/timeSpaceDiagram/shared/transformers/timeSpaceTransformerBase'
import type { ECharts, EChartsOption, SeriesOption } from 'echarts'
import { init } from 'echarts'
import { useEffect, useMemo, useRef, useState } from 'react'
import { useGpxAnimationHandler } from '../handlers/gpxAnimation.handler'
import {
  applyTimeSpaceAppearanceToOption,
  createDefaultTimeSpaceAppearanceSettings,
  type TimeSpaceAppearanceSettings,
} from '../timeSpaceAppearance'
import { GpxUploadOptions, TIME_SPACE_GPX_TRACKS_LEGEND_NAME } from '../types'
import TimeSpaceSidebar from './TimeSpaceSidebar'

export { getRequestedLegendVisibility } from '@/features/charts/timeSpaceDiagram/renderer/utils/timeSpaceLegend'
export { buildOffsetResetButtons } from '@/features/charts/timeSpaceDiagram/renderer/utils/timeSpaceChartGeometry'

export type TimeSpaceChartProps = TimeSpaceChartRendererProps

const SRM_LEGEND_PREFIXES = [
  'SRM Collection',
  'SRM Estimated Trajectory',
  'SRM Entity',
] as const

function getOptionSeries(option?: EChartsOption): SeriesOption[] {
  if (!option?.series) {
    return []
  }

  return Array.isArray(option.series)
    ? (option.series as SeriesOption[])
    : [option.series as SeriesOption]
}

function hasRenderableSrmData(series: SeriesOption) {
  if (!Array.isArray(series.data)) {
    return false
  }

  return series.data.some((entry) => entry != null)
}

function hasSrmSeriesData(option?: EChartsOption) {
  return getOptionSeries(option).some((series) => {
    const name = typeof series.name === 'string' ? series.name.trim() : ''
    const id = typeof series.id === 'string' ? series.id : ''
    const isSrmSeries =
      SRM_LEGEND_PREFIXES.some((prefix) => name.startsWith(prefix)) ||
      id.startsWith('SRM ') ||
      id.startsWith('srm-')

    return isSrmSeries && hasRenderableSrmData(series)
  })
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
    distanceSpacingMode = 'distance',
    onToggleDistanceSpacingMode,
    sidebarUploadContent,
    isVisible = true,
  } = prop

  const chartRef = useRef<HTMLDivElement>(null)
  const stickyBottomAxisRef = useRef<HTMLDivElement>(null)
  const fullscreenRef = useRef<HTMLDivElement>(null)
  const headerRef = useRef<HTMLDivElement>(null)
  const chartInstanceRef = useRef<ECharts | null>(null)
  const stickyBottomAxisChartRef = useRef<ECharts | null>(null)
  const syncingMainToBottomZoomRef = useRef(false)
  const syncingBottomToMainZoomRef = useRef(false)
  const [chart, setChart] = useState<ECharts | null>(null)
  const [locationToggleButtons, setLocationToggleButtons] = useState<
    LocationToggleButton[]
  >([])
  const [offsetResetButtons, setOffsetResetButtons] = useState<
    OffsetResetButton[]
  >([])
  const [isFullscreen, setIsFullscreen] = useState(false)
  const [isGuideCollapsed, setIsGuideCollapsed] = useState(false)
  const [sidebarTab, setSidebarTab] = useState<TimeSpaceRendererTab>('legend')
  const defaultAppearanceSettings = useMemo(
    () => createDefaultTimeSpaceAppearanceSettings(),
    []
  )
  const [appearanceSettings, setAppearanceSettings] =
    useState<TimeSpaceAppearanceSettings>(() => defaultAppearanceSettings)
  const [showPhaseInfo, setShowPhaseInfo] = useState(true)
  const [headerHeight, setHeaderHeight] = useState(0)
  const [stickyTopAxis, setStickyTopAxis] = useState<StickyTopAxis | null>(null)
  const [stickyBottomAxis, setStickyBottomAxis] =
    useState<StickyBottomAxis | null>(null)
  const [timeSpaceHandlerSyncVersion, setTimeSpaceHandlerSyncVersion] =
    useState(0)

  const hasGpxTracks = useMemo(
    () =>
      (gpxEntries ?? []).some(
        (entry) =>
          !entry?.error &&
          Array.isArray(entry.parsedData) &&
          entry.parsedData.length > 0
      ),
    [gpxEntries]
  )
  const hasSrmTracks = useMemo(
    () =>
      hasSrmSeriesData(option) ||
      (gpxEntries ?? []).some(
        (entry) =>
          !entry?.error &&
          Array.isArray(entry.parsedEntityData) &&
          entry.parsedEntityData.length > 0
      ),
    [gpxEntries, option]
  )
  const baseSelectedSeries = useMemo(() => getLegendSelectedMap(option), [option])
  const defaultSelectedSeries = useMemo(() => {
    const next = { ...baseSelectedSeries }
    if (hasGpxTracks) {
      next[TIME_SPACE_GPX_TRACKS_LEGEND_NAME] = true
    }
    return next
  }, [baseSelectedSeries, hasGpxTracks])
  const [selectedSeries, setSelectedSeries] = useState<Record<string, boolean>>(
    () => defaultSelectedSeries
  )
  const [suppressedDirections, setSuppressedDirections] = useState<
    Partial<Record<TimeSpaceRendererDirectionRole, boolean>>
  >({})
  const { titleText, rangeText, remainingTitles } = useMemo(
    () => extractHeaderContent(option),
    [option]
  )
  const topAxisConfig = useMemo(() => extractTopAxisConfig(option), [option])
  const optionWithoutHeaderTitle = useMemo(() => {
    if (!option) return option

    return {
      ...option,
      title: remainingTitles,
      xAxis: stripTopAxisVisuals(option),
    }
  }, [option, remainingTitles])
  const optionWithOverlayLegend = useMemo(
    () =>
      withSyntheticLegendEntry(
        optionWithoutHeaderTitle,
        TIME_SPACE_GPX_TRACKS_LEGEND_NAME
      ),
    [optionWithoutHeaderTitle]
  )
  const sidebarAdjustedOption = useMemo(
    () =>
      optionWithOverlayLegend
        ? buildChartOptionWithSidebar(optionWithOverlayLegend, showPhaseInfo)
        : {},
    [optionWithOverlayLegend, showPhaseInfo]
  )
  const bottomAxisConfig = useMemo(
    () => extractBottomAxisConfig(sidebarAdjustedOption),
    [sidebarAdjustedOption]
  )
  const directionControls = useMemo(
    () => getSidebarDirectionControls(optionWithOverlayLegend),
    [optionWithOverlayLegend]
  )
  const hasStyleContent = useMemo(
    () => hasTimeSpaceStyleContent(optionWithOverlayLegend),
    [optionWithOverlayLegend]
  )
  const directionRoleBySeriesName = useMemo(() => {
    const nextMap = new Map<string, TimeSpaceRendererDirectionRole>()

    directionControls.forEach((control) => {
      control.seriesNames.forEach((seriesName) => {
        nextMap.set(seriesName, control.role)
      })
    })

    return nextMap
  }, [directionControls])
  const stickyBottomAxisOption = useMemo(
    () => buildStickyBottomAxisOption(bottomAxisConfig),
    [bottomAxisConfig]
  )
  const showStickyPhaseFooterLabels = useMemo(() => {
    if (!showPhaseInfo) {
      return false
    }

    const series = Array.isArray(sidebarAdjustedOption.series)
      ? (sidebarAdjustedOption.series as SeriesOption[])
      : []

    return series.some(
      (entry) =>
        typeof entry.id === 'string' &&
        entry.id.startsWith(CYCLE_LABEL_SERIES_ID_PREFIX)
    )
  }, [showPhaseInfo, sidebarAdjustedOption])
  const stickyPhaseFooterLabelPositions = useMemo(() => {
    if (!stickyBottomAxis) {
      return null
    }

    const { cardWidth, cardGapFromPlot, cardGapBetween } =
      TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT
    const primaryCardLeft = stickyBottomAxis.axisEnd + cardGapFromPlot

    return {
      primary: primaryCardLeft + cardWidth / 2,
      opposing: primaryCardLeft + cardWidth + cardGapBetween + cardWidth / 2,
    }
  }, [stickyBottomAxis])
  const styledSidebarOption = useMemo(
    () =>
      applyTimeSpaceAppearanceToOption(
        sidebarAdjustedOption,
        appearanceSettings,
        directionRoleBySeriesName
      ),
    [appearanceSettings, directionRoleBySeriesName, sidebarAdjustedOption]
  )
  const defaultStyledSidebarOption = useMemo(
    () =>
      applyTimeSpaceAppearanceToOption(
        sidebarAdjustedOption,
        defaultAppearanceSettings,
        directionRoleBySeriesName
      ),
    [
      defaultAppearanceSettings,
      directionRoleBySeriesName,
      sidebarAdjustedOption,
    ]
  )
  const renderedOption = useMemo(
    () => ({
      ...styledSidebarOption,
      xAxis: stripBottomAxisVisuals(styledSidebarOption),
      dataZoom: stripSliderDataZoomVisuals(styledSidebarOption),
    }),
    [styledSidebarOption]
  )
  const defaultRenderedOption = useMemo(
    () => ({
      ...defaultStyledSidebarOption,
      xAxis: stripBottomAxisVisuals(defaultStyledSidebarOption),
      dataZoom: stripSliderDataZoomVisuals(defaultStyledSidebarOption),
    }),
    [defaultStyledSidebarOption]
  )
  const baseHeight = getCssLength(style?.height)
  const fullscreenViewportHeight = '100vh'
  const fullscreenContentHeight = baseHeight
    ? `max(${baseHeight}, ${fullscreenViewportHeight})`
    : fullscreenViewportHeight
  const fullscreenChartBodyHeight = baseHeight
    ? `calc(${baseHeight} - ${headerHeight}px)`
    : undefined
  const guideTopOffset = headerHeight
  const guideMaxHeight = isFullscreen
    ? `calc(${fullscreenViewportHeight} - ${headerHeight}px - ${FULLSCREEN_PADDING_BOTTOM}px)`
    : `calc(100vh - ${headerHeight}px)`

  useTimeSpaceHandler(chart, timeSpaceHandlerSyncVersion, {
    enableCycleDragging: false,
  })

  const { play: playGpxAnimations } = useGpxAnimationHandler(
    chart,
    gpxEntries as GpxUploadOptions[]
  )

  useEffect(() => {
    const dom = chartRef.current
    if (!dom) return

    chartInstanceRef.current?.dispose()

    const nextChart = init(dom, theme, { useDirtyRect: true })
    chartInstanceRef.current = nextChart
    setChart(nextChart)

    const resizeObserver = new ResizeObserver(() => {
      const inst = chartInstanceRef.current
      const element = chartRef.current
      if (!inst || !element) return
      const { width, height } = element.getBoundingClientRect()
      if (width > 0 && height > 0) inst.resize()
    })
    resizeObserver.observe(dom)

    const handleWindowResize = () => {
      chartInstanceRef.current?.resize()
    }
    window.addEventListener('resize', handleWindowResize)

    return () => {
      window.removeEventListener('resize', handleWindowResize)
      resizeObserver.disconnect()
      nextChart.dispose()
      chartInstanceRef.current = null
      setChart(null)
      setLocationToggleButtons([])
    }
  }, [theme])

  useEffect(() => {
    const dom = stickyBottomAxisRef.current
    if (!dom || !stickyBottomAxisOption) {
      stickyBottomAxisChartRef.current?.dispose()
      stickyBottomAxisChartRef.current = null
      return
    }

    stickyBottomAxisChartRef.current?.dispose()

    const stickyChart = init(dom, theme, { useDirtyRect: true })
    stickyBottomAxisChartRef.current = stickyChart

    return () => {
      stickyChart.dispose()
      if (stickyBottomAxisChartRef.current === stickyChart) {
        stickyBottomAxisChartRef.current = null
      }
    }
  }, [theme, stickyBottomAxis, stickyBottomAxisOption])

  useEffect(() => {
    setSelectedSeries(baseSelectedSeries)
    setSuppressedDirections({})
  }, [baseSelectedSeries])

  useEffect(() => {
    setSelectedSeries((current) => {
      const next = { ...current }

      if (hasGpxTracks) {
        next[TIME_SPACE_GPX_TRACKS_LEGEND_NAME] ??= true
      } else {
        delete next[TIME_SPACE_GPX_TRACKS_LEGEND_NAME]
      }

      return next
    })
  }, [hasGpxTracks])

  useEffect(() => {
    const inst = chartInstanceRef.current
    if (!inst || !renderedOption) return
    inst.setOption(renderedOption, {
      replaceMerge: ['series', 'grid', 'legend', 'toolbox'],
    })
    setTimeSpaceHandlerSyncVersion((current) => current + 1)
  }, [chart, renderedOption])

  useEffect(() => {
    playGpxAnimations()
  }, [playGpxAnimations, renderedOption])

  useEffect(() => {
    const stickyChart = stickyBottomAxisChartRef.current
    if (!stickyChart) return

    if (!stickyBottomAxisOption) {
      stickyChart.clear()
      return
    }

    stickyChart.setOption(stickyBottomAxisOption, true)
    stickyChart.resize()
  }, [stickyBottomAxis, stickyBottomAxisOption, isFullscreen, headerHeight])

  useEffect(() => {
    if (!chart) return

    const handleRestore = () => {
      setSelectedSeries(defaultSelectedSeries)
      setSuppressedDirections({})
      setAppearanceSettings(defaultAppearanceSettings)
    }

    chart.on('restore', handleRestore)

    return () => {
      chart.off('restore', handleRestore)
    }
  }, [chart, defaultAppearanceSettings, defaultSelectedSeries])

  useEffect(() => {
    if (!chart) return
    syncRequestedLegendSelections(
      chart,
      selectedSeries,
      suppressedDirections,
      directionRoleBySeriesName
    )
  }, [
    chart,
    directionRoleBySeriesName,
    renderedOption,
    selectedSeries,
    suppressedDirections,
  ])

  useEffect(() => {
    const handleFullscreenChange = () => {
      setIsFullscreen(document.fullscreenElement === fullscreenRef.current)
      window.requestAnimationFrame(() => {
        chartInstanceRef.current?.resize()
        window.dispatchEvent(new Event('resize'))
      })
    }

    document.addEventListener('fullscreenchange', handleFullscreenChange)

    return () => {
      document.removeEventListener('fullscreenchange', handleFullscreenChange)
    }
  }, [])

  useEffect(() => {
    if (!isVisible) {
      return
    }

    let firstRafId = 0
    let secondRafId = 0

    const syncVisibleChartLayout = () => {
      const mainChart = chartInstanceRef.current
      const stickyChart = stickyBottomAxisChartRef.current
      const chartElement = chartRef.current

      if (!mainChart || !chartElement) {
        return
      }

      const { width, height } = chartElement.getBoundingClientRect()
      if (width <= 0 || height <= 0) {
        return
      }

      mainChart.resize()
      stickyChart?.resize()
      window.dispatchEvent(new Event('resize'))
    }

    firstRafId = window.requestAnimationFrame(() => {
      secondRafId = window.requestAnimationFrame(syncVisibleChartLayout)
    })

    return () => {
      window.cancelAnimationFrame(firstRafId)
      window.cancelAnimationFrame(secondRafId)
    }
  }, [isVisible, chart, stickyBottomAxisOption])

  useEffect(() => {
    const node = headerRef.current
    if (!node) return

    const syncHeaderHeight = () => {
      setHeaderHeight(node.getBoundingClientRect().height)
    }

    syncHeaderHeight()

    const observer = new ResizeObserver(syncHeaderHeight)
    observer.observe(node)
    window.addEventListener('resize', syncHeaderHeight)

    return () => {
      observer.disconnect()
      window.removeEventListener('resize', syncHeaderHeight)
    }
  }, [isGuideCollapsed, titleText, rangeText])

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

  useEffect(() => {
    if (!chart || !renderedOption) {
      setOffsetResetButtons([])
      return
    }

    let rafId = 0

    const syncButtons = () => {
      rafId = window.requestAnimationFrame(() => {
        setOffsetResetButtons(
          buildOffsetResetButtons(chart, chart.getOption() as EChartsOption)
        )
      })
    }

    syncButtons()

    chart.on('rendered', syncButtons)
    chart.on('finished', syncButtons)
    chart.on('restore', syncButtons)
    window.addEventListener('resize', syncButtons)

    return () => {
      window.cancelAnimationFrame(rafId)
      chart.off('rendered', syncButtons)
      chart.off('finished', syncButtons)
      chart.off('restore', syncButtons)
      window.removeEventListener('resize', syncButtons)
    }
  }, [chart, renderedOption])

  useEffect(() => {
    if (!chart || !renderedOption || !topAxisConfig) {
      setStickyTopAxis(null)
      return
    }

    let rafId = 0

    const syncStickyAxis = () => {
      rafId = window.requestAnimationFrame(() => {
        const { x, width } = getGridRect(chart, renderedOption)

        if (!Number.isFinite(x) || !Number.isFinite(width) || width <= 0) {
          setStickyTopAxis(null)
          return
        }

        const { min, max, interval, label, formatter } = topAxisConfig
        const visibleStart =
          getAxisValueFromPixel(chart, topAxisConfig.index, x) ?? min
        const visibleEnd =
          getAxisValueFromPixel(chart, topAxisConfig.index, x + width) ?? max
        const visibleMin = Math.max(min, Math.min(visibleStart, visibleEnd))
        const visibleMax = Math.min(max, Math.max(visibleStart, visibleEnd))
        const range = visibleMax - visibleMin

        if (!Number.isFinite(range) || range <= 0) {
          setStickyTopAxis(null)
          return
        }

        const firstTick =
          Math.ceil((visibleMin - min) / interval) * interval + min
        const ticks: StickyTopAxis['ticks'] = []

        for (
          let value = firstTick;
          value <= visibleMax + interval * 0.001;
          value += interval
        ) {
          const pixel = getAxisPixel(chart, topAxisConfig.index, value)
          if (pixel == null) continue

          ticks.push({
            value,
            left: pixel,
            label: formatTopAxisTickLabel(formatter, value),
          })
        }

        setStickyTopAxis({
          label,
          axisStart: x,
          axisEnd: x + width,
          ticks,
        })
      })
    }

    syncStickyAxis()

    chart.on('finished', syncStickyAxis)
    chart.on('restore', syncStickyAxis)
    chart.on('datazoom', syncStickyAxis)
    window.addEventListener('resize', syncStickyAxis)

    return () => {
      window.cancelAnimationFrame(rafId)
      chart.off('finished', syncStickyAxis)
      chart.off('restore', syncStickyAxis)
      chart.off('datazoom', syncStickyAxis)
      window.removeEventListener('resize', syncStickyAxis)
    }
  }, [chart, renderedOption, topAxisConfig])

  useEffect(() => {
    if (!chart || !renderedOption || !bottomAxisConfig) {
      setStickyBottomAxis(null)
      return
    }

    let rafId = 0

    const syncStickyBottomAxis = () => {
      rafId = window.requestAnimationFrame(() => {
        const { x, width } = getGridRect(chart, renderedOption)

        if (!Number.isFinite(x) || !Number.isFinite(width) || width <= 0) {
          setStickyBottomAxis(null)
          return
        }

        const nextAxis = {
          label: bottomAxisConfig.label,
          axisStart: x,
          axisEnd: x + width,
        }

        setStickyBottomAxis((current) =>
          current &&
          current.label === nextAxis.label &&
          current.axisStart === nextAxis.axisStart &&
          current.axisEnd === nextAxis.axisEnd
            ? current
            : nextAxis
        )
      })
    }

    syncStickyBottomAxis()

    chart.on('finished', syncStickyBottomAxis)
    chart.on('restore', syncStickyBottomAxis)
    chart.on('datazoom', syncStickyBottomAxis)
    window.addEventListener('resize', syncStickyBottomAxis)

    return () => {
      window.cancelAnimationFrame(rafId)
      chart.off('finished', syncStickyBottomAxis)
      chart.off('restore', syncStickyBottomAxis)
      chart.off('datazoom', syncStickyBottomAxis)
      window.removeEventListener('resize', syncStickyBottomAxis)
    }
  }, [bottomAxisConfig, chart, renderedOption])

  useEffect(() => {
    const mainChart = chartInstanceRef.current
    const stickyChart = stickyBottomAxisChartRef.current

    if (!mainChart || !stickyChart || !stickyBottomAxisOption) {
      return
    }

    let rafId = 0

    const syncStickyBottomAxis = () => {
      rafId = window.requestAnimationFrame(() => {
        stickyChart.resize()

        const zoomState = getHorizontalSliderZoomState(
          mainChart.getOption() as EChartsOption
        )

        if (!zoomState) {
          return
        }

        syncingMainToBottomZoomRef.current = true
        stickyChart.dispatchAction({
          type: 'dataZoom',
          start: zoomState.start,
          end: zoomState.end,
        })
      })
    }

    syncStickyBottomAxis()

    mainChart.on('finished', syncStickyBottomAxis)
    mainChart.on('restore', syncStickyBottomAxis)
    mainChart.on('datazoom', syncStickyBottomAxis)
    window.addEventListener('resize', syncStickyBottomAxis)

    return () => {
      window.cancelAnimationFrame(rafId)
      mainChart.off('finished', syncStickyBottomAxis)
      mainChart.off('restore', syncStickyBottomAxis)
      mainChart.off('datazoom', syncStickyBottomAxis)
      window.removeEventListener('resize', syncStickyBottomAxis)
    }
  }, [chart, stickyBottomAxis, stickyBottomAxisOption])

  useEffect(() => {
    const mainChart = chartInstanceRef.current
    const stickyChart = stickyBottomAxisChartRef.current

    if (!mainChart || !stickyChart || !stickyBottomAxisOption) {
      return
    }

    const handleStickyBottomDataZoom = () => {
      if (syncingMainToBottomZoomRef.current) {
        syncingMainToBottomZoomRef.current = false
        return
      }

      const zoomState = getHorizontalSliderZoomState(
        stickyChart.getOption() as EChartsOption
      )

      if (!zoomState) {
        return
      }

      syncingBottomToMainZoomRef.current = true
      mainChart.dispatchAction({
        type: 'dataZoom',
        start: zoomState.start,
        end: zoomState.end,
      })
      window.requestAnimationFrame(() => {
        syncingBottomToMainZoomRef.current = false
      })
    }

    const handleMainChartZoomLoop = () => {
      if (syncingBottomToMainZoomRef.current) {
        syncingBottomToMainZoomRef.current = false
      }
    }

    stickyChart.on('datazoom', handleStickyBottomDataZoom)
    mainChart.on('datazoom', handleMainChartZoomLoop)

    return () => {
      stickyChart.off('datazoom', handleStickyBottomDataZoom)
      mainChart.off('datazoom', handleMainChartZoomLoop)
    }
  }, [chart, stickyBottomAxis, stickyBottomAxisOption])

  const handleSetSeriesVisibility = (
    seriesNames: string[],
    visible: boolean
  ) => {
    const uniqueSeriesNames = Array.from(new Set(seriesNames))
    setSelectedSeries((current) => {
      const nextSelections = { ...current }

      for (const seriesName of uniqueSeriesNames) {
        nextSelections[seriesName] = visible
      }

      return nextSelections
    })
  }

  const handleToggleDirectionVisibility = (
    role: TimeSpaceRendererDirectionRole
  ) => {
    setSuppressedDirections((current) => ({
      ...current,
      [role]: current[role] !== true ? true : false,
    }))
  }

  const handleToggleFullscreen = async () => {
    const container = fullscreenRef.current
    if (!container) return

    if (document.fullscreenElement === container) {
      await document.exitFullscreen()
      return
    }

    await container.requestFullscreen()
  }

  const handleToggleGuide = () => {
    setIsGuideCollapsed((current) => !current)
  }

  const handleTogglePhaseInfo = () => {
    setShowPhaseInfo((current) => !current)
  }

  const handleResetChart = () => {
    const chartInstance = chartInstanceRef.current
    if (!chartInstance) return

    chartInstance.setOption(defaultRenderedOption, {
      notMerge: true,
      lazyUpdate: false,
    })
    setAppearanceSettings(defaultAppearanceSettings)
    setSelectedSeries(defaultSelectedSeries)
    setSuppressedDirections({})
    setTimeSpaceHandlerSyncVersion((current) => current + 1)
  }

  const handleDownloadChart = async () => {
    const chartInstance = chartInstanceRef.current
    const chartElement = chartRef.current
    if (!chartInstance || !chartElement) return

    const { width, height } = chartElement.getBoundingClientRect()
    if (width <= 0 || height <= 0) return

    const exportContainer = document.createElement('div')
    exportContainer.style.position = 'fixed'
    exportContainer.style.left = '-10000px'
    exportContainer.style.top = '0'
    exportContainer.style.width = `${Math.ceil(width)}px`
    exportContainer.style.height = `${Math.ceil(height)}px`
    exportContainer.style.pointerEvents = 'none'
    exportContainer.style.opacity = '0'
    document.body.appendChild(exportContainer)

    const exportChart = init(exportContainer, theme, { useDirtyRect: true })

    try {
      const exportOption = buildTimeSpaceExportOption(
        chartInstance.getOption() as EChartsOption,
        option
      )

      exportChart.setOption(exportOption, true)

      await new Promise<void>((resolve) => {
        window.requestAnimationFrame(() => {
          window.requestAnimationFrame(() => resolve())
        })
      })

      const dataUrl = exportChart.getDataURL({
        type: 'png',
        pixelRatio: 2,
        backgroundColor: '#ffffff',
      })

      const link = document.createElement('a')
      link.href = dataUrl
      link.download = `${getChartDownloadName(option)}.png`
      link.click()
    } finally {
      exportChart.dispose()
      exportContainer.remove()
    }
  }

  return (
    <div
      ref={fullscreenRef}
      style={{
        width: '100%',
        height: isFullscreen ? '100vh' : '100%',
        position: 'relative',
        overflow: isFullscreen ? 'auto' : 'visible',
        background: isFullscreen ? '#fff' : undefined,
        boxSizing: 'border-box',
      }}
    >
      <div
        style={{
          ...style,
          width: '100%',
          height: isFullscreen
            ? fullscreenContentHeight
            : (baseHeight ?? '100%'),
          minHeight: isFullscreen
            ? fullscreenContentHeight
            : (baseHeight ?? undefined),
          position: 'relative',
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        <TimeSpaceChartHeader
          hasStyleContent={hasStyleContent}
          hasUploadContent={Boolean(sidebarUploadContent)}
          headerRef={headerRef}
          isFullscreen={isFullscreen}
          isGuideCollapsed={isGuideCollapsed}
          onDownloadChart={handleDownloadChart}
          onResetChart={handleResetChart}
          onSidebarTabChange={setSidebarTab}
          onToggleFullscreen={handleToggleFullscreen}
          onToggleGuide={handleToggleGuide}
          onToggleDistanceSpacingMode={onToggleDistanceSpacingMode}
          onTogglePhaseInfo={handleTogglePhaseInfo}
          distanceSpacingMode={distanceSpacingMode}
          rangeText={rangeText}
          showPhaseInfo={showPhaseInfo}
          sidebarTab={sidebarTab}
          titleText={titleText}
        />

        <div
          style={{
            display: 'flex',
            flex: 1,
            minHeight: 0,
            alignItems:
              isFullscreen && fullscreenChartBodyHeight
                ? 'flex-start'
                : undefined,
            position: 'relative',
            padding: isFullscreen
              ? `0 ${FULLSCREEN_PADDING_X}px ${FULLSCREEN_PADDING_BOTTOM}px`
              : undefined,
            boxSizing: 'border-box',
          }}
        >
          <div
            style={{
              flex: 1,
              minWidth: 0,
              height:
                isFullscreen && fullscreenChartBodyHeight
                  ? fullscreenChartBodyHeight
                  : '100%',
              alignSelf:
                isFullscreen && fullscreenChartBodyHeight
                  ? 'flex-start'
                  : undefined,
              boxSizing: 'border-box',
              position: 'relative',
              display: 'flex',
              flexDirection: 'column',
            }}
          >
            <TimeSpaceStickyTopAxisOverlay
              contentPadding={CHART_CONTENT_PADDING}
              headerHeight={headerHeight}
              stickyTopAxis={stickyTopAxis}
            />

            <div
              style={{
                flex: 1,
                minHeight: 0,
                position: 'relative',
                padding: `${CHART_CONTENT_PADDING}px`,
                boxSizing: 'border-box',
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

              <TimeSpaceLocationToggleOverlay
                buttons={locationToggleButtons}
                ignoredLocations={ignoredLocations}
                onToggleIgnoredLocation={onToggleIgnoredLocation}
              />
              <TimeSpaceOffsetResetOverlay
                buttons={offsetResetButtons}
                chart={chart}
              />
            </div>

            <TimeSpaceStickyBottomAxisOverlay
              contentPadding={CHART_CONTENT_PADDING}
              showStickyPhaseFooterLabels={showStickyPhaseFooterLabels}
              stickyBottomAxis={stickyBottomAxis}
              stickyBottomAxisOption={stickyBottomAxisOption}
              stickyBottomAxisRef={stickyBottomAxisRef}
              stickyPhaseFooterLabelPositions={stickyPhaseFooterLabelPositions}
            />
          </div>

          <div
            style={{
              width: isGuideCollapsed ? 0 : TIME_SPACE_GUIDE_WIDTH,
              minWidth: isGuideCollapsed ? 0 : TIME_SPACE_GUIDE_WIDTH,
              flexShrink: 0,
              position: 'sticky',
              top: `${guideTopOffset}px`,
              alignSelf: 'flex-start',
              height: guideMaxHeight,
              maxHeight: guideMaxHeight,
              zIndex: 3,
              willChange: 'width, min-width',
              transition: `width ${GUIDE_TRANSITION_MS}ms ${GUIDE_EASING}, min-width ${GUIDE_TRANSITION_MS}ms ${GUIDE_EASING}`,
              overflow: 'visible',
            }}
          >
            {!isGuideCollapsed && (
              <TimeSpaceSidebar
                option={optionWithOverlayLegend}
                selectedSeries={selectedSeries}
                suppressedDirections={suppressedDirections}
                onSetSeriesVisibility={handleSetSeriesVisibility}
                onToggleDirectionVisibility={handleToggleDirectionVisibility}
                gpxTracksAvailable={hasGpxTracks}
                srmTracksAvailable={hasSrmTracks}
                appearanceSettings={appearanceSettings}
                onAppearanceChange={setAppearanceSettings}
                onResetAppearance={() =>
                  setAppearanceSettings(defaultAppearanceSettings)
                }
                uploadContent={sidebarUploadContent}
                activeTab={sidebarTab}
                onTabChange={setSidebarTab}
                showTabs={false}
              />
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
