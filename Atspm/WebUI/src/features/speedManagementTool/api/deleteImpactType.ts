// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - deleteImpactType.ts
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
import { QueryConfig } from '@/lib/react-query'
import { useNotificationStore } from '@/stores/notifications'
import { getEnv } from '@/utils/getEnv'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'

const token = Cookies.get('token')
const deleteImpactType = async (id: string): Promise<void> => {
  const env = await getEnv()

  await axios.delete(`${env.SPEED_URL}api/v1/ImpactType/${id}`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  })
}

export const useDeleteImpactType = (
  config?: QueryConfig<typeof deleteImpactType>
) => {
  const { addNotification } = useNotificationStore() // Get notification method

  return useMutation({
    mutationFn: deleteImpactType,

    onError: (error: any) => {
      // Add error notification
      addNotification({
        type: 'error',
        title: 'Failed to Delete Impact Type',
      })
    },
    onSuccess: (_, impactTypeId: string) => {
      // Add success notification
      addNotification({
        type: 'success',
        title: 'Impact Type Deleted',
      })
    },
    ...config,
  })
}
