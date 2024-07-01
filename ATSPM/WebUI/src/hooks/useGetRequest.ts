import { configAxios } from '@/lib/axios'
import { AxiosHeaders, AxiosInstance } from 'axios'
import { QueryKey, UseQueryOptions, useQuery } from 'react-query'

export async function getRequest<T>(
  route: string,
  axiosInstance: AxiosInstance = configAxios,
  headers?: AxiosHeaders
): Promise<T> {
  return axiosInstance.get<T>(route, { headers }) as unknown as T
}

type UseDataOptions<T> = {
  route: string
  axiosInstance?: AxiosInstance
  config?: UseQueryOptions<T, unknown, T, QueryKey>
  headers?: AxiosHeaders
  enabled?: boolean
}

export function useGetRequest<T>({
  route,
  axiosInstance = configAxios,
  config = {},
  headers,
  enabled = true,
}: UseDataOptions<T>) {
  return useQuery<T, unknown>(
    [route],
    () => getRequest<T>(route, axiosInstance, headers),
    { ...config, enabled }
  )
}
