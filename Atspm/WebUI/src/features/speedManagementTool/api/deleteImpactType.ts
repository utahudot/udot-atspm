import { QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')
const localhostURL = 'https://localhost:44379/'

const deleteImpactType = async (id: string): Promise<void> => {
  // console.log(`switch for ${SPEED_URL} once cloud works`)

  await axios.delete(`${localhostURL}api/ImpactType/${id}`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  })
}

export const useDeleteImpactType = (
  config?: QueryConfig<typeof deleteImpactType>
) => {
  return useMutation({
    mutationFn: deleteImpactType,
    ...config,
  })
}
