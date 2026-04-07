import { Color } from '@/features/charts/utils'
import BlockOutlinedIcon from '@mui/icons-material/BlockOutlined'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import {
  Box,
  Button,
  Checkbox,
  Divider,
  IconButton,
  Paper,
  Slider,
  Tab,
  Tabs,
  Tooltip,
  Typography,
} from '@mui/material'
import type { EChartsOption, SeriesOption } from 'echarts'
import type { Dispatch, ReactNode, SetStateAction } from 'react'
import { useEffect, useRef, useState } from 'react'
import {
  TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS,
  TimeSpaceAppearanceDirectionRole,
  TimeSpaceAppearanceSettings,
} from '../timeSpaceAppearance'
import { TIME_SPACE_GPX_TRACKS_LEGEND_NAME } from '../types'

type PreviewKind =
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

export type SidebarDirectionRole = TimeSpaceAppearanceDirectionRole

type SidebarToggle = {
  label: string
  seriesName: string
  directionRole?: SidebarDirectionRole
}

type SidebarDetail = {
  color?: string
  glyph?: 'solid-line' | 'dotted-line' | 'zigzag-line'
  label: string
}

type SidebarItem = {
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

type SidebarLegendModel = {
  directionControls: SidebarDirectionControl[]
  items: SidebarItem[]
}

export interface TimeSpaceSidebarProps {
  option?: EChartsOption
  selectedSeries: Record<string, boolean>
  suppressedDirections?: Partial<Record<SidebarDirectionRole, boolean>>
  onSetSeriesVisibility: (seriesNames: string[], visible: boolean) => void
  onToggleDirectionVisibility?: (role: SidebarDirectionRole) => void
  appearanceSettings?: TimeSpaceAppearanceSettings
  onAppearanceChange?: Dispatch<SetStateAction<TimeSpaceAppearanceSettings>>
  onResetAppearance?: () => void
  uploadContent?: ReactNode
  activeTab?: SidebarTab
  onTabChange?: (tab: SidebarTab) => void
  showTabs?: boolean
}

export const TIME_SPACE_GUIDE_WIDTH = 360

export type SidebarTab = 'legend' | 'styles' | 'uploads'

interface TimeSpaceSidebarTabsProps {
  activeTab: SidebarTab
  onChange: (tab: SidebarTab) => void
  hasLegendContent: boolean
  hasStyleContent: boolean
  hasUploadContent: boolean
  mode?: 'sidebar' | 'header'
}

const CATEGORY_ORDER = [
  'Signal Timing',
  'Pedestrian',
  'Detection',
  'Movements & Tracks',
  'Transit Priority',
] as const

const DIRECTION_TOGGLE_INACTIVE_COLOR = 'rgba(0, 0, 0, 0.6)'
const STATUS_ICON_COLOR = '#94A3B8'
const COLOR_INPUT_COMMIT_DELAY_MS = 150
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
  'srm-entity': 'No SRM entity tracks found.',
  'early-green': 'No early greens found.',
  'extend-green': 'No extend greens found.',
  'tsp-request': 'No transit-signal priority requests found.',
  'tsp-service': 'No transit-signal priority service events found.',
}
const CATEGORY_NO_DATA_MESSAGES: Record<(typeof CATEGORY_ORDER)[number], string> = {
  'Signal Timing': 'No signal timing data found.',
  Pedestrian: 'No pedestrian data found.',
  Detection: 'No detection data found.',
  'Movements & Tracks': 'No movement or track data found.',
  'Transit Priority': 'No transit-signal priority data found.',
}
const DIRECTION_TOGGLE_CIRCLE_PATH =
  'M320 576C461.4 576 576 461.4 576 320C576 178.6 461.4 64 320 64C178.6 64 64 178.6 64 320C64 461.4 178.6 576 320 576z'
