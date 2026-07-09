import {
  getDirectionRoleDisplayLabel,
  getStylableItems,
  type SidebarItem,
  type SidebarDirectionRole,
} from '@/features/charts/timeSpaceDiagram/renderer/sidebar/timeSpaceSidebarModel'
import {
  TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS,
  type TimeSpaceAppearanceSettings,
} from '@/features/charts/timeSpaceDiagram/shared/timeSpaceAppearance'
import { Box, Button, Paper, Slider, Typography } from '@mui/material'
import type { Dispatch, ReactNode, SetStateAction } from 'react'
import { useEffect, useRef, useState } from 'react'

const COLOR_INPUT_COMMIT_DELAY_MS = 150

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
            (Array.isArray(newValue)
              ? newValue[0]
              : (newValue ?? percentValue)) / 100
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
      <Typography variant="subtitle2" sx={{ fontSize: '0.78rem' }}>
        {label}
      </Typography>
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

interface TimeSpaceSidebarStylesPanelProps {
  items: SidebarItem[]
  appearanceSettings?: TimeSpaceAppearanceSettings
  onAppearanceChange?: Dispatch<SetStateAction<TimeSpaceAppearanceSettings>>
  onResetAppearance?: () => void
}

export default function TimeSpaceSidebarStylesPanel({
  items,
  appearanceSettings,
  onAppearanceChange,
  onResetAppearance,
}: TimeSpaceSidebarStylesPanelProps) {
  const stylableItems = getStylableItems(items)
  const styleAppearance =
    appearanceSettings ?? TIME_SPACE_DEFAULT_APPEARANCE_SETTINGS
  const hasLeftTurnStyle = stylableItems.some((item) => item.key === 'left-turn')
  const hasRightTurnStyle = stylableItems.some(
    (item) => item.key === 'right-turn'
  )

  const updateAppearance = (
    updater: (
      current: TimeSpaceAppearanceSettings
    ) => TimeSpaceAppearanceSettings
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
            <AppearanceSectionCard key={item.key} title={item.label}>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.7 }}>
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
                    label="Early Green"
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
                    label="Green phase"
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
            <AppearanceSectionCard key={item.key} title={item.label}>
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

        if (
          item.key === 'left-turn' ||
          (item.key === 'right-turn' && !hasLeftTurnStyle)
        ) {
          const turnEditors = [
            hasLeftTurnStyle || item.key === 'left-turn'
              ? {
                  key: 'left-turn',
                  label: 'Left Turn',
                  appearance: styleAppearance.turns.leftTurn,
                }
              : null,
            hasRightTurnStyle || item.key === 'right-turn'
              ? {
                  key: 'right-turn',
                  label: 'Right Turn',
                  appearance: styleAppearance.turns.rightTurn,
                }
              : null,
          ].filter(Boolean) as Array<{
            key: 'left-turn' | 'right-turn'
            label: string
            appearance: { color: string; opacity: number }
          }>

          return (
            <AppearanceSectionCard key={item.key} title="Turns">
              <Box
                sx={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(auto-fit, minmax(145px, 1fr))',
                  gap: 0.75,
                }}
              >
                {turnEditors.map((turnEditor) => (
                  <DirectionAppearanceEditor
                    key={turnEditor.key}
                    label={turnEditor.label}
                    appearance={turnEditor.appearance}
                    colorAriaLabel={`${turnEditor.label} color`}
                    opacityAriaLabel={`${turnEditor.label} opacity`}
                    onColorChange={(value) =>
                      updateAppearance((current) => ({
                        ...current,
                        turns: {
                          ...current.turns,
                          [turnEditor.key === 'left-turn'
                            ? 'leftTurn'
                            : 'rightTurn']: {
                            ...current.turns[
                              turnEditor.key === 'left-turn'
                                ? 'leftTurn'
                                : 'rightTurn'
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
                          [turnEditor.key === 'left-turn'
                            ? 'leftTurn'
                            : 'rightTurn']: {
                            ...current.turns[
                              turnEditor.key === 'left-turn'
                                ? 'leftTurn'
                                : 'rightTurn'
                            ],
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

        if (item.key === 'right-turn') {
          return null
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
            <AppearanceSectionCard key={item.key} title={item.label}>
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
            <AppearanceSectionCard key={item.key} title="Transit Priority">
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
  )
}
