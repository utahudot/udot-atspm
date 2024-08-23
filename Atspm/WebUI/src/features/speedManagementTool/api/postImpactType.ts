import { ImpactType } from '@/features/speedManagementTool/types/impact'
import { QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')
const localhostURL = 'https://localhost:44379/'

const postImpactType = async (impactData: {
  name: string
  description: string
}): Promise<ImpactType> => {
  // console.log(`switch for ${SPEED_URL} once cloud works`)

  // Correcting the axios.post call
  const response = await axios.post<ImpactType>(
    `${localhostURL}api/ImpactType`,
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
  return useMutation({
    mutationFn: postImpactType, // Correcting function name here
    ...config,
  })
}
