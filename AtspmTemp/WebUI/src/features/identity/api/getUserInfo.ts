// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getUserInfo.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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
