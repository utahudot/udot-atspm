import { Color } from '@/features/charts/utils'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Box,
  Button,
  Checkbox,
  Divider,
  IconButton,
  Paper,
  Tooltip,
  Typography,
} from '@mui/material'
import type { EChartsOption } from 'echarts'
import { useState } from 'react'

type PreviewKind =
  | 'cycles'
  | 'cycle-durations'
  | 'green-bands'
  | 'detector-line'
  | 'stop-bar'
  | 'pedestrian'
  | 'turn'
  | 'srm'
  | 'triangle'
  | 'circle'
  | 'tsp-request'
  | 'tsp-service'

type SidebarToggle = {
  label: string
  seriesName: string
}

type SidebarDetail = {
  color: string
  label: string
}

type SidebarItem = {
  key: string
  label: string
  category: string
  description: string
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

export interface TimeSpaceSidebarProps {
  option?: EChartsOption
  selectedSeries: Record<string, boolean>
  onToggleSeries: (seriesName: string) => void
}

const CATEGORY_ORDER = [
  'Signal Timing',
  'Pedestrian',
  'Detection',
  'Movements & Tracks',
  'Transit Priority',
] as const

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
      'Lane-level count detector traces. Primary uses dark blue; opposing uses orange.',
    preview: 'detector-line',
    match: (name) => matchDirectionalPrefix(name, 'Lane by Lane Count'),
  },
  {
    key: 'advance-count',
    label: 'Advance Count',
    category: 'Detection',
    description:
      'Advance detector count traces upstream of the stop bar. Primary uses dark blue; opposing uses orange.',
    preview: 'detector-line',
    match: (name) => matchDirectionalPrefix(name, 'Advance Count'),
  },
  {
    key: 'stop-bar-presence',
    label: 'Stop Bar Presence',
    category: 'Detection',
    description:
      'Presence detector occupancy near the stop bar, shown as thicker occupancy bands.',
    preview: 'stop-bar',
    match: (name) => matchDirectionalPrefix(name, 'Stop Bar Presence'),
  },
  {
    key: 'pedestrian-interval',
    label: 'Pedestrian Interval',
    category: 'Pedestrian',
    description:
      "Black pedestrian state line. Solid is walk, dotted is clearance, zigzag is don't walk, and short ticks mark each transition.",
    preview: 'pedestrian',
    match: (name) => matchDirectionalPrefix(name, 'Pedestrian Interval'),
  },
  {
    key: 'left-turn',
    label: 'Left Turn',
    category: 'Movements & Tracks',
    description: 'Left-turn movement trace projected through the corridor.',
    preview: 'turn',
    match: (name) => matchDirectionalPrefix(name, 'Left Turn'),
  },
  {
    key: 'right-turn',
    label: 'Right Turn',
    category: 'Movements & Tracks',
    description: 'Right-turn movement trace projected through the corridor.',
    preview: 'turn',
    match: (name) => matchDirectionalPrefix(name, 'Right Turn'),
  },
  {
    key: 'srm-entity',
    label: 'SRM Entity',
    category: 'Movements & Tracks',
    description:
      'SRM or connected-vehicle entity tracks drawn as black path lines through the corridor.',
    preview: 'srm',
    match: (name) => matchDirectionalPrefix(name, 'SRM Entity'),
  },
  {
    key: 'early-green',
    label: 'Early Green (113)',
    category: 'Transit Priority',
    description: 'Triangle marker for an early-green priority event.',
    preview: 'triangle',
    control: 'visibility',
    match: (name) => (name.startsWith('Early Green (113)') ? '' : null),
  },
  {
    key: 'extend-green',
    label: 'Extend Green (114)',
    category: 'Transit Priority',
    description: 'Circle marker for a green-extension priority event.',
    preview: 'circle',
    control: 'visibility',
    match: (name) => (name.startsWith('Extend Green (114)') ? '' : null),
  },
  {
    key: 'tsp-request',
    label: 'TSP Request (112-115)',
    category: 'Transit Priority',
    description:
      'Red overlay band showing when a transit signal priority request was active.',
    preview: 'tsp-request',
    control: 'visibility',
    match: (name) => (name.startsWith('TSP Request (112-115)') ? '' : null),
  },
  {
    key: 'tsp-service',
    label: 'TSP Service (118-119)',
    category: 'Transit Priority',
    description:
      'Blue overlay band showing when transit signal priority service was granted.',
    preview: 'tsp-service',
    control: 'visibility',
    match: (name) => (name.startsWith('TSP Service (118-119)') ? '' : null),
  },
]

