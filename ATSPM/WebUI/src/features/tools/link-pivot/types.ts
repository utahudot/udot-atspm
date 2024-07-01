import { ToolType } from '@/features/charts/common/types'
import { RawPurdueCoordinationDiagramData } from '@/features/charts/purdueCoordinationDiagram/types'

export interface LinkPivotAdjustmentOptions {
  startDate: string
  endDate: string
  startTime: string
  endTime: string
  routeId: string
  cycleLength: number
  daysOfWeek: number[]
  direction: 'Downstream' | 'Upstream'
  bias: number
  biasDirection: 'Downstream' | 'Upstream'
}

export interface LinkPivotPcdOptions {
  startDate: string
  endDate: string
  startTime: string
  endTime: string
  locationIdentifier: string
  downstreamLocationIdentifier: string
  downstreamApproachDirection: string
  upstreamApproachDirection: string
  delta: number
}

export type LinkPivotOptions = LinkPivotAdjustmentOptions | LinkPivotPcdOptions

export interface LinkPivotComponentDto {
  locationIdentifier: string
  downstreamLocationIdentifier: string
  downstreamApproachDirection: string
  upstreamApproachDirection: string
  delta: number
}

export interface AdjustmentDto {
  linkNumber: number
  locationIdentifier: string
  location: string
  delta: number
  adjustment: number
}

export interface TransformedAdjustmentDto extends AdjustmentDto {
  editLinkData: number
  existingOffset: number
  newOffset: number
}

export interface ApproachLinksDto {
  locationIdentifier: string
  location: string
  upstreamApproachDirection: string
  downstreamLocationIdentifier: string
  downstreamLocation: string
  downstreamApproachDirection: string
  paogUpstreamBefore: number
  paogUpstreamPredicted: number
  paogDownstreamBefore: number
  paogDownstreamPredicted: number
  aogUpstreamBefore: number
  aogUpstreamPredicted: number
  aogDownstreamBefore: number
  aogDownstreamPredicted: number
  delta: number
  resultChartLocation: null
  upstreamCombinedLocation: string
  downstreamCombinedLocation: string
  aogTotalBefore: number
  pAogTotalBefore: number
  aogTotalPredicted: number
  pAogTotalPredicted: number
  totalChartExisting: number
  totalChartPositiveChange: number
  totalChartNegativeChange: number
  totalChartRemaining: number
  upstreamChartExisting: number
  upstreamChartPositiveChange: number
  upstreamChartNegativeChange: number
  upstreamChartRemaining: number
  downstreamChartExisting: number
  downstreamChartPositiveChange: number
  downstreamChartNegativeChange: number
  downstreamChartRemaining: number
  totalChartName: string
  upstreamChartName: string
  downstreamChartName: string
  linkNumber: number
}

export interface CorridorSummary {
  totalAogDownstreamBefore: number
  totalPaogDownstreamBefore: number
  totalAogDownstreamPredicted: number
  totalPaogDownstreamPredicted: number
  totalAogUpstreamBefore: number
  totalPaogUpstreamBefore: number
  totalAogUpstreamPredicted: number
  totalPaogUpstreamPredicted: number
  totalAogBefore: number
  totalPaogBefore: number
  totalAogPredicted: number
  totalPaogPredicted: number
  totalChartExisting: number
  totalChartPositiveChange: number
  totalChartNegativeChange: number
  totalChartRemaining: number
  totalUpstreamChartExisting: number
  totalUpstreamChartPositiveChange: number
  totalUpstreamChartNegativeChange: number
  totalUpstreamChartRemaining: number
  totalDownstreamChartExisting: number
  totalDownstreamChartPositiveChange: number
  totalDownstreamChartNegativeChange: number
  totalDownstreamChartRemaining: number
}

export interface RawLinkPivotData extends CorridorSummary {
  adjustments: AdjustmentDto[]
  approachLinks: ApproachLinksDto[]
}

export interface RawLinkPivotPcdData
  extends RawExistingPcdData,
    RawPredictedPcdData {}

export interface RawExistingPcdData {
  existingTotalAOG: number
  existingTotalPAOG: number
  existingVolume: number
  pcdExisting: RawPurdueCoordinationDiagramData[]
}

export interface RawPredictedPcdData {
  predictedTotalAOG: number
  predictedTotalPAOG: number
  predictedVolume: number
  pcdPredicted: RawPurdueCoordinationDiagramData[]
}

export interface RawLpPcdData {
  totalAog: number
  totalPAog: number
  volume: number
  pcd: RawPurdueCoordinationDiagramData[]
}

export interface RawLinkPivotPcdResponse {
  type: ToolType.LpPcd
  data: RawLinkPivotPcdData
}
