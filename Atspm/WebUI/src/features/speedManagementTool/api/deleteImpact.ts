import { getEnv } from '@/lib/getEnv'
import { QueryConfig } from '@/lib/react-query'
import { useNotificationStore } from '@/stores/notifications'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')

const deleteImpacts = async (id: string): Promise<void> => {
  const env = await getEnv()

  await axios.delete(`${env.SPEED_URL}api/v1/Impact/${id}`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  })
}

export const useDeleteImpacts = (
  config?: QueryConfig<typeof deleteImpacts>
) => {
  const { addNotification } = useNotificationStore()

  return useMutation({
    mutationFn: deleteImpacts,

    onError: (error: any) => {
      addNotification({
        type: 'error',
        title: 'Failed to Delete Impact',
      })
    },
    onSuccess: (_, impactId: string) => {
      addNotification({
        type: 'success',
        title: 'Impact Deleted',
      })
    },
    ...config,
  })
}
