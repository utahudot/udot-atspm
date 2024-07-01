import { RouteDistance } from '@/features/routes/types'
import { useGetRequest } from '@/hooks/useGetRequest'
import { usePatchRequest } from '@/hooks/usePatchRequest'
import { usePostRequest } from '@/hooks/usePostRequest'
import { configAxios } from '@/lib/axios'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

const route = '/RouteDistance'
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})
const axiosInstance = configAxios

export function useUpdateRouteDistance() {
  return usePatchRequest({ url: route, axiosInstance, headers, notify: false })
}

export function useCreateRouteDistance() {
  return usePostRequest({ url: route, axiosInstance, headers, notify: false })
}

export function useDeleteRouteDistance() {
  return usePatchRequest({ url: route, axiosInstance, headers, notify: false })
}

export function useGetRouteDistances() {
  return useGetRequest<ApiResponse<RouteDistance>>({
    route,
    axiosInstance,
    headers,
  })
}
