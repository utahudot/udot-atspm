import { Location, LocationExpanded } from '@/features/locations/types'
import { useDeleteRequest } from '@/hooks/useDeleteRequest'
import { useGetRequest } from '@/hooks/useGetRequest'
import { usePostRequest } from '@/hooks/usePostRequest'
import { usePutRequest } from '@/hooks/usePutRequest'
import { configAxios } from '@/lib/axios'
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
const axiosInstance = configAxios

export function useGetLocation(id: string) {
  return useGetRequest<ApiResponse<LocationExpanded>>({
    route: `/Location/${id}?$expand=areas, devices, approaches($expand=Detectors($expand=DetectionTypes, detectorComments))`,
    headers,
    enabled: false,
  })
}

export function useCreateLocation() {
  return usePostRequest({ url: route, axiosInstance, headers })
}

export function useEditLocation() {
  return usePutRequest({ url: route, axiosInstance, headers })
}

export function useDeleteVersion() {
  return useDeleteRequest({ url: route, axiosInstance, headers })
}

export function useCopyLocationToNewVersion(id: string) {
  return usePostRequest({
    url: `${route}/${id}/CopyLocationToNewVersion`,
    axiosInstance,
    headers,
  })
}

export function useSetLocationToBeDeleted(id: string) {
  return usePostRequest({
    url: `${route}/${id}/SetLocationToDeleted`,
    axiosInstance,
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
    axiosInstance,
    headers,
    config,
  })
}

export const useLatestVersionOfAllLocations = (
  config?: UseQueryOptions<Location, unknown, Location, QueryKey>
) => {
  return useGetRequest({
    route: `${route}/GetLocationsForSearch?count=false`,
    axiosInstance,
    headers,
    config: {
      ...config,
      queryKey: ['locations'],
      staleTime: 5 * 60 * 1000, // 5 minutes
      cacheTime: Infinity,
    },
  })
}
