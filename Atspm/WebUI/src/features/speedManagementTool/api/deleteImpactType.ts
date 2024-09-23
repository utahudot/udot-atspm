import { getEnv } from '@/lib/getEnv'
import { QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'
import { useNotificationStore } from '@/stores/notifications'

const token = Cookies.get('token')
const deleteImpactType = async (id: string): Promise<void> => {
  const env = await getEnv()

  await axios.delete(`${env.SPEED_URL}api/v1/ImpactType/${id}`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  })
}

export const useDeleteImpactType = (config?: QueryConfig<typeof deleteImpactType>) => {
  const { addNotification } = useNotificationStore() // Get notification method

  return useMutation({
    mutationFn: deleteImpactType,

    onError: (error: any) => {
      // Add error notification
      addNotification({
        type: 'error',
        title: 'Failed to Delete Impact Type',
      })
    },
    onSuccess: (_, impactTypeId: string) => {
      // Add success notification
      addNotification({
        type: 'success',
        title: 'Impact Type Deleted',
      })
    },
    ...config,
  })
}
