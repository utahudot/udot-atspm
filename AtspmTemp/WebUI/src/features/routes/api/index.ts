// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - index.ts
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
  return useGetRequest<ApiResponse<Route>>({ route: '/Route', headers })
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
