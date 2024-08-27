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
import {
  ChartType,
  RawChartResponse,
  RawToolResponse,
  ToolType,
} from '@/features/charts/common/types'
import transformApproachDelayData from '@/features/charts/performanceMeasures/approachDelay/transformers'
import transformApproachSpeedData from '@/features/charts/performanceMeasures/approachSpeed/transformers'
import transformApproachVolumeData from '@/features/charts/performanceMeasures/approachVolume/transformers'
import transformArrivalsOnRedData from '@/features/charts/performanceMeasures/arrivalsOnRed/transformers'
import transformGreenTimeUtilizationData from '@/features/charts/performanceMeasures/greenTimeUtilization/transformers'
import transformLeftTurnGapAnalysisData from '@/features/charts/performanceMeasures/leftTurnGapAnalysis/transformers'
import transformPedestrianDelayData from '@/features/charts/performanceMeasures/pedestrianDelay/transformers'
import transformPreemptionDetailsData from '@/features/charts/performanceMeasures/preemptionDetails/transformers'
import transformPurdueCoordinationDiagramData from '@/features/charts/performanceMeasures/purdueCoordinationDiagram/transformers'
import transformPurduePhaseTerminationData from '@/features/charts/performanceMeasures/purduePhaseTermination/transformers'
import transformPurdueSplitFailureData from '@/features/charts/performanceMeasures/purdueSplitFailure/transformers'
import transformSplitMonitorData from '@/features/charts/performanceMeasures/splitMonitor/tranformers'
import transformTimingAndActuationData from '@/features/charts/performanceMeasures/timingAndActuation/transformers'
import transformTurningMovementCountsData from '@/features/charts/performanceMeasures/turningMovementCounts/transformers'
import transformWaitTimeData from '@/features/charts/performanceMeasures/waitTime/transformers'
import transformYellowAndRedActuationsData from '@/features/charts/performanceMeasures/yellowAndRedActuations/transformers'
import transformRampMeteringData from '@/features/charts/rampMetering/transformer'
import transformTimeSpaceAverageData from '@/features/charts/timeSpaceDiagram/transformers/timeSpaceAverageTransformer'
import transformTimeSpaceHistoricData from '@/features/charts/timeSpaceDiagram/transformers/timeSpaceHistoricTransformer'
import {
  TransformedChartResponse,
  TransformedToolResponse,
} from '@/features/charts/types'

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
