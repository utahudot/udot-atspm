import {
  AnalysisPeriod,
  DataSource,
} from '@/features/speedManagementTool/enums'
import { RoutesResponse } from '@/features/speedManagementTool/types/routes'
import { speedAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

export interface RouteParams {
  dataSource: DataSource
  startDate: string
  endDate: string
  daysOfWeek: number[]
  analysisPeriod: AnalysisPeriod
  violationThreshold: number
  customStartTime?: Date
  customEndTime?: Date
}

export const getRoutes = async (
  options: RouteParams
): Promise<RoutesResponse> => {
  // options.startDate = dateToTimestamp(options.startDate)
  // options.endDate = dateToTimestamp(options.endDate)
  console.log('options', options)
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
