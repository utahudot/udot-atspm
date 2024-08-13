// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getLTGRPeakHours.ts
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
