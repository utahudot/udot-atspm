import { dataAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'
import { useQuery } from 'react-query'

// Assuming EventLog is the type of each event log entry
export type EventLog = {
  locationIdentifier: string
  timestamp: string
  eventCode: number
  eventParam: number
}

export type GetEventLogsParams = {
  locationIdentifier: string
  start: string
  end: string
  ResponseFormat: ResponseFormat
}

export type ResponseFormat = 'json' | 'xml' | 'csv'

const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

export const getEventLogs = async (
  locationIdentifier: string,
  start: string,
  end: string,
  ResponseFormat: ResponseFormat
): Promise<EventLog[]> => {
  let acceptHeader = ''

  switch (ResponseFormat) {
    case 'json':
      acceptHeader = 'application/json'
      break
    case 'xml':
      acceptHeader = 'application/xml'
      break
    case 'csv':
      acceptHeader = 'text/csv'
      break
  }

  return await dataAxios.get(
    `EventLog/GetArchivedEvents/${locationIdentifier}?start=${start}&end=${end}`,
    { headers }
  )
}

type QueryFnType = typeof getEventLogs

type UseEventLogsOptions = {
  locationIdentifier: string
  start: string
  end: string
  ResponseFormat: ResponseFormat
  config?: QueryConfig<QueryFnType>
}

export const useEventLogs = ({
  locationIdentifier,
  start,
  end,
  ResponseFormat,
  config,
}: UseEventLogsOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryKey: ['eventLogs', locationIdentifier, start, end, ResponseFormat],
    queryFn: () => getEventLogs(locationIdentifier, start, end, ResponseFormat),
  })
}
