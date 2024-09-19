import { Impact } from '@/features/speedManagementTool/types/impact'
import { getEnv } from '@/lib/getEnv'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useQuery } from 'react-query'

const token = Cookies.get('token')
const getImpacts = async (): Promise<Impact[]> => {
  const env = await getEnv()
  const { data } = await axios.get<Impact[]>(`${env.SPEED_URL}Impact`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  })

  return data
}

export const useGetImpacts = (config?: QueryConfig<typeof getImpacts>) => {
  return useQuery<ExtractFnReturnType<typeof getImpacts>>({
    queryKey: ['impacts'],
    queryFn: getImpacts,
    ...config,
  })
}
