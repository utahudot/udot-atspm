import { useQuery } from 'react-query'

import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'

import { reportsAxios } from '@/lib/axios'

export const getLeftTurnGapReportDataCheck = async (approachIds, body) => {
  const result = await Promise.all(
    approachIds.map(async (approachId) => {
      return await reportsAxios.post(
        `LeftTurnGapReportDataCheck/getReportData`,
        { ...body, approachId: approachId }
      )
    })
  )
  return result
}

type QueryFnType = typeof getLeftTurnGapReportDataCheck

type UseLeftTurnGapReportOptions = {
  config?: QueryConfig<QueryFnType>
  body: any
  approachIds: number[]
}

export const useLeftTurnGapReportDataCheck = ({
  config,
  body,
  approachIds,
}: UseLeftTurnGapReportOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['LeftTurnGapReportDataCheck', body],
    enabled: false,
    queryFn: () => getLeftTurnGapReportDataCheck(approachIds, body),
  })
}
