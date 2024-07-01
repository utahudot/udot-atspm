import transformApproachDelayData from '@/features/charts/approachDelay/transformers'
import transformApproachSpeedData from '@/features/charts/approachSpeed/transformers'
import transformApproachVolumeData from '@/features/charts/approachVolume/transformers'
import transformArrivalsOnRedData from '@/features/charts/arrivalsOnRed/transformers'
import {
  ChartType,
  RawChartResponse,
  RawToolResponse,
  ToolType,
} from '@/features/charts/common/types'
import transformGreenTimeUtilizationData from '@/features/charts/greenTimeUtilization/transformers'
import transformLeftTurnGapAnalysisData from '@/features/charts/leftTurnGapAnalysis/transformers'
import transformPedestrianDelayData from '@/features/charts/pedestrianDelay/transformers'
import transformPreemptionDetailsData from '@/features/charts/preemptionDetails/transformers'
import transformPurdueCoordinationDiagramData from '@/features/charts/purdueCoordinationDiagram/transformers'
import transformPurduePhaseTerminationData from '@/features/charts/purduePhaseTermination/transformers'
import transformPurdueSplitFailureData from '@/features/charts/purdueSplitFailure/transformers'
import transformSplitMonitorData from '@/features/charts/splitMonitor/tranformers'
import transformTimingAndActuationData from '@/features/charts/timingAndActuation/transformers'
import transformTurningMovementCountsData from '@/features/charts/turningMovementCounts/transformers'
import {
  TransformedChartResponse,
  TransformedToolResponse,
} from '@/features/charts/types'
import transformWaitTimeData from '@/features/charts/waitTime/transformers'
import transformYellowAndRedActuationsData from '@/features/charts/yellowAndRedActuations/transformers'
import transformRampMeteringData from '../rampMetering/transformer'
import transformTimeSpaceAverageData from '../timeSpaceDiagram/transformers/timeSpaceAverageTransformer'
import transformTimeSpaceHistoricData from '../timeSpaceDiagram/transformers/timeSpaceHistoricTransformer'

export const transformChartData = (
  response: RawChartResponse
): TransformedChartResponse => {
  switch (response.type) {
    case ChartType.ApproachDelay:
      return transformApproachDelayData(response)
    case ChartType.ApproachSpeed:
      return transformApproachSpeedData(response)
    case ChartType.ApproachVolume:
      return transformApproachVolumeData(response)
    case ChartType.ArrivalsOnRed:
      return transformArrivalsOnRedData(response)
    case ChartType.PurdueCoordinationDiagram:
      return transformPurdueCoordinationDiagramData(response)
    case ChartType.GreenTimeUtilization:
      return transformGreenTimeUtilizationData(response)
    case ChartType.LeftTurnGapAnalysis:
      return transformLeftTurnGapAnalysisData(response)
    case ChartType.PedestrianDelay:
      return transformPedestrianDelayData(response)
    case ChartType.PurduePhaseTermination:
      return transformPurduePhaseTerminationData(response)
    case ChartType.PreemptionDetails:
      return transformPreemptionDetailsData(response)
    case ChartType.PurdueSplitFailure:
      return transformPurdueSplitFailureData(response)
    case ChartType.SplitMonitor:
      return transformSplitMonitorData(response)
    case ChartType.TimingAndActuation:
      return transformTimingAndActuationData(response)
    case ChartType.TurningMovementCounts:
      return transformTurningMovementCountsData(response)
    case ChartType.WaitTime:
      return transformWaitTimeData(response)
    case ChartType.YellowAndRedActuations:
      return transformYellowAndRedActuationsData(response)
    case ChartType.RampMetering:
      return transformRampMeteringData(response)
    default:
      throw new Error('Unknown chart type')
  }
}

export const transformToolData = (
  response: RawToolResponse
): TransformedToolResponse => {
  switch (response.type) {
    case ToolType.TimeSpaceHistoric:
      return transformTimeSpaceHistoricData(response)
    case ToolType.TimeSpaceAverage:
      return transformTimeSpaceAverageData(response)
    default:
      throw new Error('Unknown chart type')
  }
}
