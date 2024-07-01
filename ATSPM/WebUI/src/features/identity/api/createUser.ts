import IdentityDto from '@/features/identity/types/identityDto'
import { identityAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

type QueryFnType = typeof createUser

type UseLoginOptions = {
  config?: QueryConfig<QueryFnType>
  email: string
  password: string
  firstName: string
  lastName: string
  agency: string
}

const createUser = async (
  email: string,
  password: string,
  firstName: string,
  lastName: string,
  agency: string
) => {
  const response: IdentityDto = await identityAxios.post('Account/register', {
    email,
    password,
    firstName,
    lastName,
    agency,
  })
  return response
}

export const useCreateUser = ({
  config,
  email,
  password,
  firstName,
  lastName,
  agency,
}: UseLoginOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryFn: () => createUser(email, password, firstName, lastName, agency),
  })
}
