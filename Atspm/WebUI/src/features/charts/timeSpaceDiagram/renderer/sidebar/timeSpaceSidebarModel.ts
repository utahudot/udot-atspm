// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceSidebarModel.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
import { Color } from '@/features/charts/utils'
import type {
  TimeSpaceRendererDirectionRole,
} from '@/features/charts/timeSpaceDiagram/renderer/types/timeSpaceRenderer.types'
import type { EChartsOption, SeriesOption } from 'echarts'
import type { TimeSpaceAppearanceSettings } from '../../shared/timeSpaceAppearance'
import { TIME_SPACE_GPX_TRACKS_LEGEND_NAME } from '../../shared/types'

export type PreviewKind =
  | 'cycles'
  | 'cycle-durations'
  | 'green-bands'
  | 'detector-line'
  | 'stop-bar'
  | 'pedestrian'
  | 'left-turn'
  | 'right-turn'
  | 'gpx-track'
  | 'srm'
  | 'triangle'
  | 'circle'
  | 'tsp-request'
  | 'tsp-service'

export type SidebarDirectionRole = TimeSpaceRendererDirectionRole

export type SidebarToggle = {
  label: string
  seriesName: string
  directionRole?: SidebarDirectionRole
}

export type SidebarDetail = {
  color?: string
  glyph?: 'solid-line' | 'dotted-line' | 'zigzag-line'
  label: string
}

export type SidebarItem = {
  key: string
  label: string
  category: string
  description: string
  note?: string
  unavailableReason?: string
  details?: SidebarDetail[]
  preview: PreviewKind
  control?: 'directional' | 'visibility'
  toggles: SidebarToggle[]
}

type LegendLike = {
  data?: Array<string | { name?: string }>
}

type SidebarItemDefinition = {
  key: string
  label: string
  category: string
  description: string
  details?: SidebarDetail[]
  preview: PreviewKind
  control?: 'directional' | 'visibility'
  match: (name: string) => string | null
}

export type SidebarDirectionControl = {
  role: SidebarDirectionRole
  label: string
  seriesNames: string[]
}

export type SidebarLegendModel = {
  directionControls: SidebarDirectionControl[]
  items: SidebarItem[]
}

type SidebarAvailabilityKey =
  | 'gpx-tracks'
  | 'srm-entity-continuous'
  | 'srm-entity-gap'

export const CATEGORY_ORDER = [
  'Signal Timing',
  'Pedestrian',
  'Detection',
  'Movements & Tracks',
  'Transit Priority',
] as const

const ITEM_NO_DATA_MESSAGES: Record<string, string> = {
  cycles: 'No cycles found.',
  'cycle-durations': 'No cycle durations found.',
  'green-bands': 'No green bands found.',
  'lane-by-lane-count': 'No lane-by-lane counts found.',
  'advance-count': 'No advance counts found.',
  'stop-bar-presence': 'No stop-bar presence data found.',
  'pedestrian-interval': 'No pedestrian intervals found.',
  'left-turn': 'No left turns found.',
  'right-turn': 'No right turns found.',
  'gpx-tracks': 'No GPX tracks found.',
  'srm-entity-continuous': 'No SRM collections found.',
  'srm-entity-gap': 'No SRM estimated trajectories found.',
  'early-green': 'No early greens found.',
  'extend-green': 'No extend greens found.',
  'tsp-request': 'No transit-signal priority requests found.',
  'tsp-service': 'No transit-signal priority service events found.',
}

const CATEGORY_NO_DATA_MESSAGES: Record<
  (typeof CATEGORY_ORDER)[number],
  string
> = {
  'Signal Timing': 'No signal timing data found.',
  Pedestrian: 'No pedestrian data found.',
  Detection: 'No detection data found.',
  'Movements & Tracks': 'No movement or track data found.',
  'Transit Priority': 'No transit-signal priority data found.',
}

const STYLABLE_ITEM_KEYS = new Set([
  'cycles',
  'green-bands',
  'left-turn',
  'right-turn',
  'lane-by-lane-count',
  'advance-count',
  'stop-bar-presence',
  'tsp-request',
  'tsp-service',
])

