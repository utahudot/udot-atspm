import type { DataZoomComponentOption, ECharts, EChartsOption } from 'echarts'

export type TopAxisConfig = {
  formatter?: ((value: number) => string) | string
  index: number
  interval: number
  label: string
  max: number
  min: number
}

export type StickyTopAxis = {
  axisEnd: number
  axisStart: number
  label: string
  ticks: Array<{
    label: string
    left: number
    value: number
  }>
}

export type StickyBottomAxis = Pick<
  StickyTopAxis,
  'axisEnd' | 'axisStart' | 'label'
>

export type BottomAxisConfig = {
  axis: Record<string, unknown>
  label: string
  sliderDataZoom?: DataZoomComponentOption
}

const STICKY_TOP_AXIS_HEIGHT = 42
const STICKY_AXIS_BACKGROUND_SOLID_HEIGHT = Math.round(
  STICKY_TOP_AXIS_HEIGHT * 0.72
)
const STICKY_AXIS_BACKGROUND_FADE_HEIGHT = STICKY_TOP_AXIS_HEIGHT
const STICKY_AXIS_BACKGROUND_TRANSITION_HEIGHT =
  STICKY_AXIS_BACKGROUND_FADE_HEIGHT - STICKY_AXIS_BACKGROUND_SOLID_HEIGHT

export const STICKY_BOTTOM_AXIS_HEIGHT = 76
const STICKY_BOTTOM_AXIS_BOTTOM = 60
const STICKY_BOTTOM_AXIS_TOP =
  STICKY_BOTTOM_AXIS_HEIGHT - STICKY_BOTTOM_AXIS_BOTTOM
const STICKY_BOTTOM_SLIDER_HEIGHT = 25
const STICKY_BOTTOM_SLIDER_BOTTOM =
  STICKY_BOTTOM_AXIS_BOTTOM -
  STICKY_BOTTOM_SLIDER_HEIGHT -
  STICKY_AXIS_BACKGROUND_TRANSITION_HEIGHT -
  10
export const STICKY_BOTTOM_AXIS_LABEL_OVERFLOW = 28
const STICKY_BOTTOM_SLIDER_SIDE_INSET = 2
export const STICKY_BOTTOM_PANEL_BOTTOM = 0
const STICKY_BOTTOM_BACKGROUND_FADE_START = Math.max(
  0,
  STICKY_BOTTOM_AXIS_TOP - STICKY_AXIS_BACKGROUND_TRANSITION_HEIGHT
)
const STICKY_BOTTOM_BACKGROUND_FADE_END = STICKY_BOTTOM_AXIS_TOP
export const STICKY_BOTTOM_LABEL_TOP = STICKY_BOTTOM_AXIS_TOP + 7

export const STICKY_AXIS_LABEL_TEXT_STYLE = {
  color: 'text.secondary',
  fontSize: '0.78rem',
  fontWeight: 500,
  lineHeight: 1,
  whiteSpace: 'nowrap',
  pointerEvents: 'none',
} as const

export function getStickyTopAxisHeight() {
  return STICKY_TOP_AXIS_HEIGHT
}

export function getStickyAxisBackground(direction: 'top' | 'bottom') {
  if (direction === 'top') {
    return `linear-gradient(to bottom, rgba(255,255,255,1) 0px, rgba(255,255,255,1) ${STICKY_AXIS_BACKGROUND_SOLID_HEIGHT}px, rgba(255,255,255,0) ${STICKY_AXIS_BACKGROUND_FADE_HEIGHT}px, rgba(255,255,255,0) 100%)`
  }

  return `linear-gradient(to bottom, rgba(255,255,255,0) 0px, rgba(255,255,255,0) ${STICKY_BOTTOM_BACKGROUND_FADE_START}px, rgba(255,255,255,1) ${STICKY_BOTTOM_BACKGROUND_FADE_END}px, rgba(255,255,255,1) 100%)`
}

