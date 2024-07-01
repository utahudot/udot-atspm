import { identityAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'
import { useQuery } from 'react-query'

type QueryFnType = typeof changePassword

type UseLoginOptions = {
  config?: QueryConfig<QueryFnType>
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

const changePassword = async (
  currentPassword: string,
  newPassword: string,
  confirmPassword: string
) => {
  const response: any = await identityAxios.post(
    'Account/changePassword',
    {
      currentPassword,
      newPassword,
      confirmPassword,
    },
    { headers }
  )
  return response
}

export const useChangePassword = ({
  config,
  currentPassword,
  newPassword,
  confirmPassword,
}: UseLoginOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryFn: () =>
      changePassword(currentPassword, newPassword, confirmPassword),
  })
}
