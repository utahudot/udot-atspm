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
