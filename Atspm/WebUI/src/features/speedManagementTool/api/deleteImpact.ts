import { getEnv } from '@/lib/getEnv'
import { QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')

const deleteImpacts = async (id: string): Promise<void> => {
  const env = await getEnv()

  await axios.delete(`${env.SPEED_URL}/api/Impact/${id}`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  })
}

export const useDeleteImpacts = (
  config?: QueryConfig<typeof deleteImpacts>
) => {
  return useMutation({
    mutationFn: deleteImpacts,
    ...config,
  })
}
