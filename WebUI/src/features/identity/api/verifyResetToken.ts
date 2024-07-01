import { identityAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

type QueryFnType = typeof verifyResetTokenValidity

export interface VerifyToken {
  token: string
  message: string
}

type UseVerifyResetTokenOptions = {
  config?: QueryConfig<QueryFnType>
  username: string
  token: string
}

const verifyResetTokenValidity = async (username: string, token: string) => {
  const response: VerifyToken = await identityAxios.post('Token/verify/reset', {
    username,
    token,
  })
  return response
}

export const useVerifyResetToken = ({
  config,
  username: email,
  token,
}: UseVerifyResetTokenOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryFn: () => verifyResetTokenValidity(email, token),
  })
}
