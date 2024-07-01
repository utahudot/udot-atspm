import areaDto from '@/features/areas/types/areaDto'
import { configAxios } from '@/lib/axios'
import { MutationConfig, queryClient } from '@/lib/react-query'
import { useNotificationStore } from '@/stores/notifications'
import { useMutation } from 'react-query'

type UsePostOptions = {
  apiCall: string
  config?: MutationConfig<typeof createData>
}

interface CreateDataDTO {
  apiCall: string
  data: areaDto
}

export const createData = ({ apiCall, data }: CreateDataDTO): Promise<any> => {
  const response = configAxios.post(apiCall, data)

  return response
}

export const useCreateData = ({ apiCall, config = {} }: UsePostOptions) => {
  const { addNotification } = useNotificationStore()
  return useMutation({
    onMutate: async (obj: any) => {
      await queryClient.cancelQueries([apiCall])

      const previousObjs = queryClient.getQueryData<any[]>([apiCall])

      queryClient.setQueryData([apiCall], [...(previousObjs || []), obj.data])

      return { previousObjs }
    },
    onError: (_, __, context: any) => {
      if (context?.previousObjs) {
        queryClient.setQueryData([apiCall], context.previousObjs)
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries([apiCall])
      addNotification({
        type: 'success',
        title: `${apiCall} Created`,
      })
    },
    ...config,
    mutationFn: createData,
  })
}
