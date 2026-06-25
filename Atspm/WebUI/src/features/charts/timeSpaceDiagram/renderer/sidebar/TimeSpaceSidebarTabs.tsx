import type { TimeSpaceRendererTab } from '@/features/charts/timeSpaceDiagram/renderer/types/timeSpaceRenderer.types'
import { Tab, Tabs } from '@mui/material'

interface TimeSpaceSidebarTabsProps {
  activeTab: TimeSpaceRendererTab
  onChange: (tab: TimeSpaceRendererTab) => void
  hasLegendContent: boolean
  hasStyleContent: boolean
  hasUploadContent: boolean
  mode?: 'sidebar' | 'header'
}

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
      onChange={(_, value: TimeSpaceRendererTab) => onChange(value)}
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
