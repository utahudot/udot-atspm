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
