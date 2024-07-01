import IdentityDto from '@/features/identity/types/identityDto'
import { identityAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

type QueryFnType = typeof getLogin

type ExternalQueryFnType = typeof getUtahLogin

type baseOptions = {
  config?: QueryConfig<QueryFnType>
}

type externalOptions = {
  config?: QueryConfig<ExternalQueryFnType>
}

type UseLoginOptions = baseOptions & {
  email: string
  password: string
}

const getLogin = async (
  email: string,
  password: string
): Promise<IdentityDto> => {
  const result: IdentityDto = await identityAxios.post('Account/login', {
    email,
    password,
    rememberMe: false,
  })

  return result
}

const getUtahLogin = async () => {
  const response = await identityAxios.get('/Account/external-login', {})
  return response
}

export const useExternalLogin = ({ config }: externalOptions) => {
  return useQuery<ExtractFnReturnType<ExternalQueryFnType>>({
    ...config,
    enabled: false,
    queryFn: () => getUtahLogin(),
  })
}

export const useLogin = ({ config, email, password }: UseLoginOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryFn: () => getLogin(email, password),
  })
}
