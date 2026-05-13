import type { GpxUploadOptions } from '@/features/charts/timeSpaceDiagram/shared/types'
import type { EChartsOption } from 'echarts'
import type { CSSProperties, ReactNode } from 'react'

export type TimeSpaceRendererDirectionRole = 'primary' | 'opposing'
export type TimeSpaceRendererTab = 'legend' | 'styles' | 'uploads'

export interface TimeSpaceChartRendererProps {
  id: string
  option: EChartsOption
  style?: CSSProperties
  theme?: 'light' | 'dark'
  gpxEntries?: GpxUploadOptions[]
  ignoredLocations?: string[]
  onToggleIgnoredLocation?: (location: string) => void
  sidebarUploadContent?: ReactNode
  isVisible?: boolean
}
