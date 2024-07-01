import { usePostRequest } from '@/hooks/usePostRequest'
import { reportsAxios } from '@/lib/axios'
import { AxiosHeaders } from 'axios'

const route = '/Aggregation/getReportData'
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
})
const axiosInstance = reportsAxios

export function usePostAggregateData() {
  const mutation = usePostRequest({
    url: route,
    axiosInstance,
    headers,
  })
  return mutation
}
