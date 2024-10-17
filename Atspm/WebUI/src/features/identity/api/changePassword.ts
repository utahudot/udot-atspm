// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - changePassword.ts
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

type QueryFnType = typeof changePassword

type UseLoginOptions = {
  config?: QueryConfig<QueryFnType>
  resetToken: string
  newPassword: string
  confirmPassword: string
}

const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

const changePassword = async (
  resetToken: string,
  newPassword: string,
  confirmPassword: string
) => {
  const response: any = await identityAxios.post(
    'Account/changePassword',
    {
      resetToken,
      newPassword,
      confirmPassword,
    },
    { headers }
  )
  return response
}

export const useChangePassword = ({
  config,
  resetToken,
  newPassword,
  confirmPassword,
}: UseLoginOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryFn: () => changePassword(resetToken, newPassword, confirmPassword),
  })
}
