import { useQuery } from 'react-query'

import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'

import { reportsAxios } from '@/lib/axios'
import { LTGRGapDuration } from '../types'

export const getLTGRGapDuration = async (
  approachIds: any[],
  body: LTGRGapDuration
) => {
  const results = await Promise.all(
    approachIds.map(async (approachId) => {
      return await reportsAxios.post(`LeftTurnGapDuration/getReportData`, {
        ...body,
        approachId: approachId,
      })
    })
  )
  return results
}

type QueryFnType = typeof getLTGRGapDuration

type LTGR = {
  config?: QueryConfig<QueryFnType>
  body: LTGRGapDuration
  approachIds: number[]
}

export const useLTGRGapDuration = ({ config, body, approachIds }: LTGR) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['LeftTurnGapDuration', body],
    enabled: false,
    queryFn: () => getLTGRGapDuration(approachIds, body),
  })
}
