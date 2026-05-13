import TimeSpaceSidebarLegendPanel from '@/features/charts/timeSpaceDiagram/renderer/sidebar/TimeSpaceSidebarLegendPanel'
import { TIME_SPACE_GUIDE_WIDTH } from '@/features/charts/timeSpaceDiagram/renderer/sidebar/timeSpaceSidebar.constants'
import {
  buildSidebarModel,
  getStylableItems,
  hasTimeSpaceStyleContent,
  getSidebarDirectionControls,
  type SidebarAvailabilityOverrides,
  type SidebarDirectionControl,
  type SidebarDirectionRole,
} from '@/features/charts/timeSpaceDiagram/renderer/sidebar/timeSpaceSidebarModel'
import TimeSpaceSidebarStylesPanel from '@/features/charts/timeSpaceDiagram/renderer/sidebar/TimeSpaceSidebarStylesPanel'
import { TimeSpaceSidebarTabs } from '@/features/charts/timeSpaceDiagram/renderer/sidebar/TimeSpaceSidebarTabs'
import type { TimeSpaceRendererTab } from '@/features/charts/timeSpaceDiagram/renderer/types/timeSpaceRenderer.types'
import type { Dispatch, ReactNode, SetStateAction } from 'react'
import { useState } from 'react'
import type { EChartsOption } from 'echarts'
import { Box, Divider, Typography } from '@mui/material'
import type { TimeSpaceAppearanceSettings } from '../timeSpaceAppearance'

export type { SidebarDirectionRole, SidebarDirectionControl }
export { getSidebarDirectionControls, hasTimeSpaceStyleContent, TimeSpaceSidebarTabs }
export { TIME_SPACE_GUIDE_WIDTH }

export type SidebarTab = TimeSpaceRendererTab

function getSidebarTabLabel(tab: SidebarTab) {
  if (tab === 'styles') {
    return 'Styles'
  }

  return tab === 'legend' ? 'Legend' : 'Uploads'
}

export interface TimeSpaceSidebarProps {
  option?: EChartsOption
  selectedSeries: Record<string, boolean>
  suppressedDirections?: Partial<Record<SidebarDirectionRole, boolean>>
  onSetSeriesVisibility: (seriesNames: string[], visible: boolean) => void
  onToggleDirectionVisibility?: (role: SidebarDirectionRole) => void
  gpxTracksAvailable?: boolean
  srmTracksAvailable?: boolean
  appearanceSettings?: TimeSpaceAppearanceSettings
  onAppearanceChange?: Dispatch<SetStateAction<TimeSpaceAppearanceSettings>>
  onResetAppearance?: () => void
  uploadContent?: ReactNode
  activeTab?: SidebarTab
  onTabChange?: (tab: SidebarTab) => void
  showTabs?: boolean
}

export default function TimeSpaceSidebar({
  option,
  selectedSeries,
  suppressedDirections = {},
  onSetSeriesVisibility,
  onToggleDirectionVisibility,
  gpxTracksAvailable,
  srmTracksAvailable,
  appearanceSettings,
  onAppearanceChange,
  onResetAppearance,
  uploadContent,
  activeTab: controlledActiveTab,
  onTabChange,
  showTabs = true,
}: TimeSpaceSidebarProps) {
  const availabilityOverrides = {
    'gpx-tracks': gpxTracksAvailable,
    'srm-entity-continuous': srmTracksAvailable,
    'srm-entity-gap': srmTracksAvailable,
  } satisfies SidebarAvailabilityOverrides
  const { directionControls, items } = buildSidebarModel(
    option,
    availabilityOverrides
  )
  const [internalTab, setInternalTab] = useState<SidebarTab>('legend')
  const hasLegendContent = items.length > 0
  const stylableItems = getStylableItems(items)
  const hasStyleContent =
    Boolean(appearanceSettings && onAppearanceChange) &&
    stylableItems.length > 0
  const hasUploadContent = Boolean(uploadContent)
  const availableTabs: SidebarTab[] = []

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
        <TimeSpaceSidebarTabs
          activeTab={activeTab}
          onChange={handleTabChange}
          hasLegendContent={hasLegendContent}
          hasStyleContent={hasStyleContent}
          hasUploadContent={hasUploadContent}
        />
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
          <TimeSpaceSidebarStylesPanel
            items={items}
            appearanceSettings={appearanceSettings}
            onAppearanceChange={onAppearanceChange}
            onResetAppearance={onResetAppearance}
          />
        ) : (
          <TimeSpaceSidebarLegendPanel
            items={items}
            directionControls={directionControls}
            selectedSeries={selectedSeries}
            suppressedDirections={suppressedDirections}
            onSetSeriesVisibility={onSetSeriesVisibility}
            onToggleDirectionVisibility={onToggleDirectionVisibility}
            appearanceSettings={appearanceSettings}
          />
        )}
      </Box>
    </Box>
  )
}
