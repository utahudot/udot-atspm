import { useQuery } from 'react-query'

import { speedAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { HistoricalDataResponse } from '@/types/historicalData'

export interface HistoricalDataParams {
  segmentId: string
  startDate: string
  endDate: string
  daysOfWeek: number[]
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
  const response = await speedAxios.post<HistoricalDataResponse>(
    `api/v1/SpeedManagement/GetHistoricalSpeeds`,
    requestBody
  )
  return response
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