export function extractTopAxisConfig(option?: EChartsOption): TopAxisConfig | null {
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

export function stripTopAxisVisuals(option?: EChartsOption): EChartsOption['xAxis'] {
  if (!option?.xAxis) return option?.xAxis

  const stripAxisName = <T,>(axis: T): T => {
    if (!axis || typeof axis !== 'object') return axis

    const candidate = axis as T & { position?: unknown }
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

export function extractBottomAxisConfig(
  option?: EChartsOption
): BottomAxisConfig | null {
  if (!option?.xAxis) return null

  const xAxes = Array.isArray(option.xAxis) ? option.xAxis : [option.xAxis]
  const bottomAxis = xAxes.find((axis) => {
    if (!axis || typeof axis !== 'object') return false

    const candidate = axis as { position?: unknown; show?: unknown }
    return candidate.position !== 'top' && candidate.show !== false
  })

  if (!bottomAxis || typeof bottomAxis !== 'object') {
    return null
  }

  const dataZooms = option.dataZoom
    ? Array.isArray(option.dataZoom)
      ? option.dataZoom
      : [option.dataZoom]
    : []

  const sliderDataZoom = dataZooms.find((zoom) => {
    if (!zoom || typeof zoom !== 'object') return false

    const candidate = zoom as DataZoomComponentOption
    return (
      candidate.type === 'slider' &&
      (candidate.orient ?? 'horizontal') === 'horizontal'
    )
  }) as DataZoomComponentOption | undefined

  return {
    axis: bottomAxis as Record<string, unknown>,
    label: typeof bottomAxis.name === 'string' ? bottomAxis.name.trim() : '',
    sliderDataZoom,
  }
}

export function stripBottomAxisVisuals(
  option?: EChartsOption
): EChartsOption['xAxis'] {
  if (!option?.xAxis) return option?.xAxis

  let hasStrippedBottomAxis = false

  const stripAxis = <T,>(axis: T): T => {
    if (!axis || typeof axis !== 'object') return axis

    const candidate = axis as T & { position?: unknown; show?: unknown }
    if (
      hasStrippedBottomAxis ||
      candidate.position === 'top' ||
      candidate.show === false
    ) {
      return axis
    }

    hasStrippedBottomAxis = true

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
    ? option.xAxis.map((axis) => stripAxis(axis))
    : stripAxis(option.xAxis)
}

export function stripSliderDataZoomVisuals(
  option?: EChartsOption
): EChartsOption['dataZoom'] {
  if (!option?.dataZoom) return option?.dataZoom

  const hideSlider = <T,>(zoom: T): T => {
    if (!zoom || typeof zoom !== 'object') return zoom

    const candidate = zoom as T & {
      type?: unknown
      orient?: unknown
      show?: unknown
    }

    if (
      candidate.type !== 'slider' ||
      (candidate.orient ?? 'horizontal') !== 'horizontal'
    ) {
      return zoom
    }

    return {
      ...candidate,
      show: false,
    } as T
  }

  return Array.isArray(option.dataZoom)
    ? option.dataZoom.map((zoom) => hideSlider(zoom))
    : hideSlider(option.dataZoom)
}

export function getHorizontalSliderZoomState(option?: EChartsOption) {
  const dataZooms = option?.dataZoom
    ? Array.isArray(option.dataZoom)
      ? option.dataZoom
      : [option.dataZoom]
    : []

  const slider = dataZooms.find((zoom) => {
    if (!zoom || typeof zoom !== 'object') return false

    const candidate = zoom as DataZoomComponentOption
    return (
      candidate.type === 'slider' &&
      (candidate.orient ?? 'horizontal') === 'horizontal'
    )
  }) as DataZoomComponentOption | undefined

  if (
    !slider ||
    typeof slider.start !== 'number' ||
    typeof slider.end !== 'number'
  ) {
    return null
  }

  return {
    start: slider.start,
    end: slider.end,
  }
}

export function buildStickyBottomAxisOption(
  bottomAxisConfig?: BottomAxisConfig | null
): EChartsOption | null {
  if (!bottomAxisConfig) return null

  const { axis, sliderDataZoom } = bottomAxisConfig
  const axisMin = axis.min
  const axisMax = axis.max

  return {
    animation: false,
    grid: {
      left: STICKY_BOTTOM_AXIS_LABEL_OVERFLOW,
      right: STICKY_BOTTOM_AXIS_LABEL_OVERFLOW,
      top: 0,
      bottom: STICKY_BOTTOM_AXIS_BOTTOM,
      containLabel: false,
    },
    xAxis: {
      ...axis,
      name: '',
      position: 'bottom',
      show: true,
      axisLine: {
        show: true,
        ...(typeof axis.axisLine === 'object' ? axis.axisLine : {}),
      },
      axisTick: {
        show: true,
        ...(typeof axis.axisTick === 'object' ? axis.axisTick : {}),
      },
      axisLabel: {
        show: true,
        ...(typeof axis.axisLabel === 'object' ? axis.axisLabel : {}),
      },
      splitLine: { show: false },
      minorSplitLine: { show: false },
    },
    yAxis: {
      type: 'value',
      min: 0,
      max: 1,
      show: false,
    },
    dataZoom: sliderDataZoom
      ? [
          {
            ...sliderDataZoom,
            type: 'slider',
            orient: 'horizontal',
            show: true,
            left:
              STICKY_BOTTOM_AXIS_LABEL_OVERFLOW +
              STICKY_BOTTOM_SLIDER_SIDE_INSET,
            right:
              STICKY_BOTTOM_AXIS_LABEL_OVERFLOW +
              STICKY_BOTTOM_SLIDER_SIDE_INSET,
            height: STICKY_BOTTOM_SLIDER_HEIGHT,
            bottom: STICKY_BOTTOM_SLIDER_BOTTOM,
            showDetail: false,
          },
        ]
      : [],
    series: [
      {
        type: 'line',
        symbol: 'none',
        silent: true,
        lineStyle: { opacity: 0 },
        data:
          axisMin !== undefined && axisMax !== undefined
            ? [
                [axisMin, 0],
                [axisMax, 0],
              ]
            : [],
      },
    ],
  }
}

export function formatTopAxisTickLabel(
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

export function getAxisPixel(
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

export function getAxisValueFromPixel(
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
