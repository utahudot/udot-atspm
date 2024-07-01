import { Faq } from '@/features/faq/types'
import { useDeleteRequest } from '@/hooks/useDeleteRequest'
import { useGetRequest } from '@/hooks/useGetRequest'
import { usePatchRequest } from '@/hooks/usePatchRequest'
import { usePostRequest } from '@/hooks/usePostRequest'
import { configAxios } from '@/lib/axios'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

const route = '/faq'
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})
const axiosInstance = configAxios

export function useGetFaqs() {
  return useGetRequest<ApiResponse<Faq>>({ route })
}

export function useCreateFaqs() {
  const mutation = usePostRequest({ url: route, axiosInstance, headers })
  return mutation
}

export function useEditFaqs() {
  const mutation = usePatchRequest({ url: route, axiosInstance, headers })
  return mutation
}

export function useDeleteFaqs() {
  const mutation = useDeleteRequest({ url: route, axiosInstance, headers })
  return mutation
}
