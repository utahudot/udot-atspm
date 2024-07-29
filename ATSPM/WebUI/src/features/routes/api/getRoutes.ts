import { useGetRequest } from '@/hooks/useGetRequest'
import { ApiResponse } from '@/types'
import { Route } from '../types'

export function useGetRoute() {
  const route = '/Route?expand=RouteLocations'
  return useGetRequest<ApiResponse<Route>>({ route })
}
