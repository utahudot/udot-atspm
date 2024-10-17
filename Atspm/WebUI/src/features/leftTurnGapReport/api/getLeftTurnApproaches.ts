// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getLeftTurnApproaches.ts
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

import { configAxios } from '@/lib/axios'

export const getLeftTurnApproaches = async (
  locationId: string
): Promise<any> => {
  const result: ApiResponse<any> = await configAxios.get(
    `Approach?$filter=locationId eq ${locationId} and detectors/any(i:i/movementType eq 'L')&$select=id, description`
  )
  const leftTurnApproaches = result.value
  return leftTurnApproaches
}

type QueryFnType = typeof getLeftTurnApproaches

type UseLocationsOptions = {
  config?: QueryConfig<QueryFnType>
  locationId: string
}

export const useLeftTurnApproaches = ({
  config,
  locationId,
}: UseLocationsOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['approaches', locationId],
    enabled: false,
    queryFn: () => getLeftTurnApproaches(locationId),
  })
}