const DIRECTION_TOGGLE_ARROW_PATH =
  'M337 199L417 279C426.4 288.4 426.4 303.6 417 312.9C407.6 322.2 392.4 322.3 383.1 312.9L344.1 273.9L344.1 424C344.1 437.3 333.4 448 320.1 448C306.8 448 296.1 437.3 296.1 424L296.1 273.9L257.1 312.9C247.7 322.3 232.5 322.3 223.2 312.9C213.9 303.5 213.8 288.3 223.2 279L303.2 199C312.6 189.6 327.8 189.6 337.1 199z'
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
      { color: Color.Green, label: 'Begin green' },
      { color: '#8ef08d', label: 'Trailing green' },
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
    description:
      'Lane-level count detector traces for each corridor direction.',
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
    key: 'srm-entity',
    label: 'SRM Entity',
    category: 'Movements & Tracks',
    description:
      'SRM or connected-vehicle entity tracks drawn through the corridor.',
    preview: 'srm',
    match: (name) => matchDirectionalPrefix(name, 'SRM Entity'),
  },
  {
    key: 'early-green',
    label: 'Early Green',
    category: 'Transit Priority',
    description: 'Triangle marker for an early-green priority event.',
    preview: 'triangle',
    control: 'visibility',
    match: (name) => (name.startsWith('Early Green') ? '' : null),
  },
  {
    key: 'extend-green',
    label: 'Extend Green',
    category: 'Transit Priority',
    description: 'Circle marker for a green-extension priority event.',
    preview: 'circle',
    control: 'visibility',
    match: (name) => (name.startsWith('Extend Green') ? '' : null),
  },
  {
    key: 'tsp-request',
    label: 'TSP Request',
    category: 'Transit Priority',
    description:
      'Transit signal priority request interval.',
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

export function TimeSpaceSidebarTabs({
  activeTab,
  onChange,
  hasLegendContent,
  hasStyleContent,
  hasUploadContent,
  mode = 'sidebar',
}: TimeSpaceSidebarTabsProps) {
  return (
    <Tabs
      value={activeTab}
      onChange={(_, value: SidebarTab) => onChange(value)}
      sx={{
        width: 'fit-content',
        maxWidth: '100%',
        minHeight: 'auto',
        px: mode === 'header' ? 1 : 1.5,
        pt: mode === 'header' ? 1 : 1.5,
        pb: mode === 'header' ? 0 : 0.6,
        '& .MuiTabs-scroller': {
          width: 'auto !important',
        },
        '& .MuiTabs-indicator': {
          bottom: mode === 'header' ? '-1px' : 0,
        },
        '& .MuiTab-root': {
          minWidth: 0,
          minHeight: mode === 'header' ? 34 : 30,
          padding: mode === 'header' ? '6px 12px' : '4px 10px',
          textTransform: 'none',
        },
      }}
    >
      {hasLegendContent && <Tab label="Legend" value="legend" />}
      {hasUploadContent && <Tab label="Uploads" value="uploads" />}
      {hasStyleContent && <Tab label="Styles" value="styles" />}
    </Tabs>
  )
}

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

function getSidebarItemNote(
  definition: SidebarItemDefinition,
  option?: EChartsOption
) {
  if (definition.key === 'gpx-tracks') {
    return hasSeriesDataForDefinition(option, definition)
      ? undefined
      : 'Upload GPX data from the Uploads tab to show these tracks.'
  }

  if (definition.key === 'srm-entity') {
    return hasSeriesDataForDefinition(option, definition)
      ? undefined
      : 'Upload SRM data from the Uploads tab to show these tracks.'
  }

  return undefined
}

function getSidebarItemUnavailableReason(
  definition: SidebarItemDefinition,
  option?: EChartsOption
) {
  if (hasSeriesDataForDefinition(option, definition)) {
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

function buildSidebarModel(option?: EChartsOption): SidebarLegendModel {
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
      note: getSidebarItemNote(definition, option),
      unavailableReason: getSidebarItemUnavailableReason(definition, option),
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

function getStylableItems(items: SidebarItem[]) {
  return items.filter((item) => STYLABLE_ITEM_KEYS.has(item.key))
}

export function hasTimeSpaceStyleContent(option?: EChartsOption) {
  const { items } = buildSidebarModel(option)
  return getStylableItems(items).length > 0
}

function getSidebarTabLabel(tab: SidebarTab) {
  if (tab === 'styles') {
    return 'Styles'
  }

  return tab === 'legend' ? 'Legend' : 'Uploads'
}

function PreviewCard({
  kind,
  appearanceSettings = TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS,
}: {
  kind: PreviewKind
  appearanceSettings?: TimeSpaceAppearanceSettings
}) {
  const cycleColors = appearanceSettings.cycles.indicationColors
  const cycleOpacity = appearanceSettings.cycles.opacity
  const greenBandPrimary = appearanceSettings.greenBands.primary
  const greenBandOpposing = appearanceSettings.greenBands.opposing
  const detectorPrimary = appearanceSettings.detection.laneByLaneCount.primary
  const detectorOpposing =
    appearanceSettings.detection.laneByLaneCount.opposing
  const stopBarPrimary =
    appearanceSettings.detection.stopBarPresence.primary
  const stopBarOpposing =
    appearanceSettings.detection.stopBarPresence.opposing
  const leftTurnAppearance = appearanceSettings.turns.leftTurn
  const rightTurnAppearance = appearanceSettings.turns.rightTurn

  return (
    <Box
      sx={{
        width: 70,
        minWidth: 70,
        alignSelf: 'stretch',
        background: '#eef1f5',
        borderRight: '1px solid rgba(203, 213, 225, 0.95)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      <Box
        component="svg"
        viewBox="0 0 78 48"
        sx={{ width: '100%', height: '100%' }}
      >
        {kind === 'cycles' && (
          <>
            <rect
              x="7"
              y="15"
              width="14"
              height="14"
              fill={cycleColors.beginGreen}
              opacity={cycleOpacity}
            />
            <rect
              x="21"
              y="15"
              width="10"
              height="14"
              fill={cycleColors.trailingGreen}
              opacity={cycleOpacity}
            />
            <rect
              x="31"
              y="15"
              width="8"
              height="14"
              fill={cycleColors.yellowClearance}
              opacity={cycleOpacity}
            />
            <rect
              x="39"
              y="15"
              width="10"
              height="14"
              fill={cycleColors.redClearance}
              opacity={cycleOpacity}
            />
            <rect
              x="49"
              y="15"
              width="22"
              height="14"
              fill={cycleColors.redIndication}
              opacity={cycleOpacity}
            />
          </>
        )}

        {kind === 'cycle-durations' && (
          <>
            <rect
              x="9"
              y="15"
              width="34"
              height="14"
              fill={cycleColors.trailingGreen}
              opacity={cycleOpacity}
            />
            <rect
              x="43"
              y="15"
              width="24"
              height="14"
              fill={cycleColors.yellowClearance}
              opacity={cycleOpacity}
            />
            <text
              x="26"
              y="26"
              fill="white"
              stroke="black"
              strokeWidth="1.5"
              fontSize="11"
              fontWeight="600"
              textAnchor="middle"
              paintOrder="stroke fill"
            >
              30
            </text>
            <text
              x="55"
              y="26"
              fill="white"
              stroke="black"
              strokeWidth="1.5"
              fontSize="11"
              fontWeight="600"
              textAnchor="middle"
              paintOrder="stroke fill"
            >
              4
            </text>
          </>
        )}

        {kind === 'green-bands' && (
          <>
            <polygon
              points="18,38 38,8 50,8 30,38"
              fill={greenBandPrimary.color}
              opacity={greenBandPrimary.opacity}
            />
            <polygon
              points="26,8 38,8 58,38 46,38"
              fill={greenBandOpposing.color}
              opacity={greenBandOpposing.opacity}
            />
          </>
        )}

        {kind === 'detector-line' && (
          <>
            <line
              x1="10"
              y1="18"
              x2="68"
              y2="18"
              stroke={detectorPrimary.color}
              strokeWidth="2.5"
              opacity={detectorPrimary.opacity}
            />
            <line
              x1="10"
              y1="30"
              x2="68"
              y2="30"
              stroke={detectorOpposing.color}
              strokeWidth="2.5"
              opacity={detectorOpposing.opacity}
            />
          </>
        )}

        {kind === 'stop-bar' && (
          <>
            <line
              x1="10"
              y1="18"
              x2="68"
              y2="18"
              stroke={stopBarPrimary.color}
              strokeWidth="2.5"
              opacity={stopBarPrimary.opacity}
            />
            <line
              x1="10"
              y1="30"
              x2="68"
              y2="30"
              stroke={stopBarOpposing.color}
              strokeWidth="2.5"
              opacity={stopBarOpposing.opacity}
            />
          </>
        )}

        {kind === 'pedestrian' && (
          <>
            <line
              x1="10"
              y1="14"
              x2="68"
              y2="14"
              stroke="#111827"
              strokeWidth="2"
            />
            <line
              x1="10"
              y1="24"
              x2="68"
              y2="24"
              stroke="#111827"
              strokeWidth="2"
              strokeDasharray="1 3"
              strokeLinecap="round"
            />
            <path
              d="M10 35 L15 32 L20 35 L25 32 L30 35 L35 32 L40 35 L45 32 L50 35 L55 32 L60 35 L65 32 L68 34"
              fill="none"
              stroke="#111827"
              strokeWidth="2"
              strokeLinejoin="round"
            />
            <line
              x1="10"
              y1="11"
              x2="10"
              y2="17"
              stroke="#111827"
              strokeWidth="1.5"
            />
            <line
              x1="10"
              y1="21"
              x2="10"
              y2="27"
              stroke="#111827"
              strokeWidth="1.5"
            />
            <line
              x1="10"
              y1="32"
              x2="10"
              y2="38"
              stroke="#111827"
              strokeWidth="1.5"
            />
          </>
        )}

        {kind === 'left-turn' && (
          <>
            <line
              x1="12"
              y1="34"
              x2="64"
              y2="14"
              stroke={leftTurnAppearance.color}
              strokeWidth="2"
              opacity={leftTurnAppearance.opacity}
            />
            <circle
              cx="20"
              cy="31"
              r="2.5"
              fill={leftTurnAppearance.color}
              opacity={leftTurnAppearance.opacity}
            />
            <circle
              cx="56"
              cy="17"
              r="2.5"
              fill={leftTurnAppearance.color}
              opacity={leftTurnAppearance.opacity}
            />
          </>
        )}

        {kind === 'right-turn' && (
          <>
            <line
              x1="12"
              y1="34"
              x2="64"
              y2="14"
              stroke={rightTurnAppearance.color}
              strokeWidth="2"
              opacity={rightTurnAppearance.opacity}
            />
            <circle
              cx="20"
              cy="31"
              r="2.5"
              fill={rightTurnAppearance.color}
              opacity={rightTurnAppearance.opacity}
            />
            <circle
              cx="56"
              cy="17"
              r="2.5"
              fill={rightTurnAppearance.color}
              opacity={rightTurnAppearance.opacity}
            />
          </>
        )}

        {kind === 'gpx-track' && (
          <>
            <path
              d="M10 31 C20 31, 26 12, 38 15 S56 32, 68 17"
              fill="none"
              stroke="#111827"
              strokeWidth="3"
              strokeLinecap="round"
            />
            <circle cx="16" cy="30" r="2.5" fill="#111827" />
            <circle cx="62" cy="19" r="2.5" fill="#111827" />
          </>
        )}

        {kind === 'srm' && (
          <path
            d="M10 31 C18 27, 26 14, 34 16 S50 33, 68 17"
            fill="none"
            stroke="#111827"
            strokeWidth="2.5"
            strokeLinecap="round"
          />
        )}

        {kind === 'triangle' && (
          <polygon
            points="39,11 52,34 26,34"
            fill={Color.White}
            stroke={Color.Black}
            strokeWidth="2"
          />
        )}

        {kind === 'circle' && (
          <circle
            cx="39"
            cy="24"
            r="10"
            fill={Color.White}
            stroke={Color.Black}
            strokeWidth="2"
          />
        )}

        {kind === 'tsp-request' && (
          <rect
            x="10"
            y="17"
            width="54"
            height="4"
            fill={appearanceSettings.tspRequest.color}
            opacity={appearanceSettings.tspRequest.opacity}
          />
        )}

        {kind === 'tsp-service' && (
          <>
            <rect
              x="10"
              y="27"
              width="54"
              height="4"
              fill={appearanceSettings.tspRequest.color}
              opacity={appearanceSettings.tspRequest.opacity}
            />
            <rect
              x="12"
              y="25"
              width="54"
              height="8"
              fill={appearanceSettings.tspService.color}
              opacity={appearanceSettings.tspService.opacity}
            />
          </>
        )}
      </Box>
    </Box>
  )
}

function getDirectionalToggles(item: SidebarItem) {
  return item.toggles.filter((toggle) => Boolean(toggle.directionRole))
}

function getDirectionRoleLabel(role: SidebarDirectionRole) {
  return role === 'primary' ? 'Primary phase' : 'Opposing phase'
}

function getDirectionRoleDisplayLabel(role: SidebarDirectionRole) {
  return role === 'primary' ? 'primary' : 'opposing'
}

function DirectionToggleIcon({
  role,
  isSelected,
  isDimmed = false,
}: {
  role: SidebarDirectionRole
  isSelected: boolean
  isDimmed?: boolean
}) {
  const isPrimary = role === 'primary'

  return (
    <Box
      sx={{
        width: 16,
        height: 16,
        opacity: isDimmed ? 0.7 : 1,
        display: 'inline-flex',
        alignItems: 'center',
        justifyContent: 'center',
        color: isSelected ? 'primary.main' : DIRECTION_TOGGLE_INACTIVE_COLOR,
      }}
    >
      <Box
        component="svg"
        viewBox="0 0 640 640"
        sx={{
          width: 13,
          height: 13,
          display: 'block',
          overflow: 'visible',
        }}
      >
        <g transform={isPrimary ? undefined : 'rotate(180 320 320)'}>
          {isSelected ? (
            <path
              d={DIRECTION_TOGGLE_CIRCLE_PATH}
              transform="translate(320 320) scale(1.5) translate(-320 -320)"
              fill="currentColor"
            />
          ) : null}
          <path
            d={DIRECTION_TOGGLE_ARROW_PATH}
            transform="translate(320 320) scale(1.6) translate(-320 -320)"
            fill={isSelected ? '#FFFFFF' : 'currentColor'}
          />
        </g>
      </Box>
    </Box>
  )
}

function isToggleRequestedVisible(
  toggle: SidebarToggle,
  selectedSeries: Record<string, boolean>
) {
  return selectedSeries[toggle.seriesName] !== false
}

function isToggleEffectivelyVisible(
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

function isItemRequestedVisible(
  item: SidebarItem,
  selectedSeries: Record<string, boolean>
) {
  return item.toggles.some((toggle) =>
    isToggleRequestedVisible(toggle, selectedSeries)
  )
}

function isItemEffectivelyVisible(
  item: SidebarItem,
  selectedSeries: Record<string, boolean>,
  suppressedDirections: Partial<Record<SidebarDirectionRole, boolean>>
) {
  return item.toggles.some((toggle) =>
    isToggleEffectivelyVisible(toggle, selectedSeries, suppressedDirections)
  )
}

function isItemUnavailable(item: SidebarItem) {
  return typeof item.unavailableReason === 'string'
}

function getSidebarCategoryUnavailableReason(
  category: (typeof CATEGORY_ORDER)[number]
) {
  return CATEGORY_NO_DATA_MESSAGES[category]
}

function DirectionToggleButton({
  role,
  isSelected,
  isDimmed = false,
  onClick,
  ariaLabel,
  title,
}: {
  role: SidebarDirectionRole
  isSelected: boolean
  isDimmed?: boolean
  onClick: () => void
  ariaLabel: string
  title: string
}) {
  return (
    <Tooltip title={title}>
      <Checkbox
        size="small"
        checked={isSelected}
        disableRipple
        onChange={onClick}
        inputProps={{
          'aria-label': ariaLabel,
        }}
        icon={
          <DirectionToggleIcon
            role={role}
            isSelected={false}
            isDimmed={isDimmed}
          />
        }
        checkedIcon={
          <DirectionToggleIcon role={role} isSelected isDimmed={isDimmed} />
        }
        sx={{
          p: 0.1,
          m: 0,
          minWidth: 0,
          lineHeight: 0,
          color: DIRECTION_TOGGLE_INACTIVE_COLOR,
          '&.Mui-checked': {
            color: 'primary.main',
          },
          borderRadius: '999px',
          '&:hover': {
            backgroundColor: 'rgba(15, 23, 42, 0.04)',
          },
        }}
      />
    </Tooltip>
  )
}

function UnavailableStatusIcon({
  ariaLabel,
  title,
  iconSize = 16,
  sx,
}: {
  ariaLabel: string
  title: string
  iconSize?: number
  sx?: Record<string, string | number>
}) {
  return (
    <Tooltip title={title}>
      <Box
        component="span"
        role="img"
        aria-label={ariaLabel}
        sx={{
          display: 'inline-flex',
          alignItems: 'center',
          justifyContent: 'center',
          lineHeight: 0,
          color: STATUS_ICON_COLOR,
          ...sx,
        }}
      >
        <BlockOutlinedIcon sx={{ fontSize: iconSize }} />
      </Box>
    </Tooltip>
  )
}

function InfoStatusIcon({
  ariaLabel,
  title,
  iconSize = 16,
  sx,
}: {
  ariaLabel: string
  title: string
  iconSize?: number
  sx?: Record<string, string | number>
}) {
  return (
    <Tooltip title={title}>
      <Box
        component="span"
        role="img"
        aria-label={ariaLabel}
        sx={{
          display: 'inline-flex',
          alignItems: 'center',
          justifyContent: 'center',
          lineHeight: 0,
          color: STATUS_ICON_COLOR,
          ...sx,
        }}
      >
        <InfoOutlinedIcon sx={{ fontSize: iconSize }} />
      </Box>
    </Tooltip>
  )
}

function DetailMarker({ detail }: { detail: SidebarDetail }) {
  if (detail.color) {
    return (
      <Box
        sx={{
          width: 13,
          height: 13,
          flexShrink: 0,
          borderRadius: '3px',
          bgcolor: detail.color,
        }}
      />
    )
  }

  return (
    <Box
      component="svg"
      viewBox="0 0 22 12"
      sx={{
        width: 22,
        height: 12,
        flexShrink: 0,
        overflow: 'visible',
      }}
    >
      {detail.glyph === 'solid-line' && (
        <line
          x1="1"
          y1="6"
          x2="21"
          y2="6"
          stroke="#111827"
          strokeWidth="1.8"
          strokeLinecap="round"
        />
      )}

      {detail.glyph === 'dotted-line' && (
        <line
          x1="1"
          y1="6"
          x2="21"
          y2="6"
          stroke="#111827"
          strokeWidth="1.8"
          strokeDasharray="1.4 2.4"
          strokeLinecap="round"
        />
      )}

      {detail.glyph === 'zigzag-line' && (
        <path
          d="M1 8 L4 4 L7 8 L10 4 L13 8 L16 4 L19 8 L21 6"
          fill="none"
          stroke="#111827"
          strokeWidth="1.7"
          strokeLinejoin="round"
          strokeLinecap="round"
        />
      )}
    </Box>
  )
}

function getRenderedItemDetails(
  item: SidebarItem,
  appearanceSettings?: TimeSpaceAppearanceSettings
) {
  if (!appearanceSettings || item.key !== 'cycles') {
    return item.details
  }

  return [
    {
      color: appearanceSettings.cycles.indicationColors.beginGreen,
      label: 'Begin green',
    },
    {
      color: appearanceSettings.cycles.indicationColors.trailingGreen,
      label: 'Trailing green',
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

function clampOpacity(value: number) {
  if (!Number.isFinite(value)) {
    return 0
  }

  return Math.max(0, Math.min(1, value))
}

function formatOpacityPercent(opacity: number) {
  return `${Math.round(clampOpacity(opacity) * 100)}%`
}

function ColorInputControl({
  label,
  value,
  ariaLabel,
  onChange,
}: {
  label: string
  value: string
  ariaLabel: string
  onChange: (value: string) => void
}) {
  const [draftValue, setDraftValue] = useState(value)
  const commitTimeoutRef = useRef<number | null>(null)

  useEffect(() => {
    setDraftValue(value)
  }, [value])

  useEffect(() => {
    if (commitTimeoutRef.current !== null) {
      window.clearTimeout(commitTimeoutRef.current)
      commitTimeoutRef.current = null
    }

    if (draftValue === value) {
      return
    }

    commitTimeoutRef.current = window.setTimeout(() => {
      commitTimeoutRef.current = null
      onChange(draftValue)
    }, COLOR_INPUT_COMMIT_DELAY_MS)

    return () => {
      if (commitTimeoutRef.current !== null) {
        window.clearTimeout(commitTimeoutRef.current)
        commitTimeoutRef.current = null
      }
    }
  }, [draftValue, onChange, value])

  const commitDraftValue = () => {
    if (commitTimeoutRef.current !== null) {
      window.clearTimeout(commitTimeoutRef.current)
      commitTimeoutRef.current = null
    }

    if (draftValue !== value) {
      onChange(draftValue)
    }
  }

  return (
    <Box
      component="label"
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: 1,
        minWidth: 0,
      }}
    >
      <Typography
        variant="caption"
        sx={{
          fontSize: '0.72rem',
          color: 'text.secondary',
          lineHeight: 1.2,
        }}
      >
        {label}
      </Typography>
      <Box
        component="input"
        type="color"
        aria-label={ariaLabel}
        value={draftValue}
        onChange={(event) => setDraftValue(event.target.value)}
        onBlur={commitDraftValue}
        sx={{
          width: 28,
          height: 28,
          p: 0,
          border: 'none',
          borderRadius: '50%',
          backgroundColor: 'transparent',
          cursor: 'pointer',
          overflow: 'hidden',
          '&::-webkit-color-swatch-wrapper': {
            p: 0,
          },
          '&::-webkit-color-swatch': {
            border: '1px solid rgba(203, 213, 225, 0.95)',
            borderRadius: '50%',
          },
          '&::-moz-color-swatch': {
            border: '1px solid rgba(203, 213, 225, 0.95)',
            borderRadius: '50%',
          },
        }}
      />
    </Box>
  )
}

function OpacityRangeControl({
  label,
  value,
  ariaLabel,
  onChange,
}: {
  label: string
  value: number
  ariaLabel: string
  onChange: (value: number) => void
}) {
  const percentValue = Math.round(clampOpacity(value) * 100)

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        gap: 0.45,
        pr: 0.5,
      }}
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 1,
        }}
      >
        <Typography
          variant="caption"
          sx={{ fontSize: '0.72rem', color: 'text.secondary', lineHeight: 1.2 }}
        >
          {label}
        </Typography>
      <Typography
        variant="caption"
        sx={{ fontSize: '0.72rem', color: 'text.primary', lineHeight: 1.2 }}
      >
        {formatOpacityPercent(value)}
      </Typography>
      </Box>
      <Slider
        size="small"
        value={percentValue}
        min={0}
        max={100}
        step={5}
        aria-label={ariaLabel}
        onChange={(_event, newValue) =>
          onChange(
            (Array.isArray(newValue) ? newValue[0] : newValue ?? percentValue) /
              100
          )
        }
      />
    </Box>
  )
}

function DirectionAppearanceEditor({
  label,
  appearance,
  colorAriaLabel,
  opacityAriaLabel,
  onColorChange,
  onOpacityChange,
}: {
  label: string
  appearance: { color: string; opacity: number }
  colorAriaLabel: string
  opacityAriaLabel: string
  onColorChange: (value: string) => void
  onOpacityChange: (value: number) => void
}) {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        gap: 0.7,
        p: 0.85,
        borderRadius: 1.5,
        backgroundColor: '#F8FAFC',
        border: '1px solid rgba(226, 232, 240, 0.95)',
      }}
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 1,
        }}
      >
        <Typography variant="subtitle2" sx={{ fontSize: '0.78rem' }}>
          {label}
        </Typography>
      </Box>
      <ColorInputControl
        label="Color"
        value={appearance.color}
        ariaLabel={colorAriaLabel}
        onChange={onColorChange}
      />
      <OpacityRangeControl
        label="Opacity"
        value={appearance.opacity}
        ariaLabel={opacityAriaLabel}
        onChange={onOpacityChange}
      />
      <Box
        sx={{
          mx: 0.5,
          width: 'calc(100% - 8px)',
          height: 12,
          borderRadius: '999px',
          bgcolor: appearance.color,
          opacity: appearance.opacity,
          border: '1px solid rgba(15, 23, 42, 0.08)',
        }}
      />
    </Box>
  )
}

