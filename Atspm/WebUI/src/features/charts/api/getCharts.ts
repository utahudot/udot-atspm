// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getCharts.ts
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
  [ChartType.ApproachDelay]: '/api/v1/ApproachDelay/GetReportData',
  [ChartType.ApproachSpeed]: '/api/v1/ApproachSpeed/GetReportData',
  [ChartType.ApproachVolume]: '/api/v1/ApproachVolume/GetReportData',
  [ChartType.ArrivalsOnRed]: '/api/v1/ArrivalOnRed/GetReportData',
  [ChartType.PurdueCoordinationDiagram]:
    '/api/v1/PurdueCoordinationDiagram/GetReportData',
  [ChartType.GreenTimeUtilization]:
    '/api/v1/GreenTimeUtilization/GetReportData',
  [ChartType.LeftTurnGapAnalysis]: '/api/v1/LeftTurnGapAnalysis/GetReportData',
  [ChartType.PedestrianDelay]: '/api/v1/PedDelay/GetReportData',
  [ChartType.PurduePhaseTermination]:
    '/api/v1/PurduePhaseTermination/GetReportData',
  [ChartType.PreemptionDetails]: '/api/v1/PreemptDetail/GetReportData',
  [ChartType.PurdueSplitFailure]: '/api/v1/SplitFail/GetReportData',
  [ChartType.SplitMonitor]: '/api/v1/SplitMonitor/GetReportData',
  [ChartType.TimingAndActuation]: '/api/v1/TimingAndActuation/GetReportData',
  [ChartType.TurningMovementCounts]:
    '/api/v1/TurningMovementCounts/GetReportData',
  [ChartType.WaitTime]: '/api/v1/WaitTime/GetReportData',
  [ChartType.YellowAndRedActuations]:
    '/api/v1/YellowRedActivations/GetReportData', // Todo: Fix spelling
  [ChartType.RampMetering]: '/api/v1/RampMetering/GetReportData',
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
