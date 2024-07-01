import { Route, RouteLocation } from '@/features/routes/types'
import { useDeleteRequest } from '@/hooks/useDeleteRequest'
import { useGetRequest } from '@/hooks/useGetRequest'
import { usePatchRequest } from '@/hooks/usePatchRequest'
import { usePostRequest } from '@/hooks/usePostRequest'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

const token = Cookies.get('token')

const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

export function useGetRoute(id: string) {
  return useGetRequest<Route>({
    enabled: !!id,
    route: `/GetRouteView/${id}`,
    headers,
  })
}

export function useDeleteRoute() {
  return useDeleteRequest({ url: `/Route`, headers })
}

export function useGetRoutes() {
  return useGetRequest<ApiResponse<Route[]>>({ route: '/Route', headers })
}

export function useGetRouteLocations(id: string) {
  return useGetRequest<ApiResponse<RouteLocation[]>>({
    config: {
      enabled: !!id,
    },
    route: `/Route/${id}/routeLocations`,
    headers,
  })
}

export function usePutRoute() {
  return usePostRequest<Route, Route>({
    url: '/UpsertRoute',
    headers,
  })
}

export function useEditRouteLocation() {
  return usePatchRequest({ url: '/RouteLocation', headers, notify: false })
}

export function useDeleteRouteLocation() {
  return useDeleteRequest({ url: '/RouteLocation', headers, notify: false })
}

export function useCreateRouteLocation() {
  return usePostRequest({ url: '/RouteLocation', headers, notify: false })
}
