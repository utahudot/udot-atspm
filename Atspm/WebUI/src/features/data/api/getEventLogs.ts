// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getEventLogs.ts
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
  dataType: string,
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
    `EventLog/GetArchivedEvents/${locationIdentifier}/${dataType}?start=${start}&end=${end}`,
    { headers }
  )
}

type QueryFnType = typeof getEventLogs

type UseEventLogsOptions = {
  locationIdentifier: string
  start: string
  end: string
  dataType: string
  ResponseFormat: ResponseFormat
  config?: QueryConfig<QueryFnType>
}

export const useEventLogs = ({
  locationIdentifier,
  start,
  end,
  dataType,
  ResponseFormat,
  config,
}: UseEventLogsOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryKey: [
      'eventLogs',
      locationIdentifier,
      start,
      ResponseFormat,
      end,
      dataType,
    ],
    queryFn: () =>
      getEventLogs(locationIdentifier, start, end, dataType, ResponseFormat),
  })
}
