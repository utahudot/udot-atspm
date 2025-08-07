// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - editUserInfo.ts
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
import { QueryConfig } from '@/lib/react-query'
import { useNotificationStore } from '@/stores/notifications'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'
import UserDto from '../types/userDto'

const userToken = Cookies.get('token')

const editUserInfo = async (userInfo: UserDto): Promise<UserDto> => {
  const result = await identityAxios.put('/Profile', userInfo, {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${userToken}`,
    },
  })
  return result.data
}

export const useEditUserInfo = (config?: QueryConfig<typeof editUserInfo>) => {
  const addNotification = useNotificationStore((state) => state.addNotification)
  return useMutation(editUserInfo, {
    ...config,
    onSuccess: (data) => {
      addNotification({
        type: 'success',
        title: 'Profile updated',
      })
      if (config?.onSuccess) {
        config.onSuccess(data)
      }
    },
    onError: (error) => {
      addNotification({
        type: 'error',
        title: 'Failed to update profile',
      })
      if (config?.onError) {
        config.onError(error)
      }
    },
  })
}
