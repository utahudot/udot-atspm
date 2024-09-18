import {
  AnalysisPeriod,
  DataSource,
} from '@/features/speedManagementTool/enums'
import { RoutesResponse } from '@/features/speedManagementTool/types/routes'
import { speedAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

export interface RouteParams {
  sourceId: DataSource
  startDate: string
  endDate: string
  daysOfWeek: number[]
  analysisPeriod: AnalysisPeriod
  violationThreshold: number
  startTime?: Date
  endTime?: Date
}

export const getRoutes = async (
  options: RouteParams
): Promise<RoutesResponse> => {
  const createUtcTime = (hours: number, minutes = 0, seconds = 0) => {
    const date = new Date(Date.UTC(1970, 0, 1))
    date.setUTCHours(hours)
    date.setUTCMinutes(minutes)
    date.setUTCSeconds(seconds)
    date.setUTCMilliseconds(0)
    return date
  }

  const transfomedOptions = JSON.parse(JSON.stringify(options))

  switch (options.analysisPeriod) {
    case AnalysisPeriod.OffPeak:
      transfomedOptions.startTime = createUtcTime(22)
      transfomedOptions.endTime = createUtcTime(4)
      break
    case AnalysisPeriod.AMPeak:
      transfomedOptions.startTime = createUtcTime(6)
      transfomedOptions.endTime = createUtcTime(9)
      break
    case AnalysisPeriod.PMPeak:
      transfomedOptions.startTime = createUtcTime(16)
      transfomedOptions.endTime = createUtcTime(18)
      break
    case AnalysisPeriod.MidDay:
      transfomedOptions.startTime = createUtcTime(9)
      transfomedOptions.endTime = createUtcTime(16)
      break
    case AnalysisPeriod.Evening:
      transfomedOptions.startTime = createUtcTime(18)
      transfomedOptions.endTime = createUtcTime(22)
      break
    case AnalysisPeriod.EarlyMorning:
      transfomedOptions.startTime = createUtcTime(4)
      transfomedOptions.endTime = createUtcTime(6)
      break
    default:
      transfomedOptions.startTime = createUtcTime(0)
      transfomedOptions.endTime = createUtcTime(23, 59, 59)
      break
  }

  return speedAxios.post(
    'api/v1/SpeedManagement/GetRouteSpeeds',
    transfomedOptions
  )
}

type QueryFnType = typeof getRoutes

type BaseOptions = {
  options: RouteParams
  config?: QueryConfig<QueryFnType>
}

export const useRoutes = ({ options, config }: BaseOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: true,
    queryKey: ['speedRoutes', options],
    queryFn: () => getRoutes(options),
  })
}
