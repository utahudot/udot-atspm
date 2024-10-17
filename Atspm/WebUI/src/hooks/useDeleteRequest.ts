// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - useDeleteRequest.ts
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

interface deleteRequestDto<D> {
  url: string
  data: D
  axiosInstance: AxiosInstance
  headers: AxiosHeaders
}

export async function deleteRequest<T, D>({
  url,
  data,
  axiosInstance,
  headers,
}: deleteRequestDto<D>): Promise<AxiosResponse<T, any>> {
  const result = axiosInstance.delete<T>(`${url}/${data}`, { headers })
  return result
}

type UseDataOptions = {
  url: string
  headers: AxiosHeaders
  axiosInstance?: AxiosInstance
  notify?: boolean
}

export function useDeleteRequest<T, D>({
  url,
  headers,
  axiosInstance = configAxios,
  notify = true,
}: UseDataOptions) {
  const { addNotification } = useNotificationStore()
  return useMutation({
    onMutate: async (obj: any) => {
      await queryClient.cancelQueries([url])

      const previousObjs = queryClient.getQueryData<any[]>([url])

      if (!Array.isArray(previousObjs)) {
        return
      }

      queryClient.setQueryData([url], [...(previousObjs || []), obj.data])

      return { previousObjs }
    },
    onError: (_, __, context: any) => {
      if (context?.previousObjs) {
        queryClient.setQueryData([url], context.previousObjs)
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries([url])
      if (notify) {
        addNotification({
          type: 'error',
          title: `${url.replace('/', '')} Deleted`,
        })
      }
    },
    mutationFn: async (data: D) =>
      deleteRequest<T, D>({ url, data, axiosInstance, headers }),
  })
}
