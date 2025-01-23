// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getLeftTurnGapReportDataCheck.ts
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
  approachIds: string[]
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
