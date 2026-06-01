// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - getImpacts.ts
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
import { Impact } from '@/features/speedManagementTool/types/impact'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { getEnv } from '@/utils/getEnv'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useQuery } from 'react-query'

const token = Cookies.get('token')
const getImpacts = async (): Promise<Impact[]> => {
  const env = await getEnv()
  const { data } = await axios.get<Impact[]>(`${env.SPEED_URL}api/v1/Impact`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  })

  return data
}

export const useGetImpacts = (config?: QueryConfig<typeof getImpacts>) => {
  return useQuery<ExtractFnReturnType<typeof getImpacts>>({
    queryKey: ['impacts'],
    queryFn: getImpacts,
    ...config,
  })
}
