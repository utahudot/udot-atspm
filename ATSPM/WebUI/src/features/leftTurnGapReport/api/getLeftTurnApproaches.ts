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
