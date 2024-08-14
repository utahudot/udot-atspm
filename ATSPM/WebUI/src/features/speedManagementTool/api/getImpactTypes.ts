import { SPEED_URL } from '@/config'
import { ImpactType } from '@/features/speedManagementTool/types/impact'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useQuery } from 'react-query'

const token = Cookies.get('token')
const localhostURL = 'https://localhost:44379/'
const getImpactTypes = async (): Promise<ImpactType[]> => {
    console.log(`switch for ${SPEED_URL} once cloud works`)
  const { data } = await axios.get<ImpactType[]>(`${localhostURL}api/ImpactType`, {
    headers: {
      Authorization: `Bearer ${token}`,
      //   'Content-Type': 'application/json',
    },
  })

  return data
}

export const useGetImpactTypes = (config?: QueryConfig<typeof getImpactTypes>) => {
  return useQuery<ExtractFnReturnType<typeof getImpactTypes>>({
    queryKey: ['impactsTypes'],
    queryFn: getImpactTypes,
    ...config,
  })
}
