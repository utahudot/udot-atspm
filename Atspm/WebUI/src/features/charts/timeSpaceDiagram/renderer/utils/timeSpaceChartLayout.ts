import {
  CYCLE_LABEL_SERIES_ID_PREFIX,
  TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT,
} from '@/features/charts/timeSpaceDiagram/shared/transformers/timeSpaceTransformerBase'
import type {
  EChartsOption,
  GridComponentOption,
  LegendComponentOption,
  SeriesOption,
  ToolboxComponentOption,
} from 'echarts'

type LegendSelectionProvider = LegendComponentOption & {
  selected?: Record<string, boolean>
}

const MIN_RIGHT_PLOT_GUTTER = 10

export const GUIDE_TRANSITION_MS = 200
export const GUIDE_EASING = 'cubic-bezier(0.2, 0, 0, 1)'
export const CHART_CONTENT_PADDING = 16
export const FULLSCREEN_PADDING_X = 24
export const FULLSCREEN_PADDING_BOTTOM = 24
export const TIME_SPACE_LABEL_GUTTER_WIDTH =
  TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT.cardWidth * 2 +
  TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT.cardGapFromPlot +
  TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT.cardGapBetween +
  12

function getPrimaryLegend(
  option?: EChartsOption
): LegendSelectionProvider | undefined {
  if (!option?.legend) return undefined

  return Array.isArray(option.legend)
    ? (option.legend[0] as LegendSelectionProvider | undefined)
    : (option.legend as LegendSelectionProvider)
}

export function getCssLength(value: string | number | undefined) {
  if (typeof value === 'number') {
    return `${value}px`
  }

  if (typeof value === 'string' && value.trim()) {
    return value
  }

  return undefined
}

export function buildChartOptionWithSidebar(
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
