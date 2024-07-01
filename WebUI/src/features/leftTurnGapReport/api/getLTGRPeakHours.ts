import { ApiResponse } from '@/types'
import { useQuery } from 'react-query'

import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'

import { reportsAxios } from '@/lib/axios'
import { peakHours } from '../types'

export const getLTGRPeakHours = async (body: peakHours): Promise<any> => {
  const result: ApiResponse<any> = await reportsAxios.post(
    `LeftTurnPeakHours/getReportData`,
    body
  )
  return result
}

type QueryFnType = typeof getLTGRPeakHours

type UseLTGRPeakHours = {
  config?: QueryConfig<QueryFnType>
  body: peakHours
}

export const useLTGRPeakHours = ({ config, body }: UseLTGRPeakHours) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['LeftTurnPeakHours', body],
    enabled: false,
    queryFn: () => getLTGRPeakHours(body),
  })
}
