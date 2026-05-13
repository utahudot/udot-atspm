import type {
  LocationToggleButton,
  OffsetResetButton,
} from '@/features/charts/timeSpaceDiagram/renderer/utils/timeSpaceChartGeometry'
import {
  getStickyAxisBackground,
  getStickyTopAxisHeight,
  STICKY_AXIS_LABEL_TEXT_STYLE,
  STICKY_BOTTOM_AXIS_HEIGHT,
  STICKY_BOTTOM_AXIS_LABEL_OVERFLOW,
  STICKY_BOTTOM_LABEL_TOP,
  STICKY_BOTTOM_PANEL_BOTTOM,
  type StickyBottomAxis,
  type StickyTopAxis,
} from '@/features/charts/timeSpaceDiagram/renderer/utils/timeSpaceStickyAxes'
import VisibilityIcon from '@mui/icons-material/Visibility'
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff'
import { Box, IconButton, Tooltip, Typography } from '@mui/material'
import type { ECharts, EChartsOption } from 'echarts'
import type { RefObject } from 'react'

const LOCATION_TOGGLE_ICON_SIZE = 18
const OFFSET_RESET_OVERLAY_OFFSET_X = 15
const OFFSET_RESET_OVERLAY_OFFSET_Y = 15

type TimeSpaceLocationToggleOverlayProps = {
  buttons: LocationToggleButton[]
  ignoredLocations: string[]
  onToggleIgnoredLocation?: (location: string) => void
}

type TimeSpaceOffsetResetOverlayProps = {
  buttons: OffsetResetButton[]
  chart: ECharts | null
}

type TimeSpaceStickyTopAxisOverlayProps = {
  contentPadding: number
  headerHeight: number
  stickyTopAxis: StickyTopAxis | null
}

type TimeSpaceStickyBottomAxisOverlayProps = {
  contentPadding: number
  showStickyPhaseFooterLabels: boolean
  stickyBottomAxis: StickyBottomAxis | null
  stickyBottomAxisOption: EChartsOption | null
  stickyBottomAxisRef: RefObject<HTMLDivElement | null>
  stickyPhaseFooterLabelPositions:
    | {
        primary: number
        opposing: number
      }
    | null
}

