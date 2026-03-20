import { ApacheEChartsProps } from '@/features/charts/components/apacheEChart'
import { useTimeSpaceHandler } from '@/features/charts/timeSpaceDiagram/shared/handlers/timeSpace.handler'
import {
  CYCLE_LABEL_SERIES_ID_PREFIX,
  TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT,
  TIME_SPACE_LOCATION_CARD_LAYOUT,
} from '@/features/charts/timeSpaceDiagram/shared/transformers/timeSpaceTransformerBase'
import VisibilityIcon from '@mui/icons-material/Visibility'
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff'
import {
  Box,
  Divider,
  IconButton,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
  SvgIcon,
  Tooltip,
  Typography,
} from '@mui/material'
import type {
  ECharts,
  EChartsOption,
  GridComponentOption,
  LegendComponentOption,
  SeriesOption,
  TitleComponentOption,
  ToolboxComponentOption,
} from 'echarts'
import { init } from 'echarts'
import type { ReactNode } from 'react'
import { useEffect, useMemo, useRef, useState } from 'react'
import { useGpxAnimationHandler } from '../handlers/gpxAnimation.handler'
import { GpxUploadOptions } from '../types'
import TimeSpaceSidebar, {
  SidebarTab,
  TIME_SPACE_GUIDE_WIDTH,
  TimeSpaceSidebarTabs,
} from './TimeSpaceSidebar'

