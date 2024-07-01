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
  return useMutation<AxiosResponse<T>, unknown, D, unknown>({
    onMutate: async (obj: any) => {
      await queryClient.cancelQueries([url])

      const previousObjs = queryClient.getQueryData<any[]>([url])

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
