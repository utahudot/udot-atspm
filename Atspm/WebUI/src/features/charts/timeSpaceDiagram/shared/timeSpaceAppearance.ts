import { Color } from '@/features/charts/utils'
import {
  CYCLE_INDICATIONS,
  TIME_SPACE_CONTINUATION_NODE_NAME,
} from './transformers/timeSpaceTransformerBase'
import type { CustomSeriesRenderItemReturn, EChartsOption, SeriesOption } from 'echarts'

export type TimeSpaceAppearanceDirectionRole = 'primary' | 'opposing'

export type TimeSpaceDirectionalAppearance = {
  color: string
  opacity: number
}

export type TimeSpaceAppearanceSettings = {
  cycles: {
    opacity: number
    indicationColors: {
      beginGreen: string
      trailingGreen: string
      yellowClearance: string
      redClearance: string
      redIndication: string
    }
  }
  greenBands: Record<TimeSpaceAppearanceDirectionRole, TimeSpaceDirectionalAppearance>
  turns: {
    leftTurn: TimeSpaceDirectionalAppearance
    rightTurn: TimeSpaceDirectionalAppearance
  }
  detection: {
    laneByLaneCount: Record<
      TimeSpaceAppearanceDirectionRole,
      TimeSpaceDirectionalAppearance
    >
    advanceCount: Record<
      TimeSpaceAppearanceDirectionRole,
      TimeSpaceDirectionalAppearance
    >
    stopBarPresence: Record<
      TimeSpaceAppearanceDirectionRole,
      TimeSpaceDirectionalAppearance
    >
  }
  tspRequest: TimeSpaceDirectionalAppearance
  tspService: TimeSpaceDirectionalAppearance
}

const TIME_SPACE_DEFAULT_DETECTION_PRIMARY: TimeSpaceDirectionalAppearance = {
  color: '#00008B',
  opacity: 0.6,
}

const TIME_SPACE_DEFAULT_DETECTION_OPPOSING: TimeSpaceDirectionalAppearance = {
  color: '#FFA500',
  opacity: 0.75,
}

export const TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS: TimeSpaceAppearanceSettings =
  {
    cycles: {
      opacity: 1,
      indicationColors: {
        beginGreen: CYCLE_INDICATIONS[0]?.color ?? Color.Green,
        trailingGreen: CYCLE_INDICATIONS[1]?.color ?? '#79DE79',
        yellowClearance: CYCLE_INDICATIONS[2]?.color ?? Color.Yellow,
        redClearance: CYCLE_INDICATIONS[3]?.color ?? '#FB6962',
        redIndication: CYCLE_INDICATIONS[4]?.color ?? '#B34747',
      },
    },
    greenBands: {
      primary: { color: '#4F9BAC', opacity: 0.3 },
      opposing: { color: '#202D30', opacity: 0.2 },
    },
    turns: {
      leftTurn: { color: Color.Black, opacity: 1 },
      rightTurn: { color: Color.Black, opacity: 1 },
    },
    detection: {
      laneByLaneCount: {
        primary: { ...TIME_SPACE_DEFAULT_DETECTION_PRIMARY },
        opposing: { ...TIME_SPACE_DEFAULT_DETECTION_OPPOSING },
      },
      advanceCount: {
        primary: { ...TIME_SPACE_DEFAULT_DETECTION_PRIMARY },
        opposing: { ...TIME_SPACE_DEFAULT_DETECTION_OPPOSING },
      },
      stopBarPresence: {
        primary: { ...TIME_SPACE_DEFAULT_DETECTION_PRIMARY },
        opposing: { ...TIME_SPACE_DEFAULT_DETECTION_OPPOSING },
      },
    },
    tspRequest: {
      color: Color.Black,
      opacity: 0.95,
    },
    tspService: {
      color: Color.Black,
      opacity: 0.95,
    },
  }

