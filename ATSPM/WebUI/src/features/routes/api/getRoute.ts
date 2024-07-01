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
