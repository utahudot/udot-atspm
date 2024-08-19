// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - usePutRequest.ts
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

interface PutRequestDto<D> {
  url: string
  data: D
  axiosInstance: AxiosInstance
  headers: AxiosHeaders
}

export async function putRequest<T, D>({
  url,
  data,
  axiosInstance,
  headers,
}: PutRequestDto<D>): Promise<AxiosResponse<T, any>> {
  return axiosInstance.put<T>(url, data, { headers })
}

interface MutationParams<D> {
  data: D
  id: number
}

type UseDataOptions = {
  url: string
  headers: AxiosHeaders
  axiosInstance?: AxiosInstance
  notify?: boolean
}

export function usePutRequest<T, D>({
  url,
  headers,
  axiosInstance = configAxios,
  notify = true,
}: UseDataOptions) {
  const { addNotification } = useNotificationStore()

  return useMutation<AxiosResponse<T>, unknown, MutationParams<D>, unknown>({
    onMutate: async ({ data }) => {
      await queryClient.cancelQueries([url])

      const previousData = queryClient.getQueryData<T>(url)
      // Optimistically update the cache if needed
      queryClient.setQueryData(url, { ...previousData, ...data })

      return { previousData }
    },
    onError: (error, _, context) => {
      if (context?.previousData) {
        queryClient.setQueryData(url, context.previousData)
      }
      if (notify)
        addNotification({
          type: 'error',
          title: `Error updating ${url.replace('/', '')}`,
          message: (error as Error).message || 'An error occurred',
        })
    },
    onSuccess: () => {
      queryClient.invalidateQueries(url)
      if (notify) {
        addNotification({
          type: 'success',
          title: `${url.replace('/', '')} updated successfully`,
        })
      }
    },
    mutationFn: (params: MutationParams<D>) =>
      putRequest<T, D>({
        url: `${url}/${params.id}`,
        data: params.data,
        axiosInstance,
        headers,
      }),
  })
}