function CycleAppearancePreview({
  opacity,
  colors,
}: {
  opacity: number
  colors: TimeSpaceAppearanceSettings['cycles']['indicationColors']
}) {
  const segments = [
    { color: colors.beginGreen, flex: 14 },
    { color: colors.trailingGreen, flex: 10 },
    { color: colors.yellowClearance, flex: 8 },
    { color: colors.redClearance, flex: 10 },
    { color: colors.redIndication, flex: 22 },
  ]

  return (
    <Box
      sx={{
        mx: 0.5,
        width: 'calc(100% - 8px)',
        height: 12,
        display: 'flex',
        overflow: 'hidden',
        borderRadius: '999px',
        border: '1px solid rgba(15, 23, 42, 0.08)',
        backgroundColor: '#fff',
      }}
    >
      {segments.map((segment, index) => (
        <Box
          key={`${segment.color}-${index}`}
          sx={{
            flex: segment.flex,
            bgcolor: segment.color,
            opacity,
          }}
        />
      ))}
    </Box>
  )
}

function AppearanceSectionCard({
  title,
  subtitle,
  children,
}: {
  title: string
  subtitle?: string
  children: ReactNode
}) {
  return (
    <Paper
      variant="outlined"
      sx={{
        p: 1,
        display: 'flex',
        flexDirection: 'column',
        gap: 0.9,
        borderColor: 'rgba(203, 213, 225, 0.9)',
        backgroundColor: '#fff',
      }}
    >
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.25 }}>
        <Typography variant="subtitle2" sx={{ fontSize: '0.84rem' }}>
          {title}
        </Typography>
        {subtitle ? (
          <Typography
            variant="caption"
            sx={{
              fontSize: '0.72rem',
              color: 'text.secondary',
              lineHeight: 1.35,
            }}
          >
            {subtitle}
          </Typography>
        ) : null}
      </Box>
      {children}
    </Paper>
  )
}

