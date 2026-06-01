// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - getRoutes.ts
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
import {
  AnalysisPeriod,
  DataSource,
} from '@/features/speedManagementTool/enums'
import { RoutesResponse } from '@/features/speedManagementTool/types/routes'
import { speedAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

export interface RouteParams {
  sourceId: DataSource
  startDate: string
  endDate: string
  daysOfWeek: number[]
  analysisPeriod: AnalysisPeriod
  violationThreshold: number
  startTime?: Date
  endTime?: Date
}

export const getRoutes = async (
  options: RouteParams
): Promise<RoutesResponse> => {
  return speedAxios.post('api/v1/SpeedManagement/GetRouteSpeeds', options)
}

type QueryFnType = typeof getRoutes

type BaseOptions = {
  options: RouteParams
  config?: QueryConfig<QueryFnType>
}

export const useRoutes = ({ options, config }: BaseOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: true,
    queryKey: ['speedRoutes', options],
    queryFn: () => getRoutes(options),
  })
}
