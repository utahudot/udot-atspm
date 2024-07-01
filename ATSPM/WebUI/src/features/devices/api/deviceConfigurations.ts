import { DeviceConfiguration } from '@/features/devices/types'
import { useDeleteRequest } from '@/hooks/useDeleteRequest'
import { useGetRequest } from '@/hooks/useGetRequest'
import { usePatchRequest } from '@/hooks/usePatchRequest'
import { usePostRequest } from '@/hooks/usePostRequest'
import { configAxios } from '@/lib/axios'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

const route = '/DeviceConfiguration'
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})
const axiosInstance = configAxios

export function useGetDeviceConfigurations() {
  return useGetRequest<ApiResponse<DeviceConfiguration>>({ route })
}

export function useCreateDeviceConfiguration() {
  const mutation = usePostRequest({ url: route, axiosInstance, headers })
  return mutation
}

export function useEditDeviceConfiguration() {
  const mutation = usePatchRequest({ url: route, axiosInstance, headers })
  return mutation
}

export function useDeleteDeviceConfiguration() {
  const mutation = useDeleteRequest({ url: route, axiosInstance, headers })
  return mutation
}
