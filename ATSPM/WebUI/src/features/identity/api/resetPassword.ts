import { identityAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

type QueryFnType = typeof resetPassword

type UseLoginOptions = {
  config?: QueryConfig<QueryFnType>
  email: string
}

// const userToken = Cookies.get('token')

const resetPassword = async (email: string) => {
  const response: any = await identityAxios.post('/Account/forgotpassword', {
    email,
  })
  return response
}

export const useResetPassword = ({ config, email }: UseLoginOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryFn: () => resetPassword(email),
  })
}
