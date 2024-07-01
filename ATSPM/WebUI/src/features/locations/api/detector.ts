import { useDeleteRequest } from '@/hooks/useDeleteRequest'
import { useGetRequest } from '@/hooks/useGetRequest'
import { usePatchRequest } from '@/hooks/usePatchRequest'
import { usePostRequest } from '@/hooks/usePostRequest'
import { usePutRequest } from '@/hooks/usePutRequest'
import { configAxios } from '@/lib/axios'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

interface Comment {
  id: string | number
  comment: string
  timeStamp: string
  detectorId: string | number
}

const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})
const axiosInstance = configAxios

export function useCreateDetector() {
  const mutation = usePostRequest({ url: '/Detector', axiosInstance, headers })
  return mutation
}

export function useEditDetector() {
  const mutation = usePatchRequest({ url: '/Detector', axiosInstance, headers })
  return mutation
}

export function useDeleteDetector() {
  const mutation = useDeleteRequest({
    url: '/Detector',
    axiosInstance,
    headers,
  })
  return mutation
}

export function usePutDetector() {
  return usePutRequest({
    url: '/Detector',
    axiosInstance,
    headers,
    notify: false,
  })
}

export function useGetDetectorComments(detectorId: string) {
  return useGetRequest<ApiResponse<Comment>>({
    route: `/DetectorComment?detectorId=${detectorId}`,
    axiosInstance,
    headers,
  })
}

export function useCreateDetectorComment() {
  const mutation = usePostRequest({
    url: '/DetectorComment',
    axiosInstance,
    headers,
  })
  return mutation
}

export function useUpdateDetectorComment() {
  const mutation = usePatchRequest({
    url: '/DetectorComment',
    axiosInstance,
    headers,
  })
  return mutation
}

export function useDeleteDetectorComment() {
  const mutation = useDeleteRequest({
    url: '/DetectorComment',
    axiosInstance,
    headers,
  })
  return mutation
}
