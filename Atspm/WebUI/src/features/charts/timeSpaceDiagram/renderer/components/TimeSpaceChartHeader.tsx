import { TIME_SPACE_GUIDE_WIDTH } from '@/features/charts/timeSpaceDiagram/renderer/sidebar/timeSpaceSidebar.constants'
import { TimeSpaceSidebarTabs } from '@/features/charts/timeSpaceDiagram/renderer/sidebar/TimeSpaceSidebarTabs'
import type { TimeSpaceRendererTab } from '@/features/charts/timeSpaceDiagram/renderer/types/timeSpaceRenderer.types'
import {
  CHART_CONTENT_PADDING,
  FULLSCREEN_PADDING_X,
  GUIDE_EASING,
  GUIDE_TRANSITION_MS,
} from '@/features/charts/timeSpaceDiagram/renderer/utils/timeSpaceChartLayout'
import {
  Box,
  Divider,
  IconButton,
  SvgIcon,
  Tooltip,
  Typography,
} from '@mui/material'
import type { RefObject } from 'react'

const HEADER_TOOLBAR_ICON_SIZE = 18
const HEADER_TOOLBAR_ICON_STROKE_WIDTH = 1.7

type TimeSpaceChartHeaderProps = {
  hasStyleContent: boolean
  hasUploadContent: boolean
  headerRef: RefObject<HTMLDivElement | null>
  isFullscreen: boolean
  isGuideCollapsed: boolean
  onDownloadChart: () => void
  onResetChart: () => void
  onSidebarTabChange: (tab: TimeSpaceRendererTab) => void
  onToggleFullscreen: () => void | Promise<void>
  onToggleGuide: () => void
  onTogglePhaseInfo: () => void
  rangeText: string
  showPhaseInfo: boolean
  sidebarTab: TimeSpaceRendererTab
  titleText: string
}

function PanelSidebarIcon({ side }: { side: 'left' | 'right' }) {
  return (
    <SvgIcon
      viewBox="0 0 24 24"
      sx={{
        fontSize: HEADER_TOOLBAR_ICON_SIZE,
        ...(side === 'left' ? { transform: 'rotate(180deg)' } : undefined),
      }}
    >
      <rect
        x="3"
        y="3"
        width="18"
        height="18"
        rx="2"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
      />
      <path
        d="M15 3v18"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </SvgIcon>
  )
}

function ToolbarActionIcon() {
  return (
    <SvgIcon sx={{ fontSize: HEADER_TOOLBAR_ICON_SIZE }} viewBox="0 0 24 24">
      <path
        d="M12 15V3"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="m7 10 5 5 5-5"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </SvgIcon>
  )
}

function PhaseInfoActionIcon() {
  return (
    <SvgIcon sx={{ fontSize: HEADER_TOOLBAR_ICON_SIZE }} viewBox="0 0 24 24">
      <path
        d="m21 16-4 4-4-4"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M17 20V4"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="m3 8 4-4 4 4"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M7 4v16"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </SvgIcon>
  )
}

function FullscreenActionIcon({ expanded }: { expanded: boolean }) {
  return (
    <SvgIcon sx={{ fontSize: HEADER_TOOLBAR_ICON_SIZE }} viewBox="0 0 24 24">
      {expanded ? (
        <>
          <path
            d="m15 15 6 6m-6-6v4.8m0-4.8h4.8"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M9 19.8V15m0 0H4.2M9 15l-6 6"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M15 4.2V9m0 0h4.8M15 9l6-6"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M9 4.2V9m0 0H4.2M9 9 3 3"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </>
      ) : (
        <>
          <path
            d="m15 15 6 6"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="m15 9 6-6"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M21 16v5h-5"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M21 8V3h-5"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M3 16v5h5"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="m3 21 6-6"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M3 8V3h5"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M9 9 3 3"
            fill="none"
            stroke="currentColor"
            strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </>
      )}
    </SvgIcon>
  )
}

function ResetActionIcon() {
  return (
    <SvgIcon sx={{ fontSize: HEADER_TOOLBAR_ICON_SIZE }} viewBox="0 0 24 24">
      <path
        d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M3 3v5h5"
        fill="none"
        stroke="currentColor"
        strokeWidth={HEADER_TOOLBAR_ICON_STROKE_WIDTH}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </SvgIcon>
  )
}