const SIDEBAR_ITEM_DEFINITIONS: SidebarItemDefinition[] = [
  {
    key: 'cycles',
    label: 'Cycles',
    category: 'Signal Timing',
    description:
      'Horizontal phase-state band showing how each cycle progresses through the signal indications.',
    details: [
      { color: Color.Green, label: 'Early green' },
      { color: '#8ef08d', label: 'Green phase' },
      { color: Color.Yellow, label: 'Yellow clearance' },
      { color: '#FF0000', label: 'Red clearance' },
      { color: '#f0807f', label: 'Red indication' },
    ],
    preview: 'cycles',
    match: (name) => matchDirectionalPrefix(name, 'Cycles'),
  },
  {
    key: 'cycle-durations',
    label: 'Cycle Durations',
    category: 'Signal Timing',
    description:
      'Centered labels showing each cycle segment duration in seconds.',
    preview: 'cycle-durations',
    control: 'visibility',
    match: (name) => matchDirectionalPrefix(name, 'Cycle Durations'),
  },
  {
    key: 'green-bands',
    label: 'Green Bands',
    category: 'Signal Timing',
    description:
      'Diagonal translucent windows showing where corridor progression should arrive on green.',
    preview: 'green-bands',
    match: (name) => matchDirectionalPrefix(name, 'Green Bands'),
  },
  {
    key: 'lane-by-lane-count',
    label: 'Lane-by-Lane Count',
    category: 'Detection',
    description: 'Lane-level count detector traces for each corridor direction.',
    preview: 'detector-line',
    match: (name) => matchDirectionalPrefix(name, 'Lane by Lane Count'),
  },
  {
    key: 'advance-count',
    label: 'Advance Count',
    category: 'Detection',
    description:
      'Advance detector count traces upstream of the stop bar for each corridor direction.',
    preview: 'detector-line',
    match: (name) => matchDirectionalPrefix(name, 'Advance Count'),
  },
  {
    key: 'stop-bar-presence',
    label: 'Stop Bar Presence',
    category: 'Detection',
    description:
      'Presence detector occupancy near the stop bar, shown as occupancy traces.',
    preview: 'stop-bar',
    match: (name) => matchDirectionalPrefix(name, 'Stop Bar Presence'),
  },
  {
    key: 'pedestrian-interval',
    label: 'Pedestrian Interval',
    category: 'Pedestrian',
    description:
      'Pedestrian state line showing the pedestrian interval and transition marks.',
    details: [
      { glyph: 'solid-line', label: 'Walk' },
      { glyph: 'dotted-line', label: 'Clearance' },
      { glyph: 'zigzag-line', label: "Don't walk" },
    ],
    preview: 'pedestrian',
    match: (name) => matchDirectionalPrefix(name, 'Pedestrian Interval'),
  },
  {
    key: 'left-turn',
    label: 'Left Turn',
    category: 'Movements & Tracks',
    description: 'Left-turn movement trace projected through the corridor.',
    preview: 'left-turn',
    match: (name) => matchDirectionalPrefix(name, 'Left Turn'),
  },
  {
    key: 'right-turn',
    label: 'Right Turn',
    category: 'Movements & Tracks',
    description: 'Right-turn movement trace projected through the corridor.',
    preview: 'right-turn',
    match: (name) => matchDirectionalPrefix(name, 'Right Turn'),
  },
  {
    key: 'gpx-tracks',
    label: TIME_SPACE_GPX_TRACKS_LEGEND_NAME,
    category: 'Movements & Tracks',
    description:
      'Uploaded GPX traces mapped to the corridor and drawn through the corridor.',
    preview: 'gpx-track',
    control: 'visibility',
    match: (name) => (name === TIME_SPACE_GPX_TRACKS_LEGEND_NAME ? '' : null),
  },
  {
    key: 'srm-entity-continuous',
    label: 'SRM Collection',
    category: 'Movements & Tracks',
    description:
      'SRM or connected-vehicle entity tracks drawn through the corridor.',
    preview: 'srm',
    match: (name) => matchDirectionalPrefix(name, 'SRM Collection'),
  },
  {
    key: 'srm-entity-gap',
    label: 'SRM Estimated Trajectory',
    category: 'Movements & Tracks',
    description:
      'Dotted connector between SRM entity track points separated by a location gap.',
    preview: 'srm',
    match: (name) => matchDirectionalPrefix(name, 'SRM Estimated Trajectory'),
  },
  {
    key: 'early-green',
    label: 'Early Green',
    category: 'Transit Priority',
    description: 'Circle marker for an early-green priority event.',
    preview: 'circle',
    control: 'visibility',
    match: (name) => (name.startsWith('Early Green') ? '' : null),
  },
  {
    key: 'extend-green',
    label: 'Extend Green',
    category: 'Transit Priority',
    description: 'Triangle marker for a green-extension priority event.',
    preview: 'triangle',
    control: 'visibility',
    match: (name) => (name.startsWith('Extend Green') ? '' : null),
  },
  {
    key: 'tsp-request',
    label: 'TSP Request',
    category: 'Transit Priority',
    description: 'Transit signal priority request interval.',
    preview: 'tsp-request',
    control: 'visibility',
    match: (name) => (name.startsWith('TSP Request') ? '' : null),
  },
  {
    key: 'tsp-service',
    label: 'TSP Service',
    category: 'Transit Priority',
    description:
      'Thicker transit signal priority service interval overlaid on the request interval.',
    preview: 'tsp-service',
    control: 'visibility',
    match: (name) => (name.startsWith('TSP Service') ? '' : null),
  },
]

