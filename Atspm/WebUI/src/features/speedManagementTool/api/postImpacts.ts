import { Impact } from '@/features/speedManagementTool/types/impact'
import { getEnv } from '@/lib/getEnv'
import { QueryConfig } from '@/lib/react-query'
import { useNotificationStore } from '@/stores/notifications'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')
const postImpacts = async (impactData: Impact): Promise<Impact> => {
  const env = await getEnv()

  const { data } = await axios.post<Impact>(
    `${env.SPEED_URL}api/v1/Impact`,
    impactData,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  )

  return data
}

export const usePostImpacts = (config?: QueryConfig<typeof postImpacts>) => {
  const { addNotification } = useNotificationStore()

  return useMutation({
    mutationFn: postImpacts,
    onMutate: async (impactData: Impact) => {},
    onError: (error: any) => {
      addNotification({
        type: 'error',
        title: 'Failed to Create Impact',
      })
    },
    onSuccess: (data: Impact) => {
      addNotification({
        type: 'success',
        title: 'Impact Created',
      })
    },
    ...config,
  })
}
