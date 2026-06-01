// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - postImpactType.ts
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
import { ImpactType } from '@/features/speedManagementTool/types/impact'
import { QueryConfig } from '@/lib/react-query'
import { useNotificationStore } from '@/stores/notifications'
import { getEnv } from '@/utils/getEnv'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')
const postImpactType = async (impactData: {
  name: string
  description: string
}): Promise<ImpactType> => {
  const env = await getEnv()

  // Correcting the axios.post call
  const response = await axios.post<ImpactType>(
    `${env.SPEED_URL}/api/v1/ImpactType`,
    impactData,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  )

  return response.data
}

export const usePostImpactType = (
  config?: QueryConfig<typeof postImpactType>
) => {
  const { addNotification } = useNotificationStore()

  return useMutation({
    mutationFn: postImpactType,

    onError: (error: any) => {
      addNotification({
        type: 'error',
        title: 'Failed to Create Impact Type',
      })
    },
    onSuccess: (_, variables: any) => {
      addNotification({
        type: 'success',
        title: 'Impact Type Created',
      })
    },
    ...config,
  })
}
