import { useEffect, useState } from 'react'
import { useEditUserInfo } from '../../api/editUserInfo'
import { useUserInfo } from '../../api/getUserInfo'
import UserDto from '../../types/userDto'
import { ResponseHandler } from './baseHandler'

export interface ProfileHandler extends ResponseHandler {
  profileData: UserDto
  submitted: boolean
  isEditing: boolean
  isLoading: boolean
  handleInputChange(field: string, value: string): void
  handleSaveClick(): void
  handleEditClick(): void
}

export const useProfileHandler = (): ProfileHandler => {
  const [isEditing, setIsEditing] = useState(false)
  const [submitted, setSubmitted] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [responseSuccess, setResponseSuccess] = useState(false)
  const [responseError, setResponseError] = useState(false)
  const profileData = useUserInfo({})
  const [formData, setFormData] = useState<UserDto>({
    firstName: '',
    lastName: '',
    agency: '',
    email: '',
    phoneNumber: '',
    roles: '',
  })

  const {
    refetch,
    data: saveUser,
    isSuccess,
    error,
  } = useEditUserInfo({ userInfo: formData })

  useEffect(() => {
    if (profileData.data) {
      setFormData({
        firstName: profileData.data.firstName,
        lastName: profileData.data.lastName,
        agency: profileData.data.agency,
        email: profileData.data.email,
        phoneNumber: profileData.data?.phoneNumber
          ? profileData.data?.phoneNumber
          : '',
        roles: profileData.data.roles,
      })
      setIsLoading(false)
    }
  }, [profileData.data])

  useEffect(() => {
    if (saveUser !== undefined && isSuccess) {
      setResponseSuccess(true)
      setSubmitted(true)
      setIsEditing(false)
    }
    if (error) {
      setResponseError(true)
    }
  }, [error, isSuccess, saveUser])

  const component: ProfileHandler = {
    profileData: formData,
    submitted,
    responseError,
    responseSuccess,
    isEditing,
    isLoading,
    handleResponseError: (val: boolean) => {
      setResponseError(val)
    },
    handleResponseSuccess: (val: boolean) => {
      setResponseSuccess(val)
    },
    handleInputChange: (field: string, value: string) => {
      setFormData((prev) => ({
        ...prev,
        [field]: value,
      }))
    },
    handleSaveClick: () => {
      refetch()
    },
    handleEditClick: () => {
      setIsEditing(true)
    },
  }

  return component
}