function getPrimaryLegend(option?: EChartsOption): LegendLike | undefined {
  if (!option?.legend) return undefined
  return Array.isArray(option.legend)
    ? (option.legend[0] as LegendLike | undefined)
    : (option.legend as LegendLike)
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

function buildSidebarItems(option?: EChartsOption): SidebarItem[] {
  const legend = getPrimaryLegend(option)
  if (!Array.isArray(legend?.data)) {
    return []
  }

  const itemMap = new Map<string, SidebarItem>()

  for (const entry of legend.data) {
    const name = getLegendEntryName(entry)
    if (!name) continue

    const definition = SIDEBAR_ITEM_DEFINITIONS.find(
      (candidate) => candidate.match(name) !== null
    )

    if (!definition) continue

    const toggleLabel = definition.match(name) || 'Visible'
    const existingItem = itemMap.get(definition.key)

    if (existingItem) {
      existingItem.toggles.push({
        label: toggleLabel,
        seriesName: name,
      })
      continue
    }

    itemMap.set(definition.key, {
      key: definition.key,
      label: definition.label,
      category: definition.category,
      description: definition.description,
      details: definition.details,
      preview: definition.preview,
      control: definition.control,
      toggles: [
        {
          label: toggleLabel,
          seriesName: name,
        },
      ],
    })
  }

  return SIDEBAR_ITEM_DEFINITIONS.flatMap((definition) => {
    const item = itemMap.get(definition.key)
    if (!item) return []

    item.toggles.sort((a, b) => a.label.localeCompare(b.label))
    return [item]
  })
}

function PreviewCard({ kind }: { kind: PreviewKind }) {
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
            <rect x="7" y="15" width="14" height="14" fill={Color.Green} />
            <rect x="21" y="15" width="10" height="14" fill="#8ef08d" />
            <rect x="31" y="15" width="8" height="14" fill={Color.Yellow} />
            <rect x="39" y="15" width="10" height="14" fill="#FF0000" />
            <rect x="49" y="15" width="22" height="14" fill="#f0807f" />
          </>
        )}

        {kind === 'cycle-durations' && (
          <>
            <rect x="9" y="15" width="34" height="14" fill="#8ef08d" />
            <rect x="43" y="15" width="24" height="14" fill={Color.Yellow} />
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
              points="12,38 28,8 40,8 24,38"
              fill="rgba(16, 185, 129, 0.35)"
            />
            <polygon
              points="36,38 52,8 64,8 48,38"
              fill="rgba(13, 148, 136, 0.25)"
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
              stroke="#0f3d91"
              strokeWidth="2.5"
            />
            <line
              x1="10"
              y1="30"
              x2="68"
              y2="30"
              stroke="#f59e0b"
              strokeWidth="2.5"
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
              stroke="#7dd3fc"
              strokeWidth="6"
              strokeLinecap="round"
            />
            <line
              x1="10"
              y1="30"
              x2="68"
              y2="30"
              stroke="#f59e0b"
              strokeWidth="6"
              strokeLinecap="round"
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

        {kind === 'turn' && (
          <>
            <line
              x1="12"
              y1="34"
              x2="64"
              y2="14"
              stroke="#111827"
              strokeWidth="2"
            />
            <circle cx="20" cy="31" r="2.5" fill="#111827" />
            <circle cx="56" cy="17" r="2.5" fill="#111827" />
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
          <polygon points="39,11 52,34 26,34" fill="#111827" />
        )}

        {kind === 'circle' && <circle cx="39" cy="24" r="10" fill="#111827" />}

        {kind === 'tsp-request' && (
          <line
            x1="10"
            y1="24"
            x2="68"
            y2="24"
            stroke={Color.Red}
            strokeWidth="8"
            strokeLinecap="round"
          />
        )}

        {kind === 'tsp-service' && (
          <line
            x1="10"
            y1="24"
            x2="68"
            y2="24"
            stroke={Color.LightBlue}
            strokeWidth="8"
            strokeLinecap="round"
          />
        )}
      </Box>
    </Box>
  )
}

