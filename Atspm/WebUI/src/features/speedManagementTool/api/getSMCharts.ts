import { transformCongestionTrackerData } from '@/features/charts/speedManagementTool/congestionTracker/congestionTracker.transformer'
import transformSpeedOverDistanceData from '@/features/charts/speedManagementTool/speedOverDistance/components/speedOverDistance.transformer'
import transformSpeedOverTimeData from '@/features/charts/speedManagementTool/speedOverTime/speedOverTime.transformer'
import { TransformedChartResponse } from '@/features/charts/types'
import { speedAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

export enum SM_ChartType {
  CONGESTION_TRACKING = 'Congestion Tracking',
  SPEED_OVER_TIME = 'Speed over Time',
  SPEED_OVER_DISTANCE = 'Speed over Distance',
}

const SM_TypeApiMap = {
  [SM_ChartType.CONGESTION_TRACKING]: '/CongestionTracking/GetReportData',
  [SM_ChartType.SPEED_OVER_TIME]: '/SpeedOverTime/GetReportData',
  [SM_ChartType.SPEED_OVER_DISTANCE]: '/SpeedOverDistance/GetReportData',
}

export interface ChartOptions {
  start: Date
  end: Date
  routeId: number
  sourceId: number
  daysOfWeek: number[]
  timeOfDay: string
  dayOfWeek: number
  month: number
  year: number
  segmentId: number
  direction: string
  speedLimit: number
  speedLimitSourceId: number
  speedLimitSource: string
  speedLimitSourceDescription: string
  speedLimitSourceUrl: string
}

type StringBooleanMap = Record<string, boolean | string | Date>

const mapStringBooleansToBoolean = (obj: ChartOptions) => {
  return Object.entries(obj).reduce<StringBooleanMap>((acc, [key, value]) => {
    if (typeof value === 'string') {
      if (value.toLowerCase() === 'true') {
        acc[key] = true
      } else if (value.toLowerCase() === 'false') {
        acc[key] = false
      } else {
        acc[key] = value
      }
    } else {
      acc[key] = value
    }
    return acc
  }, {})
}

export const getSMCharts = async (
  type: SM_ChartType,
  options: ChartOptions
): Promise<TransformedChartResponse> => {
  const endpoint = SM_TypeApiMap[type]

  const transformedOptions = mapStringBooleansToBoolean(options)

  const response = await speedAxios.post(endpoint, transformedOptions)

  switch (type) {
    case SM_ChartType.CONGESTION_TRACKING:
      return transformCongestionTrackerData(response)
    case SM_ChartType.SPEED_OVER_TIME:
      return transformSpeedOverTimeData(response)
    case SM_ChartType.SPEED_OVER_DISTANCE:
      return transformSpeedOverDistanceData(response)
  }
}

type QueryFnType = typeof getSMCharts

type UseSMChartsOptions = BaseOptions & {
  chartType: SM_ChartType
  chartOptions: ChartOptions
}

type BaseOptions = {
  config?: QueryConfig<QueryFnType>
}

export const useSMCharts = ({
  chartType,
  chartOptions,
  config,
}: UseSMChartsOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryKey: ['sm_charts', chartType, chartOptions],
    queryFn: () => getSMCharts(chartType, chartOptions),
  })
}