export function TimeSpaceLocationToggleOverlay({
  buttons,
  ignoredLocations,
  onToggleIgnoredLocation,
}: TimeSpaceLocationToggleOverlayProps) {
  if (buttons.length === 0 || !onToggleIgnoredLocation) {
    return null
  }

  return (
    <div
      style={{
        position: 'absolute',
        inset: 0,
        pointerEvents: 'none',
      }}
    >
      {buttons.map((button) => {
        const isIgnored = ignoredLocations.includes(button.location)
        const title = isIgnored
          ? `Show location ${button.location}`
          : `Ignore location ${button.location}`

        return (
          <Tooltip key={button.location} title={title} placement="top">
            <IconButton
              size="small"
              onClick={() => onToggleIgnoredLocation(button.location)}
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
  )
}

export function TimeSpaceOffsetResetOverlay({
  buttons,
  chart,
}: TimeSpaceOffsetResetOverlayProps) {
  const activeButtons = buttons.filter((button) => button.active)

  if (activeButtons.length === 0) {
    return null
  }

  return (
    <div
      style={{
        position: 'absolute',
        inset: 0,
        pointerEvents: 'none',
        zIndex: 3,
      }}
    >
      {activeButtons.map((button) => (
        <Tooltip
          key={`offset-reset-${button.location}`}
          title="double click to reset"
          placement="top"
          enterDelay={0}
          PopperProps={{
            modifiers: [
              {
                name: 'offset',
                options: {
                  offset: [0, -10],
                },
              },
            ],
          }}
        >
          <Box
            component="button"
            type="button"
            onDoubleClick={(event) => {
              event.preventDefault()
              event.stopPropagation()

              if (!button.active) {
                return
              }

              const zr = chart?.getZr() as
                | {
                    handler?: {
                      dispatch: (
                        eventName: 'dblclick',
                        eventArgs?: { zrX: number; zrY: number }
                      ) => void
                    }
                  }
                | undefined

              zr?.handler?.dispatch('dblclick', {
                zrX: button.left + button.width / 2,
                zrY: button.top + button.height / 2,
              })
            }}
            role="button"
            sx={{
              pointerEvents: 'auto',
              position: 'absolute',
              left: `${button.left + OFFSET_RESET_OVERLAY_OFFSET_X}px`,
              top: `${button.top + OFFSET_RESET_OVERLAY_OFFSET_Y}px`,
              width: `${button.width}px`,
              height: `${button.height}px`,
              display: 'block',
              p: 0,
              m: 0,
              border: 0,
              backgroundColor: 'transparent',
              cursor: 'pointer',
              '&:hover': {
                backgroundColor: 'rgba(15, 23, 42, 0.04)',
              },
            }}
            aria-label="double click to reset"
          />
        </Tooltip>
      ))}
    </div>
  )
}

export function TimeSpaceStickyTopAxisOverlay({
  contentPadding,
  headerHeight,
  stickyTopAxis,
}: TimeSpaceStickyTopAxisOverlayProps) {
  if (!stickyTopAxis) {
    return null
  }

  const stickyTopAxisHeight = getStickyTopAxisHeight()

  return (
    <Box
      sx={{
        position: 'sticky',
        top: `${headerHeight}px`,
        zIndex: 4,
        height: `${stickyTopAxisHeight}px`,
        marginBottom: `-${stickyTopAxisHeight}px`,
        pointerEvents: 'none',
        px: `${contentPadding}px`,
        boxSizing: 'border-box',
        background: getStickyAxisBackground('top'),
      }}
    >
      <Box
        sx={{
          position: 'relative',
          width: '100%',
          height: '100%',
          overflow: 'visible',
        }}
      >
        {stickyTopAxis.label ? (
          <Typography
            variant="caption"
            sx={{
              position: 'absolute',
              top: 10,
              left: `${Math.max(0, stickyTopAxis.axisStart - 12)}px`,
              transform: 'translateX(-100%)',
              maxWidth: `${Math.max(0, stickyTopAxis.axisStart - 20)}px`,
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
  )
}

export function TimeSpaceStickyBottomAxisOverlay({
  contentPadding,
  showStickyPhaseFooterLabels,
  stickyBottomAxis,
  stickyBottomAxisOption,
  stickyBottomAxisRef,
  stickyPhaseFooterLabelPositions,
}: TimeSpaceStickyBottomAxisOverlayProps) {
  if (!stickyBottomAxisOption || !stickyBottomAxis) {
    return null
  }

  return (
    <Box
      sx={{
        position: 'sticky',
        bottom: `${STICKY_BOTTOM_PANEL_BOTTOM}px`,
        zIndex: 4,
        height: `${STICKY_BOTTOM_AXIS_HEIGHT}px`,
        marginTop: `-${STICKY_BOTTOM_AXIS_HEIGHT}px`,
        pointerEvents: 'none',
        px: `${contentPadding}px`,
        boxSizing: 'border-box',
      }}
    >
      <Box sx={{ position: 'relative', width: '100%', height: '100%' }}>
        <Box
          sx={{
            position: 'absolute',
            top: 0,
            bottom: 0,
            left: 0,
            right: 0,
            background: getStickyAxisBackground('bottom'),
          }}
        />
        {stickyBottomAxis.label ? (
          <Typography
            variant="caption"
            sx={{
              position: 'absolute',
              top: STICKY_BOTTOM_LABEL_TOP,
              left: `${Math.max(0, stickyBottomAxis.axisStart - 24)}px`,
              transform: 'translateX(-100%)',
              maxWidth: `${Math.max(0, stickyBottomAxis.axisStart - 26)}px`,
              ...STICKY_AXIS_LABEL_TEXT_STYLE,
              textAlign: 'right',
            }}
          >
            {stickyBottomAxis.label}
          </Typography>
        ) : null}
        {showStickyPhaseFooterLabels && stickyPhaseFooterLabelPositions ? (
          <>
            <Typography
              variant="caption"
              sx={{
                position: 'absolute',
                top: STICKY_BOTTOM_LABEL_TOP,
                left: `${stickyPhaseFooterLabelPositions.primary}px`,
                transform: 'translateX(-50%)',
                ...STICKY_AXIS_LABEL_TEXT_STYLE,
                textAlign: 'center',
              }}
            >
              primary
            </Typography>
            <Typography
              variant="caption"
              sx={{
                position: 'absolute',
                top: STICKY_BOTTOM_LABEL_TOP,
                left: `${stickyPhaseFooterLabelPositions.opposing}px`,
                transform: 'translateX(-50%)',
                ...STICKY_AXIS_LABEL_TEXT_STYLE,
                textAlign: 'center',
              }}
            >
              opposing
            </Typography>
          </>
        ) : null}
        <Box
          sx={{
            position: 'absolute',
            top: 0,
            bottom: 0,
            left: `${stickyBottomAxis.axisStart - STICKY_BOTTOM_AXIS_LABEL_OVERFLOW}px`,
            width: `${
              Math.max(0, stickyBottomAxis.axisEnd - stickyBottomAxis.axisStart) +
              STICKY_BOTTOM_AXIS_LABEL_OVERFLOW * 2
            }px`,
            pointerEvents: 'none',
          }}
        >
          <Box
            ref={stickyBottomAxisRef}
            sx={{
              position: 'absolute',
              inset: 0,
              pointerEvents: 'auto',
            }}
          />
        </Box>
      </Box>
    </Box>
  )
}
