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
