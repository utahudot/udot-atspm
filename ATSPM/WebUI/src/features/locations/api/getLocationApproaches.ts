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
