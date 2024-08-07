// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - verifyUser.ts
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
