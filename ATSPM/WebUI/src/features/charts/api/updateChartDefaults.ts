// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - updateChartDefaults.ts
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
