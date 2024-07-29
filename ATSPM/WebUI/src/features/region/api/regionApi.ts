import { regionDto } from '@/features/region/types/regionDto'
import { useDeleteRequest } from '@/hooks/useDeleteRequest'
import { useGetRequest } from '@/hooks/useGetRequest'
import { usePatchRequest } from '@/hooks/usePatchRequest'
import { usePostRequest } from '@/hooks/usePostRequest'
import { configAxios } from '@/lib/axios'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

const route = '/Region'
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})
const axiosInstance = configAxios

export function useGetRegion() {
  return useGetRequest<ApiResponse<regionDto>>({ route })
}

export function useCreateRegion() {
  const mutation = usePostRequest({ url: route, axiosInstance, headers })
  return mutation
}

export function useEditRegion() {
  const mutation = usePatchRequest({ url: route, axiosInstance, headers })
  return mutation
}

export function useDeleteRegion() {
  const mutation = useDeleteRequest({ url: route, axiosInstance, headers })
  return mutation
}
