// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getLocationApproaches.ts
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

import { LocationApproach } from '@/features/locations/types/LocationApproach'
import { configAxios } from '@/lib/axios'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

const token = Cookies.get('token')

const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

export const getLocationApproaches = async (
  locationId?: string
): Promise<ApiResponse<LocationApproach>> => {
  return await configAxios.get(`Location/${locationId}/approaches`, { headers })
}

type QueryFnType = typeof getLocationApproaches

type UseLocationsOptions = {
  config?: QueryConfig<QueryFnType>
  locationId?: string | undefined
}

export const useLocationApproaches = ({
  config,
  locationId,
}: UseLocationsOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: !!locationId,
    queryKey: ['locationApproaches', locationId],
    queryFn: () => getLocationApproaches(locationId),
  })
}
