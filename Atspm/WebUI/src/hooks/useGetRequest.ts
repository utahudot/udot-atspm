// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - useGetRequest.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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
