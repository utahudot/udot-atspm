import { identityAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'
import { useQuery } from 'react-query'
import { VerifyUserResponseDto } from '../types/verifyUserResponseDto'

type QueryFnType = typeof verifyUser

type UseVerifyResetTokenOptions = {
  config?: QueryConfig<QueryFnType>
  password: string
}

const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

const verifyUser = async (password: string): Promise<VerifyUserResponseDto> => {
  const response: VerifyUserResponseDto = await identityAxios.post(
    'Account/verifyUserPasswordReset',
    {
      password,
    },
    { headers }
  )
  return response
}

export const useVerifyUser = ({
  config,
  password,
}: UseVerifyResetTokenOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryFn: () => verifyUser(password),
  })
}