function getPrimaryLegend(option?: EChartsOption): LegendLike | undefined {
  if (!option?.legend) return undefined
  return Array.isArray(option.legend)
    ? (option.legend[0] as LegendLike | undefined)
    : (option.legend as LegendLike)
}

function isSeriesOption(
  value: SeriesOption | null | undefined
): value is SeriesOption {
  return Boolean(value && typeof value === 'object')
}

function getAllSeries(option?: EChartsOption): SeriesOption[] {
  if (!option?.series) {
    return []
  }

  return (Array.isArray(option.series) ? option.series : [option.series]).filter(
    isSeriesOption
  )
}

function getSeriesName(series: SeriesOption): string | null {
  return typeof series.name === 'string' && series.name.trim()
    ? series.name.trim()
    : null
}

function hasRenderableSeriesData(series: SeriesOption): boolean {
  if (!Array.isArray(series.data)) {
    return false
  }

  return series.data.some((entry) => entry != null)
}

function hasSeriesDataForDefinition(
  option: EChartsOption | undefined,
  definition: SidebarItemDefinition
) {
  return getAllSeries(option).some((series) => {
    const name = getSeriesName(series)
    if (!name || definition.match(name) === null) {
      return false
    }

    return hasRenderableSeriesData(series)
  })
}

export type SidebarAvailabilityOverrides = Partial<
  Record<SidebarAvailabilityKey, boolean>
>

function isAvailabilityOverrideKey(key: string): key is SidebarAvailabilityKey {
  return (
    key === 'gpx-tracks' ||
    key === 'srm-entity-continuous' ||
    key === 'srm-entity-gap'
  )
}

function hasSidebarItemData(
  definition: SidebarItemDefinition,
  option: EChartsOption | undefined,
  availabilityOverrides?: SidebarAvailabilityOverrides
) {
  const availabilityOverride = isAvailabilityOverrideKey(definition.key)
    ? availabilityOverrides?.[definition.key]
    : undefined

  if (typeof availabilityOverride === 'boolean') {
    return availabilityOverride
  }

  return hasSeriesDataForDefinition(option, definition)
}

function getSidebarItemNote(
  definition: SidebarItemDefinition,
  option?: EChartsOption,
  availabilityOverrides?: SidebarAvailabilityOverrides
) {
  if (definition.key === 'gpx-tracks') {
    return hasSidebarItemData(definition, option, availabilityOverrides)
      ? undefined
      : 'Upload GPX data from the Uploads tab to show these tracks.'
  }

  if (
    definition.key === 'srm-entity-continuous' ||
    definition.key === 'srm-entity-gap'
  ) {
    return hasSidebarItemData(definition, option, availabilityOverrides)
      ? undefined
      : 'Upload SRM data from the Uploads tab to show these tracks.'
  }

  return undefined
}

function getSidebarItemUnavailableReason(
  definition: SidebarItemDefinition,
  option?: EChartsOption,
  availabilityOverrides?: SidebarAvailabilityOverrides
) {
  if (hasSidebarItemData(definition, option, availabilityOverrides)) {
    return undefined
  }

  return (
    ITEM_NO_DATA_MESSAGES[definition.key] ??
    `No ${definition.label.toLowerCase()} found.`
  )
}

