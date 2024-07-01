import { useGetRequest } from '@/hooks/useGetRequest'
import { RouteWithExpandedLocations } from '../types'

type UseDataOptions = {
  routeId: string
  includeLocationDetail: boolean
}

export const useGetRouteWithExpandedLocations = ({
  routeId,
  includeLocationDetail,
}: UseDataOptions) => {
  const route = `GetRouteView/${routeId}?includeLocationDetail=${includeLocationDetail}`
  return useGetRequest<RouteWithExpandedLocations>({ route, enabled: false })
}
