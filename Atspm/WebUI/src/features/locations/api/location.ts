// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - location.ts
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
import { Location, LocationExpanded } from '@/features/locations/types'
import { useDeleteRequest } from '@/hooks/useDeleteRequest'
import { useGetRequest } from '@/hooks/useGetRequest'
import { usePostRequest } from '@/hooks/usePostRequest'
import { usePutRequest } from '@/hooks/usePutRequest'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'
import { QueryKey, UseQueryOptions } from 'react-query'

const route = '/Location'
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

export function useGetLocation(id: number) {
  return useGetRequest<ApiResponse<LocationExpanded>>({
    route: `/Location/${id}?$expand=areas, devices, approaches($expand=Detectors($expand=DetectionTypes, detectorComments))`,
    headers,
    enabled: false,
  })
}

export function useCreateLocation() {
  return usePostRequest({ url: route, headers })
}

export function useEditLocation() {
  return usePutRequest({ url: route, headers })
}

export function useDeleteVersion() {
  return useDeleteRequest({ url: route, headers })
}

export function useCopyLocationToNewVersion(id: string) {
  return usePostRequest({
    url: `${route}/${id}/CopyLocationToNewVersion`,
    headers,
  })
}

export function useSetLocationToBeDeleted(id: string) {
  return usePostRequest({
    url: `${route}/${id}/SetLocationToDeleted`,
    headers,
    notify: false,
  })
}

export const useAllVersionsOfLocation = (
  locationIdentifier: string,
  config?: UseQueryOptions<
    ApiResponse<Location>,
    unknown,
    ApiResponse<Location>,
    QueryKey
  >
) => {
  return useGetRequest({
    route: `${route}/GetAllVersionsOfLocation(identifier=%27${locationIdentifier}%27)`,
    headers,
    config,
  })
}

export const useLatestVersionOfAllLocations = (
  config?: UseQueryOptions<Location, unknown, Location, QueryKey>
) => {
  return useGetRequest<ApiResponse<Location>>({
    route: `${route}/GetLocationsForSearch?count=false`,
    headers,
    config: {
      ...config,
      queryKey: ['locations'],
      staleTime: 5 * 60 * 1000, // 5 minutes
      cacheTime: Infinity,
    },
  })
}
