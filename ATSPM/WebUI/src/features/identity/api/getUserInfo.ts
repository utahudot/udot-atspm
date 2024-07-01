import { identityAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import Cookies from 'js-cookie'
import { useQuery } from 'react-query'
import UserDto from '../types/userDto'

type QueryFnType = typeof getUserInfo

type UseLoginOptions = {
  config?: QueryConfig<QueryFnType>
}
const userToken = Cookies.get('token')

const getUserInfo = async (): Promise<UserDto> => {
  const result: UserDto = await identityAxios.get('Profile', {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${userToken}`,
    },
  })

  return result
}

export const useUserInfo = ({ config }: UseLoginOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryKey: [userToken],
    queryFn: () => getUserInfo(),
  })
}
