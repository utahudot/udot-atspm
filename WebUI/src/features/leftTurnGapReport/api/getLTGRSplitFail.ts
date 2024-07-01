import { useQuery } from 'react-query'

import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'

import { reportsAxios } from '@/lib/axios'
import { LTGRDataCheck } from '../types'

export const getLTGRSplitFail = async (
  approachIds: number[],
  body: LTGRDataCheck
) => {
  const results = await Promise.all(
    approachIds.map(async (approachId) => {
      return await reportsAxios.post(`LeftTurnSplitFail/getReportData`, {
        ...body,
        approachId: approachId,
      })
    })
  )
  return results
}

type QueryFnType = typeof getLTGRSplitFail

type LTGR = {
  config?: QueryConfig<QueryFnType>
  body: LTGRDataCheck
  approachIds: string[]
}

export const useLTGRSplitFail = ({ config, body, approachIds }: LTGR) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['LeftTurnSplitFail', body],
    enabled: false,
    queryFn: () => getLTGRSplitFail(approachIds, body),
  })
}
