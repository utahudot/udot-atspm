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
  const { routeId, startDate, endDate, startTime, endTime, daysOfWeek } = params

  const newStartDate = getPriorDate(startDate)
  const url = `${SPEED_URL}/RouteSpeed/historical?routeId=${routeId}&startDate=${newStartDate}&endDate=${endDate}&startTime=${startTime}&endTime=${endTime}&daysOfWeek=${daysOfWeek}`
  const response = await axios.get<HistoricalDataResponse>(url)
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
