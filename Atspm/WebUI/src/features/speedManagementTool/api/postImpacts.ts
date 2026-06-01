// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - postImpacts.ts
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
import { QueryConfig } from '@/lib/react-query'
import { useNotificationStore } from '@/stores/notifications'
import { getEnv } from '@/utils/getEnv'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')
const postImpacts = async (impactData: Impact): Promise<Impact> => {
  const env = await getEnv()

  const { data } = await axios.post<Impact>(
    `${env.SPEED_URL}api/v1/Impact`,
    impactData,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  )

  return data
}

export const usePostImpacts = (config?: QueryConfig<typeof postImpacts>) => {
  const { addNotification } = useNotificationStore()

  return useMutation({
    mutationFn: postImpacts,
    onMutate: async (impactData: Impact) => {},
    onError: (error: any) => {
      addNotification({
        type: 'error',
        title: 'Failed to Create Impact',
      })
    },
    onSuccess: (data: Impact) => {
      addNotification({
        type: 'success',
        title: 'Impact Created',
      })
    },
    ...config,
  })
}
