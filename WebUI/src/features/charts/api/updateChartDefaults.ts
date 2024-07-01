import { configAxios } from '@/lib/axios'
import { MutationConfig, queryClient } from '@/lib/react-query'
import { useNotificationStore } from '@/stores/notifications'
import { useMutation } from 'react-query'

type UpdateChartDefault = {
  value: string
  id: number
}

export const updateChartDefaults = ({
  id,
  value,
}: UpdateChartDefault): Promise<UpdateChartDefault> => {
  return configAxios.patch(`/measureOption/${id}`, { value: value.toString() })
}

type UseUpdateChartDefaultsOptions = {
  config?: MutationConfig<typeof updateChartDefaults>
}

export const useUpdateChartDefaults = ({
  config,
}: UseUpdateChartDefaultsOptions = {}) => {
  const { addNotification } = useNotificationStore()
  return useMutation({
    onMutate: async () => {
      await queryClient.cancelQueries('chartDefaults')

      const previousChartDefaults =
        queryClient.getQueryData<UpdateChartDefault[]>('chartDefaults')

      return { previousChartDefaults }
    },
    onError: (_, __, context: any) => {
      if (context?.previousChartDefaults) {
        queryClient.setQueryData('chartDefaults', context.previousChartDefaults)
      }
      addNotification({
        type: 'error',
        title: 'Error Updating Chart Default',
      })
    },
    onSuccess: () => {
      queryClient.invalidateQueries('chartDefaults')
      addNotification({
        type: 'success',
        title: 'Chart Default Updated',
      })
    },
    ...config,
    mutationFn: updateChartDefaults,
  })
}
