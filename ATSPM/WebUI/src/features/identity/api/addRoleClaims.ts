import { identityAxios } from '@/lib/axios'
import { AxiosError } from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'
interface AddRoleClaimsData {
  roleName: string
  claims: string[]
}

const addRoleClaims = async (data: AddRoleClaimsData): Promise<void> => {
  const token = Cookies.get('token')
  const headers = {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token}`,
  }

  const response = await identityAxios.post(
    `Claims/add/${data.roleName}`,
    {
      claims: data.claims,
    },
    {
      headers,
    }
  )
}

export const useAddRoleClaims = () => {
  return useMutation<void, AxiosError, AddRoleClaimsData>(addRoleClaims)
}
