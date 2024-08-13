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
  phoneNumberError: string | null
  handleInputChange(field: string, value: string): void
  handleSaveClick(): void
  handleEditClick(): void
  validatePhoneNumber(phoneNumber: string): void
}

export const useProfileHandler = (): ProfileHandler => {
  const [isEditing, setIsEditing] = useState(false)
  const [submitted, setSubmitted] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [responseSuccess, setResponseSuccess] = useState(false)
  const [responseError, setResponseError] = useState(false)
  const [phoneNumberError, setPhoneNumberError] = useState<string | null>(null)
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
      validatePhoneNumber(profileData.data?.phoneNumber || '')
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

  const validatePhoneNumber = (phoneNumber: string) => {
    const phoneRegex = /^(\+1|1)?[-.\s]?\(?[2-9]\d{2}\)?[-.\s]?\d{3}[-.\s]?\d{4}$/
    
    if (!phoneNumber) {
      setPhoneNumberError("Phone number is required")
    } else if (!phoneRegex.test(phoneNumber)) {
      setPhoneNumberError("Must be a valid phone number")
    } else {
      setPhoneNumberError(null)
    }
  }

  const handleInputChange = (field: string, value: string) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }))
    if (field === 'phoneNumber') {
      validatePhoneNumber(value)
    }
  }

  const handleSaveClick = () => {
    if (!phoneNumberError) {
      refetch()
    }
  }

  const component: ProfileHandler = {
    profileData: formData,
    submitted,
    responseError,
    responseSuccess,
    isEditing,
    isLoading,
    phoneNumberError,
    handleResponseError: (val: boolean) => {
      setResponseError(val)
    },
    handleResponseSuccess: (val: boolean) => {
      setResponseSuccess(val)
    },
    handleInputChange,
    handleSaveClick,
    handleEditClick: () => {
      setIsEditing(!isEditing)
    },
    validatePhoneNumber,
  }

  return component
}