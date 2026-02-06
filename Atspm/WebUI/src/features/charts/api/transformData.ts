// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - transformData.ts
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
import transformApproachDelayData from '@/features/charts/approachDelay/approachDelay.transformer'
import transformApproachSpeedData from '@/features/charts/approachSpeed/approachSpeed.transformer'
import transformApproachVolumeData from '@/features/charts/approachVolume/approachVolume.transformer'
import transformArrivalsOnRedData from '@/features/charts/arrivalsOnRed/arrivalsOnRed.transformer'
import {
  ChartType,
  RawChartResponse,
  ToolType,
} from '@/features/charts/common/types'
import transformGreenTimeUtilizationData from '@/features/charts/greenTimeUtilization/greenTimeUtilization.transformer'
import transformLeftTurnGapAnalysisData from '@/features/charts/leftTurnGapAnalysis/leftTurnGapAnalysis.transformer'
import transformPedestrianDelayData from '@/features/charts/pedestrianDelay/pedestrianDelay.transformer'
import transformPreemptionDetailsData from '@/features/charts/preemptionDetails/preemptionDetails.transformer'
import transformPrioritySummaryData from '@/features/charts/prioritySummary/prioritySummary.transformer'
import transformPurdueCoordinationDiagramData from '@/features/charts/purdueCoordinationDiagram/purdueCoordinationDiagram.transformer'
import transformPurduePhaseTerminationData from '@/features/charts/purduePhaseTermination/purduePhaseTermination.transformer'
import transformPurdueSplitFailureData from '@/features/charts/purdueSplitFailure/purdueSplitFailure.transformer'
import transformRampMeteringData from '@/features/charts/rampMetering/rampMetering.transformer'
import transformSplitMonitorData from '@/features/charts/splitMonitor/splitMonitor.tranformer'
import transformTimeSpaceAverageData from '@/features/charts/timeSpaceDiagram/average/timeSpaceAverage.transformer'
import transformTimeSpaceHistoricData from '@/features/charts/timeSpaceDiagram/historic/timeSpaceHistoric.transformer'
import transformTimingAndActuationData from '@/features/charts/timingAndActuation/timingAndActuation.transformer'
import transformTurningMovementCountsData from '@/features/charts/turningMovementCounts/turningMovementCounts.transformer'
import {
  TransformedChartResponse,
  TransformedTimeSpaceResponse,
} from '@/features/charts/types'
import transformWaitTimeData from '@/features/charts/waitTime/waitTime.transformer'
import transformYellowAndRedActuationsData from '@/features/charts/yellowAndRedActuations/yellowAndRedActuations.transformer'
import { RawTimeSpaceDiagramResponse } from '../timeSpaceDiagram/shared/types'

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
    case ChartType.PrioritySummary:
      return transformPrioritySummaryData(response)
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

export const transformTimeSpaceData = (
  response: RawTimeSpaceDiagramResponse
): TransformedTimeSpaceResponse => {
  switch (response.type) {
    case ToolType.TimeSpaceHistoric:
      return transformTimeSpaceHistoricData(response)
    case ToolType.TimeSpaceAverage:
      return transformTimeSpaceAverageData(response)
    default:
      throw new Error('Unknown chart type')
  }
}
