import { identityAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import Cookies from 'js-cookie'
import { useQuery } from 'react-query'
import UserDto from '../types/userDto'

type QueryFnType = typeof editUserInfo

type UseEditUserOptions = {
  config?: QueryConfig<QueryFnType>
  userInfo: UserDto
}
const userToken = Cookies.get('token')

const editUserInfo = async (userInfo: UserDto): Promise<UserDto> => {
  const result: UserDto = await identityAxios.put('/Profile', userInfo, {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${userToken}`,
    },
  })
  return result
}

export const useEditUserInfo = ({ config, userInfo }: UseEditUserOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryFn: () => editUserInfo(userInfo),
  })
}