export default function TimeSpaceSidebar({
  option,
  selectedSeries,
  suppressedDirections = {},
  onSetSeriesVisibility,
  onToggleDirectionVisibility,
  appearanceSettings,
  onAppearanceChange,
  onResetAppearance,
  uploadContent,
  activeTab: controlledActiveTab,
  onTabChange,
  showTabs = true,
}: TimeSpaceSidebarProps) {
  const { directionControls, items } = buildSidebarModel(option)
  const [collapsedSections, setCollapsedSections] = useState<
    Record<string, boolean>
  >({})
  const [expandedDetails, setExpandedDetails] = useState<
    Record<string, boolean>
  >({})
  const [internalTab, setInternalTab] = useState<SidebarTab>('legend')
  const hasLegendContent = items.length > 0
  const stylableItems = getStylableItems(items)
  const hasStyleContent =
    Boolean(appearanceSettings && onAppearanceChange) && stylableItems.length > 0
  const hasUploadContent = Boolean(uploadContent)
  const availableTabs: SidebarTab[] = []
  const sectionCategories = CATEGORY_ORDER.filter((category) =>
    items.some((item) => item.category === category)
  )
  const hasGlobalSectionToggle = sectionCategories.length > 1
  const allSectionsCollapsed =
    hasGlobalSectionToggle &&
    sectionCategories.every((category) => collapsedSections[category] === true)

  if (hasLegendContent) {
    availableTabs.push('legend')
  }

  if (hasStyleContent) {
    availableTabs.push('styles')
  }

  if (hasUploadContent) {
    availableTabs.push('uploads')
  }

  if (!availableTabs.length) {
    return null
  }

  const activeTab = availableTabs.includes(controlledActiveTab ?? internalTab)
    ? (controlledActiveTab ?? internalTab)
    : availableTabs[0]
  const handleTabChange = (tab: SidebarTab) => {
    onTabChange?.(tab)
    if (controlledActiveTab == null) {
      setInternalTab(tab)
    }
  }

  const toggleSectionCollapse = (category: string) => {
    setCollapsedSections((current) => ({
      ...current,
      [category]: !current[category],
    }))
  }

  const toggleAllSections = () => {
    setCollapsedSections((current) => {
      const areAllCollapsed = sectionCategories.every(
        (category) => current[category] === true
      )
      const next = { ...current }

      sectionCategories.forEach((category) => {
        next[category] = !areAllCollapsed
      })

      return next
    })
  }

  const toggleItemDetails = (itemKey: string) => {
    setExpandedDetails((current) => ({
      ...current,
      [itemKey]: !current[itemKey],
    }))
  }

  const setSectionVisibility = (
    sectionItems: SidebarItem[],
    visible: boolean
  ) => {
    const seriesNames = Array.from(
      new Set(
        sectionItems.flatMap((item) =>
          item.toggles.map((toggle) => toggle.seriesName)
        )
      )
    )

    onSetSeriesVisibility(seriesNames, visible)
  }

  const setItemVisibility = (item: SidebarItem, visible: boolean) => {
    onSetSeriesVisibility(
      item.toggles.map((toggle) => toggle.seriesName),
      visible
    )
  }

  const setDirectionVisibility = (seriesName: string, visible: boolean) => {
    onSetSeriesVisibility([seriesName], visible)
  }

  const styleAppearance =
    appearanceSettings ?? TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS

  const updateAppearance = (
    updater: (current: TimeSpaceAppearanceSettings) => TimeSpaceAppearanceSettings
  ) => {
    onAppearanceChange?.((current) => updater(current))
  }

  const getDirectionalStyleLabels = (item: SidebarItem) => {
    const seenRoles = new Set<SidebarDirectionRole>()

    return item.toggles.flatMap((toggle) => {
      const role = toggle.directionRole
      if (!role || seenRoles.has(role)) {
        return []
      }

      seenRoles.add(role)
      return [
        {
          role,
          label: getDirectionRoleDisplayLabel(role),
        },
      ]
    })
  }

  return (
    <Box
      sx={{
        width: TIME_SPACE_GUIDE_WIDTH,
        flexShrink: 0,
        height: '100%',
        minHeight: 0,
        overflow: 'hidden',
        borderLeft: '1px solid',
        borderColor: 'divider',
        display: 'flex',
        flexDirection: 'column',
        bgcolor: '#fff',
      }}
    >
      {showTabs && availableTabs.length > 1 ? (
        <>
          <TimeSpaceSidebarTabs
            activeTab={activeTab}
            onChange={handleTabChange}
            hasLegendContent={hasLegendContent}
            hasStyleContent={hasStyleContent}
            hasUploadContent={hasUploadContent}
          />
        </>
      ) : showTabs ? (
        <>
          <Typography
            variant="overline"
            sx={{
              px: 1.5,
              pt: 1.5,
              pb: 0.9,
              letterSpacing: 0.9,
              color: 'text.secondary',
              fontWeight: 700,
              fontSize: '0.7rem',
            }}
          >
            {getSidebarTabLabel(activeTab)}
          </Typography>
          <Divider />
        </>
      ) : null}

      <Box
        sx={{
          flex: 1,
          minHeight: 0,
          overflowY: 'auto',
          p: 1.25,
        }}
      >
        {activeTab === 'uploads' ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
            {uploadContent}
          </Box>
        ) : activeTab === 'styles' ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                gap: 1,
              }}
            >
              <Typography
                variant="caption"
                sx={{
                  fontSize: '0.74rem',
                  color: 'text.secondary',
                  lineHeight: 1.35,
                }}
              >
                Update chart colors and opacity.
              </Typography>
              {onResetAppearance ? (
                <Button
                  size="small"
                  variant="text"
                  onClick={onResetAppearance}
                  sx={{ minWidth: 0, px: 0.75, textTransform: 'none' }}
                >
                  Reset
                </Button>
              ) : null}
            </Box>

            {stylableItems.map((item) => {
              if (item.key === 'cycles') {
                const cycleColors = styleAppearance.cycles.indicationColors

                return (
                  <AppearanceSectionCard
                    key={item.key}
                    title={item.label}
                  >
                    <Box
                      sx={{
                        display: 'flex',
                        flexDirection: 'column',
                        gap: 0.7,
                      }}
                    >
                      <Box
                        sx={{
                          display: 'grid',
                          gridTemplateColumns: 'repeat(2, minmax(0, 1fr))',
                          gridTemplateRows: 'repeat(3, auto)',
                          gridAutoFlow: 'column',
                          gap: 0.7,
                        }}
                      >
                        <ColorInputControl
                          label="Begin green"
                          value={cycleColors.beginGreen}
                          ariaLabel="Cycles begin green color"
                          onChange={(value) =>
                            updateAppearance((current) => ({
                              ...current,
                              cycles: {
                                ...current.cycles,
                                indicationColors: {
                                  ...current.cycles.indicationColors,
                                  beginGreen: value,
                                },
                              },
                            }))
                          }
                        />
                        <ColorInputControl
                          label="Trailing green"
                          value={cycleColors.trailingGreen}
                          ariaLabel="Cycles trailing green color"
                          onChange={(value) =>
                            updateAppearance((current) => ({
                              ...current,
                              cycles: {
                                ...current.cycles,
                                indicationColors: {
                                  ...current.cycles.indicationColors,
                                  trailingGreen: value,
                                },
                              },
                            }))
                          }
                        />
                        <ColorInputControl
                          label="Yellow clearance"
                          value={cycleColors.yellowClearance}
                          ariaLabel="Cycles yellow clearance color"
                          onChange={(value) =>
                            updateAppearance((current) => ({
                              ...current,
                              cycles: {
                                ...current.cycles,
                                indicationColors: {
                                  ...current.cycles.indicationColors,
                                  yellowClearance: value,
                                },
                              },
                            }))
                          }
                        />
                        <ColorInputControl
                          label="Red clearance"
                          value={cycleColors.redClearance}
                          ariaLabel="Cycles red clearance color"
                          onChange={(value) =>
                            updateAppearance((current) => ({
                              ...current,
                              cycles: {
                                ...current.cycles,
                                indicationColors: {
                                  ...current.cycles.indicationColors,
                                  redClearance: value,
                                },
                              },
                            }))
                          }
                        />
                        <ColorInputControl
                          label="Red indication"
                          value={cycleColors.redIndication}
                          ariaLabel="Cycles red indication color"
                          onChange={(value) =>
                            updateAppearance((current) => ({
                              ...current,
                              cycles: {
                                ...current.cycles,
                                indicationColors: {
                                  ...current.cycles.indicationColors,
                                  redIndication: value,
                                },
                              },
                            }))
                          }
                        />
                      </Box>
                      <OpacityRangeControl
                        label="Cycle opacity"
                        value={styleAppearance.cycles.opacity}
                        ariaLabel="Cycles opacity"
                        onChange={(value) =>
                          updateAppearance((current) => ({
                            ...current,
                            cycles: {
                              ...current.cycles,
                              opacity: value,
                            },
                          }))
                        }
                      />
                      <CycleAppearancePreview
                        opacity={styleAppearance.cycles.opacity}
                        colors={cycleColors}
                      />
                    </Box>
                  </AppearanceSectionCard>
                )
              }

              if (item.key === 'green-bands') {
                return (
                  <AppearanceSectionCard
                    key={item.key}
                    title={item.label}
                  >
                    <Box
                      sx={{
                        display: 'grid',
                        gridTemplateColumns: 'repeat(auto-fit, minmax(145px, 1fr))',
                        gap: 0.75,
                      }}
                    >
                      {getDirectionalStyleLabels(item).map(({ role, label }) => (
                        <DirectionAppearanceEditor
                          key={`${item.key}-${role}`}
                          label={label}
                          appearance={styleAppearance.greenBands[role]}
                          colorAriaLabel={`Green Bands ${label} color`}
                          opacityAriaLabel={`Green Bands ${label} opacity`}
                          onColorChange={(value) =>
                            updateAppearance((current) => ({
                              ...current,
                              greenBands: {
                                ...current.greenBands,
                                [role]: {
                                  ...current.greenBands[role],
                                  color: value,
                                },
                              },
                            }))
                          }
                          onOpacityChange={(value) =>
                            updateAppearance((current) => ({
                              ...current,
                              greenBands: {
                                ...current.greenBands,
                                [role]: {
                                  ...current.greenBands[role],
                                  opacity: value,
                                },
                              },
                            }))
                          }
                        />
                      ))}
                    </Box>
                  </AppearanceSectionCard>
                )
              }

              if (item.key === 'left-turn' || item.key === 'right-turn') {
                const turnAppearance =
                  item.key === 'left-turn'
                    ? styleAppearance.turns.leftTurn
                    : styleAppearance.turns.rightTurn

                return (
                  <AppearanceSectionCard
                    key={item.key}
                    title={item.label}
                  >
                    <DirectionAppearanceEditor
                      label={item.label}
                      appearance={turnAppearance}
                      colorAriaLabel={`${item.label} color`}
                      opacityAriaLabel={`${item.label} opacity`}
                      onColorChange={(value) =>
                        updateAppearance((current) => ({
                          ...current,
                          turns: {
                            ...current.turns,
                            [item.key === 'left-turn' ? 'leftTurn' : 'rightTurn']: {
                              ...current.turns[
                                item.key === 'left-turn' ? 'leftTurn' : 'rightTurn'
                              ],
                              color: value,
                            },
                          },
                        }))
                      }
                      onOpacityChange={(value) =>
                        updateAppearance((current) => ({
                          ...current,
                          turns: {
                            ...current.turns,
                            [item.key === 'left-turn' ? 'leftTurn' : 'rightTurn']: {
                              ...current.turns[
                                item.key === 'left-turn' ? 'leftTurn' : 'rightTurn'
                              ],
                              opacity: value,
                            },
                          },
                        }))
                      }
                    />
                  </AppearanceSectionCard>
                )
              }

              if (
                item.key === 'lane-by-lane-count' ||
                item.key === 'advance-count' ||
                item.key === 'stop-bar-presence'
              ) {
                const detectionAppearance =
                  item.key === 'lane-by-lane-count'
                    ? styleAppearance.detection.laneByLaneCount
                    : item.key === 'advance-count'
                      ? styleAppearance.detection.advanceCount
                      : styleAppearance.detection.stopBarPresence

                return (
                  <AppearanceSectionCard
                    key={item.key}
                    title={item.label}
                  >
                    <Box
                      sx={{
                        display: 'grid',
                        gridTemplateColumns: 'repeat(auto-fit, minmax(145px, 1fr))',
                        gap: 0.75,
                      }}
                    >
                      {getDirectionalStyleLabels(item).map(({ role, label }) => (
                        <DirectionAppearanceEditor
                          key={`${item.key}-${role}`}
                          label={label}
                          appearance={detectionAppearance[role]}
                          colorAriaLabel={`${item.label} ${label} color`}
                          opacityAriaLabel={`${item.label} ${label} opacity`}
                          onColorChange={(value) =>
                            updateAppearance((current) => ({
                              ...current,
                              detection: {
                                ...current.detection,
                                [item.key === 'lane-by-lane-count'
                                  ? 'laneByLaneCount'
                                  : item.key === 'advance-count'
                                    ? 'advanceCount'
                                    : 'stopBarPresence']: {
                                  ...current.detection[
                                    item.key === 'lane-by-lane-count'
                                      ? 'laneByLaneCount'
                                      : item.key === 'advance-count'
                                        ? 'advanceCount'
                                        : 'stopBarPresence'
                                  ],
                                  [role]: {
                                    ...current.detection[
                                      item.key === 'lane-by-lane-count'
                                        ? 'laneByLaneCount'
                                        : item.key === 'advance-count'
                                          ? 'advanceCount'
                                          : 'stopBarPresence'
                                    ][role],
                                    color: value,
                                  },
                                },
                              },
                            }))
                          }
                          onOpacityChange={(value) =>
                            updateAppearance((current) => ({
                              ...current,
                              detection: {
                                ...current.detection,
                                [item.key === 'lane-by-lane-count'
                                  ? 'laneByLaneCount'
                                  : item.key === 'advance-count'
                                    ? 'advanceCount'
                                    : 'stopBarPresence']: {
                                  ...current.detection[
                                    item.key === 'lane-by-lane-count'
                                      ? 'laneByLaneCount'
                                      : item.key === 'advance-count'
                                        ? 'advanceCount'
                                        : 'stopBarPresence'
                                  ],
                                  [role]: {
                                    ...current.detection[
                                      item.key === 'lane-by-lane-count'
                                        ? 'laneByLaneCount'
                                        : item.key === 'advance-count'
                                          ? 'advanceCount'
                                          : 'stopBarPresence'
                                    ][role],
                                    opacity: value,
                                  },
                                },
                              },
                            }))
                          }
                        />
                      ))}
                    </Box>
                  </AppearanceSectionCard>
                )
              }

              if (item.key === 'tsp-request') {
                const tspRequestAppearance = styleAppearance.tspRequest
                const tspServiceAppearance = styleAppearance.tspService

                return (
                  <AppearanceSectionCard
                    key={item.key}
                    title="Transit Priority"
                  >
                    <Box
                      sx={{
                        display: 'grid',
                        gridTemplateColumns: 'repeat(auto-fit, minmax(145px, 1fr))',
                        gap: 0.75,
                      }}
                    >
                      <DirectionAppearanceEditor
                        label="TSP Request"
                        appearance={tspRequestAppearance}
                        colorAriaLabel="TSP Request color"
                        opacityAriaLabel="TSP Request opacity"
                        onColorChange={(value) =>
                          updateAppearance((current) => ({
                            ...current,
                            tspRequest: {
                              ...current.tspRequest,
                              color: value,
                            },
                          }))
                        }
                        onOpacityChange={(value) =>
                          updateAppearance((current) => ({
                            ...current,
                            tspRequest: {
                              ...current.tspRequest,
                              opacity: value,
                            },
                          }))
                        }
                      />
                      <DirectionAppearanceEditor
                        label="TSP Service"
                        appearance={tspServiceAppearance}
                        colorAriaLabel="TSP Service color"
                        opacityAriaLabel="TSP Service opacity"
                        onColorChange={(value) =>
                          updateAppearance((current) => ({
                            ...current,
                            tspService: {
                              ...current.tspService,
                              color: value,
                            },
                          }))
                        }
                        onOpacityChange={(value) =>
                          updateAppearance((current) => ({
                            ...current,
                            tspService: {
                              ...current.tspService,
                              opacity: value,
                            },
                          }))
                        }
                      />
                    </Box>
                  </AppearanceSectionCard>
                )
              }

              if (item.key === 'tsp-service') {
                return null
              }

              return null
            })}
          </Box>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
            {directionControls.length || hasGlobalSectionToggle ? (
              <Box
                sx={{
                  display: 'inline-flex',
                  justifyContent: 'flex-end',
                  alignItems: 'center',
                  gap: 0.15,
                  px: 0.35,
                  py: 0.2,
                  pr: 0.35,
                  ml: 'auto',
                  borderRadius: 1.5,
                  backgroundColor: '#EEF1F5',
                  border: '1px solid rgba(203, 213, 225, 0.9)',
                }}
              >
                {directionControls.map((control) => {
                  const isSuppressed =
                    suppressedDirections[control.role] === true
                  const roleLabel = getDirectionRoleLabel(control.role)

                  return (
                    <DirectionToggleButton
                      key={control.role}
                      role={control.role}
                      isSelected={!isSuppressed}
                      onClick={() =>
                        onToggleDirectionVisibility?.(control.role)
                      }
                      ariaLabel={`Toggle ${control.role} direction`}
                      title={
                        isSuppressed
                          ? `Show ${roleLabel}`
                          : `Hide ${roleLabel}`
                      }
                    />
                  )
                })}
                {directionControls.length && hasGlobalSectionToggle ? (
                  <Divider
                    orientation="vertical"
                    flexItem
                    sx={{
                      mx: 0.25,
                      my: 0.25,
                      borderColor: 'rgba(148, 163, 184, 0.55)',
                    }}
                  />
                ) : null}
                {hasGlobalSectionToggle ? (
                  <Tooltip
                    title={
                      allSectionsCollapsed
                        ? 'Expand all legend sections'
                        : 'Collapse all legend sections'
                    }
                  >
                    <IconButton
                      size="small"
                      onClick={toggleAllSections}
                      aria-label={
                        allSectionsCollapsed
                          ? 'Expand all legend sections'
                          : 'Collapse all legend sections'
                      }
                      sx={{
                        p: 0.15,
                        color: 'text.secondary',
                      }}
                    >
                      {allSectionsCollapsed ? (
                        <ExpandMoreIcon fontSize="small" />
                      ) : (
                        <ExpandLessIcon fontSize="small" />
                      )}
                    </IconButton>
                  </Tooltip>
                ) : null}
              </Box>
            ) : null}

            {CATEGORY_ORDER.map((category) => {
              const categoryItems = items.filter(
                (item) => item.category === category
              )
              if (!categoryItems.length) return null

              const isCollapsed = collapsedSections[category] === true
              const availableCategoryItems = categoryItems.filter(
                (item) => !isItemUnavailable(item)
              )
              const allItemsUnavailable = availableCategoryItems.length === 0
              const categoryUnavailableReason = allItemsUnavailable
                ? getSidebarCategoryUnavailableReason(category)
                : undefined
              const visibleItemCount = availableCategoryItems.filter((item) =>
                isItemRequestedVisible(item, selectedSeries)
              ).length
              const hasVisibleItems = visibleItemCount > 0
              const allItemsVisible =
                availableCategoryItems.length > 0 &&
                visibleItemCount === availableCategoryItems.length
              const showSectionVisibilityToggle =
                availableCategoryItems.length > 1
              const showSectionUnavailableIcon =
                categoryItems.length > 1 && allItemsUnavailable

              return (
                <Box key={category}>
                  <Box
                    sx={{
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'space-between',
                      gap: 1,
                      mb: isCollapsed ? 0.25 : 0.6,
                      pl: 0.75,
                    }}
                  >
                    <Typography
                      variant="subtitle2"
                      sx={{ fontSize: '0.85rem' }}
                    >
                      {category}
                    </Typography>
                    <Box
                      sx={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: 0.1,
                      }}
                    >
                      {showSectionUnavailableIcon ? (
                        <UnavailableStatusIcon
                          ariaLabel={`${category} unavailable`}
                          title={categoryUnavailableReason ?? ''}
                          iconSize={17}
                        />
                      ) : showSectionVisibilityToggle ? (
                        <Tooltip
                          title={hasVisibleItems ? 'Hide all' : 'Show all'}
                        >
                          <Checkbox
                            size="small"
                            checked={allItemsVisible}
                            indeterminate={hasVisibleItems && !allItemsVisible}
                            onChange={() =>
                              setSectionVisibility(
                                availableCategoryItems,
                                !allItemsVisible
                              )
                            }
                            inputProps={{
                              'aria-label': `Toggle all ${category}`,
                            }}
                            sx={{
                              p: 0.2,
                              color: 'text.secondary',
                              '& .MuiSvgIcon-root': {
                                fontSize: 18,
                              },
                            }}
                          />
                        </Tooltip>
                      ) : null}
                      <IconButton
                        size="small"
                        onClick={() => toggleSectionCollapse(category)}
                        aria-label={
                          isCollapsed
                            ? `Expand ${category}`
                            : `Collapse ${category}`
                        }
                        sx={{ p: 0.15, color: 'text.secondary' }}
                      >
                        {isCollapsed ? (
                          <ExpandMoreIcon fontSize="small" />
                        ) : (
                          <ExpandLessIcon fontSize="small" />
                        )}
                      </IconButton>
                    </Box>
                  </Box>

                  <Box
                    sx={{
                      display: isCollapsed ? 'none' : 'flex',
                      flexDirection: 'column',
                      gap: 0.9,
                    }}
                  >
                    {categoryItems.map((item) => {
                      const itemIsUnavailable = isItemUnavailable(item)
                      const itemIsRequestedActive =
                        !itemIsUnavailable &&
                        isItemRequestedVisible(item, selectedSeries)
                      const itemIsEffectivelyShown =
                        !itemIsUnavailable &&
                        isItemEffectivelyVisible(
                          item,
                          selectedSeries,
                          suppressedDirections
                        )
                      const itemIsMasked =
                        itemIsRequestedActive && !itemIsEffectivelyShown
                      const directionalToggles = getDirectionalToggles(item)
                      const visibleToggleCount = itemIsUnavailable
                        ? 0
                        : item.toggles.filter((toggle) =>
                            isToggleRequestedVisible(toggle, selectedSeries)
                          ).length
                      const allItemSeriesVisible =
                        !itemIsUnavailable &&
                        visibleToggleCount === item.toggles.length
                      const someItemSeriesVisible =
                        visibleToggleCount > 0 && !allItemSeriesVisible

                      return (
                        <Paper
                          key={item.key}
                          variant="outlined"
                          sx={{
                            overflow: 'hidden',
                            background: itemIsRequestedActive
                              ? itemIsMasked
                                ? '#F8FAFC'
                                : '#fff'
                              : '#f3f4f6',
                            borderColor: itemIsRequestedActive
                              ? itemIsMasked
                                ? 'rgba(148, 163, 184, 0.88)'
                                : 'rgba(203, 213, 225, 0.9)'
                              : 'rgba(203, 213, 225, 0.7)',
                            opacity: itemIsRequestedActive
                              ? itemIsMasked
                                ? 0.78
                                : 1
                              : 0.6,
                            transition:
                              'background-color 120ms ease, opacity 120ms ease, border-color 120ms ease',
                          }}
                        >
                          <Box
                            sx={{
                              display: 'flex',
                              gap: 0,
                              alignItems: 'stretch',
                            }}
                          >
                            <PreviewCard
                              kind={item.preview}
                              appearanceSettings={appearanceSettings}
                            />
                            <Box
                              sx={{
                                minWidth: 0,
                                flex: 1,
                                p: 0.9,
                              }}
                            >
                              <Box
                                sx={{
                                  display: 'flex',
                                  alignItems: 'center',
                                  justifyContent: 'space-between',
                                  gap: 0.5,
                                }}
                              >
                                <Typography
                                  variant="subtitle2"
                                  sx={{ fontSize: '0.8rem' }}
                                >
                                  {item.label}
                                </Typography>
                                <Box
                                  sx={{
                                    display: 'flex',
                                    alignItems: 'center',
                                    gap: 0.35,
                                    ml: 'auto',
                                  }}
                                >
                                  {getRenderedItemDetails(
                                    item,
                                    appearanceSettings
                                  )?.length ? (
                                    <Tooltip
                                      title={
                                        expandedDetails[item.key]
                                          ? 'Hide details'
                                          : 'Show details'
                                      }
                                    >
                                      <IconButton
                                        size="small"
                                        onClick={() =>
                                          toggleItemDetails(item.key)
                                        }
                                        sx={{
                                          p: 0.15,
                                          mr: 0.3,
                                          color: 'text.secondary',
                                        }}
                                      >
                                        {expandedDetails[item.key] ? (
                                          <ExpandLessIcon fontSize="small" />
                                        ) : (
                                          <ExpandMoreIcon fontSize="small" />
                                        )}
                                      </IconButton>
                                    </Tooltip>
                                  ) : null}

                                  {item.note ? (
                                    <InfoStatusIcon
                                      ariaLabel={`${item.label} info`}
                                      title={item.note}
                                    />
                                  ) : itemIsUnavailable ? (
                                    <UnavailableStatusIcon
                                      ariaLabel={`${item.label} unavailable`}
                                      title={item.unavailableReason ?? ''}
                                    />
                                  ) : (
                                    <Box
                                      sx={{
                                        display: 'inline-flex',
                                        alignItems: 'center',
                                        gap: 0.15,
                                        px: 0.35,
                                        py: 0.2,
                                        borderRadius: 1.5,
                                        backgroundColor: '#EEF1F5',
                                        border:
                                          '1px solid rgba(203, 213, 225, 0.9)',
                                      }}
                                    >
                                      {directionalToggles.map((toggle) => {
                                        const role = toggle.directionRole
                                        if (!role) {
                                          return null
                                        }

                                        const isSelected =
                                          isToggleRequestedVisible(
                                            toggle,
                                            selectedSeries
                                          )
                                        const isSuppressed =
                                          suppressedDirections[role] === true
                                        const roleLabel =
                                          getDirectionRoleLabel(role)

                                        return (
                                          <DirectionToggleButton
                                            key={toggle.seriesName}
                                            role={role}
                                            isSelected={isSelected}
                                            isDimmed={isSuppressed}
                                            onClick={() =>
                                              setDirectionVisibility(
                                                toggle.seriesName,
                                                !isSelected
                                              )
                                            }
                                            ariaLabel={`Toggle ${role} direction for ${item.label}`}
                                            title={`${isSelected ? 'Hide' : 'Show'} ${roleLabel}`}
                                          />
                                        )
                                      })}

                                      <Tooltip
                                        title={
                                          allItemSeriesVisible
                                            ? `Hide ${item.label}`
                                            : `Show ${item.label}`
                                        }
                                      >
                                        <Checkbox
                                          size="small"
                                          checked={allItemSeriesVisible}
                                          indeterminate={someItemSeriesVisible}
                                          onChange={() =>
                                            setItemVisibility(
                                              item,
                                              !allItemSeriesVisible
                                            )
                                          }
                                          inputProps={{
                                            'aria-label': `Toggle ${item.label}`,
                                          }}
                                          sx={{
                                            p: 0.1,
                                            m: 0,
                                            minWidth: 0,
                                            lineHeight: 0,
                                            color: 'text.secondary',
                                            '& .MuiSvgIcon-root': {
                                              fontSize: 18,
                                            },
                                          }}
                                        />
                                      </Tooltip>
                                    </Box>
                                  )}
                                </Box>
                              </Box>
                              <Typography
                                variant="caption"
                                sx={{
                                  mt: 0.35,
                                  display: 'block',
                                  lineHeight: 1.4,
                                  color: 'text.secondary',
                                  fontSize: '0.76rem',
                                }}
                              >
                                {item.description}
                              </Typography>

                              {getRenderedItemDetails(
                                item,
                                appearanceSettings
                              )?.length &&
                              expandedDetails[item.key] ? (
                                <Box
                                  sx={{
                                    display: 'flex',
                                    flexDirection: 'column',
                                    gap: 0.5,
                                    mt: 0.7,
                                    pt: 0.7,
                                    borderTop:
                                      '1px solid rgba(203, 213, 225, 0.9)',
                                  }}
                                >
                                  {getRenderedItemDetails(
                                    item,
                                    appearanceSettings
                                  )?.map((detail) => (
                                    <Box
                                      key={`${item.key}-${detail.label}`}
                                      sx={{
                                        display: 'flex',
                                        alignItems: 'center',
                                        gap: 0.55,
                                        minWidth: 0,
                                      }}
                                    >
                                      <DetailMarker detail={detail} />
                                      <Typography
                                        variant="caption"
                                        sx={{
                                          fontSize: '0.69rem',
                                          lineHeight: 1.2,
                                          color: 'text.secondary',
                                        }}
                                      >
                                        {detail.label}
                                      </Typography>
                                    </Box>
                                  ))}
                                </Box>
                              ) : null}
                            </Box>
                          </Box>
                        </Paper>
                      )
                    })}
                  </Box>
                </Box>
              )
            })}
          </Box>
        )}
      </Box>
    </Box>
  )
}
