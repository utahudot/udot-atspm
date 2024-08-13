// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getLogin.ts
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
