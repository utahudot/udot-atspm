import { Impact } from '@/features/speedManagementTool/types/impact'
import { QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')
const localhostURL = 'https://localhost:44379/'

const postImpacts = async (impactData: Impact): Promise<Impact> => {
  // console.log(`switch for ${SPEED_URL} once cloud works`)

  const { data } = await axios.post<Impact>(
    `${localhostURL}api/Impact`,
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
