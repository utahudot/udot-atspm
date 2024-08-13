import { SPEED_URL } from '@/config'
import { ImpactType } from '@/features/speedManagementTool/types/impact'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'
import { QueryConfig } from '@/lib/react-query'

const token = Cookies.get('token')
const localhostURL = 'https://localhost:44379/'

const editImpactType = async (params: { name: string; description: string; id: string }): Promise<ImpactType> => {
  console.log(`switch for ${SPEED_URL} once cloud works`)
  console.log("EDIT TEST ID", params.id) 
  const body = { name: params.name, description: params.description }
  const response = await axios.put<ImpactType>(`${localhostURL}api/ImpactType/${params.id}`, body, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  })

  return response.data
}

export const useEditImpactType = (config?: QueryConfig<typeof editImpactType>) => {
  return useMutation({
    mutationFn: editImpactType, 
    ...config,
  })
}

