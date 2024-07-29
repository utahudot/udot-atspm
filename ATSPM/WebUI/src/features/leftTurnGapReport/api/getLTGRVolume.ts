import { useQuery } from 'react-query'

import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'

import { reportsAxios } from '@/lib/axios'

export const getLTGRVolume = async (approachIds: number[], body: any) => {
  const results = await Promise.all(
    approachIds.map(async (approachId) => {
      return await reportsAxios.post(`LeftTurnVolume/getReportData`, {
        ...body,
        approachId: approachId,
      })
    })
  )
  return results
}

type QueryFnType = typeof getLTGRVolume

type LTGR = {
  config?: QueryConfig<QueryFnType>
  body: any
  approachIds: number[]
}

export const useLTGRVolume = ({ config, body, approachIds }: LTGR) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['LeftTurnVolume', body],
    enabled: false,
    queryFn: () => getLTGRVolume(approachIds, body),
  })
}
