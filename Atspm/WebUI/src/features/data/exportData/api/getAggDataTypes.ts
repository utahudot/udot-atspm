// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getAggDataTypes.ts
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
import { dataAxios } from '@/lib/axios'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'
import { useQuery } from 'react-query'

const route = `/Aggregation/GetDataTypes`
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

export function useGetAggDataTypes() {
  return useQuery<ApiResponse<string[]>>('aggDataTypes', async () => {
    const response = await dataAxios.get(route, { headers })
    return response
  })
}
