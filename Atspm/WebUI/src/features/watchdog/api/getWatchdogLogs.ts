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
import { Area } from '@/features/areas/types'
import { reportsAxios } from '@/lib/axios'
import { AxiosHeaders } from 'axios'
import { useMutation, useQuery } from 'react-query'

export interface WatchdogReportDataRequestBody {
  start: string
  end: string
  areaId?: number | null
  regionId?: number | null
  jurisdictionId?: number | null
  issueType?: number | null
  locationIdentifier?: string | null
}

export interface WatchDogIssueTypeDTO {
  id: number
  name: string
}

export interface LogEvent {
  regionId: number
  regionDescription: string
  jurisdictionId: number
  jurisdictionName: string
  areas: Area[]
  id: number
  locationId: number
  locationIdentifier: string | null
  timestamp: string
  componentType: number
  componentId: number
  issueType: number
  details: string
  phase: string | null
}

export interface LogEventsData {
  logEvents: LogEvent[]
  start: string
  end: string
}

// React Query GET request for issue types
export const useGetIssueTypes = () => {
  return useQuery<WatchDogIssueTypeDTO[]>('issueTypes', async () => {
    const response = await reportsAxios.get('/Watchdog/GetIssueTypes')
    return response
  })
}

// React Query POST request for watchdog logs
export const useGetWatchdogLogs = () => {
  return useMutation<LogEventsData, unknown, WatchdogReportDataRequestBody>(
    async (requestBody) => {
      const response = await reportsAxios.post(
        '/Watchdog/getReportData',
        requestBody,
        {
          headers: new AxiosHeaders({
            'Content-Type': 'application/json',
          }),
        }
      )
      return response
    }
  )
}
