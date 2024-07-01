import { REPORTS_URL } from '@/config'
import { RawApproachSpeedData } from '@/features/charts/approachSpeed/types'
import { RawApproachVolumeData } from '@/features/charts/approachVolume/types'
import { RawArrivalsOnRedData } from '@/features/charts/arrivalsOnRed/types'
import { RawGreenTimeUtilizationData } from '@/features/charts/greenTimeUtilization/types'
import { RawLeftTurnGapData } from '@/features/charts/leftTurnGapAnalysis/types'
import { RawPedestrianDelayData } from '@/features/charts/pedestrianDelay/types'
import { RawPreemptionDetailsResponse } from '@/features/charts/preemptionDetails/types'
import { RawPurdueCoordinationDiagramData } from '@/features/charts/purdueCoordinationDiagram/types'
import { RawPurduePhaseTerminationData } from '@/features/charts/purduePhaseTermination/types'
import { RawPurdueSplitFailureData } from '@/features/charts/purdueSplitFailure/types'
import { RampMeteringData } from '@/features/charts/rampMetering/types'
import { RawSplitMonitorData } from '@/features/charts/splitMonitor/types'
import {
  RawTimeSpaceAverageData,
  RawTimeSpaceHistoricData,
} from '@/features/charts/timeSpaceDiagram/types'
import { RawTimingAndActuationData } from '@/features/charts/timingAndActuation/types'
import { RawTurningMovementCountsData } from '@/features/charts/turningMovementCounts/types'
import { RawWaitTimeData } from '@/features/charts/waitTime/types'
import { RawYellowAndRedActuationsData } from '@/features/charts/yellowAndRedActuations/types'
import { approachSpeedData } from '@/mocks/data/charts/ApproachSpeedData'
import { arrivalsOnRedData } from '@/mocks/data/charts/ArrivalsOnRedData'
import { phaseTerminationData } from '@/mocks/data/charts/PhaseTerminationData'
import { preemptDetailsData } from '@/mocks/data/charts/PreemptServiceData'
import { approachDelayData } from '@/mocks/data/charts/approachDelayData'
import { approachVolumeData } from '@/mocks/data/charts/approachVolumeData'
import { greenTimeUtilizationData } from '@/mocks/data/charts/greenTimeUtilizationData'
import { leftTurnGapAnalysisData } from '@/mocks/data/charts/leftTurnGapAnalysisData'
import { pedestrianDelayData } from '@/mocks/data/charts/pedestrianDelayData'
import { purdueCoordinationDiagramData } from '@/mocks/data/charts/purdueCoordinationDiagram'
import { purdueSplitFailureData } from '@/mocks/data/charts/purdueSplitFailureData'
import { splitMonitorData } from '@/mocks/data/charts/splitMonitorData'
import { timingAndActuationData } from '@/mocks/data/charts/timingAndActuationData'
import { turningMovementCountsData } from '@/mocks/data/charts/turningMovementCountsData'
import { waitTimeData } from '@/mocks/data/charts/waitTimeData'
import { yellowAndRedActuationsData } from '@/mocks/data/charts/yellowAndRedActuations'
import { rest } from 'msw'
import { rampMeteringData } from '../data/charts/rampMeteringData'
import { timeSpaceAverageData } from '../data/charts/timeSpaceAverageData'
import { timeSpaceData } from '../data/charts/timeSpaceData'

const reports = (path: string) => `${REPORTS_URL}${path}`

export const rampMeteringHandler = [
  rest.post(reports('RampMetering/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RampMeteringData[]>(rampMeteringData))
  }),
]

export const chartHandlers = [
  rest.post(reports('ApproachDelay/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RawApproachDelayData[]>(approachDelayData))
  }),
  rest.post(reports('PhaseTermination/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RawPurduePhaseTerminationData>(phaseTerminationData))
  }),
  rest.post(reports('ArrivalOnRed/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RawArrivalsOnRedData[]>(arrivalsOnRedData))
  }),
  rest.post(reports('PedDelay/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RawPedestrianDelayData[]>(pedestrianDelayData))
  }),
  rest.post(
    reports('PurdueCoordinationDiagram/GetReportData'),
    (_req, res, ctx) => {
      return res(
        ctx.json<RawPurdueCoordinationDiagramData[]>(
          purdueCoordinationDiagramData
        )
      )
    }
  ),
  rest.post(reports('SplitFail/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RawPurdueSplitFailureData[]>(purdueSplitFailureData))
  }),
  rest.post(
    reports('TurningMovementCounts/GetReportData'),
    (_req, res, ctx) => {
      return res(
        ctx.json<RawTurningMovementCountsData[]>(turningMovementCountsData)
      )
    }
  ),
  rest.post(reports('ApproachSpeed/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RawApproachSpeedData[]>(approachSpeedData))
  }),
  rest.post(reports('SplitMonitor/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RawSplitMonitorData[]>(splitMonitorData))
  }),
  rest.post(reports('TimingAndActuation/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RawTimingAndActuationData[]>(timingAndActuationData))
  }),
  rest.post(reports('TimeSpaceDiagram/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RawTimeSpaceHistoricData[]>(timeSpaceData))
  }),
  rest.post(
    reports('TimeSpaceDiagramAverage/GetReportData'),
    (_req, res, ctx) => {
      return res(ctx.json<RawTimeSpaceAverageData[]>(timeSpaceAverageData))
    }
  ),
  rest.post(reports('YellowRedActivations/GetReportData'), (_req, res, ctx) => {
    return res(
      ctx.json<RawYellowAndRedActuationsData[]>(yellowAndRedActuationsData)
    )
  }),
  rest.post(reports('ApproachVolume/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RawApproachVolumeData[]>(approachVolumeData))
  }),
  rest.post(reports('LeftTurnGapAnalysis/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RawLeftTurnGapData[]>(leftTurnGapAnalysisData))
  }),
  rest.post(reports('PreemptDetail/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RawPreemptionDetailsResponse>(preemptDetailsData))
  }),
  rest.post(reports('WaitTime/GetReportData'), (_req, res, ctx) => {
    return res(ctx.json<RawWaitTimeData[]>(waitTimeData))
  }),
  rest.post(reports('GreenTimeUtilization/GetReportData'), (_req, res, ctx) => {
    return res(
      ctx.json<RawGreenTimeUtilizationData[]>(greenTimeUtilizationData)
    )
  }),
]
