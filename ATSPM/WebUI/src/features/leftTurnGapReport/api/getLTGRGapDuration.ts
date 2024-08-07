// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getLTGRGapDuration.ts
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
