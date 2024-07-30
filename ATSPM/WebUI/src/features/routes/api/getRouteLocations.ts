import { configAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

export const getLocationRoutes = async (routeId: string) => {
  const response = await configAxios.get(`Route/${routeId}/routeLocations`)

  return response
}

type UseDataOptions = {
  routeId: string
  config?: QueryConfig<any>
}

export const useGetRouteLocations = ({ routeId, config }: UseDataOptions) => {
  return useQuery<ExtractFnReturnType<any>>({
    ...config,
    queryKey: ['Routes'],
    queryFn: () => getLocationRoutes(routeId),
  })
}
