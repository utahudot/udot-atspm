// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - verifyResetToken.ts
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

type QueryFnType = typeof verifyResetTokenValidity

export interface VerifyToken {
  token: string
  message: string
}

type UseVerifyResetTokenOptions = {
  config?: QueryConfig<QueryFnType>
  username: string
  token: string
}

const verifyResetTokenValidity = async (username: string, token: string) => {
  const response: VerifyToken = await identityAxios.post('Token/verify/reset', {
    username,
    token,
  })
  return response
}

export const useVerifyResetToken = ({
  config,
  username: email,
  token,
}: UseVerifyResetTokenOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryFn: () => verifyResetTokenValidity(email, token),
  })
}
