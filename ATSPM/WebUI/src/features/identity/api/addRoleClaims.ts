// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - addRoleClaims.ts
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
import { AxiosError } from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'
interface AddRoleClaimsData {
  roleName: string
  claims: string[]
}

const addRoleClaims = async (data: AddRoleClaimsData): Promise<void> => {
  const token = Cookies.get('token')
  const headers = {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token}`,
  }

  const response = await identityAxios.post(
    `Claims/add/${data.roleName}`,
    {
      claims: data.claims,
    },
    {
      headers,
    }
  )
}

export const useAddRoleClaims = () => {
  return useMutation<void, AxiosError, AddRoleClaimsData>(addRoleClaims)
}
