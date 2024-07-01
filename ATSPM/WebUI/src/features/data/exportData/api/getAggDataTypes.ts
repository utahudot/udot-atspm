import { useGetRequest } from '@/hooks/useGetRequest'
import { dataAxios } from '@/lib/axios'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

const route = `/Aggregation/GetDataTypes`
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})
const axiosInstance = dataAxios

export function useGetAggDataTypes() {
  return useGetRequest<ApiResponse<string[]>>({ route, axiosInstance, headers })
}
