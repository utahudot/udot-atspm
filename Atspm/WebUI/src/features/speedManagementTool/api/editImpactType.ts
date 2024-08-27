import { ImpactType } from '@/features/speedManagementTool/types/impact'
import { getEnv } from '@/lib/getEnv'
import { QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')
const editImpactType = async (params: {
  name: string
  description: string
  id: string
}): Promise<ImpactType> => {
  const env = await getEnv()
  const body = { name: params.name, description: params.description }
  const response = await axios.put<ImpactType>(
    `${env.SPEED_URL}/api/ImpactType/${params.id}`,
    body,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  )

  return response.data
}

export const useEditImpactType = (
  config?: QueryConfig<typeof editImpactType>
) => {
  return useMutation({
    mutationFn: editImpactType,
    ...config,
  })
}
