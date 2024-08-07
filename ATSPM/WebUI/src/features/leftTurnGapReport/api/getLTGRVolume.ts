// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getLTGRVolume.ts
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
