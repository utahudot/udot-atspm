import { Impact } from '@/features/speedManagementTool/types/impact'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useQuery } from 'react-query'

const token = Cookies.get('token')
const localhostURL = 'https://localhost:44379/'
const getImpacts = async (): Promise<Impact[]> => {
  // console.log(`switch for ${SPEED_URL} once cloud works`)
  const { data } = await axios.get<Impact[]>(`${localhostURL}api/Impact`, {
    headers: {
      Authorization: `Bearer ${token}`,
      //   'Content-Type': 'application/json',
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
