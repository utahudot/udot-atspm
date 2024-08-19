// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - usePostRequest.ts
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
import { queryClient } from '@/lib/react-query'
import { useNotificationStore } from '@/stores/notifications'
import { AxiosHeaders, AxiosInstance, AxiosResponse } from 'axios'
import { useMutation } from 'react-query'

interface postRequestDto<D> {
  url: string
  data: D
  axiosInstance: AxiosInstance
  headers: AxiosHeaders
}

export async function postRequest<T, D>({
  url,
  data,
  axiosInstance,
  headers,
}: postRequestDto<D>): Promise<AxiosResponse<T, any>> {
  const result = axiosInstance.post<T>(url, data, { headers })
  return result
}

type UseDataOptions = {
  url: string
  axiosInstance?: AxiosInstance
  headers: AxiosHeaders
  notify?: boolean
}

export function usePostRequest<T, D>({
  url,
  axiosInstance = configAxios,
  headers,
  notify = true,
}: UseDataOptions) {
  const { addNotification } = useNotificationStore()
  return useMutation<AxiosResponse<T>, unknown, D, unknown>({
    onMutate: async (data: D) => {
      await queryClient.cancelQueries([url])
      const previousObjs = queryClient.getQueryData<any[]>([url])
      queryClient.setQueryData([url], [...(previousObjs || []), data])
      return { previousObjs }
    },
    onError: (_: any, __: any, context: any) => {
      if (notify)
        addNotification({
          type: 'error',
          title: `${url.replace('/', '')} Failed`,
        })
      if (context?.previousObjs) {
        queryClient.setQueryData([url], context.previousObjs)
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries([url])
      if (notify)
        addNotification({
          type: 'success',
          title: `${url.replace('/', '')} Created`,
        })
    },
    mutationFn: async (data: D) =>
      postRequest<T, D>({ url, data, axiosInstance, headers }),
  })
}