export function createDefaultTimeSpaceAppearanceSettings(): TimeSpaceAppearanceSettings {
  return {
    cycles: {
      opacity: TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.cycles.opacity,
      indicationColors: {
        ...TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.cycles.indicationColors,
      },
    },
    greenBands: {
      primary: { ...TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.greenBands.primary },
      opposing: {
        ...TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.greenBands.opposing,
      },
    },
    turns: {
      leftTurn: { ...TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.turns.leftTurn },
      rightTurn: { ...TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.turns.rightTurn },
    },
    detection: {
      laneByLaneCount: {
        primary: {
          ...TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.detection.laneByLaneCount
            .primary,
        },
        opposing: {
          ...TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.detection.laneByLaneCount
            .opposing,
        },
      },
      advanceCount: {
        primary: {
          ...TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.detection.advanceCount
            .primary,
        },
        opposing: {
          ...TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.detection.advanceCount
            .opposing,
        },
      },
      stopBarPresence: {
        primary: {
          ...TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.detection.stopBarPresence
            .primary,
        },
        opposing: {
          ...TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.detection.stopBarPresence
            .opposing,
        },
      },
    },
    tspRequest: { ...TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.tspRequest },
    tspService: { ...TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.tspService },
  }
}

function isSeriesOption(
  value: SeriesOption | null | undefined
): value is SeriesOption {
  return Boolean(value && typeof value === 'object')
}

function getSeriesName(series: SeriesOption): string | null {
  return typeof series.name === 'string' && series.name.trim()
    ? series.name.trim()
    : null
}

function normalizeColorToken(value: unknown) {
  return typeof value === 'string' ? value.trim().toLowerCase() : null
}

function mapGraphicNode(
  node: unknown,
  visitor: (value: Record<string, unknown>) => Record<string, unknown>
): unknown {
  if (Array.isArray(node)) {
    return node.map((entry) => mapGraphicNode(entry, visitor))
  }

  if (!node || typeof node !== 'object') {
    return node
  }

  const nextNode = { ...(node as Record<string, unknown>) }

  if (Array.isArray(nextNode.children)) {
    nextNode.children = nextNode.children.map((child) =>
      mapGraphicNode(child, visitor)
    )
  }

  return visitor(nextNode)
}

function wrapCustomRenderItem(
  series: SeriesOption,
  mapResult: (value: CustomSeriesRenderItemReturn) => CustomSeriesRenderItemReturn
) {
  const renderItem = (series as SeriesOption & {
    renderItem?: (...args: unknown[]) => CustomSeriesRenderItemReturn
  }).renderItem

  if (typeof renderItem !== 'function') {
    return series
  }

  return {
    ...series,
    renderItem: (...args: Parameters<typeof renderItem>) =>
      mapResult(renderItem(...args)),
  }
}

function applyLineAppearance(
  series: SeriesOption,
  appearance: TimeSpaceDirectionalAppearance
) {
  return {
    ...series,
    color: appearance.color,
    lineStyle: {
      ...(series.lineStyle as Record<string, unknown> | undefined),
      color: appearance.color,
      opacity: appearance.opacity,
    },
    itemStyle: {
      ...(series.itemStyle as Record<string, unknown> | undefined),
      color: appearance.color,
      opacity: appearance.opacity,
    },
  }
}

function applyCyclesAppearance(
  series: SeriesOption,
  appearance: TimeSpaceAppearanceSettings['cycles']
) {
  const cycleColorMap = new Map<string, string>([
    [
      normalizeColorToken(TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.cycles.indicationColors.beginGreen) ??
        '',
      appearance.indicationColors.beginGreen,
    ],
    [
      normalizeColorToken(
        TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.cycles.indicationColors
          .trailingGreen
      ) ?? '',
      appearance.indicationColors.trailingGreen,
    ],
    [
      normalizeColorToken(
        TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.cycles.indicationColors
          .yellowClearance
      ) ?? '',
      appearance.indicationColors.yellowClearance,
    ],
    [
      normalizeColorToken(
        TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.cycles.indicationColors
          .redClearance
      ) ?? '',
      appearance.indicationColors.redClearance,
    ],
    [
      normalizeColorToken(
        TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS.cycles.indicationColors
          .redIndication
      ) ?? '',
      appearance.indicationColors.redIndication,
    ],
  ])

  return wrapCustomRenderItem(series, (result) =>
    mapGraphicNode(result, (node) => {
      const fill = normalizeColorToken(node?.style?.fill)
      const nextFill = fill ? cycleColorMap.get(fill) : undefined

      if (!nextFill) {
        return node
      }

      return {
        ...node,
        style: {
          ...(node.style as Record<string, unknown> | undefined),
          fill: nextFill,
          opacity: appearance.opacity,
        },
      }
    }) as CustomSeriesRenderItemReturn
  )
}

