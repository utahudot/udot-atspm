// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - resetPassword.ts
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
