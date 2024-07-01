import { Approach } from '@/features/locations/types'
import { useDeleteRequest } from '@/hooks/useDeleteRequest'
import { useGetRequest } from '@/hooks/useGetRequest'
import { usePostRequest } from '@/hooks/usePostRequest'
import { configAxios } from '@/lib/axios'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})
const axiosInstance = configAxios

export function useGetApproach(id: number) {
  return useGetRequest<ApiResponse<Approach>>({
    route: `/Approach/${id}/?expand=detectors($expand=DetectionTypes, detectorComments)`,
    headers,
    axiosInstance,
  })
}

export function useCreateApproach() {
  const mutation = usePostRequest({ url: '/Approach', axiosInstance, headers })
  return mutation
}

export function useEditApproach() {
  const mutation = usePostRequest({
    url: '/UpsertApproach',
    axiosInstance,
    headers,
  })
  return mutation
}

export function useDeleteApproach() {
  const mutation = useDeleteRequest({
    url: '/Approach',
    axiosInstance,
    headers,
  })
  return mutation
}
