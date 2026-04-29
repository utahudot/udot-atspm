import type { ECharts, EChartsOption, LegendComponentOption } from 'echarts'
import type { TimeSpaceRendererDirectionRole } from '../types/timeSpaceRenderer.types'

type LegendSelectionProvider = LegendComponentOption & {
  selected?: Record<string, boolean>
}

const CYCLE_PREFIX = 'Cycles '
const CYCLE_DURATION_PREFIX = 'Cycle Durations '

function getPrimaryLegend(
  option?: EChartsOption
): LegendSelectionProvider | undefined {
  if (!option?.legend) return undefined

  return Array.isArray(option.legend)
    ? (option.legend[0] as LegendSelectionProvider | undefined)
    : (option.legend as LegendSelectionProvider)
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

export function getLegendSelectedMap(
  option?: EChartsOption
): Record<string, boolean> {
  const selected = getPrimaryLegend(option)?.selected
  return selected ? { ...selected } : {}
}

export function withSyntheticLegendEntry(
  option: EChartsOption | undefined,
  name: string,
  selectedByDefault = true
) {
  if (!option?.legend) {
    return option
  }

  const addLegendEntry = (legend: LegendSelectionProvider) => {
    const legendData = Array.isArray(legend.data) ? legend.data : []
    const hasEntry = legendData.some(
      (entry) => getLegendEntryName(entry) === name
    )

    if (hasEntry) {
      return legend
    }

    return {
      ...legend,
      data: [...legendData, name],
      selected: {
        ...(legend.selected ?? {}),
        [name]: selectedByDefault,
      },
    }
  }

  return {
    ...option,
    legend: Array.isArray(option.legend)
      ? option.legend.map((legend, index) =>
          index === 0
            ? addLegendEntry(legend as LegendSelectionProvider)
            : legend
        )
      : addLegendEntry(option.legend as LegendSelectionProvider),
  }
}

export function getRequestedLegendVisibility(
  seriesName: string,
  requestedSelections: Record<string, boolean>,
  suppressedDirections: Partial<
    Record<TimeSpaceRendererDirectionRole, boolean>
  >,
  directionRoleBySeriesName: Map<string, TimeSpaceRendererDirectionRole>
) {
  const directionRole = directionRoleBySeriesName.get(seriesName)
  const isSuppressed =
    directionRole != null && suppressedDirections[directionRole] === true
  let shouldBeVisible =
    requestedSelections[seriesName] !== false && !isSuppressed

  const direction = getDirectionalSuffix(seriesName, CYCLE_DURATION_PREFIX)
  if (direction) {
    const cycleName = `${CYCLE_PREFIX}${direction}`
    const cycleDirectionRole = directionRoleBySeriesName.get(cycleName)
    const isCycleSuppressed =
      cycleDirectionRole != null &&
      suppressedDirections[cycleDirectionRole] === true

    shouldBeVisible =
      shouldBeVisible &&
      requestedSelections[cycleName] !== false &&
      !isCycleSuppressed
  }

  return shouldBeVisible
}

export function syncRequestedLegendSelections(
  chart: ECharts,
  requestedSelections: Record<string, boolean>,
  suppressedDirections: Partial<
    Record<TimeSpaceRendererDirectionRole, boolean>
  >,
  directionRoleBySeriesName: Map<string, TimeSpaceRendererDirectionRole>
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
    const shouldBeVisible = getRequestedLegendVisibility(
      name,
      requestedSelections,
      suppressedDirections,
      directionRoleBySeriesName
    )

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
