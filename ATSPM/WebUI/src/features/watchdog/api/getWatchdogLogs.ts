import { reportsAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { ApiResponse } from '@/types'
import { useQuery } from 'react-query'

import { Watchdog } from '@/features/watchdog/types'

export interface WatchdogReportDataRequestBody {
  start: Date
  end: Date
  areaId?: number | null // Allowing number, undefined, or null
  regionId?: number | null
  jurisdictionId?: number | null
  issueType?: number | null
  locationIdentifier?: string | null
}

export interface WatchDogIssueTypeDTO {
  Id: number
  name: string
}
export const getWatchdogLogs = async (
  requestBody: WatchdogReportDataRequestBody
): Promise<ApiResponse<Watchdog>> => {
  return await reportsAxios.post('/Watchdog/getReportData', requestBody)
}

type QueryFnType = typeof getWatchdogLogs

type UseWatchDogOptions = {
  requestBody: WatchdogReportDataRequestBody
  config?: QueryConfig<QueryFnType>
  enabled?: boolean
}

export const useWatchdogLogs = ({
  requestBody,
  // config,
  enabled = false,
}: UseWatchDogOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    queryKey: ['watchdogLogs', requestBody],
    queryFn: () => getWatchdogLogs(requestBody),
    enabled,
  })
}

export const getIssueTypes = async (): Promise<
  ApiResponse<WatchDogIssueTypeDTO[]>
> => {
  return await reportsAxios.get('/Watchdog/GetIssueTypes')
}
