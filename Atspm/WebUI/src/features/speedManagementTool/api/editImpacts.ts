import { Impact } from '@/features/speedManagementTool/types/impact'
import { getEnv } from '@/lib/getEnv'
import { QueryConfig } from '@/lib/react-query'
import { useNotificationStore } from '@/stores/notifications'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')

const editImpact = async (
  impactId: string,
  impactData: Impact
): Promise<Impact> => {
  const env = await getEnv()

  const { data } = await axios.put<Impact>(
    `${env.SPEED_URL}api/v1/impact/${impactId}`,
    impactData,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  )

  return data
}

export const useEditImpact = (config?: QueryConfig<typeof editImpact>) => {
  const { addNotification } = useNotificationStore();

  return useMutation({
    mutationFn: ({
      impactId,
      impactData,
    }: {
      impactId: string
      impactData: Impact
    }) => editImpact(impactId, impactData),

    onError: (error: any) => {
      addNotification({
        type: 'error',
        title: 'Failed to Edit Impact',
      })
    },
    onSuccess: (data: Impact) => {
      addNotification({
        type: 'success',
        title: 'Impact Edited',
      })
    },
    ...config,
  })
}