function getLegendEntryName(entry: string | { name?: string }): string | null {
  if (typeof entry === 'string') {
    return entry.trim() || null
  }

  return typeof entry.name === 'string' && entry.name.trim()
    ? entry.name.trim()
    : null
}

function matchDirectionalPrefix(name: string, prefix: string): string | null {
  if (!name.startsWith(`${prefix} `)) {
    return null
  }

  const suffix = name.slice(prefix.length).trim()
  return suffix.length ? suffix : null
}

function getDirectionalLabels(option?: EChartsOption): string[] {
  const legend = getPrimaryLegend(option)
  if (!Array.isArray(legend?.data)) {
    return []
  }

  const cycleDirections: string[] = []

  for (const entry of legend.data) {
    const name = getLegendEntryName(entry)
    if (!name) continue

    const direction = matchDirectionalPrefix(name, 'Cycles')
    if (!direction || cycleDirections.includes(direction)) {
      continue
    }

    cycleDirections.push(direction)
    if (cycleDirections.length === 2) {
      return cycleDirections
    }
  }

  const fallbackDirections: string[] = []

  for (const entry of legend.data) {
    const name = getLegendEntryName(entry)
    if (!name) continue

    const direction = SIDEBAR_ITEM_DEFINITIONS.reduce<string | null>(
      (match, definition) => match ?? definition.match(name),
      null
    )

    if (
      !direction ||
      fallbackDirections.includes(direction) ||
      direction === 'Visible'
    ) {
      continue
    }

    fallbackDirections.push(direction)
    if (fallbackDirections.length === 2) {
      break
    }
  }

  return fallbackDirections
}

export function getSidebarDirectionControls(
  option?: EChartsOption
): SidebarDirectionControl[] {
  const legend = getPrimaryLegend(option)
  if (!Array.isArray(legend?.data)) {
    return []
  }

  const directionalLabels = getDirectionalLabels(option)
  const directionRoleByLabel = new Map<string, SidebarDirectionRole>()

  if (directionalLabels[0]) {
    directionRoleByLabel.set(directionalLabels[0], 'primary')
  }

  if (directionalLabels[1]) {
    directionRoleByLabel.set(directionalLabels[1], 'opposing')
  }

  const directionSeriesNames = new Map<SidebarDirectionRole, Set<string>>([
    ['primary', new Set<string>()],
    ['opposing', new Set<string>()],
  ])

  for (const entry of legend.data) {
    const name = getLegendEntryName(entry)
    if (!name) continue

    const directionLabel = SIDEBAR_ITEM_DEFINITIONS.reduce<string | null>(
      (match, definition) => match ?? definition.match(name),
      null
    )

    if (!directionLabel || directionLabel === 'Visible') {
      continue
    }

    const role = directionRoleByLabel.get(directionLabel)
    if (!role) {
      continue
    }

    directionSeriesNames.get(role)?.add(name)
  }

  return (
    [
      ['primary', directionalLabels[0]],
      ['opposing', directionalLabels[1]],
    ] as const
  ).flatMap(([role, label]) => {
    if (!label) {
      return []
    }

    const seriesNames = Array.from(directionSeriesNames.get(role) ?? [])
    if (!seriesNames.length) {
      return []
    }

    return [
      {
        role,
        label,
        seriesNames,
      },
    ]
  })
}

