import {
  CATEGORY_ORDER,
  getDirectionalToggles,
  getDirectionRoleLabel,
  getRenderedItemDetails,
  getSidebarCategoryUnavailableReason,
  isItemEffectivelyVisible,
  isItemRequestedVisible,
  isItemUnavailable,
  isToggleRequestedVisible,
  type PreviewKind,
  type SidebarDetail,
  type SidebarDirectionControl,
  type SidebarDirectionRole,
  type SidebarItem,
} from '@/features/charts/timeSpaceDiagram/renderer/sidebar/timeSpaceSidebarModel'
import { Color } from '@/features/charts/utils'
import BlockOutlinedIcon from '@mui/icons-material/BlockOutlined'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import {
  Box,
  Checkbox,
  Divider,
  IconButton,
  Paper,
  Tooltip,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import {
  TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS,
  type TimeSpaceAppearanceSettings,
} from '../../shared/timeSpaceAppearance'

const DIRECTION_TOGGLE_INACTIVE_COLOR = 'rgba(0, 0, 0, 0.6)'
const STATUS_ICON_COLOR = '#94A3B8'
const DIRECTION_TOGGLE_CIRCLE_PATH =
  'M320 576C461.4 576 576 461.4 576 320C576 178.6 461.4 64 320 64C178.6 64 64 178.6 64 320C64 461.4 178.6 576 320 576z'
const DIRECTION_TOGGLE_ARROW_PATH =
  'M337 199L417 279C426.4 288.4 426.4 303.6 417 312.9C407.6 322.2 392.4 322.3 383.1 312.9L344.1 273.9L344.1 424C344.1 437.3 333.4 448 320.1 448C306.8 448 296.1 437.3 296.1 424L296.1 273.9L257.1 312.9C247.7 322.3 232.5 322.3 223.2 312.9C213.9 303.5 213.8 288.3 223.2 279L303.2 199C312.6 189.6 327.8 189.6 337.1 199z'

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
  const stopBarPrimary = appearanceSettings.detection.stopBarPresence.primary
  const stopBarOpposing = appearanceSettings.detection.stopBarPresence.opposing
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
      <Box component="svg" viewBox="0 0 78 48" sx={{ width: '100%', height: '100%' }}>
        {kind === 'cycles' && (
          <>
            <rect x="7" y="15" width="14" height="14" fill={cycleColors.beginGreen} opacity={cycleOpacity} />
            <rect x="21" y="15" width="10" height="14" fill={cycleColors.trailingGreen} opacity={cycleOpacity} />
            <rect x="31" y="15" width="8" height="14" fill={cycleColors.yellowClearance} opacity={cycleOpacity} />
            <rect x="39" y="15" width="10" height="14" fill={cycleColors.redClearance} opacity={cycleOpacity} />
            <rect x="49" y="15" width="22" height="14" fill={cycleColors.redIndication} opacity={cycleOpacity} />
          </>
        )}
        {kind === 'cycle-durations' && (
          <>
            <rect x="9" y="15" width="34" height="14" fill={cycleColors.trailingGreen} opacity={cycleOpacity} />
            <rect x="43" y="15" width="24" height="14" fill={cycleColors.yellowClearance} opacity={cycleOpacity} />
            <text x="26" y="26" fill="white" stroke="black" strokeWidth="1.5" fontSize="11" fontWeight="600" textAnchor="middle" paintOrder="stroke fill">
              30
            </text>
            <text x="55" y="26" fill="white" stroke="black" strokeWidth="1.5" fontSize="11" fontWeight="600" textAnchor="middle" paintOrder="stroke fill">
              4
            </text>
          </>
        )}
        {kind === 'green-bands' && (
          <>
            <polygon points="18,38 38,8 50,8 30,38" fill={greenBandPrimary.color} opacity={greenBandPrimary.opacity} />
            <polygon points="26,8 38,8 58,38 46,38" fill={greenBandOpposing.color} opacity={greenBandOpposing.opacity} />
          </>
        )}
        {kind === 'detector-line' && (
          <>
            <line x1="10" y1="18" x2="68" y2="18" stroke={detectorPrimary.color} strokeWidth="2.5" opacity={detectorPrimary.opacity} />
            <line x1="10" y1="30" x2="68" y2="30" stroke={detectorOpposing.color} strokeWidth="2.5" opacity={detectorOpposing.opacity} />
          </>
        )}
        {kind === 'stop-bar' && (
          <>
            <line x1="10" y1="18" x2="68" y2="18" stroke={stopBarPrimary.color} strokeWidth="2.5" opacity={stopBarPrimary.opacity} />
            <line x1="10" y1="30" x2="68" y2="30" stroke={stopBarOpposing.color} strokeWidth="2.5" opacity={stopBarOpposing.opacity} />
          </>
        )}
        {kind === 'pedestrian' && (
          <>
            <line x1="10" y1="14" x2="68" y2="14" stroke="#111827" strokeWidth="2" />
            <line x1="10" y1="24" x2="68" y2="24" stroke="#111827" strokeWidth="2" strokeDasharray="1 3" strokeLinecap="round" />
            <path d="M10 35 L15 32 L20 35 L25 32 L30 35 L35 32 L40 35 L45 32 L50 35 L55 32 L60 35 L65 32 L68 34" fill="none" stroke="#111827" strokeWidth="2" strokeLinejoin="round" />
            <line x1="10" y1="11" x2="10" y2="17" stroke="#111827" strokeWidth="1.5" />
            <line x1="10" y1="21" x2="10" y2="27" stroke="#111827" strokeWidth="1.5" />
            <line x1="10" y1="32" x2="10" y2="38" stroke="#111827" strokeWidth="1.5" />
          </>
        )}
        {kind === 'left-turn' && (
          <line x1="12" y1="34" x2="64" y2="14" stroke={leftTurnAppearance.color} strokeWidth="2" opacity={leftTurnAppearance.opacity} />
        )}
        {kind === 'right-turn' && (
          <line x1="12" y1="34" x2="64" y2="14" stroke={rightTurnAppearance.color} strokeWidth="2" opacity={rightTurnAppearance.opacity} />
        )}
        {kind === 'gpx-track' && (
          <path d="M10 31 C20 31, 26 12, 38 15 S56 32, 68 17" fill="none" stroke="#111827" strokeWidth="3" strokeLinecap="round" strokeDasharray="1.4 3.2" />
        )}
        {kind === 'srm' && (
          <path d="M10 31 C18 27, 26 14, 34 16 S50 33, 68 17" fill="none" stroke="#111827" strokeWidth="2.5" strokeLinecap="round" />
        )}
        {kind === 'triangle' && (
          <polygon points="39,11 52,34 26,34" fill={Color.White} stroke={Color.Black} strokeWidth="2" />
        )}
        {kind === 'circle' && (
          <circle cx="39" cy="24" r="10" fill={Color.White} stroke={Color.Black} strokeWidth="2" />
        )}
        {kind === 'tsp-request' && (
          <rect x="10" y="17" width="54" height="4" fill={appearanceSettings.tspRequest.color} opacity={appearanceSettings.tspRequest.opacity} />
        )}
        {kind === 'tsp-service' && (
          <rect
            x="12"
            y="25"
            width="54"
            height="8"
            fill="transparent"
            stroke={appearanceSettings.tspService.color}
            strokeWidth="2"
            strokeOpacity={appearanceSettings.tspService.opacity}
          />
        )}
      </Box>
    </Box>
  )
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
      <Box component="svg" viewBox="0 0 640 640" sx={{ width: 13, height: 13, display: 'block', overflow: 'visible' }}>
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
    <Box component="svg" viewBox="0 0 22 12" sx={{ width: 22, height: 12, flexShrink: 0, overflow: 'visible' }}>
      {detail.glyph === 'solid-line' && (
        <line x1="1" y1="6" x2="21" y2="6" stroke="#111827" strokeWidth="1.8" strokeLinecap="round" />
      )}
      {detail.glyph === 'dotted-line' && (
        <line x1="1" y1="6" x2="21" y2="6" stroke="#111827" strokeWidth="1.8" strokeDasharray="1.4 2.4" strokeLinecap="round" />
      )}
      {detail.glyph === 'zigzag-line' && (
        <path d="M1 8 L4 4 L7 8 L10 4 L13 8 L16 4 L19 8 L21 6" fill="none" stroke="#111827" strokeWidth="1.7" strokeLinejoin="round" strokeLinecap="round" />
      )}
    </Box>
  )
}

