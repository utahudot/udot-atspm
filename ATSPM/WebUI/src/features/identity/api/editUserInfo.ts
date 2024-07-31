import { identityAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import Cookies from 'js-cookie'
import { useMutation } from 'react-query'
import UserDto from '../types/userDto'
import { useNotificationStore } from '@/stores/notifications'

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
        title: 'Success',
        message: 'Profile updated successfully',
      })
      if (config?.onSuccess) {
        config.onSuccess(data)
      }
    },
    onError: (error) => {
      addNotification({
        type: 'error',
        title: 'Error',
        message: 'Failed to update profile',
      })
      if (config?.onError) {
        config.onError(error)
      }
    },
  })
}