export function TimeSpaceChartHeader({
  hasStyleContent,
  hasUploadContent,
  headerRef,
  isFullscreen,
  isGuideCollapsed,
  onDownloadChart,
  onResetChart,
  onSidebarTabChange,
  onToggleFullscreen,
  onToggleGuide,
  onTogglePhaseInfo,
  rangeText,
  showPhaseInfo,
  sidebarTab,
  titleText,
}: TimeSpaceChartHeaderProps) {
  return (
    <Box
      ref={headerRef}
      sx={{
        position: 'sticky',
        top: 0,
        zIndex: 5,
        display: 'flex',
        backgroundColor: '#fff',
      }}
    >
      <Box
        sx={{
          flex: 1,
          minWidth: 0,
          px: isFullscreen
            ? `${CHART_CONTENT_PADDING + FULLSCREEN_PADDING_X}px`
            : `${CHART_CONTENT_PADDING}px`,
          py: 1.25,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 2,
          borderBottom: '1px solid',
          borderColor: 'divider',
        }}
      >
        <Box
          sx={{
            flex: '1 1 auto',
            minWidth: 0,
            display: 'flex',
            alignItems: 'baseline',
            gap: 1,
            flexWrap: 'wrap',
          }}
        >
          <Typography
            variant="h5"
            sx={{
              fontSize: '1rem',
              fontWeight: 700,
              lineHeight: 1.2,
              minWidth: 0,
            }}
          >
            {titleText || 'Time Space Diagram'}
          </Typography>
          {rangeText ? (
            <Typography
              variant="body2"
              sx={{
                color: 'text.secondary',
                fontWeight: 500,
                lineHeight: 1.2,
                whiteSpace: 'nowrap',
              }}
            >
              {' \u2022 '}
              {rangeText}
            </Typography>
          ) : null}
        </Box>

        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 0.25,
            flexShrink: 0,
          }}
        >
          <Tooltip
            title={isGuideCollapsed ? 'Show right sidebar' : 'Hide right sidebar'}
            placement="bottom"
          >
            <IconButton
              size="small"
              onClick={onToggleGuide}
              sx={{
                color: isGuideCollapsed ? '#64748B' : '#334155',
                p: 0.45,
                '&:hover': {
                  backgroundColor: 'rgba(15, 23, 42, 0.06)',
                },
              }}
            >
              <PanelSidebarIcon side="right" />
            </IconButton>
          </Tooltip>
          <Tooltip
            title={showPhaseInfo ? 'Hide phase info' : 'Show phase info'}
            placement="bottom"
          >
            <IconButton
              size="small"
              onClick={onTogglePhaseInfo}
              sx={{
                color: showPhaseInfo ? '#334155' : '#64748B',
                p: 0.45,
                '&:hover': {
                  backgroundColor: 'rgba(15, 23, 42, 0.06)',
                },
              }}
            >
              <PhaseInfoActionIcon />
            </IconButton>
          </Tooltip>
          <Tooltip
            title={isFullscreen ? 'Exit fullscreen' : 'Enter fullscreen'}
            placement="bottom"
          >
            <IconButton
              size="small"
              onClick={onToggleFullscreen}
              sx={{
                color: isFullscreen ? '#334155' : '#64748B',
                p: 0.45,
                '&:hover': {
                  backgroundColor: 'rgba(15, 23, 42, 0.06)',
                },
              }}
            >
              <FullscreenActionIcon expanded={isFullscreen} />
            </IconButton>
          </Tooltip>
          <Divider
            orientation="vertical"
            flexItem
            sx={{
              mx: 0.35,
              my: 0.25,
              borderColor: 'rgba(148, 163, 184, 0.55)',
            }}
          />
          <Tooltip title="Download chart" placement="bottom">
            <IconButton
              size="small"
              onClick={onDownloadChart}
              sx={{
                color: '#475569',
                p: 0.45,
                '&:hover': {
                  backgroundColor: 'rgba(15, 23, 42, 0.06)',
                },
              }}
            >
              <ToolbarActionIcon />
            </IconButton>
          </Tooltip>
          <Tooltip title="Reset chart" placement="bottom">
            <IconButton
              size="small"
              onClick={onResetChart}
              sx={{
                color: '#475569',
                p: 0.45,
                '&:hover': {
                  backgroundColor: 'rgba(15, 23, 42, 0.06)',
                },
              }}
            >
              <ResetActionIcon />
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      <Box
        sx={{
          width: isGuideCollapsed ? 0 : TIME_SPACE_GUIDE_WIDTH,
          minWidth: isGuideCollapsed ? 0 : TIME_SPACE_GUIDE_WIDTH,
          flexShrink: 0,
          mr: isFullscreen ? `${FULLSCREEN_PADDING_X}px` : 0,
          display: 'flex',
          alignItems: 'flex-end',
          borderLeft: isGuideCollapsed ? 'none' : '1px solid',
          borderBottom: '1px solid',
          borderColor: 'divider',
          overflow: 'hidden',
          transition: `width ${GUIDE_TRANSITION_MS}ms ${GUIDE_EASING}, min-width ${GUIDE_TRANSITION_MS}ms ${GUIDE_EASING}`,
        }}
      >
        {!isGuideCollapsed && (
          <TimeSpaceSidebarTabs
            activeTab={sidebarTab}
            onChange={onSidebarTabChange}
            hasLegendContent
            hasStyleContent={hasStyleContent}
            hasUploadContent={hasUploadContent}
            mode="header"
          />
        )}
      </Box>
    </Box>
  )
}
