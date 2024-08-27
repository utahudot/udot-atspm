import { Impact } from '@/features/speedManagementTool/types/impact'
import { getEnv } from '@/lib/getEnv'
import { QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')
const postImpacts = async (impactData: Impact): Promise<Impact> => {
  const env = await getEnv()

  const { data } = await axios.post<Impact>(
    `${env.SPEED_URL}/api/Impact`,
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
  return useMutation({
    mutationFn: postImpacts,
    ...config,
  })
}
