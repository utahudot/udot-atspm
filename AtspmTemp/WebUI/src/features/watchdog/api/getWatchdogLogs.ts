// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getWatchdogLogs.ts
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
