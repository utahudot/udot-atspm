import { addMinutes } from 'date-fns'
import Cookies from 'js-cookie'
import { FormEvent, useEffect, useState } from 'react'
import { useVerifyUser } from '../../api/verifyUser'
import { VerifyUserResponseDto } from '../../types/verifyUserResponseDto'
import { PasswordHandler, ResponseHandler } from './baseHandler'

export interface VerifyUserHandler extends PasswordHandler, ResponseHandler {
  data: VerifyUserResponseDto
  submitted: boolean
  handleSubmit(event: FormEvent<HTMLFormElement>): void
}

export const useVerifyUserHandler = (): VerifyUserHandler => {
  const [submitted, setSubmitted] = useState(false)
  const [responseError, setResponseError] = useState(false)
  const [responseSuccess, setResponseSuccess] = useState(false)
  const [password, setPassword] = useState<string>('')
  const [data, setData] = useState<VerifyUserResponseDto>()

  const {
    data: verifyTokenData,
    refetch,
    status,
    error,
  } = useVerifyUser({
    password,
  })

  useEffect(() => {
    if (status === 'error' && error) {
      setData((error as any).response.data as VerifyUserResponseDto)
    }
    if (verifyTokenData) {
      setData(verifyTokenData as VerifyUserResponseDto)
    }
  }, [error, verifyTokenData, status])

  useEffect(() => {
    if (status === 'success' && data !== undefined) {
      Cookies.set('resetToken', data.token, {
        expires: addMinutes(new Date(), 5),
        // httpOnly: true,
        secure: true,
        sameSite: 'strict',
      })
      Cookies.set('username', data.username, {
        expires: addMinutes(new Date(), 5),
      })
      window.location.href = '/changePassword'
    }
  }, [data, status])

  useEffect(() => {
    if (status === 'success') {
      setResponseSuccess(true)
    }

    if (status === 'error') {
      setResponseError(true)
    }
  }, [status])

  const handleSubmitForm = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSubmitted(true)
    refetch()
  }

  const component: VerifyUserHandler = {
    data: data as VerifyUserResponseDto,
    password,
    responseError,
    responseSuccess,
    submitted,
    handleSubmit: (event: FormEvent<HTMLFormElement>) => {
      handleSubmitForm(event)
    },
    handleResponseError: (val: boolean) => {
      setResponseError(val)
    },
    handleResponseSuccess: (val: boolean) => {
      setResponseSuccess(val)
    },
    savePassword: (pass: string) => {
      setPassword(pass)
    },
    validatePassword: () => {
      return null
    },
  }

  return component
}
