import { useQuery } from 'react-query'

import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'

import { reportsAxios } from '@/lib/axios'
import { LTGRDataCheck } from '../types'

export const getLTGRPedActuation = async (
  approachIds: number[],
  body: LTGRDataCheck
) => {
  const results = await Promise.all(
    approachIds.map(async (approachId) => {
      return await reportsAxios.post(`LeftTurnPedActuation/getReportData`, {
        ...body,
        approachId: approachId,
      })
    })
  )
  return results
}

type QueryFnType = typeof getLTGRPedActuation

type useLTGRPedActu = {
  config?: QueryConfig<QueryFnType>
  body: LTGRDataCheck
  approachIds: string[]
}

export const useLTGRPedActuation = ({
  config,
  body,
  approachIds,
}: useLTGRPedActu) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['LeftTurnPedActuation', body],
    enabled: false,
    queryFn: () => getLTGRPedActuation(approachIds, body),
  })
}