interface TimeSpaceSidebarLegendPanelProps {
  items: SidebarItem[]
  directionControls: SidebarDirectionControl[]
  selectedSeries: Record<string, boolean>
  suppressedDirections?: Partial<Record<SidebarDirectionRole, boolean>>
  onSetSeriesVisibility: (seriesNames: string[], visible: boolean) => void
  onToggleDirectionVisibility?: (role: SidebarDirectionRole) => void
  appearanceSettings?: TimeSpaceAppearanceSettings
}

export default function TimeSpaceSidebarLegendPanel({
  items,
  directionControls,
  selectedSeries,
  suppressedDirections = {},
  onSetSeriesVisibility,
  onToggleDirectionVisibility,
  appearanceSettings,
}: TimeSpaceSidebarLegendPanelProps) {
  const [collapsedSections, setCollapsedSections] = useState<
    Record<string, boolean>
  >({})
  const [expandedDetails, setExpandedDetails] = useState<Record<string, boolean>>(
    {}
  )

  const sectionCategories = CATEGORY_ORDER.filter((category) =>
    items.some((item) => item.category === category)
  )
  const hasGlobalSectionToggle = sectionCategories.length > 1
  const allSectionsCollapsed =
    hasGlobalSectionToggle &&
    sectionCategories.every((category) => collapsedSections[category] === true)

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

  const setSectionVisibility = (sectionItems: SidebarItem[], visible: boolean) => {
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

  return (
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
            const isSuppressed = suppressedDirections[control.role] === true
            const roleLabel = getDirectionRoleLabel(control.role)

            return (
              <DirectionToggleButton
                key={control.role}
                role={control.role}
                isSelected={!isSuppressed}
                onClick={() => onToggleDirectionVisibility?.(control.role)}
                ariaLabel={`Toggle ${control.role} direction`}
                title={isSuppressed ? `Show ${roleLabel}` : `Hide ${roleLabel}`}
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
        const categoryItems = items.filter((item) => item.category === category)
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
        const showSectionVisibilityToggle = availableCategoryItems.length > 1
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
              <Typography variant="subtitle2" sx={{ fontSize: '0.85rem' }}>
                {category}
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.1 }}>
                {showSectionUnavailableIcon ? (
                  <UnavailableStatusIcon
                    ariaLabel={`${category} unavailable`}
                    title={categoryUnavailableReason ?? ''}
                    iconSize={17}
                  />
                ) : showSectionVisibilityToggle ? (
                  <Tooltip title={hasVisibleItems ? 'Hide all' : 'Show all'}>
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
                    isCollapsed ? `Expand ${category}` : `Collapse ${category}`
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
                    <Box sx={{ display: 'flex', gap: 0, alignItems: 'stretch' }}>
                      <PreviewCard
                        kind={item.preview}
                        appearanceSettings={appearanceSettings}
                      />
                      <Box sx={{ minWidth: 0, flex: 1, p: 0.9 }}>
                        <Box
                          sx={{
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'space-between',
                            gap: 0.5,
                          }}
                        >
                          <Typography variant="subtitle2" sx={{ fontSize: '0.8rem' }}>
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
                                  onClick={() => toggleItemDetails(item.key)}
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
                                  border: '1px solid rgba(203, 213, 225, 0.9)',
                                }}
                              >
                                {directionalToggles.map((toggle) => {
                                  const role = toggle.directionRole
                                  if (!role) {
                                    return null
                                  }

                                  const isSelected = isToggleRequestedVisible(
                                    toggle,
                                    selectedSeries
                                  )
                                  const isSuppressed =
                                    suppressedDirections[role] === true
                                  const roleLabel = getDirectionRoleLabel(role)

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
                                      title={`${
                                        isSelected ? 'Hide' : 'Show'
                                      } ${roleLabel}`}
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
                                      setItemVisibility(item, !allItemSeriesVisible)
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

                        {getRenderedItemDetails(item, appearanceSettings)?.length &&
                        expandedDetails[item.key] ? (
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
  )
}