function applyGraphicFillAppearance(
  series: SeriesOption,
  appearance: TimeSpaceDirectionalAppearance,
  options?: {
    includeFillOpacity?: boolean
    forceOpacity?: number
  }
) {
  return wrapCustomRenderItem(series, (result) =>
    mapGraphicNode(result, (node) => {
      if (!node?.style) {
        return node
      }

      if (node.name === TIME_SPACE_CONTINUATION_NODE_NAME) {
        return node
      }

      return {
        ...node,
        style: {
          ...(node.style as Record<string, unknown> | undefined),
          fill: appearance.color,
          ...(options?.includeFillOpacity
            ? { fillOpacity: appearance.opacity }
            : null),
          opacity: options?.forceOpacity ?? appearance.opacity,
        },
      }
    }) as CustomSeriesRenderItemReturn
  )
}

export function applyTimeSpaceAppearanceToOption(
  option: EChartsOption,
  appearance: TimeSpaceAppearanceSettings,
  directionRoleBySeriesName: Map<string, TimeSpaceAppearanceDirectionRole>
): EChartsOption {
  if (!option?.series) {
    return option
  }

  const applySeriesAppearance = (series: SeriesOption) => {
    const name = getSeriesName(series)
    if (!name) {
      return series
    }

    if (name.startsWith('Cycles ')) {
      return applyCyclesAppearance(series, appearance.cycles)
    }

    if (name.startsWith('Green Bands ')) {
      const role = directionRoleBySeriesName.get(name)
      return role
        ? applyGraphicFillAppearance(series, appearance.greenBands[role])
        : series
    }

    if (name.startsWith('Lane by Lane Count ')) {
      const role = directionRoleBySeriesName.get(name)
      return role
        ? applyLineAppearance(series, appearance.detection.laneByLaneCount[role])
        : series
    }

    if (name.startsWith('Advance Count ')) {
      const role = directionRoleBySeriesName.get(name)
      return role
        ? applyLineAppearance(series, appearance.detection.advanceCount[role])
        : series
    }

    if (name.startsWith('Left Turn ')) {
      return applyLineAppearance(series, appearance.turns.leftTurn)
    }

    if (name.startsWith('Right Turn ')) {
      return applyLineAppearance(series, appearance.turns.rightTurn)
    }

    if (name.startsWith('Stop Bar Presence ')) {
      const role = directionRoleBySeriesName.get(name)
      return role
        ? applyGraphicFillAppearance(series, appearance.detection.stopBarPresence[role], {
            includeFillOpacity: true,
            forceOpacity: 1,
          })
        : series
    }

    if (name === 'TSP Request (112-115)') {
      return series.type === 'custom'
        ? applyGraphicFillAppearance(series, appearance.tspRequest)
        : applyLineAppearance(series, appearance.tspRequest)
    }

    if (name === 'TSP Service (118-119)') {
      return series.type === 'custom'
        ? applyGraphicFillAppearance(series, appearance.tspService)
        : applyLineAppearance(series, appearance.tspService)
    }

    return series
  }

  return {
    ...option,
    series: Array.isArray(option.series)
      ? option.series.map((series) =>
          isSeriesOption(series) ? applySeriesAppearance(series) : series
        )
      : isSeriesOption(option.series)
        ? applySeriesAppearance(option.series)
        : option.series,
  }
}
