// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getRoute.ts
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

import { Route } from '@/features/routes/types'
import { configAxios } from '@/lib/axios'

export const getRoute = async (
  id: string | undefined
): Promise<ApiResponse<Route>> => {
  if (!id) {
    throw new Error('Route ID is required')
  }
  return await configAxios.get(`/Route/${id}?expand=routeLocations`)
}

type QueryFnType = typeof getRoute

type UseRoutesOptions = {
  id: string | undefined
  config?: QueryConfig<QueryFnType>
}

export const useGetRoute = ({ id, config }: UseRoutesOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: id ? true : false,
    queryKey: ['route', id],
    queryFn: () => getRoute(id),
  })
}
