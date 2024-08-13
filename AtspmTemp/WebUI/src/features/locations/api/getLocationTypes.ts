// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getLocationTypes.ts
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
import { LocationType } from '@/features/locations/types'
import { configAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { ApiResponse } from '@/types'
import { useQuery } from 'react-query'

export const getLocationTypes = async (): Promise<
  ApiResponse<LocationType>
> => {
  return await configAxios.get('/LocationType')
}

type QueryFnType = typeof getLocationTypes

type UseLocationTypesOptions = {
  config?: QueryConfig<QueryFnType>
}

export const useLocationTypes = ({ config }: UseLocationTypesOptions = {}) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['locationtypes'],
    queryFn: () => getLocationTypes(),
  })
}
