import jurisdictionDto from '@/features/jurisdictions/types/jurisdictionDto'
import { useDeleteRequest } from '@/hooks/useDeleteRequest'
import { useGetRequest } from '@/hooks/useGetRequest'
import { usePatchRequest } from '@/hooks/usePatchRequest'
import { usePostRequest } from '@/hooks/usePostRequest'
import { configAxios } from '@/lib/axios'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

const route = '/Jurisdiction'
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})
const axiosInstance = configAxios

export function useGetJurisdiction() {
  return useGetRequest<ApiResponse<jurisdictionDto>>({ route })
}

export function useCreateJurisdiction() {
  const mutation = usePostRequest({ url: route, axiosInstance, headers })
  return mutation
}

export function useEditJurisdiction() {
  const mutation = usePatchRequest({ url: route, axiosInstance, headers })
  return mutation
}

export function useDeleteJurisdiction() {
  const mutation = useDeleteRequest({ url: route, axiosInstance, headers })
  return mutation
}