export function buildSidebarModel(
  option?: EChartsOption,
  availabilityOverrides?: SidebarAvailabilityOverrides
): SidebarLegendModel {
  const legend = getPrimaryLegend(option)
  if (!Array.isArray(legend?.data)) {
    return {
      directionControls: [],
      items: [],
    }
  }

  const itemMap = new Map<string, SidebarItem>()
  const directionControls = getSidebarDirectionControls(option)
  const directionRoleByLabel = new Map<string, SidebarDirectionRole>()
  directionControls.forEach((control) => {
    directionRoleByLabel.set(control.label, control.role)
  })

  for (const entry of legend.data) {
    const name = getLegendEntryName(entry)
    if (!name) continue

    const definition = SIDEBAR_ITEM_DEFINITIONS.find(
      (candidate) => candidate.match(name) !== null
    )

    if (!definition) continue

    const toggleLabel = definition.match(name) || 'Visible'
    const directionRole = directionRoleByLabel.get(toggleLabel)
    const existingItem = itemMap.get(definition.key)

    if (existingItem) {
      existingItem.toggles.push({
        label: toggleLabel,
        seriesName: name,
        directionRole,
      })
      continue
    }

    itemMap.set(definition.key, {
      key: definition.key,
      label: definition.label,
      category: definition.category,
      description: definition.description,
      note: getSidebarItemNote(definition, option, availabilityOverrides),
      unavailableReason: getSidebarItemUnavailableReason(
        definition,
        option,
        availabilityOverrides
      ),
      details: definition.details,
      preview: definition.preview,
      control: definition.control,
      toggles: [
        {
          label: toggleLabel,
          seriesName: name,
          directionRole,
        },
      ],
    })
  }

  const items = SIDEBAR_ITEM_DEFINITIONS.flatMap((definition) => {
    const item = itemMap.get(definition.key)
    if (!item) return []

    item.toggles.sort((a, b) => {
      const aOrder =
        a.directionRole === 'primary'
          ? 0
          : a.directionRole === 'opposing'
            ? 1
            : 2
      const bOrder =
        b.directionRole === 'primary'
          ? 0
          : b.directionRole === 'opposing'
            ? 1
            : 2

      if (aOrder !== bOrder) {
        return aOrder - bOrder
      }

      return a.label.localeCompare(b.label)
    })
    return [item]
  })

  return {
    directionControls,
    items,
  }
}

export function getStylableItems(items: SidebarItem[]) {
  return items.filter((item) => STYLABLE_ITEM_KEYS.has(item.key))
}

export function hasTimeSpaceStyleContent(
  option?: EChartsOption,
  availabilityOverrides?: SidebarAvailabilityOverrides
) {
  const { items } = buildSidebarModel(option, availabilityOverrides)
  return getStylableItems(items).length > 0
}

export function getDirectionalToggles(item: SidebarItem) {
  return item.toggles.filter((toggle) => Boolean(toggle.directionRole))
}

export function getDirectionRoleLabel(role: SidebarDirectionRole) {
  return role === 'primary' ? 'Primary phase' : 'Opposing phase'
}

export function getDirectionRoleDisplayLabel(role: SidebarDirectionRole) {
  return role === 'primary' ? 'primary' : 'opposing'
}

export function isToggleRequestedVisible(
  toggle: SidebarToggle,
  selectedSeries: Record<string, boolean>
) {
  return selectedSeries[toggle.seriesName] !== false
}

export function isToggleEffectivelyVisible(
  toggle: SidebarToggle,
  selectedSeries: Record<string, boolean>,
  suppressedDirections: Partial<Record<SidebarDirectionRole, boolean>>
) {
  if (!isToggleRequestedVisible(toggle, selectedSeries)) {
    return false
  }

  return !(
    toggle.directionRole && suppressedDirections[toggle.directionRole] === true
  )
}

export function isItemRequestedVisible(
  item: SidebarItem,
  selectedSeries: Record<string, boolean>
) {
  return item.toggles.some((toggle) =>
    isToggleRequestedVisible(toggle, selectedSeries)
  )
}

export function isItemEffectivelyVisible(
  item: SidebarItem,
  selectedSeries: Record<string, boolean>,
  suppressedDirections: Partial<Record<SidebarDirectionRole, boolean>>
) {
  return item.toggles.some((toggle) =>
    isToggleEffectivelyVisible(toggle, selectedSeries, suppressedDirections)
  )
}

export function isItemUnavailable(item: SidebarItem) {
  return typeof item.unavailableReason === 'string'
}

export function getSidebarCategoryUnavailableReason(
  category: (typeof CATEGORY_ORDER)[number]
) {
  return CATEGORY_NO_DATA_MESSAGES[category]
}

export function getRenderedItemDetails(
  item: SidebarItem,
  appearanceSettings?: TimeSpaceAppearanceSettings
) {
  if (!appearanceSettings || item.key !== 'cycles') {
    return item.details
  }

  return [
    {
      color: appearanceSettings.cycles.indicationColors.beginGreen,
      label: 'Early green',
    },
    {
      color: appearanceSettings.cycles.indicationColors.trailingGreen,
      label: 'Green phase',
    },
    {
      color: appearanceSettings.cycles.indicationColors.yellowClearance,
      label: 'Yellow clearance',
    },
    {
      color: appearanceSettings.cycles.indicationColors.redClearance,
      label: 'Red clearance',
    },
    {
      color: appearanceSettings.cycles.indicationColors.redIndication,
      label: 'Red indication',
    },
  ]
}