function isItemVisible(
  item: SidebarItem,
  selectedSeries: Record<string, boolean>
): boolean {
  return item.toggles.some(
    (toggle) => selectedSeries[toggle.seriesName] !== false
  )
}

export default function TimeSpaceSidebar({
  option,
  selectedSeries,
  onToggleSeries,
}: TimeSpaceSidebarProps) {
  const items = buildSidebarItems(option)
  const [collapsedSections, setCollapsedSections] = useState<
    Record<string, boolean>
  >({})
  const [expandedDetails, setExpandedDetails] = useState<
    Record<string, boolean>
  >({})

  if (!items.length) {
    return null
  }

  const toggleSectionCollapse = (category: string) => {
    setCollapsedSections((current) => ({
      ...current,
      [category]: !current[category],
    }))
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

    for (const seriesName of seriesNames) {
      const isSelected = selectedSeries[seriesName] !== false
      if (isSelected !== visible) {
        onToggleSeries(seriesName)
      }
    }
  }

  const setItemVisibility = (item: SidebarItem, visible: boolean) => {
    for (const toggle of item.toggles) {
      const isSelected = selectedSeries[toggle.seriesName] !== false
      if (isSelected !== visible) {
        onToggleSeries(toggle.seriesName)
      }
    }
  }

  return (
    <Box
      sx={{
        width: 304,
        flexShrink: 0,
        height: '100%',
        overflowY: 'auto',
        borderLeft: '1px solid',
        borderColor: 'divider',
        marginTop: '-16px',
        p: 1.25,
      }}
    >
      <Typography
        variant="overline"
        sx={{
          letterSpacing: 0.9,
          color: 'text.secondary',
          fontWeight: 700,
          fontSize: '0.7rem',
        }}
      >
        Guide
      </Typography>

      <Divider sx={{ my: 1.25 }} />

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
        {CATEGORY_ORDER.map((category) => {
          const categoryItems = items.filter(
            (item) => item.category === category
          )
          if (!categoryItems.length) return null

          const isCollapsed = collapsedSections[category] === true
          const visibleItemCount = categoryItems.filter((item) =>
            isItemVisible(item, selectedSeries)
          ).length
          const hasVisibleItems = visibleItemCount > 0
          const allItemsVisible = visibleItemCount === categoryItems.length

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
                <Typography variant="subtitle2" sx={{ fontSize: '0.85rem' }}>
                  {category}
                </Typography>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 0.1,
                  }}
                >
                  <Tooltip title={hasVisibleItems ? 'Hide all' : 'Show all'}>
                    <Checkbox
                      size="small"
                      checked={allItemsVisible}
                      indeterminate={hasVisibleItems && !allItemsVisible}
                      onChange={() =>
                        setSectionVisibility(categoryItems, !allItemsVisible)
                      }
                      sx={{
                        p: 0.2,
                        color: 'text.secondary',
                        '& .MuiSvgIcon-root': {
                          fontSize: 18,
                        },
                      }}
                    />
                  </Tooltip>
                  <IconButton
                    size="small"
                    onClick={() => toggleSectionCollapse(category)}
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
                  const itemIsActive = isItemVisible(item, selectedSeries)

                  return (
                    <Paper
                      key={item.key}
                      variant="outlined"
                      sx={{
                        overflow: 'hidden',
                        background: itemIsActive ? '#fff' : '#f3f4f6',
                        borderColor: itemIsActive
                          ? 'rgba(203, 213, 225, 0.9)'
                          : 'rgba(203, 213, 225, 0.7)',
                        opacity: itemIsActive ? 1 : 0.6,
                        transition:
                          'background-color 120ms ease, opacity 120ms ease, border-color 120ms ease',
                      }}
                    >
                      <Box
                        sx={{ display: 'flex', gap: 0, alignItems: 'stretch' }}
                      >
                        <PreviewCard kind={item.preview} />
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
                                gap: 0.1,
                                ml: 'auto',
                              }}
                            >
                              {item.details?.length ? (
                                <Tooltip
                                  title={
                                    expandedDetails[item.key]
                                      ? 'Hide details'
                                      : 'Show details'
                                  }
                                >
                                  <IconButton
                                    size="small"
                                    onClick={() => toggleItemDetails(item.key)}
                                    sx={{ p: 0.15, color: 'text.secondary' }}
                                  >
                                    {expandedDetails[item.key] ? (
                                      <ExpandLessIcon fontSize="small" />
                                    ) : (
                                      <ExpandMoreIcon fontSize="small" />
                                    )}
                                  </IconButton>
                                </Tooltip>
                              ) : null}

                              {item.control === 'visibility' && (
                                <Tooltip
                                  title={
                                    itemIsActive
                                      ? `Hide ${item.label}`
                                      : `Show ${item.label}`
                                  }
                                >
                                  <Checkbox
                                    size="small"
                                    checked={itemIsActive}
                                    onChange={() =>
                                      setItemVisibility(item, !itemIsActive)
                                    }
                                    sx={{
                                      p: 0.2,
                                      color: 'text.secondary',
                                      '& .MuiSvgIcon-root': {
                                        fontSize: 18,
                                      },
                                    }}
                                  />
                                </Tooltip>
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

                          {item.details?.length && expandedDetails[item.key] ? (
                            <Box
                              sx={{
                                display: 'flex',
                                flexDirection: 'column',
                                gap: 0.5,
                                mt: 0.7,
                                pt: 0.7,
                                borderTop: '1px solid rgba(203, 213, 225, 0.9)',
                              }}
                            >
                              {item.details.map((detail) => (
                                <Box
                                  key={`${item.key}-${detail.label}`}
                                  sx={{
                                    display: 'flex',
                                    alignItems: 'center',
                                    gap: 0.55,
                                    minWidth: 0,
                                  }}
                                >
                                  <Box
                                    sx={{
                                      width: 13,
                                      height: 13,
                                      flexShrink: 0,
                                      borderRadius: '3px',
                                      bgcolor: detail.color,
                                    }}
                                  />
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

                          {item.control !== 'visibility' && (
                            <Box
                              sx={{
                                display: 'flex',
                                flexWrap: 'wrap',
                                gap: 0.5,
                                mt: item.details?.length ? 0.7 : 0.8,
                              }}
                            >
                              {item.toggles.map((toggle) => {
                                const isSelected =
                                  selectedSeries[toggle.seriesName] !== false

                                return (
                                  <Button
                                    key={toggle.seriesName}
                                    size="small"
                                    onClick={() =>
                                      onToggleSeries(toggle.seriesName)
                                    }
                                    variant="contained"
                                    disableElevation
                                    sx={{
                                      minWidth: 0,
                                      fontSize: '0.8rem',
                                      lineHeight: 1.1,
                                      textTransform: 'none',
                                      boxShadow: 'none',
                                      bgcolor: isSelected
                                        ? 'rgba(15, 23, 42, 0.92)'
                                        : 'rgba(226, 232, 240, 0.95)',
                                      color: isSelected
                                        ? '#fff'
                                        : 'rgba(71, 85, 105, 0.95)',
                                      '&:hover': {
                                        bgcolor: isSelected
                                          ? 'rgba(15, 23, 42, 0.82)'
                                          : 'rgba(203, 213, 225, 0.98)',
                                        boxShadow: 'none',
                                      },
                                    }}
                                  >
                                    {toggle.label}
                                  </Button>
                                )
                              })}
                            </Box>
                          )}
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
    </Box>
  )
}
