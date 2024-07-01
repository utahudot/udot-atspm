import {
  ChartOptions,
  ChartType,
  RawChartResponse,
} from '@/features/charts/common/types'
import { TransformedChartResponse } from '@/features/charts/types'
import { reportsAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { dateToTimestamp } from '@/utils/dateTime'
import { useQuery } from 'react-query'
import { transformChartData } from './transformData'

export const TypeApiMap: Record<ChartType, string> = {
  [ChartType.ApproachDelay]: '/ApproachDelay/GetReportData',
  [ChartType.ApproachSpeed]: '/ApproachSpeed/GetReportData',
  [ChartType.ApproachVolume]: '/ApproachVolume/GetReportData',
  [ChartType.ArrivalsOnRed]: '/ArrivalOnRed/GetReportData',
  [ChartType.PurdueCoordinationDiagram]:
    '/PurdueCoordinationDiagram/GetReportData',
  [ChartType.GreenTimeUtilization]: '/GreenTimeUtilization/GetReportData',
  [ChartType.LeftTurnGapAnalysis]: '/LeftTurnGapAnalysis/GetReportData',
  [ChartType.PedestrianDelay]: '/PedDelay/GetReportData',
  [ChartType.PurduePhaseTermination]: '/PurduePhaseTermination/GetReportData',
  [ChartType.PreemptionDetails]: '/PreemptDetail/GetReportData',
  [ChartType.PurdueSplitFailure]: '/SplitFail/GetReportData',
  [ChartType.SplitMonitor]: '/SplitMonitor/GetReportData',
  [ChartType.TimingAndActuation]: '/TimingAndActuation/GetReportData',
  [ChartType.TurningMovementCounts]: '/TurningMovementCounts/GetReportData',
  [ChartType.WaitTime]: '/WaitTime/GetReportData',
  [ChartType.YellowAndRedActuations]: '/YellowRedActivations/GetReportData', // Todo: Fix spelling
  [ChartType.RampMetering]: '/RampMetering/GetReportData',
}

type StringBooleanMap = Record<string, boolean | string | Date>

const mapStringBooleansToBoolean = (obj: ChartOptions) => {
  return Object.entries(obj).reduce<StringBooleanMap>((acc, [key, value]) => {
    // Check if the value is exactly "true" or "false" (case-insensitive)
    if (typeof value === 'string') {
      if (value.toLowerCase() === 'true') {
        acc[key] = true
      } else if (value.toLowerCase() === 'false') {
        acc[key] = false
      } else {
        // If it's a string but not "true" or "false", keep it unchanged
        acc[key] = value
      }
    } else {
      // If the value is not a string, keep it unchanged
      acc[key] = value
    }
    return acc
  }, {})
}

export const getCharts = async (
  type: ChartType,
  options: ChartOptions
): Promise<TransformedChartResponse> => {
  const endpoint = TypeApiMap[type]

  const transformedOptions = mapStringBooleansToBoolean(options)

  transformedOptions.start = dateToTimestamp(transformedOptions.start as Date)
  transformedOptions.end = dateToTimestamp(transformedOptions.end as Date)

  console.log('combinedOptions', transformedOptions.start)

  const response = await reportsAxios.post(endpoint, transformedOptions)

  return transformChartData({
    type: type,
    data: response,
  } as unknown as RawChartResponse)
}

type QueryFnType = typeof getCharts

type UseChartsOptions = BaseOptions & {
  chartType: ChartType
  chartOptions: ChartOptions
}

type BaseOptions = {
  config?: QueryConfig<QueryFnType>
}

export const useCharts = ({
  chartType,
  chartOptions,
  config,
}: UseChartsOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryKey: ['charts', chartType, chartOptions],
    queryFn: () => getCharts(chartType, chartOptions),
  })
}
