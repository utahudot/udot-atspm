import { ImpactType } from '@/features/speedManagementTool/types/impact'
import { getEnv } from '@/lib/getEnv'
import { QueryConfig } from '@/lib/react-query'
import { useNotificationStore } from '@/stores/notifications'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')
const postImpactType = async (impactData: {
  name: string
  description: string
}): Promise<ImpactType> => {
  const env = await getEnv()

  // Correcting the axios.post call
  const response = await axios.post<ImpactType>(
    `${env.SPEED_URL}/api/v1/ImpactType`,
    impactData,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  )

  return response.data
}

export const usePostImpactType = (
  config?: QueryConfig<typeof postImpactType>
) => {
  const { addNotification } = useNotificationStore()

  return useMutation({
    mutationFn: postImpactType,

    onError: (error: any) => {
      addNotification({
        type: 'error',
        title: 'Failed to Create Impact Type',
      })
    },
    onSuccess: (_, variables: any) => {
      addNotification({
        type: 'success',
        title: 'Impact Type Created',
      })
    },
    ...config,
  })
}