export interface TimeSpaceChartProps extends ApacheEChartsProps {
  gpxEntries?: GpxUploadOptions[]
  ignoredLocations?: string[]
  onToggleIgnoredLocation?: (location: string) => void
  sidebarUploadContent?: ReactNode
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

type ContextMenuPosition = {
  mouseX: number
  mouseY: number
}

function PanelSidebarIcon({ side }: { side: 'left' | 'right' }) {
  return (
    <SvgIcon
      viewBox="0 0 24 24"
      sx={{
        fontSize: HEADER_TOOLBAR_ICON_SIZE,
        ...(side === 'left' ? { transform: 'rotate(180deg)' } : undefined),
      }}
    >
      <rect
        x="3"
        y="3"
        width="18"
        height="18"
        rx="2"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
      />
      <path
        d="M15 3v18"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </SvgIcon>
  )
}

function ToolbarActionIcon() {
  return (
    <SvgIcon sx={{ fontSize: HEADER_TOOLBAR_ICON_SIZE }} viewBox="0 0 24 24">
      <path
        d="M12 15V3"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="m7 10 5 5 5-5"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </SvgIcon>
  )
}

function PhaseInfoActionIcon() {
  return (
    <SvgIcon sx={{ fontSize: HEADER_TOOLBAR_ICON_SIZE }} viewBox="0 0 24 24">
      <path
        d="m21 16-4 4-4-4"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M17 20V4"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="m3 8 4-4 4 4"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M7 4v16"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </SvgIcon>
  )
}

function FullscreenActionIcon({ expanded }: { expanded: boolean }) {
  return (
    <SvgIcon sx={{ fontSize: HEADER_TOOLBAR_ICON_SIZE }} viewBox="0 0 24 24">
      {expanded ? (
        <>
          <path
            d="m15 15 6 6m-6-6v4.8m0-4.8h4.8"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M9 19.8V15m0 0H4.2M9 15l-6 6"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M15 4.2V9m0 0h4.8M15 9l6-6"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M9 4.2V9m0 0H4.2M9 9 3 3"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </>
      ) : (
        <>
          <path
            d="m15 15 6 6"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="m15 9 6-6"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M21 16v5h-5"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M21 8V3h-5"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M3 16v5h5"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="m3 21 6-6"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M3 8V3h5"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M9 9 3 3"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </>
      )}
    </SvgIcon>
  )
}

function ResetActionIcon() {
  return (
    <SvgIcon sx={{ fontSize: HEADER_TOOLBAR_ICON_SIZE }} viewBox="0 0 24 24">
      <path
        d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M3 3v5h5"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </SvgIcon>
  )
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
          width?: number
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

function getGridRect(chart: ECharts, option?: EChartsOption) {
  let gridRect:
    | {
        x?: number
        width?: number
      }
    | undefined

  try {
    gridRect = (chart as GridRectProvider)
      ?.getModel?.()
      ?.getComponent?.('grid', 0)
      ?.coordinateSystem?.getRect?.()
  } catch {
    gridRect = undefined
  }

  if (typeof gridRect?.x === 'number' && typeof gridRect?.width === 'number') {
    return {
      x: gridRect.x,
      width: gridRect.width,
    }
  }

  const grid = Array.isArray(option?.grid) ? option.grid[0] : option?.grid
  const left = typeof grid?.left === 'number' ? grid.left : 0
  const right = typeof grid?.right === 'number' ? grid.right : 0
  let chartWidth = 0

  try {
    const dom = chart.getDom?.()
    if (dom instanceof HTMLElement && Number.isFinite(dom.clientWidth)) {
      chartWidth = dom.clientWidth
    } else {
      chartWidth = chart.getWidth()
    }
  } catch {
    chartWidth = 0
  }

  return {
    x: left,
    width: Math.max(0, chartWidth - left - right),
  }
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

function getPrimaryToolbox(
  option?: EChartsOption
): ToolboxComponentOption | undefined {
  if (!option?.toolbox) return undefined

  return Array.isArray(option.toolbox)
    ? (option.toolbox[0] as ToolboxComponentOption | undefined)
    : (option.toolbox as ToolboxComponentOption)
}

const CYCLE_PREFIX = 'Cycles '
const CYCLE_DURATION_PREFIX = 'Cycle Durations '
const GUIDE_TRANSITION_MS = 200
const GUIDE_EASING = 'cubic-bezier(0.2, 0, 0, 1)'
const MIN_RIGHT_PLOT_GUTTER = 10
const CHART_CONTENT_PADDING = 16
const FULLSCREEN_PADDING_X = 24
const FULLSCREEN_PADDING_BOTTOM = 24
const STICKY_TOP_AXIS_HEIGHT = 42
const LOCATION_TOGGLE_ICON_SIZE = 18
const HEADER_TOOLBAR_ICON_SIZE = 18
const HEADER_TOOLBAR_ICON_STROKE_WIDTH = 1.7
const TIME_SPACE_LABEL_GUTTER_WIDTH =
  TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT.cardWidth * 2 +
  TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT.cardGapFromPlot +
  TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT.cardGapBetween +
  12

function getCssLength(value: string | number | undefined) {
  if (typeof value === 'number') {
    return `${value}px`
  }

  if (typeof value === 'string' && value.trim()) {
    return value
  }

  return undefined
}

function getLegendEntryName(entry: string | { name?: string }): string | null {
  if (typeof entry === 'string') {
    return entry.trim() || null
  }

  return typeof entry?.name === 'string' && entry.name.trim()
    ? entry.name.trim()
    : null
}

function getDirectionalSuffix(name: string, prefix: string): string | null {
  if (!name.startsWith(prefix)) {
    return null
  }

  const suffix = name.slice(prefix.length).trim()
  return suffix.length ? suffix : null
}

function syncCycleDurationSelections(
  chart: ECharts,
  requestedSelections: Record<string, boolean>
) {
  const option = chart.getOption() as EChartsOption
  const legend = getPrimaryLegend(option)

  if (!Array.isArray(legend?.data)) {
    return
  }

  const currentSelections = getLegendSelectedMap(option)

  for (const entry of legend.data) {
    const name = getLegendEntryName(entry)
    if (!name) continue

    const direction = getDirectionalSuffix(name, CYCLE_DURATION_PREFIX)
    if (!direction) continue

    const cycleName = `${CYCLE_PREFIX}${direction}`
    const shouldBeVisible =
      requestedSelections[name] !== false &&
      requestedSelections[cycleName] !== false
    const isVisible = currentSelections[name] !== false

    if (shouldBeVisible === isVisible) {
      continue
    }

    chart.dispatchAction({
      type: shouldBeVisible ? 'legendSelect' : 'legendUnSelect',
      name,
    })
  }
}

function buildChartOptionWithSidebar(
  option: EChartsOption,
  showPhaseInfo: boolean
): EChartsOption {
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

  const allSeries = Array.isArray(option.series)
    ? (option.series as SeriesOption[])
    : []
  const hasCycleLabels =
    showPhaseInfo &&
    allSeries.some(
      (entry) =>
        typeof entry.id === 'string' &&
        entry.id.startsWith(CYCLE_LABEL_SERIES_ID_PREFIX)
    )
  const series = showPhaseInfo
    ? allSeries
    : allSeries.filter(
        (entry) =>
          !(
            typeof entry.id === 'string' &&
            entry.id.startsWith(CYCLE_LABEL_SERIES_ID_PREFIX)
          )
      )

  const nextLegend = Array.isArray(option.legend)
    ? option.legend.map((legend, index) =>
        index === 0 ? hideLegend(legend as LegendComponentOption) : legend
      )
    : hideLegend(primaryLegend)

  const shrinkGrid = (grid?: GridComponentOption) => {
    if (!grid) return grid

    const rightGutter = hasCycleLabels
      ? TIME_SPACE_LABEL_GUTTER_WIDTH
      : MIN_RIGHT_PLOT_GUTTER

    return {
      ...grid,
      right:
        typeof grid.right === 'number'
          ? Math.min(grid.right, rightGutter)
          : rightGutter,
    }
  }

  const nextGrid = Array.isArray(option.grid)
    ? option.grid.map((grid, index) =>
        index === 0 ? shrinkGrid(grid as GridComponentOption) : grid
      )
    : shrinkGrid(option.grid as GridComponentOption | undefined)

  const hideToolbox = (toolbox: ToolboxComponentOption) => ({
    ...toolbox,
    show: false,
  })

  const nextToolbox = Array.isArray(option.toolbox)
    ? option.toolbox.map((toolbox) =>
        hideToolbox(toolbox as ToolboxComponentOption)
      )
    : option.toolbox
      ? hideToolbox(option.toolbox as ToolboxComponentOption)
      : option.toolbox

  return {
    ...option,
    legend: nextLegend,
    grid: nextGrid,
    series,
    toolbox: nextToolbox,
  }
}

function sanitizeFileName(name: string) {
  return name
    .trim()
    .replace(/[<>:"/\\|?*\x00-\x1F]/g, '-')
    .replace(/\s+/g, ' ')
}

function getChartDownloadName(option?: EChartsOption) {
  const toolbox = getPrimaryToolbox(option)
  const saveAsImage = toolbox?.feature?.saveAsImage as
    | { name?: unknown }
    | undefined

  if (typeof saveAsImage?.name === 'string' && saveAsImage.name.trim()) {
    return sanitizeFileName(saveAsImage.name)
  }

  const title = option?.title
  const rawTitle = Array.isArray(title)
    ? title
        .map((entry) =>
          typeof entry?.text === 'string' ? entry.text.trim() : ''
        )
        .filter(Boolean)
        .join(' - ')
    : typeof title?.text === 'string'
      ? title.text.trim()
      : ''

  return sanitizeFileName(rawTitle || 'time-space-diagram')
}

function getTitleEntries(option?: EChartsOption): TitleComponentOption[] {
  if (!option?.title) return []

  return Array.isArray(option.title)
    ? (option.title as TitleComponentOption[])
    : ([option.title] as TitleComponentOption[])
}

function stripRichText(text: string) {
  return text.replace(/\{[^|}]+\|([^}]+)\}/g, '$1')
}

function normalizeHeaderText(text: string) {
  return stripRichText(text).replace(/\s+/g, ' ').trim()
}

function extractHeaderContent(option?: EChartsOption) {
  const titleEntries = getTitleEntries(option)
  const primaryTitle =
    typeof titleEntries[0]?.text === 'string' ? titleEntries[0].text : ''
  const secondaryTitle =
    typeof titleEntries[1]?.text === 'string' ? titleEntries[1].text : ''

  const titleText = normalizeHeaderText(
    primaryTitle.split('\n')[0] ?? ''
  ).replace(/[,\s]+$/, '')
  const rangeText = normalizeHeaderText(secondaryTitle)

  const remainingTitles =
    titleEntries.length > 2 ? titleEntries.slice(2) : undefined

  return {
    titleText,
    rangeText,
    remainingTitles,
  }
}

type TopAxisConfig = {
  formatter?: ((value: number) => string) | string
  index: number
  interval: number
  label: string
  max: number
  min: number
}

type StickyTopAxis = {
  axisEnd: number
  axisStart: number
  label: string
  ticks: Array<{
    label: string
    left: number
    value: number
  }>
}

function extractTopAxisConfig(option?: EChartsOption): TopAxisConfig | null {
  if (!option?.xAxis) return null

  const xAxes = Array.isArray(option.xAxis) ? option.xAxis : [option.xAxis]
  const topAxisIndex = xAxes.findIndex(
    (axis) =>
      axis &&
      typeof axis === 'object' &&
      (axis as { position?: unknown }).position === 'top'
  )

  if (topAxisIndex < 0) {
    return null
  }

  const topAxis = xAxes[topAxisIndex] as {
    axisLabel?: {
      formatter?: ((value: number) => string) | string
    }
    interval?: unknown
    max?: unknown
    maxInterval?: unknown
    min?: unknown
    minInterval?: unknown
    name?: unknown
  }

  const min = typeof topAxis.min === 'number' ? topAxis.min : 0
  const max = typeof topAxis.max === 'number' ? topAxis.max : min
  const interval =
    typeof topAxis.interval === 'number'
      ? topAxis.interval
      : typeof topAxis.maxInterval === 'number'
        ? topAxis.maxInterval
        : typeof topAxis.minInterval === 'number'
          ? topAxis.minInterval
          : 60

  return {
    index: topAxisIndex,
    label: typeof topAxis.name === 'string' ? topAxis.name.trim() : '',
    min,
    max,
    interval: interval > 0 ? interval : 60,
    formatter: topAxis.axisLabel?.formatter,
  }
}

function stripTopAxisVisuals(option?: EChartsOption): EChartsOption['xAxis'] {
  if (!option?.xAxis) return option?.xAxis

  const stripAxisName = <T,>(axis: T): T => {
    if (!axis || typeof axis !== 'object') return axis

    const candidate = axis as T & { position?: unknown; name?: unknown }
    if (candidate.position !== 'top') {
      return axis
    }

    return {
      ...candidate,
      name: '',
      axisLabel: { show: false },
      axisLine: { show: false },
      axisTick: { show: false },
      minorTick: { show: false },
      show: false,
    } as T
  }

  return Array.isArray(option.xAxis)
    ? option.xAxis.map((axis) => stripAxisName(axis))
    : stripAxisName(option.xAxis)
}

function formatTopAxisTickLabel(
  formatter: TopAxisConfig['formatter'],
  value: number
) {
  if (typeof formatter === 'function') {
    try {
      return String(formatter(value))
    } catch {
      return String(value)
    }
  }

  return String(value)
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
    headerActionOverlayOffsetX,
    headerActionOverlayOffsetY,
    verticalOffsetY,
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

      const cardTop = y - cardHeight / 2 + verticalOffsetY

      return {
        location,
        left: iconLeft + headerActionOverlayOffsetX,
        top:
          cardTop +
          (headerHeight - headerActionSize) / 2 +
          headerActionOverlayOffsetY,
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

function getAxisPixel(
  chart: ECharts,
  xAxisIndex: number,
  value: number
): number | null {
  try {
    const pixel = chart.convertToPixel({ xAxisIndex }, value)

    if (typeof pixel === 'number' && Number.isFinite(pixel)) {
      return pixel
    }

    if (Array.isArray(pixel) && Number.isFinite(pixel[0])) {
      return pixel[0]
    }

    return null
  } catch {
    return null
  }
}

function getAxisValueFromPixel(
  chart: ECharts,
  xAxisIndex: number,
  pixelX: number
): number | null {
  try {
    const value = chart.convertFromPixel({ xAxisIndex }, pixelX)

    if (typeof value === 'number' && Number.isFinite(value)) {
      return value
    }

    if (Array.isArray(value) && Number.isFinite(value[0])) {
      return value[0]
    }

    return null
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
    sidebarUploadContent,
  } = prop

  const chartRef = useRef<HTMLDivElement>(null)
  const fullscreenRef = useRef<HTMLDivElement>(null)
  const headerRef = useRef<HTMLDivElement>(null)
  const chartInstanceRef = useRef<ECharts | null>(null)
  const [chart, setChart] = useState<ECharts | null>(null)
  const [locationToggleButtons, setLocationToggleButtons] = useState<
    LocationToggleButton[]
  >([])
  const [isFullscreen, setIsFullscreen] = useState(false)
  const [isGuideCollapsed, setIsGuideCollapsed] = useState(false)
  const [sidebarTab, setSidebarTab] = useState<SidebarTab>('legend')
  const [showPhaseInfo, setShowPhaseInfo] = useState(true)
  const [headerHeight, setHeaderHeight] = useState(0)
  const [stickyTopAxis, setStickyTopAxis] = useState<StickyTopAxis | null>(null)
  const [contextMenuPosition, setContextMenuPosition] =
    useState<ContextMenuPosition | null>(null)
  const [selectedSeries, setSelectedSeries] = useState<Record<string, boolean>>(
    () => getLegendSelectedMap(option)
  )
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
  const renderedOption = useMemo(
    () => buildChartOptionWithSidebar(optionWithoutHeaderTitle, showPhaseInfo),
    [optionWithoutHeaderTitle, showPhaseInfo]
  )
  const baseHeight = getCssLength(style?.height)
  const fullscreenViewportHeight = '100vh'
  const fullscreenContentHeight = baseHeight
    ? `max(${baseHeight}, ${fullscreenViewportHeight})`
    : fullscreenViewportHeight
  const guideTopOffset = headerHeight
  const guideMaxHeight = isFullscreen
    ? `calc(${fullscreenViewportHeight} - ${headerHeight}px - ${FULLSCREEN_PADDING_BOTTOM}px)`
    : `calc(100vh - ${headerHeight}px)`

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
    inst.setOption(renderedOption, {
      replaceMerge: ['series', 'grid', 'legend', 'toolbox'],
    })
  }, [chart, renderedOption])

  useEffect(() => {
    if (!chart) return

    const handleRestore = () => {
      setSelectedSeries(getLegendSelectedMap(option))
    }

    chart.on('restore', handleRestore)

    return () => {
      chart.off('restore', handleRestore)
    }
  }, [chart, option])

  useEffect(() => {
    if (!chart) return
    syncCycleDurationSelections(chart, selectedSeries)
  }, [chart, renderedOption, selectedSeries])

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

  const handleToggleFullscreen = async () => {
    const container = fullscreenRef.current
    if (!container) return

    if (document.fullscreenElement === container) {
      await document.exitFullscreen()
      return
    }

    await container.requestFullscreen()
  }

  const handleContextMenu = (event: React.MouseEvent<HTMLDivElement>) => {
    event.preventDefault()
    setContextMenuPosition({
      mouseX: event.clientX,
      mouseY: event.clientY,
    })
  }

  const handleCloseContextMenu = () => {
    setContextMenuPosition(null)
  }

  const handleCloseMenus = () => {
    handleCloseContextMenu()
  }

  const handleToggleGuide = () => {
    setIsGuideCollapsed((current) => !current)
    handleCloseMenus()
  }

  const handleTogglePhaseInfo = () => {
    setShowPhaseInfo((current) => !current)
    handleCloseMenus()
  }

  const handleToggleFullscreenFromMenu = async () => {
    handleCloseMenus()
    await handleToggleFullscreen()
  }

  const handleResetChart = () => {
    handleCloseMenus()
    const chartInstance = chartInstanceRef.current
    if (!chartInstance) return

    chartInstance.setOption(renderedOption, {
      notMerge: true,
      lazyUpdate: false,
    })
    setSelectedSeries(getLegendSelectedMap(option))
  }

  const handleDownloadChart = () => {
    const chartInstance = chartInstanceRef.current
    if (!chartInstance) return

    const dataUrl = chartInstance.getDataURL({
      type: 'png',
      pixelRatio: 2,
      backgroundColor: '#ffffff',
    })

    const link = document.createElement('a')
    link.href = dataUrl
    link.download = `${getChartDownloadName(option)}.png`
    link.click()
    handleCloseMenus()
  }

  const chartMenuItems = (
    <>
      <MenuItem dense onClick={handleToggleGuide}>
        <ListItemIcon sx={{ minWidth: 28 }}>
          <PanelSidebarIcon side="right" />
        </ListItemIcon>
        <ListItemText
          primaryTypographyProps={{ variant: 'body2' }}
          primary={
            isGuideCollapsed ? 'Show right sidebar' : 'Hide right sidebar'
          }
        />
      </MenuItem>
      <MenuItem dense onClick={handleTogglePhaseInfo}>
        <ListItemIcon sx={{ minWidth: 28 }}>
          <PhaseInfoActionIcon />
        </ListItemIcon>
        <ListItemText
          primaryTypographyProps={{ variant: 'body2' }}
          primary={showPhaseInfo ? 'Hide phase info' : 'Show phase info'}
        />
      </MenuItem>
      <MenuItem dense onClick={handleToggleFullscreenFromMenu}>
        <ListItemIcon sx={{ minWidth: 28 }}>
          <FullscreenActionIcon expanded={isFullscreen} />
        </ListItemIcon>
        <ListItemText
          primaryTypographyProps={{ variant: 'body2' }}
          primary={isFullscreen ? 'Exit fullscreen' : 'Enter fullscreen'}
        />
      </MenuItem>
      <Divider />
      <MenuItem dense onClick={handleDownloadChart}>
        <ListItemIcon sx={{ minWidth: 28 }}>
          <ToolbarActionIcon />
        </ListItemIcon>
        <ListItemText
          primaryTypographyProps={{ variant: 'body2' }}
          primary="Download chart"
        />
      </MenuItem>
      <MenuItem dense onClick={handleResetChart}>
        <ListItemIcon sx={{ minWidth: 28 }}>
          <ResetActionIcon />
        </ListItemIcon>
        <ListItemText
          primaryTypographyProps={{ variant: 'body2' }}
          primary="Reset to default"
        />
      </MenuItem>
    </>
  )
  const toolbarButtons = (
    <>
      <Tooltip
        title={isGuideCollapsed ? 'Show right sidebar' : 'Hide right sidebar'}
        placement="bottom"
      >
        <IconButton
          size="small"
          onClick={handleToggleGuide}
          sx={{
            color: isGuideCollapsed ? '#64748B' : '#334155',
            p: 0.45,
            '&:hover': {
              backgroundColor: 'rgba(15, 23, 42, 0.06)',
            },
          }}
        >
          <PanelSidebarIcon side="right" />
        </IconButton>
      </Tooltip>
      <Tooltip
        title={showPhaseInfo ? 'Hide phase info' : 'Show phase info'}
        placement="bottom"
      >
        <IconButton
          size="small"
          onClick={handleTogglePhaseInfo}
          sx={{
            color: showPhaseInfo ? '#334155' : '#64748B',
            p: 0.45,
            '&:hover': {
              backgroundColor: 'rgba(15, 23, 42, 0.06)',
            },
          }}
        >
          <PhaseInfoActionIcon />
        </IconButton>
      </Tooltip>
      <Tooltip
        title={isFullscreen ? 'Exit fullscreen' : 'Enter fullscreen'}
        placement="bottom"
      >
        <IconButton
          size="small"
          onClick={handleToggleFullscreenFromMenu}
          sx={{
            color: isFullscreen ? '#334155' : '#64748B',
            p: 0.45,
            '&:hover': {
              backgroundColor: 'rgba(15, 23, 42, 0.06)',
            },
          }}
        >
          <FullscreenActionIcon expanded={isFullscreen} />
        </IconButton>
      </Tooltip>
      <Tooltip title="Download chart" placement="bottom">
        <IconButton
          size="small"
          onClick={handleDownloadChart}
          sx={{
            color: '#475569',
            p: 0.45,
            '&:hover': {
              backgroundColor: 'rgba(15, 23, 42, 0.06)',
            },
          }}
        >
          <ToolbarActionIcon />
        </IconButton>
      </Tooltip>
      <Tooltip title="Reset chart" placement="bottom">
        <IconButton
          size="small"
          onClick={handleResetChart}
          sx={{
            color: '#475569',
            p: 0.45,
            '&:hover': {
              backgroundColor: 'rgba(15, 23, 42, 0.06)',
            },
          }}
        >
          <ResetActionIcon />
        </IconButton>
      </Tooltip>
    </>
  )

  return (
    <div
      ref={fullscreenRef}
      onContextMenu={handleContextMenu}
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
        <Box
          ref={headerRef}
          sx={{
            position: 'sticky',
            top: 0,
            zIndex: 5,
            display: 'flex',
            backgroundColor: '#fff',
          }}
        >
          <Box
            sx={{
              flex: 1,
              minWidth: 0,
              px: isFullscreen
                ? `${CHART_CONTENT_PADDING + FULLSCREEN_PADDING_X}px`
                : `${CHART_CONTENT_PADDING}px`,
              py: 1.25,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              gap: 2,
              borderBottom: '1px solid',
              borderColor: 'divider',
            }}
          >
            <Box
              sx={{
                flex: '1 1 auto',
                minWidth: 0,
                display: 'flex',
                alignItems: 'baseline',
                gap: 1,
                flexWrap: 'wrap',
              }}
            >
              <Typography
                variant="h5"
                sx={{
                  fontSize: '1rem',
                  fontWeight: 700,
                  lineHeight: 1.2,
                  minWidth: 0,
                }}
              >
                {titleText || 'Time Space Diagram'}
              </Typography>
              {rangeText ? (
                <Typography
                  variant="body2"
                  sx={{
                    color: 'text.secondary',
                    fontWeight: 500,
                    lineHeight: 1.2,
                    whiteSpace: 'nowrap',
                  }}
                >
                  • {rangeText}
                </Typography>
              ) : null}
            </Box>

            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 0.25,
                flexShrink: 0,
              }}
            >
              {toolbarButtons}
            </Box>
          </Box>

          <Box
            sx={{
              width: isGuideCollapsed ? 0 : TIME_SPACE_GUIDE_WIDTH,
              minWidth: isGuideCollapsed ? 0 : TIME_SPACE_GUIDE_WIDTH,
              flexShrink: 0,
              mr: isFullscreen ? `${FULLSCREEN_PADDING_X}px` : 0,
              display: 'flex',
              alignItems: 'flex-end',
              borderLeft: isGuideCollapsed ? 'none' : '1px solid',
              borderBottom: '1px solid',
              borderColor: 'divider',
              overflow: 'hidden',
              transition: `width ${GUIDE_TRANSITION_MS}ms ${GUIDE_EASING}, min-width ${GUIDE_TRANSITION_MS}ms ${GUIDE_EASING}`,
            }}
          >
            {!isGuideCollapsed && (
              <TimeSpaceSidebarTabs
                activeTab={sidebarTab}
                onChange={setSidebarTab}
                hasLegendContent
                hasUploadContent={Boolean(sidebarUploadContent)}
                mode="header"
              />
            )}
          </Box>
        </Box>

        <div
          style={{
            display: 'flex',
            flex: 1,
            minHeight: 0,
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
              height: '100%',
              boxSizing: 'border-box',
              position: 'relative',
              display: 'flex',
              flexDirection: 'column',
            }}
          >
            {stickyTopAxis ? (
              <Box
                sx={{
                  position: 'sticky',
                  top: `${headerHeight}px`,
                  zIndex: 4,
                  height: `${STICKY_TOP_AXIS_HEIGHT}px`,
                  marginBottom: `-${STICKY_TOP_AXIS_HEIGHT}px`,
                  pointerEvents: 'none',
                  px: `${CHART_CONTENT_PADDING}px`,
                  boxSizing: 'border-box',
                  background:
                    'linear-gradient(to bottom, rgba(255,255,255,1) 72%, rgba(255,255,255,0) 100%)',
                }}
              >
                <Box
                  sx={{ position: 'relative', width: '100%', height: '100%' }}
                >
                  {stickyTopAxis.label ? (
                    <Typography
                      variant="caption"
                      sx={{
                        position: 'absolute',
                        top: 10,
                        left: `${Math.max(0, stickyTopAxis.axisStart - 12)}px`,
                        transform: 'translateX(-100%)',
                        maxWidth: `${Math.max(
                          0,
                          stickyTopAxis.axisStart - 20
                        )}px`,
                        color: 'text.secondary',
                        fontSize: '0.78rem',
                        fontWeight: 500,
                        lineHeight: 1,
                        whiteSpace: 'nowrap',
                        textAlign: 'right',
                      }}
                    >
                      {stickyTopAxis.label}
                    </Typography>
                  ) : null}
                  <Box
                    sx={{
                      position: 'absolute',
                      left: `${stickyTopAxis.axisStart}px`,
                      width: `${Math.max(
                        0,
                        stickyTopAxis.axisEnd - stickyTopAxis.axisStart
                      )}px`,
                      top: 30,
                      borderTop: '2px solid #6B7280',
                    }}
                  />
                  {stickyTopAxis.ticks.map((tick) => (
                    <Box
                      key={tick.value}
                      sx={{
                        position: 'absolute',
                        left: `${tick.left}px`,
                        top: 8,
                        transform: 'translateX(-50%)',
                        textAlign: 'center',
                      }}
                    >
                      <Typography
                        variant="caption"
                        sx={{
                          display: 'block',
                          color: 'text.primary',
                          fontSize: '0.72rem',
                          lineHeight: 1,
                          whiteSpace: 'nowrap',
                        }}
                      >
                        {tick.label}
                      </Typography>
                      <Box
                        sx={{
                          width: '1px',
                          height: '8px',
                          mt: '2px',
                          mx: 'auto',
                          bgcolor: '#6B7280',
                        }}
                      />
                    </Box>
                  ))}
                </Box>
              </Box>
            ) : null}

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
                      <Tooltip
                        key={button.location}
                        title={title}
                        placement="top"
                      >
                        <IconButton
                          size="small"
                          onClick={() =>
                            onToggleIgnoredLocation(button.location)
                          }
                          sx={{
                            pointerEvents: 'auto',
                            position: 'absolute',
                            left: `${button.left}px`,
                            top: `${button.top}px`,
                            p: 0,
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            color: isIgnored ? '#6B7280' : '#1F2937',
                            '&:hover': {
                              backgroundColor: 'rgba(15, 23, 42, 0.08)',
                            },
                          }}
                        >
                          {isIgnored ? (
                            <VisibilityOffIcon
                              sx={{
                                fontSize: `${LOCATION_TOGGLE_ICON_SIZE}px`,
                              }}
                            />
                          ) : (
                            <VisibilityIcon
                              sx={{
                                fontSize: `${LOCATION_TOGGLE_ICON_SIZE}px`,
                              }}
                            />
                          )}
                        </IconButton>
                      </Tooltip>
                    )
                  })}
                </div>
              )}
            </div>
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
                option={optionWithoutHeaderTitle}
                selectedSeries={selectedSeries}
                onToggleSeries={handleToggleSeries}
                uploadContent={sidebarUploadContent}
                activeTab={sidebarTab}
                onTabChange={setSidebarTab}
                showTabs={false}
              />
            )}
          </div>
        </div>

        <Menu
          open={contextMenuPosition !== null}
          onClose={handleCloseMenus}
          container={fullscreenRef.current}
          disablePortal={isFullscreen}
          disableScrollLock
          anchorReference="anchorPosition"
          MenuListProps={{ dense: true, sx: { py: 0.5 } }}
          anchorPosition={
            contextMenuPosition
              ? {
                  top: contextMenuPosition.mouseY,
                  left: contextMenuPosition.mouseX,
                }
              : undefined
          }
          transformOrigin={{ horizontal: 'left', vertical: 'top' }}
          anchorOrigin={{ horizontal: 'left', vertical: 'top' }}
        >
          {chartMenuItems}
        </Menu>
      </div>
    </div>
  )
}
