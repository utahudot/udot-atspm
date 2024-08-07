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
