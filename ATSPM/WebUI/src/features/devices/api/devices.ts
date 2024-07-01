import { Device } from '@/features/devices/types'
import { useDeleteRequest } from '@/hooks/useDeleteRequest'
import { useGetRequest } from '@/hooks/useGetRequest'
import { usePatchRequest } from '@/hooks/usePatchRequest'
import { usePostRequest } from '@/hooks/usePostRequest'
import { configAxios } from '@/lib/axios'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

const route = '/Device'
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})
const axiosInstance = configAxios

export function useGetDevices() {
  return useGetRequest<ApiResponse<Device>>({ route })
}

export function useGetDevicesForLocation(locationId: string) {
  return useGetRequest<ApiResponse<Device>>({
    route: `/Location/${locationId}/Devices?expand=DeviceConfiguration`,
  })
}

export function useCreateDevice() {
  const mutation = usePostRequest({ url: route, axiosInstance, headers })
  return mutation
}

export function useUpdateDevice() {
  const mutation = usePatchRequest({ url: route, axiosInstance, headers })
  return mutation
}

export function useDeleteDevice() {
  const mutation = useDeleteRequest({ url: route, axiosInstance, headers })
  return mutation
}
