// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - createUser.ts
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
