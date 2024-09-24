import { ImpactType } from '@/features/speedManagementTool/types/impact'
import { getEnv } from '@/lib/getEnv'
import { QueryConfig } from '@/lib/react-query'
import { useNotificationStore } from '@/stores/notifications'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')

const editImpactType = async (params: {
  name: string
  description: string
  id: string
}): Promise<ImpactType> => {
  const env = await getEnv()
  const body = { name: params.name, description: params.description }
  const response = await axios.put<ImpactType>(
    `${env.SPEED_URL}api/v1/ImpactType/${params.id}`,
    body,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  )

  return response.data
}

export const useEditImpactType = (
  config?: QueryConfig<typeof editImpactType>
) => {
  const { addNotification } = useNotificationStore()

  return useMutation({
    mutationFn: editImpactType,

    onSuccess: (data) => {
      addNotification({
        type: 'success',
        title: 'Impact Type Edited',
      })
    },

    onError: (error: any) => {
      addNotification({
        type: 'error',
        title: 'Failed to Edit Impact Type',
      })
    },

    ...config,
  })
}
