import { ImpactType } from '@/features/speedManagementTool/types/impact'
import { getEnv } from '@/lib/getEnv'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useQuery } from 'react-query'

const token = Cookies.get('token')
const getImpactTypes = async (): Promise<ImpactType[]> => {
  const env = await getEnv()

  const { data } = await axios.get<ImpactType[]>(
    `${env.SPEED_URL}ImpactType`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  )

  return data
}

export const useGetImpactTypes = (
  config?: QueryConfig<typeof getImpactTypes>
) => {
  return useQuery<ExtractFnReturnType<typeof getImpactTypes>>({
    queryKey: ['impactsTypes'],
    queryFn: getImpactTypes,
    ...config,
  })
}
