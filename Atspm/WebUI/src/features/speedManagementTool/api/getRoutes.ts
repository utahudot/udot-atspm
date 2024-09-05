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
  console.log('options', options)
  // switch (options.analysisPeriod) {
  //   case AnalysisPeriod.AMPeak:
  //     options.startTime = new Date('2021-01-01T06:00:00')
  //     options.endTime = new Date('2021-05-01T09:00:00')
  //     break
  //   case AnalysisPeriod.PMPeak:
  //     options.startTime = new Date('2021-01-01T15:00:00')
  //     options.endTime = new Date('2021-05-01T18:00:00')
  //     break
  //   case AnalysisPeriod.MidDay:
  //     options.startTime = new Date('2021-01-01T10:00:00')
  //     options.endTime = new Date('2021-05-01T14:00:00')
  //     break
  //   case AnalysisPeriod.Evening:
  //     options.startTime = new Date('2021-01-01T18:00:00')
  //     options.endTime = new Date('2021-05-01T21:00:00')
  //     break
  //   case AnalysisPeriod.EarlyMorning:
  //     options.startTime = new Date('2021-01-01T03:00:00')
  //     options.endTime = new Date('2021-05-01T06:00:00')
  //     break
  //   case AnalysisPeriod.OffPeak:
  //     options.startTime = new Date('2021-01-01T21:00:00')
  //     options.endTime = new Date('2025-01-01T03:00:00')
  //     break
  //   default:
  //     options.startTime = new Date('2021-01-01T00:00:00')
  //     options.endTime = new Date('2025-01-01T23:59:59')
  //     break
  // }

  delete options.startTime
  delete options.endTime

  return speedAxios.post('/SpeedManagement/GetRouteSpeeds', options)
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
