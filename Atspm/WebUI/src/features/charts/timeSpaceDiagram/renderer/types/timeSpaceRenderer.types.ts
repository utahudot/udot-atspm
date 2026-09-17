// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceRenderer.types.ts
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
import type {
  GpxUploadOptions,
  TimeSpaceDistanceSpacingMode,
} from '@/features/charts/timeSpaceDiagram/shared/types'
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
  distanceSpacingMode?: TimeSpaceDistanceSpacingMode
  onToggleDistanceSpacingMode?: (mode: TimeSpaceDistanceSpacingMode) => void
  sidebarUploadContent?: ReactNode
  isVisible?: boolean
}
