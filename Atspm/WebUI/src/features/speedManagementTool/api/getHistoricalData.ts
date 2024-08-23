import axios from 'axios'
import { useQuery } from 'react-query'

import { SPEED_URL } from '@/config'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { HistoricalDataResponse } from '@/types/historicalData'

interface HistoricalDataParams {
  routeId: string
  startDate: string
  endDate: string
  startTime: string
  endTime: string
  daysOfWeek: string
}

export const getHistoricalData = async (
  params: HistoricalDataParams
): Promise<HistoricalDataResponse> => {
  // Modify the startDate before sending the request
  const newStartDate = getPriorDate(params.startDate)

  // Construct the request body
  const requestBody = {
    ...params,
    startDate: newStartDate,
  }

  // Make the POST request with params in the body
  const response = await axios.post<HistoricalDataResponse>(
    `${SPEED_URL}/SpeedManagement/GetHistoricalSpeeds`,
    {
      segmentId: params.routeId,
      startDate: newStartDate,
      endDate: '2024-08-21',
      daysOfWeek: [0, 2, 3, 4, 5, 6],
    }
  )
  return response.data
}

export function getPriorDate(dateString: string) {
  const date = new Date(dateString)
  date.setMonth(date.getMonth() - 13)
  return date.toISOString().slice(0, 10)
}

type QueryFnType = typeof getHistoricalData

type UseHistoricalDataOptions = {
  params: HistoricalDataParams
  config?: QueryConfig<QueryFnType>
}

export const useHistoricalData = ({
  params,
  config,
}: UseHistoricalDataOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['historicalData', params],
    queryFn: () => getHistoricalData(params),
  })
}
